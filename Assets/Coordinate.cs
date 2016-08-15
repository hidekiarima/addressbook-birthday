using UnityEngine;
using System;
using System.Collections;


public class Coordinate : MonoBehaviour
{
    static string failString;
    static bool contactListLoadDone=false;
    static float plateHeight;
    static float lastMousePositionY;

    static float scrollSpeed=0.0f;

    static float scrollULimit;
    static float scrollDLimit;

    static bool personThisWeekDisp;
    static string userNameAndID;

    public struct MyContact
    {
        public string Name;
        public GameObject NamePlate;
        public string ID;
        public string birthDay;
        public string displayText;
        public Vector3 destination;
        public string properties; 
    }

    public struct birthDayProperty
    {
        public string birthDay;
        public string propertyText;
    }

    public struct kishitsu
    {
        public int ID;
        public string shortText;
        public string longText;
    }

    static MyContact[] MyContacts ;
    static birthDayProperty[] birthdayProperties;
    static kishitsu[] kishitsuList;


    int dispItemIndex = 19; 

    void Start()
    {
        //Read kishitsu data
        var astKishitsu = Resources.Load<TextAsset>("kishitsu");
        var txtKishitsu = astKishitsu.text;
        var txtKishitsuRows = txtKishitsu.Split('\n');
        Array.Resize(ref kishitsuList, txtKishitsuRows.Length);
        for (int i = 0; i < txtKishitsuRows.Length; i++)
        {
            var column = txtKishitsuRows[i].Split(',');
            kishitsuList[i].ID = int.Parse(column[0]);
            kishitsuList[i].shortText = column[1];
            kishitsuList[i].longText = column[2];
        }

        //Read text data
        var astBirthdays = Resources.Load<TextAsset>("birthdays");
        var txtBirthdays = astBirthdays.text;
        var txtBirthdayRows = txtBirthdays.Split('\n');
        Array.Resize(ref birthdayProperties, txtBirthdayRows.Length);
        for (int i=0; i < txtBirthdayRows.Length; i++)
        {
            birthdayProperties[i].birthDay = txtBirthdayRows[i].Split(',')[0];
            birthdayProperties[i].propertyText = txtBirthdayRows[i];
        }

        //
        personThisWeekDisp = PlayerPrefs.GetInt("personThisWeekDisp", 1)==1?true:false;
        userNameAndID = PlayerPrefs.GetString("userNameAndID", "");


        //インターネットから今週の有名人情報をダウンロード
        getPersonThisWeek();

        //連絡先の読み込みをスタートする
        if (Application.platform==RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            Contacts.LoadContactList(onDone, onLoadFailed);
        }
        else
        {
            onDone();
        }


    }

    void Update()
    {
        if (scrollSpeed != 0.0f)
        {
            scroll(scrollSpeed);
            scrollSpeed *= 0.95f;
            if (-0.1f < scrollSpeed && scrollSpeed < 0.1f) { scrollSpeed = 0.0f; }
        }

        //スクロールしすぎて画面外にはみ出してるときは端っこギリギリまで戻す
        Transform BG = GameObject.Find("BackGround").GetComponent<Transform>();
        if (BG.position.y < scrollULimit)
        {
            if (scrollULimit - BG.position.y < plateHeight * 0.05f) { BG.position = new Vector3(0.0f, scrollULimit, 0.0f); }
            scrollSpeed = (scrollULimit - BG.position.y) * 0.3f;
        }
        if (scrollDLimit < BG.position.y)
        {
            if (BG.position.y - scrollDLimit < plateHeight * 0.05f) { BG.position = new Vector3(0.0f, scrollDLimit, 0.0f); }
            scrollSpeed = (scrollDLimit - BG.position.y) * 0.3f;
        }


        //パラレルで動いている連絡先の読み込みが終わっていたら、NamePlateの位置調整を行う。各NamePlateは移動先の座標を持っているので、そこを目指して動く
        if (contactListLoadDone)
        {
            for (int i = 0; i < MyContacts.Length; i++)
            {
                if (Vector3.Distance(MyContacts[i].destination, MyContacts[i].NamePlate.transform.localPosition) < 0.1)
                {
                    MyContacts[i].NamePlate.transform.localPosition = MyContacts[i].destination;
                }
                else
                {
                    MyContacts[i].NamePlate.transform.localPosition = MyContacts[i].NamePlate.transform.localPosition + (MyContacts[i].destination - MyContacts[i].NamePlate.transform.localPosition) * 0.1f;
                }
            }

        }
        
    }

