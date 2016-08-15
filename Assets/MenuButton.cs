using UnityEngine;
using System.Collections;

public class MenuButton : MonoBehaviour {

    static Vector3 myHome;
    static Vector3 hideOffset = new Vector3(0.0f, 0.0f,100.0f); 

    // Use this for initialization
    void Start () {
        GameObject menu = GameObject.Find("Menu");
        myHome = menu.transform.position;

        hideMenu();
    }

    // Update is called once per frame
    void Update () {
	
	}

    void OnMouseDown()
    {
        toggleMenu();
    }

    public void toggleMenu()
    {
        GameObject menu = GameObject.Find("Menu");
        menu.transform.position = hideOffset - menu.transform.position;//画面外に逃がして表示非表示を切り替える
    }

    public void hideMenu()
    {
        GameObject menu = GameObject.Find("Menu");
        menu.transform.position = hideOffset - myHome;//画面外に逃がして非表示にする
    }

    public void popupMenu()
    {
        hideMenu();//一旦非表示にしてから
        toggleMenu();//トグルすることで表示する
    }
}
