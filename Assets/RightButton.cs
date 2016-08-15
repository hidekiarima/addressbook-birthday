using UnityEngine;
using System.Collections;

public class RightButton : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnMouseDown()
    {
        GameObject refObj = GameObject.Find("Coordinator");
        Coordinate coord = refObj.GetComponent<Coordinate>();
        coord.changeDispItem(1);
    }
}
    