    void onLoadFailed(string reason)
    {
        //failString = reason;
    }

    

    void onDone()
    {
        //連絡先の情報の読み込みが終わるとこのモジュールが呼び出される
        var headerColumn = birthdayProperties[0].propertyText.Split(',');
        TextMesh dispTextMesh = GameObject.Find("txtDispItem").GetComponent<TextMesh>();
        dispTextMesh.text = "begin";

        failString = null;

        int numOfContacts;

        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            //iPhone または Android で実行されたときは端末の連絡先を取ってくる
            numOfContacts = Contacts.ContactsList.Count;
            Array.Resize(ref MyContacts, numOfContacts);
            for (int i = 0; i < numOfContacts; i++)
            {
                Contact c = Contacts.ContactsList[i];
                MyContacts[i].Name = c.Name;
                MyContacts[i].birthDay = c.BirthDay;
                MyContacts[i].ID = c.Id;

                dispTextMesh.text = "reading :" + i.ToString();
            }
        }
        else
        {
            //開発中のテスト用。Windowsで実行されたときはダミーの連絡先を作成する
            numOfContacts = 50;
            Array.Resize(ref MyContacts, numOfContacts);
            for (int i = 0; i < numOfContacts; i++)
            {
                MyContacts[i].Name = i.ToString();
                MyContacts[i].birthDay = "19" + UnityEngine.Random.Range(50.0f, 98.0f).ToString("00") + "-" + UnityEngine.Random.Range(2.0f, 11.0f).ToString("00") + "-" + UnityEngine.Random.Range(2.0f, 27.0f).ToString("00");
                MyContacts[i].ID = i.ToString();

                dispTextMesh.text = "reading :" + i.ToString();
            }
        }

        dispTextMesh.text = "read done";

        //Prepare name plates
        GameObject objBG = GameObject.Find("BackGround");
        

        for (int i = 0; i < numOfContacts; i++)
        {

            MyContacts[i].NamePlate = (GameObject)Instantiate(Resources.Load("NamePlate"), new Vector3(0.0f, 0.0f + i * 0.1f, 0.0f), Quaternion.identity);
            MyContacts[i].properties = getBirthdayPropertyText(MyContacts[i].birthDay);


            TextMesh m = MyContacts[i].NamePlate.transform.FindChild("txtName").GetComponent<TextMesh>();
            m.text = MyContacts[i].Name;

            m = MyContacts[i].NamePlate.transform.FindChild("txtBirthday").GetComponent<TextMesh>();
            m.text = MyContacts[i].birthDay;

            if (MyContacts[i].properties != "")
            {
                var column = MyContacts[i].properties.Split(',');
                int idx = int.Parse(column[20]);
                MyContacts[i].properties = MyContacts[i].properties + "," + getKishitsuShortText(idx);
            }

            MyContacts[i].NamePlate.transform.parent = objBG.transform;
            MyContacts[i].destination = MyContacts[i].NamePlate.transform.position;

            dispTextMesh.text = "plate :" + i.ToString();

            //ユーザ本人の情報は画面上部にも表示
            if (MyContacts[i].Name + ":" + MyContacts[i].ID == userNameAndID)
            {
                GameObject myNamePlate = GameObject.Find("MyNamePlate");

                m = myNamePlate.transform.FindChild("txtName").GetComponent<TextMesh>();
                m.text = MyContacts[i].Name;

                m = myNamePlate.transform.FindChild("txtBirthday").GetComponent<TextMesh>();
                m.text = MyContacts[i].birthDay;
            }
        }

