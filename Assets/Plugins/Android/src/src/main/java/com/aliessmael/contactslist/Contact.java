package com.aliessmael.contactslist;

import android.util.Log;

import com.unity3d.player.UnityPlayer;


import java.nio.ByteBuffer;
import java.nio.charset.Charset;
import java.nio.charset.StandardCharsets;
import java.util.List;


public class Contact {

	String 				Id 	;
	String 				Name ;

	byte[] 				Photo 		;

    List<ContactPhone> 	Phones ;
	List<ContactEmail> 	Emails;
	List<String>	   	Connections ;

	public static ByteBuffer Buffer = null;


	volatile boolean unityGotThis;
	public byte[] toBytes()
	{
		try {

			putString(Id);
			putString(Name);

			if (Photo == null) {
				log( "write empty photo");
				Buffer.putShort((short) 0);
			} else {
				log( "write photo length == " + Photo.length);
				Buffer.putShort((short) Photo.length);
				Buffer.put(Photo);
			}

			if (Phones == null || Phones.size() == 0)
			{
				log( "write empty Phones");
				Buffer.putShort((short) 0);
			} else {
				log( "write Phones size == " + Phones.size() );
				Buffer.putShort((short) Phones.size());
				for (int i = 0; i < Phones.size(); i++) {
					ContactPhone cp = Phones.get(i);
					putString(cp.Number);
					putString(cp.Type);
				}
			}

			if (Emails == null || Emails.size() == 0) {
				log( "write empty Emails");
				Buffer.putShort((short) 0);
			} else {
				log( "write Emails size == " + Emails.size() );
				Buffer.putShort((short) Emails.size());
				for (int i = 0; i < Emails.size(); i++) {
					ContactEmail ce = Emails.get(i);
					putString(ce.Address);
					putString(ce.Type);
				}
			}

			if (Connections == null || Connections.size() == 0) {
				log( "write empty Connections");
				Buffer.putShort((short) 0);
			} else {
				log( "write Connections size == " + Connections.size() );
				Buffer.putShort((short) Connections.size());
				for (int i = 0; i < Connections.size(); i++) {
					putString(Connections.get(i));
				}
			}

			return Buffer.array();
		}
		catch ( Exception e ) {
			e.printStackTrace();
			logException(e);
			return null;
		}
	}

	Charset utf8 = Charset.forName("UTF-8");
	void putString( String value )
	{
		if( value == null || value == "") {
			log( "write empty string ");
			Buffer.putShort((short) 0);
		}
		else
		{
			byte[] data = value.getBytes( utf8 );
			Buffer.putShort((short) data.length );
			Buffer.put(data );
			log( "write string " + value + " length is " + value.length());
		}
	}


	void log( String message )
	{
		//UnityPlayer.UnitySendMessage("ContactsListMessageReceiver", "Log", message);
	}
	void logException( Exception e )
	{
		String message = Log.getStackTraceString(e);
		UnityPlayer.UnitySendMessage("ContactsListMessageReceiver", "Error", message);
		e.printStackTrace();
	}
}

