using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    private GUIText paintGUI;
	private GUIText distGUI; 	//draws the distance to the target
    private GUITexture crosshairGUI;
    private PlayerGUI myGUI;
    private List<int> owners;
    private GameObject parentCube;
    private Cube pCubeScript;
	
	public bool isFlying = false;
	public float switchDelay = 0.5f;	// delay for switching camera scripts
	
	
	//movement variables
    // speeds 50.00, 60.00 , 70.0f
	private float moveSpeed = 70.00f;
	private Vector3 startPos;

    public MouseLook mouseLook;
    private SmoothFollowCS mouseFollow;
    public bool gameStarted = false;
    public int paintLeft = 0;

    // 5, 10, 15
    public int paintCapacity = 15;

    GameManager gManScript;

	// Use this for initialization
	void Start () {
        mouseLook = Camera.mainCamera.GetComponent<MouseLook>();
        mouseFollow = Camera.mainCamera.GetComponent<SmoothFollowCS>();
        //set camera
        mouseFollow.enabled = false;
        mouseFollow.target = gameObject.transform;
        mouseFollow.targetLocation = gameObject.transform.position;
		sender = GetComponent<NetworkLaunchMessageSender>();

        //GUI initializers
        owners = new List<int>();
        myGUI = this.GetComponent<PlayerGUI>();
        distGUI = (GUIText)GameObject.Find("GUI Text Distance").GetComponent<GUIText>();
        paintGUI = (GUIText)GameObject.Find("GUI Text Paint").GetComponent<GUIText>();
        crosshairGUI = (GUITexture)GameObject.Find("Crosshair Outer").GetComponent<GUITexture>();
        crosshairGUI.color = color;

        GameObject gameMan = GameObject.Find("GameManager");
        gManScript = gameMan.GetComponent<GameManager>();
        refuel();
	}

    // Update is called once per frame
    void Update()
    {
        if(gameStarted)
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
                if (targetObject.CompareTag("Paintable"))
                {
                    distGUI.text = Mathf.Round(distance) + "m";

                    parentCube = targetObject.transform.parent.gameObject;
                    pCubeScript = parentCube.GetComponent<Cube>();
                    owners = new List<int>();
                    for (int i = 0; i < 6; i++)
                    {
                        Side tempSide = pCubeScript.Sides[i].GetComponent<Side>();
                        owners.Add(tempSide.teamOwnedBy);
                    }
                    myGUI.updateColors(true, owners);
                }
                else
                {
                    distGUI.text = Mathf.Round(distance) + "m";
                    myGUI.updateColors(false);
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

                while(distanceVector.magnitude >= 1)
                {
                    holderPosition += (distanceVector.normalized * moveSpeed * .0001f);
                    distanceVector = targetPosition - holderPosition;
                    counter++;
                }

			    double calcETA = counter * .0001f * 1000;
                calcETA += TimeManager.Instance.ClientTimeStamp;
                //Debug.Log("calcETA: " + calcETA);
                //*********SEND DATA ABOUT CLICK***********//

                bool shouldPaint;
                if (paintLeft > 0)
                {
                    shouldPaint = true;
                }
                else
                {
                    shouldPaint = false;
                }


                LaunchPacket launchMessage;
                //grand cube
                if (targetObjectSide.name.Contains("CubeSide"))
                {
                    int cubeID = targetObjectSide.transform.parent.GetComponent<Cube>().id;
                    int sideID = targetObjectSide.GetComponent<Side>().id;
                    launchMessage = new LaunchPacket(this.transform.position, targetPosition, TimeManager.Instance.ClientTimeStamp, calcETA,cubeID,sideID,shouldPaint);
                    GameObject side = GameManager.Instance.GetSide(launchMessage.CubeID, launchMessage.SideID);
                    side.GetComponent<Side>().TakeSide(launchMessage, GameValues.teamNum, true);
                }//cube chunk
                else if (targetObjectSide.name.Contains("Cube Peice"))
                {
                    Debug.Log("here in teh hcunk");
                    int cubeChunkID = targetObjectSide.transform.parent.GetComponent<CubeChunk>().id;
                    int chuckSideID = targetObjectSide.GetComponent<ChunkSide>().id;
                    launchMessage = new LaunchPacket(this.transform.position, targetPosition, TimeManager.Instance.ClientTimeStamp, calcETA, cubeChunkID, chuckSideID, shouldPaint);
                    GameObject side = GameManager.Instance.GetChunkSide(launchMessage.CubeID, launchMessage.SideID);
                    side.GetComponent<ChunkSide>().TakeSide(launchMessage, GameValues.teamNum, true);
                }
                else
                {
                    launchMessage = new LaunchPacket(this.transform.position, targetPosition, TimeManager.Instance.ClientTimeStamp, calcETA, -1, -1, shouldPaint);
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
				    mouseLook.setLookingDir(transform.localEulerAngles);

                    //set camera

                    StartCoroutine("CameraSwitch",switchDelay);
                
	                //Camera.main.ScreenPointToRay(-normal);
	                //Debug.Log("normal: " + hit.normal);
			    }
		    }//end isFlying
        }//end if
    } //end update

    public void subtractPaint()
    {
		if(paintLeft > 0)
		{
        	paintLeft--;
        	refreshPaintText();
        	if(paintLeft < 4)
            	gManScript.TurnOnArrow();
		}
    }

    private void refreshPaintText()
    {
        paintGUI.text = paintLeft + " / " + paintCapacity;
    }

    public void refuel()
    {
        paintLeft = paintCapacity;
        refreshPaintText();
            gManScript.TurnOffArrow();
    }

    IEnumerator CameraSwitch(float delay)
    {
		mouseFollow.arriving = true;	// we're arriving for this delay before we switch camera modes
        yield return new WaitForSeconds(delay);
        isFlying = false;
        mouseLook.enabled = true;
        mouseFollow.enabled = false;

        StopCoroutine("CameraSwitch");
    }

    public void setMoveSpeed(float _moveSpeed)
    {
        moveSpeed = _moveSpeed;
    }
}