        Transform TopBar = MyContacts[0].NamePlate.transform.FindChild("sprFrame");
        SpriteRenderer SPRen = TopBar.GetComponent<SpriteRenderer>();
        plateHeight = SPRen.bounds.size.y;

        for (int i = 0; i < numOfContacts; i++)
        {
            MyContacts[i].destination = new Vector3(0.0f, -i * plateHeight , 0.0f);
        }

        //暫定。スクロールの上下の端を決める。ほんとは画面サイズなどを見て調整する必要がある気がする
        scrollULimit = 2.56f;
        scrollDLimit = plateHeight * numOfContacts - 5.8f;
        if (scrollDLimit < scrollULimit) { scrollDLimit = scrollULimit; }
        
        changeDispItem(0);

        contactListLoadDone = true;

    }

    //void onDoneWindows()
    //{
    //    failString = null;

    //    //Prepare name plates
    //    int numOfDummyPlates = 300;
    //    GameObject objBG = GameObject.Find("BackGround");
    //    Array.Resize(ref MyContacts, numOfDummyPlates);
    //    for (int i = 0; i < numOfDummyPlates; i++)
    //    {
    //        MyContacts[i].NamePlate = (GameObject)Instantiate(Resources.Load("NamePlate"), new Vector3(0.0f, 0.0f ,0.0f), Quaternion.identity);

    //        TextMesh m = MyContacts[i].NamePlate.transform.FindChild("txtName").GetComponent<TextMesh>();
    //        m.text = i.ToString();

    //        m = MyContacts[i].NamePlate.transform.FindChild("txtBirthday").GetComponent<TextMesh>();
    //        MyContacts[i].birthDay = "1969-05-" + i.ToString("00");
    //        m.text = MyContacts[i].birthDay;

    //        MyContacts[i].properties = getBirthdayPropertyText(MyContacts[i].birthDay);
    //        if (MyContacts[i].properties != "")
    //        {
    //            var column = MyContacts[i].properties.Split(',');
    //            int idx = int.Parse(column[20]);
    //            MyContacts[i].properties = MyContacts[i].properties + "," + getKishitsuShortText(idx);
    //        }

    //        MyContacts[i].NamePlate.transform.parent = objBG.transform;
    //        MyContacts[i].destination = MyContacts[i].NamePlate.transform.position;
    //    }

    //    Transform TopBar = MyContacts[0].NamePlate.transform.FindChild("sprFrame");
    //    SpriteRenderer SPRen = TopBar.GetComponent<SpriteRenderer>();
    //    plateHeight = SPRen.bounds.size.y;

    //    for (int i = 0; i < numOfDummyPlates; i++)
    //    {
    //        MyContacts[i].destination = new Vector3(0.0f,  - i * plateHeight * 0.01f, 0.0f);
    //    }

    //    changeDispItem(0);

    //    contactListLoadDone = true;
    //}

    string getBirthdayPropertyText(string birthday)
    {
        for (int i=0; i<birthdayProperties.Length; i++)
        {
            if(birthday==birthdayProperties[i].birthDay )
            {
                return birthdayProperties[i].propertyText;
            }
        }
        return "";
    }

    string getKishitsuShortText(int kishitsuNo)
    {

        if (1 <= kishitsuNo && kishitsuNo <= kishitsuList.Length)
        {
            return kishitsuList[kishitsuNo-1].shortText;
        }
        else
        {
            return "-";
        }
    }

    public void changeDispItem(int moveBy)
    {
        dispItemIndex += moveBy;
        if (dispItemIndex<1)    { dispItemIndex = 21; }
        if (dispItemIndex > 21) { dispItemIndex = 1; }

        for (int i = 0; i<MyContacts.Length;i++)
        {
            if (MyContacts[i].properties == "")
            {
                MyContacts[i].displayText = "";
            }
            else
            {
                var column = MyContacts[i].properties.Split(',');
                MyContacts[i].displayText = column[dispItemIndex];
            }
            TextMesh m = MyContacts[i].NamePlate.transform.FindChild("txtDesc").GetComponent<TextMesh>();
            m.text = MyContacts[i].displayText;

            //ユーザ本人の情報の表示も変更
            if (MyContacts[i].Name + ":" + MyContacts[i].ID == userNameAndID)
            {
                GameObject myNamePlate = GameObject.Find("MyNamePlate");

                m = myNamePlate.transform.FindChild("txtDesc").GetComponent<TextMesh>();
                m.text = MyContacts[i].displayText;
            }
        }

        var headerColumn = birthdayProperties[0].propertyText.Split(',');
        TextMesh dispTextMesh = GameObject.Find("txtDispItem").GetComponent<TextMesh>();
        dispTextMesh.text = headerColumn[dispItemIndex];



        //メニューの並べ替えの表示も変更
        GameObject sortByDispItemMenu = GameObject.Find("MenuItem3");
        Transform menuItem = sortByDispItemMenu.transform.FindChild("txtMenuItem");
        TextMesh menuTextMesh = menuItem.GetComponent<TextMesh>();
        menuTextMesh.text = headerColumn[dispItemIndex] + "順";
    }


    public void sortNamePlate(string sortBy)
    {
        int[] sortID = new int[MyContacts.Length];
        string[] sortKey = new string[MyContacts.Length];

        for (int i = 0; i< MyContacts.Length; i++)
        {
            sortID[i] = i;

            switch (sortBy)
            {
                case "名前順":
                    sortKey[i] = MyContacts[i].Name;
                    break;
                case "誕生日順":
                    sortKey[i] = MyContacts[i].birthDay;
                    break;
                default:
                    sortKey[i] = MyContacts[i].displayText;
                    break;
            }

        }

        int minIndex;
        int tempSortID;
        string tempSortKey;
        for (int i = 0; i < sortID.Length; i++)
        {
            minIndex = i;
            for (int j = i; j < sortID.Length; j++)
            {
                if (sortKey[minIndex].CompareTo(sortKey[j]) > 0 )
                {
                    minIndex = j;
                }
            }

            tempSortID = sortID[i];
            tempSortKey = sortKey[i];

            sortID[i] = sortID[minIndex];
            sortKey[i] = sortKey[minIndex];

            sortID[minIndex] = tempSortID;
            sortKey[minIndex] = tempSortKey;
        }

        for (int i=0; i<MyContacts.Length;i++)
        {
            MyContacts[sortID[i]].destination = new Vector3(0.0f, -i * plateHeight , 0.0f);
        }

    }

    public void OnMouseDown()
    {
        lastMousePositionY = Input.mousePosition.y;
        scrollSpeed=0.0f;

        //メニューが表示されているときは消す
        GameObject refObj = GameObject.Find("MenuButton");//まずオブジェクトを見つける
        MenuButton menuButtonScript = refObj.GetComponent<MenuButton>();//そのオブジェクトにアタッチされているスクリプトファイル名を指定して参照する
        menuButtonScript.hideMenu();

    }

    public void OnMouseDrag()
    {
        float currMousePositionY = Input.mousePosition.y;

        scroll((currMousePositionY - lastMousePositionY) * 0.02f);

        lastMousePositionY = currMousePositionY;
    }

    public void OnMouseUp()
    {
        float currMousePositionY = Input.mousePosition.y;

        scrollSpeed=((currMousePositionY - lastMousePositionY) * 0.02f);
    }

    void scroll(float offset)
    {

        Transform BG = GameObject.Find("BackGround").GetComponent<Transform>();
        BG.position = BG.position + new Vector3(0.0f, offset, 0.0f);

        if  (BG.position.y < scrollULimit )
        {
            if (scrollULimit - BG.position.y  < plateHeight*0.05f)
            {
                BG.position = new Vector3(0.0f, scrollULimit, 0.0f);
            }

            scrollSpeed = (scrollULimit - BG.position.y) * 0.3f;
        }

        if ( scrollDLimit< BG.position.y)
        {

        }
    }

    void getPersonThisWeek()
    {
        //WWW myWWW = new WWW("http://nb-united.info/birthday/birthday.txt");
        WWW myWWW = new WWW("http://localhost/persontoday.txt");
    }
}

