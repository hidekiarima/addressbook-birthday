using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class userSelectedConfirm : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        Text txtBody = gameObject.transform.FindChild("Panel").transform.FindChild("txtBody").GetComponent<Text>();
        TextMesh selectedUserName = Coordinate.selectedNamePlate.transform.FindChild("txtName").GetComponent<TextMesh>();
        txtBody.text = "あなたの連絡先は " + selectedUserName.text + " ですか。";
    }

    public void userSelected()
    {
        string nameAndID;

        TextMesh m = Coordinate.selectedNamePlate.transform.FindChild("txtName").GetComponent<TextMesh>();
        nameAndID = m.text;

        m = Coordinate.selectedNamePlate.transform.FindChild("txtID").GetComponent<TextMesh>();
        nameAndID += (":" + m.text);

        Coordinate.userNameAndID = nameAndID;
        PlayerPrefs.SetString("userNameAndID", nameAndID);

        Coordinate.userSelectingNow = false;
        Coordinate.userSelected = true;

        //
        GameObject canvasUserSelected = GameObject.Find("canvasUserSelected");
        canvasUserSelected.SetActive(false);

        Coordinate.userSelectingNow = false;
        Coordinate.userSelected = true;

    }

    public void selectAgain()
    {
        //ユーザセレクト中の状態を継続するので、なにもせずに閉じる
        GameObject canvasUserSelected = GameObject.Find("canvasUserSelected");
        canvasUserSelected.SetActive(false);
    }

    public void selectLater()
    {
        //ユーザセレクト中の状態を中止する
        GameObject canvasUserSelected = GameObject.Find("canvasUserSelected");
        canvasUserSelected.SetActive(false);

        Coordinate.userSelectingNow = false;
    }

}
