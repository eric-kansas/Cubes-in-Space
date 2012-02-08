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
	public float sensitivityX = 10F;
	public float sensitivityY = 10F;

	public float minimumX = -45F;
	public float maximumX = 45F;

	public float minimumY = -45F;
	public float maximumY = 45F;
	
	public GameObject player;

    public Vector3 lookingDir = new Vector3(0, 0, 0);
    public Vector3 lookingDirV = new Vector3(0, 0, 0);
	
	private bool inGame = false;

    Vector3 localTransfrom; 
    float rotationX = 0F;
    float rotationY = 0F;

	void Update ()
	{
		if (inGame)
		{
			//keep the camera's position relative to the player's
			//change this is you want the camera to not be inside of the player
			transform.position = player.transform.position;
			
			if (axes == RotationAxes.MouseXAndY)
			{
	
	
	            rotationX = localTransfrom.y + Input.GetAxis("Mouse X") * sensitivityX;
	            rotationX = Mathf.Clamp(rotationX, lookingDir.y + minimumX, lookingDir.y + maximumX);
				
				
				rotationY = localTransfrom.x + Input.GetAxis("Mouse Y") * sensitivityY * -1;			
	            rotationY = Mathf.Clamp(rotationY, lookingDir.x + minimumY, lookingDir.x + maximumY);
	
	            localTransfrom = new Vector3(rotationY, rotationX, 0);
	            transform.localEulerAngles = new Vector3(rotationY, rotationX, 0);
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
	}
	public void init(GameObject playerChar)
	{
		// find the GO tagged player, the player the camera will follow
		player = playerChar;
		
        localTransfrom = player.transform.eulerAngles;

        lookingDir = player.transform.eulerAngles;
		
		inGame = true;
	}
	
}