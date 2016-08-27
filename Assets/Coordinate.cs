using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;


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
    static public string userNameAndID;

    string displayingItems;
    string userStar="";

    static public GameObject canvasDescription;
    static public GameObject canvasUserSelect;
    static public GameObject canvasUserSelected;
    static public Transform selectedNamePlate;

    static public bool userSelected = false;
    static public bool userSelectingNow = false;

    WWW myWWW;
    int contactOffsetByPersonThisWeek = 0;

    public struct MyContact
    {
        public string Name;
        public GameObject NamePlate;
        public string ID;
        public string birthDay;
        public string displayText;
        public Vector3 destination;
        public string properties;
        public string propertieDescs;
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

    public struct aisyo
    {
        public string userStar;
        public string contactStar;
        public string degree;
        public string desc;
    }

    public struct powerDesc
    {
        public int minPower;
        public int maxPower;
        public string desc;
    }

    public struct sainou
    {
        public string userStar;
        public string desc;
    }

    public struct honshitsu
    {
        public string userStar;
        public string desc;
    }



    static MyContact[] MyContacts ;
    static birthDayProperty[] birthdayProperties;
    static kishitsu[] kishitsuList;
    static aisyo[] aisyoList;
    static powerDesc[] powerDescList;
    static sainou[] sainouList;
    static honshitsu[] honshitsuList;


    int dispItemIndex = 0; 

    void Start()
    {
        //
        canvasDescription = GameObject.Find("canvasDescription");
        canvasDescription.SetActive(false);

        canvasUserSelect = GameObject.Find("canvasUserSelect");
        canvasUserSelect.SetActive(false);

        canvasUserSelected = GameObject.Find("canvasUserSelected");
        canvasUserSelected.SetActive(false);


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

        //Read 相性 data
        var astAisyo = Resources.Load<TextAsset>("aisyo");
        var txtAisyo = astAisyo.text;
        var txtAisyoRows = txtAisyo.Split('\n');
        Array.Resize(ref aisyoList, txtAisyoRows.Length);
        for (int i = 0; i < txtAisyoRows.Length; i++)
        {
            var column = txtAisyoRows[i].Split(',');
            aisyoList[i].userStar = column[0];
            aisyoList[i].contactStar = column[1];
            aisyoList[i].degree = column[2];
            aisyoList[i].desc = column[3];
        }

        //Read powerDesc data
        var astPowerDesc = Resources.Load<TextAsset>("power");
        var txtPowerDesc = astPowerDesc.text;
        var txtPowerDescRows = txtPowerDesc.Split('\n');
        Array.Resize(ref powerDescList, txtPowerDescRows.Length);
        for (int i = 0; i < txtPowerDescRows.Length; i++)
        {
            var column = txtPowerDescRows[i].Split(',');
            powerDescList[i].minPower = int.Parse(column[0]);
            powerDescList[i].maxPower = int.Parse(column[1]);
            powerDescList[i].desc = column[2];
        }

        //Read 才能 data
        var astSainou = Resources.Load<TextAsset>("sainou");
        var txtSainou = astSainou.text;
        var txtSainouRows = txtSainou.Split('\n');
        Array.Resize(ref sainouList, txtSainouRows.Length);
        for (int i = 0; i < txtSainouRows.Length; i++)
        {
            var column = txtSainouRows[i].Split(',');
            sainouList[i].userStar = column[0];
            sainouList[i].desc = column[1];
        }

        //Read 本質 data
        var astHonshitsu = Resources.Load<TextAsset>("honshitsu");
        var txtHonshitsu = astHonshitsu.text;
        var txtHonshitsuRows = txtHonshitsu.Split('\n');
        Array.Resize(ref honshitsuList, txtHonshitsuRows.Length);
        for (int i = 0; i < txtHonshitsuRows.Length; i++)
        {
            var column = txtHonshitsuRows[i].Split(',');
            honshitsuList[i].userStar = column[0];
            honshitsuList[i].desc = column[1];
        }


        //Read birthday data
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

        //
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
            if (scrollULimit - BG.position.y < plateHeight * 0.05f) { BG.position = new Vector3(0.0f, scrollULimit, BG.position.z); }
            scrollSpeed = (scrollULimit - BG.position.y) * 0.3f;
        }
        if (scrollDLimit < BG.position.y)
        {
            if (BG.position.y - scrollDLimit < plateHeight * 0.05f) { BG.position = new Vector3(0.0f, scrollDLimit, BG.position.z); }
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
            numOfContacts = 45;
            Array.Resize(ref MyContacts, numOfContacts);
            for (int i = 0; i < numOfContacts; i++)
            {
                MyContacts[i].Name = i.ToString();
                MyContacts[i].birthDay = "19" + UnityEngine.Random.Range(50.0f, 98.0f).ToString("00") + "-" + UnityEngine.Random.Range(2.0f, 11.0f).ToString("00") + "-" + UnityEngine.Random.Range(2.0f, 27.0f).ToString("00");
                if (UnityEngine.Random.Range(0.0f,1.0f)<0.2f)
                {
                    MyContacts[i].birthDay = "";
                }
                MyContacts[i].ID = i.ToString();

                dispTextMesh.text = "reading :" + i.ToString();
            }
        }

        dispTextMesh.text = "read done";


        //今週の有名人の読み込みが成功しているときはMyContactの内容をひとつづつずらして先頭に今週の有名人を入れる
        if (myWWW.error == null)
        {
            numOfContacts += 1;
            Array.Resize(ref MyContacts, numOfContacts);
            for (int i = numOfContacts-1; i >0; i--)
            {
                MyContacts[i].Name = MyContacts[i-1].Name;
                MyContacts[i].birthDay = MyContacts[i-1].birthDay;
                MyContacts[i].ID = MyContacts[i-1].ID;

                dispTextMesh.text = "shifting :" + i.ToString();
            }

            MyContacts[0].Name = myWWW.text.Split(',')[0];
            MyContacts[0].birthDay = myWWW.text.Split(',')[1];
            MyContacts[0].ID = "";

            contactOffsetByPersonThisWeek = 1;
        }

        //Prepare name plates
        GameObject objBG = GameObject.Find("BackGround");


        userSelected = false;


        displayingItems = "誕生日,自然にたとえると,本質,才能,パワー,天中殺";
        for (int i = 0; i < numOfContacts; i++)
        {

            MyContacts[i].NamePlate = (GameObject)Instantiate(Resources.Load("NamePlate"), new Vector3(0.0f, 0.0f + i * 0.1f, 5.0f), Quaternion.identity);
            MyContacts[i].properties = getBirthdayPropertyText(MyContacts[i].birthDay);


            TextMesh m = MyContacts[i].NamePlate.transform.FindChild("txtName").GetComponent<TextMesh>();
            m.text = MyContacts[i].Name;

            m = MyContacts[i].NamePlate.transform.FindChild("txtBirthday").GetComponent<TextMesh>();
            m.text = MyContacts[i].birthDay;

            m = MyContacts[i].NamePlate.transform.FindChild("txtID").GetComponent<TextMesh>();
            m.text = MyContacts[i].ID;

            if (i >= contactOffsetByPersonThisWeek)
            {
                m = MyContacts[i].NamePlate.transform.FindChild("txtNameTitle").GetComponent<TextMesh>();
                m.text = "";
            }

            if (MyContacts[i].properties != "")
            {
                var column = MyContacts[i].properties.Split(',');
                int idx = int.Parse(column[20]);

                string propertyText = column[0] + "," +  getKishitsuShortText(idx);

                string txtHonshitsu = "";
                for (int j = 0; j < honshitsuList.Length; j++)
                {
                    if (honshitsuList[j].userStar  == column[12])
                    {
                        txtHonshitsu = honshitsuList[j].desc;
                    }
                }

                string txtSainou = "";
                for (int j = 0; j < sainouList.Length; j++)
                {
                    if (sainouList[j].userStar == column[13])
                    {
                        txtSainou = sainouList[j].desc;
                    }
                }

                string txtPowerDesc = "";
                for (int j = 0; j < powerDescList.Length; j++)
                {
                    if (powerDescList[j].minPower <= int.Parse(column[19]) && int.Parse(column[19]) <= powerDescList[j].maxPower)
                    {
                        txtPowerDesc = powerDescList[j].desc;
                    }
                }

                string txtTentyusatsu = column[18].Substring(0, 2);
                int startYear;
                for ( startYear = "子丑寅卯辰巳午未猿酉戌亥".IndexOf(txtTentyusatsu) + 2008
                    ; startYear < DateTime.Today.Year + 1
                    ; startYear +=12) { /*startYearの調節のためのループなので特になにもしない*/ }



                MyContacts[i].properties = column[0] + ","
                                            + getKishitsuShortText(idx) + ","
                                            + column[12] + ","
                                            + column[13] + ","
                                            + column[19] + ","
                                            + txtTentyusatsu + "(" + startYear.ToString() + "-" + (startYear+1-2000).ToString() + ")";

                MyContacts[i].propertieDescs = ","
                                            + getKishitsuLongText(idx) + ","
                                            + txtHonshitsu + ","
                                            + txtSainou + ","
                                            + txtPowerDesc + ","
                                            + "";

            }

            MyContacts[i].NamePlate.transform.parent = objBG.transform;
            MyContacts[i].destination = MyContacts[i].NamePlate.transform.localPosition;

            dispTextMesh.text = "plate :" + i.ToString();

            //ユーザ本人の情報は画面上部にも表示
            if (MyContacts[i].Name + ":" + MyContacts[i].ID == userNameAndID)
            {
                userSelected = true;

                GameObject myNamePlate = GameObject.Find("MyNamePlate");

                m = myNamePlate.transform.FindChild("txtName").GetComponent<TextMesh>();
                m.text = MyContacts[i].Name;

                m = myNamePlate.transform.FindChild("txtBirthday").GetComponent<TextMesh>();
                m.text = MyContacts[i].birthDay;

                //顔画像があるときは表示する
                if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    if (Contacts.ContactsList[i - contactOffsetByPersonThisWeek].PhotoTexture != null)
                    {
                        GUITexture faceTexture = GameObject.Find("userFace").GetComponent<GUITexture>();
                        faceTexture.texture = Contacts.ContactsList[i - contactOffsetByPersonThisWeek].PhotoTexture;
                    }
                }

                if (MyContacts[i].properties!="")
                {
                    userStar = MyContacts[i].properties.Split(',')[2];
                }
            }
        }

        //相性を追加
        if (userStar != "")
        {

            displayingItems += ",あなたとの相性";
            for (int i=0; i<numOfContacts; i++)
            {
                string aisyoLevel = "";
                string aisyoText = "";

                var contactStar = "";
                if (MyContacts[i].properties!="")
                {
                    contactStar = MyContacts[i].properties.Split(',')[2];
                }

                for (int j=0;j<aisyoList.Length;j++)
                {
                    if (userStar== aisyoList[j].userStar && contactStar == aisyoList[j].contactStar)
                    {
                        aisyoLevel = aisyoList[j].degree;
                        aisyoText = aisyoList[j].desc;
                    }
                }

                if(MyContacts[i].properties!="")
                {
                    MyContacts[i].properties += ("," + aisyoLevel);
                    MyContacts[i].propertieDescs += ("," + aisyoText);
                }
            }
        }

        Transform TopBar = MyContacts[0].NamePlate.transform.FindChild("sprFrame");
        SpriteRenderer SPRen = TopBar.GetComponent<SpriteRenderer>();
        plateHeight = SPRen.bounds.size.y;

        for (int i = 0; i < numOfContacts; i++)
        {
            MyContacts[i].destination = new Vector3(0.0f, -i * plateHeight , MyContacts[i].NamePlate.transform.localPosition.z);
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

    string getKishitsuLongText(int kishitsuNo)
    {

        if (1 <= kishitsuNo && kishitsuNo <= kishitsuList.Length)
        {
            return kishitsuList[kishitsuNo - 1].longText;
        }
        else
        {
            return "";
        }
    }

    public void changeDispItem(int moveBy)
    {
        dispItemIndex += moveBy;

        var headerColumn = displayingItems.Split(',');

        if (dispItemIndex < 0) { dispItemIndex = headerColumn.Length-1; }
        if (dispItemIndex >= headerColumn.Length) { dispItemIndex = 0; }

        TextMesh dispTextMesh = GameObject.Find("txtDispItem").GetComponent<TextMesh>();
        dispTextMesh.text = headerColumn[dispItemIndex];


        for (int i = 0; i<MyContacts.Length;i++)
        {
            string descDetail = "";
            if (MyContacts[i].properties == "")
            {
                MyContacts[i].displayText = "";
            }
            else
            {
                MyContacts[i].displayText = MyContacts[i].properties.Split(',')[dispItemIndex];
                descDetail = MyContacts[i].propertieDescs.Split(',')[dispItemIndex];
            }
            TextMesh m = MyContacts[i].NamePlate.transform.FindChild("txtDesc").GetComponent<TextMesh>();
            m.text = MyContacts[i].displayText;
            TextMesh md = MyContacts[i].NamePlate.transform.FindChild("txtDescDetail").GetComponent<TextMesh>();
            md.text = descDetail;

            //ユーザ本人の情報の表示も変更
            if (MyContacts[i].Name + ":" + MyContacts[i].ID == userNameAndID)
            {
                GameObject myNamePlate = GameObject.Find("MyNamePlate");

                m = myNamePlate.transform.FindChild("txtDesc").GetComponent<TextMesh>();
                m.text = MyContacts[i].displayText;
            }
        }




        //メニューの並べ替えの表示も変更
        GameObject sortByDispItemMenu = GameObject.Find("MenuItemSort3");
        Transform menuItem = sortByDispItemMenu.transform.FindChild("txtMenuItem");
        TextMesh menuTextMesh = menuItem.GetComponent<TextMesh>();
        menuTextMesh.text = headerColumn[dispItemIndex] + "順";
    }

    public void menuClicked(string menuItem)
    {
        switch (menuItem)
        {
            case "自分の連絡先を選択":
                userSelectingNow = true;
                canvasUserSelect.SetActive(true);
                hideMenu();
                break;

            case "名前順":
            case "誕生日順":
            default:
                sortNamePlate(menuItem);
                hideMenu();
                break;
        }

    }

    void hideMenu()
    {
        GameObject menuButton = GameObject.Find("MenuButton");//まずオブジェクトを見つける
        MenuButton menuButtonScript = menuButton.GetComponent<MenuButton>();//そのオブジェクトにアタッチされているスクリプトファイル名を指定して参照する
        menuButtonScript.hideMenu();
    }

    void sortNamePlate(string sortBy)
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
        for (int i = contactOffsetByPersonThisWeek; i < sortID.Length; i++)
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

        for (int i= contactOffsetByPersonThisWeek; i<MyContacts.Length;i++)
        {
            MyContacts[sortID[i]].destination = new Vector3(0.0f, -i * plateHeight , MyContacts[sortID[i]].NamePlate.transform.localPosition.z);
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
                BG.position = new Vector3(0.0f, scrollULimit, BG.position.z);
            }

            scrollSpeed = (scrollULimit - BG.position.y) * 0.3f;
        }

        if ( scrollDLimit< BG.position.y)
        {

        }
    }

    void getPersonThisWeek()
    {
        int i = 0;
        //ダウンロードした内容が勝手にキャッシュされるので、URLの末尾にタイムスタンプを追加してキャッシュされないようにしている
        for (myWWW = new WWW("http://nb-united.info/birthday/birthday-utf8.txt?t=" + DateTime.Now.ToString("yyyyMMddHHmmss")) ; myWWW.isDone == false;)
        {/*WWWの読み込みが終わるまで空ループ*/ i++; }
        //for (myWWW = new WWW("http://192.168.100.3/persontoday.txt?t=" + DateTime.Now.ToString("yyyyMMddHHmmss")); myWWW.isDone == false;)

    }

    public void displayDescription(string title, string body)
    {
        canvasDescription.SetActive(true);

        Text txtTitle = canvasDescription.transform.FindChild("txtTitle").GetComponent<Text>();
        txtTitle.text = title;

        //元のコード
        //Text txtBody = canvasDescription.transform.FindChild("scrollPanel").transform.FindChild("txtBody").GetComponent<Text>();
        //txtBody.text = body;
        //改行の禁則処理のAssetを使ったコード
        HyphenationJpn txtBody = canvasDescription.transform.FindChild("scrollPanel").transform.FindChild("txtBody").GetComponent<HyphenationJpn>();
        txtBody.text = body;
            

    }
}

