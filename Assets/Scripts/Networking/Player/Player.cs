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
	public GUIText guiText; 	//draws the distance to the target
	public bool isFlying = false;
	
	
	//movement variables
	private float moveSpeed = 60.00f;
	private Vector3 startPos;

    private MouseLook mouseLook;
    private SmoothFollowCS mouseFollow;

	// Use this for initialization
	void Start () {
        mouseLook = Camera.mainCamera.GetComponent<MouseLook>();
        mouseFollow = Camera.mainCamera.GetComponent<SmoothFollowCS>();
        //set camera
        mouseFollow.enabled = false;
        mouseFollow.target = gameObject.transform;
        mouseFollow.targetLocation = gameObject.transform.position;
		sender = GetComponent<NetworkLaunchMessageSender>();
		
		guiText = (GUIText)GameObject.Find("GUI Text").GetComponent<GUIText>();
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
			
			//draw some stuff the the screen
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

            mouseLook.enabled = false;
            mouseFollow.enabled = true;
            mouseFollow.startLocation = gameObject.transform.position;
            mouseFollow.targetLocation = targetPosition;
            mouseFollow.targetNormal = hit.normal;
            mouseFollow.reset();

        
            Vector3 holderPosition = transform.position;
            Vector3 distanceVector = targetPosition - holderPosition;
            int counter = 0;

            while(distanceVector.magnitude >= 5)
            {
                holderPosition += (distanceVector.normalized * moveSpeed * .0001f);
                distanceVector = targetPosition - holderPosition;
                counter++;
            }

			double calcETA = counter * .0001f * 1000;
            calcETA += TimeManager.Instance.ClientTimeStamp;
            //Debug.Log("calcETA: " + calcETA);
            //*********SEND DATA ABOUT CLICK***********//
            LaunchPacket launchMessage;
            if (targetObjectSide.name.Contains("Cube"))
            {
                int cubeID = targetObjectSide.transform.parent.GetComponent<Cube>().id;
                int sideID = targetObjectSide.GetComponent<Side>().id;
                launchMessage = new LaunchPacket(this.transform.position, targetPosition, TimeManager.Instance.ClientTimeStamp, calcETA,cubeID,sideID);
                GameObject side = GameManager.Instance.GetSide(launchMessage.CubeID, launchMessage.SideID);
                side.GetComponent<Side>().TakeSide(launchMessage, GameValues.teamNum);
            }
            else
            {
                launchMessage = new LaunchPacket(this.transform.position, targetPosition, TimeManager.Instance.ClientTimeStamp, calcETA, -1, -1);
            }
            
            sender.SendLaunchOnRequest(launchMessage);
        }
		
		if(isFlying){
			
			//calculate the distance to target
			Vector3 distanceVector = targetPosition - transform.position;
			float distance = distanceVector.magnitude;
			
			if (distance >= 5)
			{
				//move towards the target slowly
				transform.position += (distanceVector.normalized * moveSpeed * Time.deltaTime);
				//player.transform.position = Vector3.Lerp(player.transform.position, targetPosition, Time.deltaTime * moveSpeed);
			}
			else
			{
				//we've hit, color the cube our color
				if (targetObjectSide.name != "arena" && targetObjectSide.name != "icosahedron" && targetObjectSide.name != "icosahedronframe")
				{
					GameObject cube = targetObjectSide.transform.parent.gameObject;
					Cube theCube = cube.GetComponent<Cube>();
					
					//theCube.setSideColor(targetObjectSide, color);
				}
				//upon arrival, turn around
				transform.position = targetPosition;
				
				//slow these down somehow
				transform.forward = normal; 
				mouseLook.lookingDir = transform.localEulerAngles;

                //set camera

                StartCoroutine("CameraSwitch",1.0f);
                
	            //Camera.main.ScreenPointToRay(-normal);
	            //Debug.Log("normal: " + hit.normal);
			}
		}//end isFlying
    } //end update

    IEnumerator CameraSwitch(float delay)
    {
        yield return new WaitForSeconds(delay);
        isFlying = false;
        mouseLook.enabled = true;
        mouseFollow.enabled = false;
        StopCoroutine("CameraSwitch");
    }
}
