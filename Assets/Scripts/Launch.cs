using UnityEngine;
using System.Collections;

public class Launch : MonoBehaviour {

    private Vector3 targetPosition;
    private Vector3 normal;
    public GameObject player;

    private MouseLook mouseLook;

	// Use this for initialization
	void Start () {
        mouseLook = this.GetComponent<MouseLook>();
	}

    // Update is called once per frame
    void Update()
    {

        // variable for the raycast info
        RaycastHit hit;

        //check for click on plane
        if (Input.GetMouseButtonDown(0))
        {
            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
                return;

            //our target is where we clicked
            targetPosition = hit.point;
            normal = hit.normal;
            player.transform.position = targetPosition;
            player.transform.forward = normal;

            mouseLook.lookingDir = transform.localEulerAngles;
            //Camera.main.ScreenPointToRay(-normal);
            Debug.Log("normal: " + hit.normal);

            //*********SEND DATA ABOUT CLICK***********//	
            //SFSObject myData = new SFSObject();		//create an object for sending data

            // data goes into the myData object
            // put x and z in the object, with string keys "x", "z"

            // then use the Send command with the smartFox object 
            // smartFox.Send(...);
        }

    }
}
