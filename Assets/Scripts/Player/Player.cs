using UnityEngine;
using System.Collections;

using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Entities.Variables;
using Sfs2X.Requests;
using Sfs2X.Exceptions;

public class Player : MonoBehaviour {

    private Vector3 targetPosition;
	private GameObject targetObjectSide;
	private NetworkLaunchMessageSender sender;
    private Vector3 normal;
	public Color color;

    public GUIText guiText;
	
	private bool isFlying = false;
	
	
	//movement variables
	private float moveSpeed = 1.25f;
	private Vector3 startPos;

    private MouseLook mouseLook;

	// Use this for initialization
	void Start () {
        guiText = (GUIText)GameObject.Find("GUI Text").GetComponent<GUIText>();
        mouseLook = Camera.mainCamera.GetComponent<MouseLook>();
		sender = GetComponent<NetworkLaunchMessageSender>();
	}

    // Update is called once per frame
    void Update()
    {

        // variable for the raycast info
        RaycastHit hit;
        bool didHit = false;

        
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1000))
        {
            Vector3 distanceVector = hit.point - transform.position;
            float distance = distanceVector.magnitude;
            GameObject targetObject = hit.transform.gameObject; //the side of the cube we hit

            if (targetObject.name != "arena")
            {
                guiText.material.color = new Color(color.r, color.g, color.b);
                guiText.text = "Distance: " + Mathf.Round(distance);
            }
            else
            {
                guiText.material.color = new Color(255, 255, 255);
                guiText.text = "Distance: " + Mathf.Round(distance);
            }
            didHit = true;
        }

        //check for click on plane
        if (Input.GetMouseButtonDown(0) && didHit && !isFlying)
        {
            //our target is where we clicked
            targetPosition = hit.point;
            normal = hit.normal;
            targetObjectSide = hit.transform.gameObject; //the side of the cube we hit
			isFlying = true;
			
            //*********SEND DATA ABOUT CLICK***********//	
            LaunchPacket launchMessage = new LaunchPacket(this.transform.position, targetPosition, TimeManager.Instance.ClientTimeStamp);
            sender.SendLaunchOnRequest(launchMessage);
        }
		
		if(isFlying){
			
			//calculate the distance to target
			Vector3 distanceVector = targetPosition - transform.position;
			float distance = distanceVector.magnitude;
			
			if (distance >= 5)
			{
				//move towards the target slowly
				transform.position += (distanceVector.normalized * moveSpeed);
				//player.transform.position = Vector3.Lerp(player.transform.position, targetPosition, Time.deltaTime * moveSpeed);
			}
			else
			{
				//we've hit, color the cube our color
				if (targetObjectSide.name != "arena" && targetObjectSide.name != "icosahedron" && targetObjectSide.name != "icosahedronframe")
				{
					GameObject cube = targetObjectSide.transform.parent.gameObject;
					Cube theCube = cube.GetComponent<Cube>();
					
					theCube.setSideColor(targetObjectSide, color);
				}
				//upon arrival, turn around
				transform.position = targetPosition;
				
				//slow these down somehow
				transform.forward = normal; 
				mouseLook.lookingDir = transform.localEulerAngles;
				isFlying = false;
	            //Camera.main.ScreenPointToRay(-normal);
	            //Debug.Log("normal: " + hit.normal);
			}
		}
    }
}
