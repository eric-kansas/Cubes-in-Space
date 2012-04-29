using UnityEngine;
using System.Collections;

/// MouseLook rotates the transform based on the mouse delta.
/// Minimum and Maximum values can be used to constrain the possible rotation

/// To make an FPS style character:
/// - Create a capsule.
/// - Add the MouseLook script to the capsule.
///   -> Set the mouse look to use LookX. (You want to only turn character but not tilt it)
/// - Add FPSInputController script to the capsule
///   -> A CharacterMotor and a CharacterController component will be automatically added.

/// - Create a camera. Make the camera a child of the capsule. Reset it's transform.
/// - Add a MouseLook script to the camera.
///   -> Set the mouse look to use LookY. (You want the camera to tilt up and down like a head. The character already turns.)
[AddComponentMenu("Camera-Control/Mouse Look")]
public class MouseLook : MonoBehaviour {

	public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
	public RotationAxes axes = RotationAxes.MouseXAndY;
	private float sensitivityX = 100F;
	private float sensitivityY = 100F;

    //30 ( pi/5 ), 45 ( pi/4 ), 60 ( pi/3)
    private float clamp = 30;
	public float minimumX = 0;	
	public float maximumX = 00;

	public float minimumY = 0;
	public float maximumY = 0;
	
	public Player player;

    public Vector3 lookingDir = new Vector3(0, 0, 0);
    public Vector3 lookingDirV = new Vector3(0, 0, 0);
	
	private bool inGame = false;

    Vector3 localTransfrom; 
    float rotationX = 0F;
    float rotationY = 0F;
	
	float cummulativeRotationX = 0f;
	float cummulativeRotationY = 0f;

	void Update ()
	{
		if (inGame)
		{
			//keep the camera's position relative to the player's
			//change this is you want the camera to not be inside of the player
            if (player.isFlying)
            {
                transform.position = player.transform.position - (player.transform.position.normalized * -5);
            }
            else
            {
                transform.position = player.transform.position;
            }
			
			if (axes == RotationAxes.MouseXAndY)
			{
				// Get the mouse x rotation and use time along with a scaling factor
				// to add a controlled amount to our cummulative rotation about the y-axis
				cummulativeRotationX += Input.GetAxis("Mouse Y") * Time.deltaTime * sensitivityX;
				cummulativeRotationX = Mathf.Clamp(cummulativeRotationX, minimumX, maximumX);
				// Do the same for the y, about the x-axis
				cummulativeRotationY += Input.GetAxis("Mouse X") * Time.deltaTime * sensitivityY;
				cummulativeRotationY = Mathf.Clamp(cummulativeRotationY, minimumY, maximumY);
				
				// create a Quaternion to hold our current cummulative rotation about the x and y axes
                Quaternion currentRotation = Quaternion.Euler(-cummulativeRotationX, cummulativeRotationY, lookingDir.z);
				
				// Use the Quaternion to update the transform of our camera based on initial rotation
				// and the current rotation
				Quaternion initialOrientation = Quaternion.Euler(lookingDir);
				transform.rotation = initialOrientation * currentRotation;
	

			}
			else if (axes == RotationAxes.MouseX)
			{
				transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityX, 0);
			}
			else
			{
				rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
				rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
				
				transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
			}
		}
	}
	
	void Start ()
	{
		// Make the rigid body not change rotation
		if (rigidbody)
			rigidbody.freezeRotation = true;

        minimumX = -clamp;
        maximumX = clamp;

        minimumY = -clamp;
        maximumY = clamp;
	}
	public void init(Player playerChar)
	{
		// find the GO tagged player, the player the camera will follow
		player = playerChar;
		
        localTransfrom = player.transform.eulerAngles;

        lookingDir = player.transform.eulerAngles;
		cummulativeRotationX = lookingDir.x;
		cummulativeRotationY = lookingDir.y;
		
		inGame = true;
	}
	public void setLookingDir(Vector3 newDir)
	{
		lookingDir = newDir;
		cummulativeRotationX = 0.0f;
		cummulativeRotationY = 0.0f;
	}
	
}