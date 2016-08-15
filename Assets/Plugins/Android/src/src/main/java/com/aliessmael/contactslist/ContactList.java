package com.aliessmael.contactslist;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.HashMap;

import com.unity3d.player.UnityPlayer;

import android.app.Activity;
import android.content.ContentResolver;
import android.content.ContentUris;
import android.content.Context;
import android.database.Cursor;
import android.net.Uri;
import android.provider.ContactsContract;
import android.provider.ContactsContract.CommonDataKinds.Phone;


import android.telephony.TelephonyManager;
import android.util.Log;



public class ContactList {
	static String myNumber ;
	static String simSerialNumber ;
	static String networkOperator ;
	static String networkCountryIso ;
	static void fillSimInfo( Activity activity ){
	    TelephonyManager mTelephonyMgr = (TelephonyManager) activity.getSystemService(Context.TELEPHONY_SERVICE); 
	    myNumber = mTelephonyMgr.getLine1Number();
	    simSerialNumber = mTelephonyMgr.getSimSerialNumber();
	    networkOperator = mTelephonyMgr.getNetworkOperator();
	    networkCountryIso = mTelephonyMgr.getNetworkCountryIso();
	}



	


	private static Map<String, Contact> contactsMap = null;
	private static ArrayList<Contact> contactList = null;
	

	static ContentResolver cr = null;
	static Activity activity;
	static boolean mLoadPhoto;
	static boolean mLoadNumbers;
	static boolean mLoadEmails;
	static boolean mLoadConnections;
	public static void LoadInformation( Activity _activity , boolean _loadPhoto ,boolean _loadNumbers,  boolean _loadEmails , boolean _loadConnections)
	{
		activity = _activity;
		mLoadPhoto = _loadPhoto;
		mLoadNumbers = _loadNumbers;
		mLoadEmails = _loadEmails;
		mLoadConnections = _loadConnections;

		Runnable runnable = new Runnable()
		{
			public void run()
			{
				LoadInformation_thread();
			}
		};
		new Thread(runnable).start();
	}

	static byte[] getContact( int id )
	{
		Contact contact = contactList.get( id );
		byte[] data = contact.toBytes();
		contact.unityGotThis = true;
		return data;
	}
	public static void LoadInformation_thread( )
	{
		fillSimInfo(activity);

		contactList = new ArrayList<Contact>();
		contactsMap = new HashMap<>( );

		try {

			loadNames();

			if( mLoadNumbers )
				loadNumbers();
			if( mLoadEmails)
				loadEmails();
			if( mLoadConnections )
				loadConnections();

			Contact.Buffer = ByteBuffer.allocate(1024 * 1024);
			Contact.Buffer.order(ByteOrder.LITTLE_ENDIAN);
			for( int i = 0 ; i < contactList.size() ; i++ )
			{
				Contact.Buffer.clear();
				Contact c = contactList.get( i );
				if( mLoadPhoto )
				{
					c.Photo = getPhoto( c );
				}

				UnityPlayer.UnitySendMessage( "ContactsListMessageReceiver", "OnContactReady", Integer.toString(i) );
				while (!c.unityGotThis)
				{
					Thread.sleep(1);
				};
			}

			UnityPlayer.UnitySendMessage( "ContactsListMessageReceiver", "OnInitializeDone", "" );
			contactList = null;
			contactsMap = null;
			Contact.Buffer = null;

		} catch (Exception e) {
			String error = Log.getStackTraceString( e);
			UnityPlayer.UnitySendMessage("ContactsListMessageReceiver", "OnInitializeFail", error);

		}
	}


	static void loadNames()
	{
		Cursor cc = null;
		cr = activity.getContentResolver() ;
		String[] projection =
		{
			ContactsContract.Contacts._ID,
			ContactsContract.Contacts.DISPLAY_NAME,
		};
		String sortOrder = ContactsContract.Contacts.DISPLAY_NAME + " COLLATE LOCALIZED ASC";
		cc = cr.query( ContactsContract.Contacts.CONTENT_URI,projection,null,null, sortOrder);
		if(cc.getCount()>0){

			while (cc.moveToNext())
			{
				String idContact = cc.getString(cc.getColumnIndex(ContactsContract.Contacts._ID));

				String name = cc.getString(cc.getColumnIndex(ContactsContract.Contacts.DISPLAY_NAME));

				Contact item = new Contact();
				item.Id	 =  idContact ;
				item.Name = name ;
				item.Phones = new ArrayList<ContactPhone>();
				item.Emails = new ArrayList<ContactEmail>();
				item.Connections = new ArrayList<String>();

				contactsMap.put( idContact, item );
				contactList.add(item);

			}
		}
	}


