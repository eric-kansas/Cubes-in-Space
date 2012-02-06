using UnityEngine;
using System.Collections;

public class Launch : MonoBehaviour {

    private Vector3 targetPosition;
	private GameObject targetObjectSide;
    private Vector3 normal;
    public GameObject player;
	public Color color;
	
	
	private bool isFlying = false;
	
	
	//movement variables
	private float moveSpeed = .75f;
	private Vector3 startPos;

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
            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000))
                return;

            //our target is where we clicked
            targetPosition = hit.point;
            normal = hit.normal;
			targetObjectSide = hit.transform.gameObject; //the side of the cube we hit
			
			
			isFlying = true;
			
            //*********SEND DATA ABOUT CLICK***********//	
            //SFSObject myData = new SFSObject();		//create an object for sending data

            // data goes into the myData object
            // put x and z in the object, with string keys "x", "z"

            // then use the Send command with the smartFox object 
            // smartFox.Send(...);
        }
		
		if(isFlying){
			
			//calculate the distance to target
			Vector3 distanceVector = targetPosition - player.transform.position;
			float distance = distanceVector.magnitude;
			Debug.Log("Launch Click-  Distance to target: " + distance);
			
			if (distance >= 5)
			{
				//move towards the target slowly
				player.transform.position += (distanceVector.normalized * moveSpeed);
				//player.transform.position = Vector3.Lerp(player.transform.position, targetPosition, Time.deltaTime * moveSpeed);
			}
			else
			{
				//we've hit, color the cube our color
				if (targetObjectSide.name != "arena")
				{
					GameObject cube = targetObjectSide.transform.parent.gameObject;
					Debug.Log("Launch Click-   We have hit cube " + cube.name);
					Cube theCube = cube.GetComponent<Cube>();
					
					theCube.setSideColor(targetObjectSide, color);
				}
				//upon arrival, turn around
				player.transform.position = targetPosition;
				
				//slow these down somehow
				player.transform.forward = normal; 
				mouseLook.lookingDir = transform.localEulerAngles;
				isFlying = false;
	            //Camera.main.ScreenPointToRay(-normal);
	            //Debug.Log("normal: " + hit.normal);
			}
		}
    }
}
