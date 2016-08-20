using UnityEngine;
using System.Collections;

public class canvasDescription : MonoBehaviour {

    Vector3 originalPosition;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnMouseDown()
    {
        originalPosition = Input.mousePosition;

        //GameObject canvasDescription = GameObject.Find("canvasDescription");
        //canvasDescription.SetActive(false);
    }

    void OnMouseUp()
    {
        if ( Vector3.Distance(originalPosition, Input.mousePosition) < 0.1f)
        {
            GameObject canvasDescription = GameObject.Find("canvasDescription");
            canvasDescription.SetActive(false);
        }
    }

}