	static void loadNumbers()
	{
		final String[] numberProjection = new String[]{
				Phone.NUMBER,
				Phone.TYPE,
				Phone.CONTACT_ID,
		};

		Cursor phone = cr.query(
				Phone.CONTENT_URI,
				numberProjection,
				null,
				null,
				null);

		if (phone.moveToFirst()) {
			final int contactNumberColumnIndex = phone.getColumnIndex(Phone.NUMBER);
			final int contactTypeColumnIndex = phone.getColumnIndex(Phone.TYPE);
			final int contactIdColumnIndex = phone.getColumnIndex(Phone.CONTACT_ID);

			while (!phone.isAfterLast()) {
				final String number = phone.getString(contactNumberColumnIndex);
				final String contactId = phone.getString(contactIdColumnIndex);
				Contact contact = contactsMap.get(contactId);
				if (contact == null) {
					phone.moveToNext();
					continue;
				}
				final int type = phone.getInt(contactTypeColumnIndex);

				String phoneType = (String)ContactsContract.CommonDataKinds.Phone.getTypeLabel(activity.getResources(), type , "Mobile");
				if( phoneType.equals("Custom") )
					phoneType = phone.getString(phone.getColumnIndex(ContactsContract.CommonDataKinds.Phone.LABEL));

				ContactPhone cp = new ContactPhone();
				cp.Number = number;
				cp.Type = phoneType;
				contact.Phones.add( cp );
				phone.moveToNext();
			}
		}

		phone.close();
	}
	static void loadEmails()
	{
		final String[] emailProjection = new String[]{
				ContactsContract.CommonDataKinds.Email.DATA,
				ContactsContract.CommonDataKinds.Email.TYPE,
				ContactsContract.CommonDataKinds.Email.CONTACT_ID,
		};

		Cursor email = cr.query(
				ContactsContract.CommonDataKinds.Email.CONTENT_URI,
				emailProjection,
				null,
				null,
				null);

		if (email.moveToFirst()) {
			final int contactEmailColumnIndex = email.getColumnIndex(ContactsContract.CommonDataKinds.Email.DATA);
			final int contactTypeColumnIndex = email.getColumnIndex(ContactsContract.CommonDataKinds.Email.TYPE);
			final int contactIdColumnsIndex = email.getColumnIndex(ContactsContract.CommonDataKinds.Email.CONTACT_ID);

			while (!email.isAfterLast()) {
				final String address = email.getString(contactEmailColumnIndex);
				final String contactId = email.getString(contactIdColumnsIndex);
				final int type = email.getInt(contactTypeColumnIndex);
				String customLabel = "Custom";
				Contact contact = contactsMap.get(contactId);
				if (contact == null) {
					email.moveToNext();
					continue;
				}
				CharSequence emailType = ContactsContract.CommonDataKinds.Email.getTypeLabel(activity.getResources(), type, customLabel);
				ContactEmail ce = new ContactEmail();
				ce.Address = address;
				ce.Type    = emailType.toString() ;
				contact.Emails.add( ce );
				email.moveToNext();
			}
		}

		email.close();

	}

	static void loadConnections()
	{
		final String[] connectionsProjection = new String[]{
				ContactsContract.RawContacts.CONTACT_ID,
				ContactsContract.RawContacts.ACCOUNT_NAME,
		};

		Cursor connection = cr.query(
				ContactsContract.RawContacts.CONTENT_URI,
				connectionsProjection,
				null,
				null,
				null);

		if (connection.moveToFirst()) {
			final int idColumnIndex = connection.getColumnIndex(ContactsContract.RawContacts.CONTACT_ID);
			final int connectionColumnIndex = connection.getColumnIndex(ContactsContract.RawContacts.ACCOUNT_NAME);


			while (!connection.isAfterLast()) {
				final String contactId = connection.getString(idColumnIndex);
				final String connectionName = connection.getString(connectionColumnIndex);


				Contact contact = contactsMap.get(contactId);
				if (contact == null) {
					connection.moveToNext();
					continue;
				}
				contact.Connections.add( connectionName );
				connection.moveToNext();
			}
		}

		connection.close();
	}


	static byte[] getPhoto( Contact item ) {
		try {
			long contactId = Long.parseLong( item.Id );
			Uri contactUri = ContentUris.withAppendedId(ContactsContract.Contacts.CONTENT_URI, contactId);
	        Uri photoUri = Uri.withAppendedPath(contactUri, ContactsContract.Contacts.Photo.CONTENT_DIRECTORY);
	        Cursor cursor = cr.query(photoUri,
	                new String[] {ContactsContract.CommonDataKinds.Photo.PHOTO}, null, null, null);
	        if (cursor == null) {
	            return null;
	        }
	        try {
	            if (cursor.moveToFirst()) {
	                byte[] data = cursor.getBlob(0);
	                if (data != null) {
	                    return data ;
	                }
	            }
	        } finally {
	            cursor.close();
	        }
	        return null;
		} catch (Exception e) {
			logException( e);
	    	return null;
		}
	 }

	static void log( String message )
	{
		//UnityPlayer.UnitySendMessage("ContactsListMessageReceiver", "Log", message);
	}

	static void logException( Exception e )
	{
		String message = Log.getStackTraceString( e);
		UnityPlayer.UnitySendMessage("ContactsListMessageReceiver","Error",message);
		e.printStackTrace();
	}
}
