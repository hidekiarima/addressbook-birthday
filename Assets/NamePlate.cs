using UnityEngine;
using System.Collections;

public class NamePlate : MonoBehaviour {
    static float clickTime;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnMouseDown()
    {
        GameObject refObj = GameObject.Find("Coordinator");//まずオブジェクトを見つける
        Coordinate coord = refObj.GetComponent<Coordinate>();//そのオブジェクトにアタッチされているスクリプトファイル名を指定して参照する
        coord.OnMouseDown();

        clickTime = 0.0f;
    }

    void OnMouseDrag()
    {
        GameObject refObj = GameObject.Find("Coordinator");//まずオブジェクトを見つける
        Coordinate coord = refObj.GetComponent<Coordinate>();//そのオブジェクトにアタッチされているスクリプトファイル名を指定して参照する
        coord.OnMouseDrag();

        clickTime += Time.deltaTime;
    }

    void OnMouseUp()
    {
        GameObject refObj = GameObject.Find("Coordinator");//まずオブジェクトを見つける
        Coordinate coord = refObj.GetComponent<Coordinate>();//そのオブジェクトにアタッチされているスクリプトファイル名を指定して参照する
        coord.OnMouseUp();

        if (clickTime < 0.1f)
        {
            //「自分」選択中のときは選んだものが自分かどうか確認する
            if (Coordinate.userSelectingNow  == true)
            {
                Coordinate.selectedNamePlate = gameObject.transform.parent;
                Coordinate.canvasUserSelected.SetActive(true);
                return;
            }

            //それ以外のときは表示中のアイテムの詳細表示
            TextMesh desc = gameObject.transform.parent.transform.FindChild("txtDesc").GetComponent<TextMesh>();
            TextMesh descDetail = gameObject.transform.parent.transform.FindChild("txtDescDetail").GetComponent<TextMesh>();
            if (descDetail.text!="")
            {
                coord.displayDescription(desc.text, descDetail.text);
            }
        }
    }
}
