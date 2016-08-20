using UnityEngine;
using System.Collections;

public class MenuItem : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnMouseDown()
    {
        Transform menuItem = transform.parent.FindChild("txtMenuItem");
        TextMesh menuTextMesh = menuItem.GetComponent<TextMesh>();
        string menuText = menuTextMesh.text;

        GameObject refObj = GameObject.Find("Coordinator");//まずオブジェクトを見つける
        Coordinate coord = refObj.GetComponent<Coordinate>();//そのオブジェクトにアタッチされているスクリプトファイル名を指定して参照する
        coord.menuClicked(menuText);
        
    }

}