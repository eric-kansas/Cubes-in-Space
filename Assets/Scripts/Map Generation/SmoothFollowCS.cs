using UnityEngine;
using System.Collections;

using UnityEngine;
using System.Collections;
/*
//This camera smoothes out rotation around the y-axis and height.
//Horizontal Distance to the target is always fixed.
//For every of those smoothed values we calculate the wanted value and the current value.
//Then we smooth it using the Lerp function.
//Then we apply the smoothed values to the transform's position.
*/
public class SmoothFollowCS : MonoBehaviour
{
    public Transform target;
    public Vector3 targetLocation;
    public Vector3 startLocation;
    private Vector3 targetToLookAt;
    private float distance = 1.0f;
    private float height = 1.0f;
    private float heightDamping = 2.0f;
    private float positionDamping = 2.0f;

    private float lastPercentMark = .75f;

    public Vector3 targetNormal;
	
	public bool arriving = false;
	//public bool slerpFinished = false;
	private float slerpSpeed = 10.0f;
	private float lookatDistance = 10.0f;
    

    // Update is called once per frame
    void LateUpdate()
    {
        // Early out if we don't have a target
        if (!target)
            return;
		float wantedHeight = target.position.y + height;
    	float currentHeight = transform.position.y;

        //calc percent of distance traveled
        Vector3 totalDistance = (targetLocation - startLocation);
        Vector3 distanceLeft = (targetLocation - target.position);
        float percent = distanceLeft.magnitude / totalDistance.magnitude;
        if (percent < lastPercentMark)
        {
            positionDamping += .3f;
            heightDamping += .3f;
            lastPercentMark -= .05f;
            //distance -= 0.60f;
        }

        // Damp the height
        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

        // Set the position of the camera
        Vector3 wantedPosition = target.position - target.forward * distance;

        transform.position = Vector3.Lerp(transform.position, wantedPosition, positionDamping * Time.deltaTime);

        // adjust the height of the camera
        transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);
        
		
		// check if we're arriving at the target, if not just look at the target
		if (!arriving)
		{
	        // look at the target
	        transform.LookAt(target);
		}
		else
		{	
			// the player has arrived at the position so just set the camera's position to be the player's
			transform.position = target.position;
			
			// the dot product between the camera's rotation and the player's rotation
			float rDot = Quaternion.Dot(transform.rotation, target.rotation);
			//Debug.Log("rDot = " + rDot);
			/*
			// if we are not very close in rotation use the lookat to get us pointing in the same general 
			if ( rDot < 0.75f)
			{
				//Debug.Log("Using LookAt");
				// we want to lookAt a point in front of the player as we arrive to the player's position
				// this will have us align our camera with the correct player view
				// the multiplications at the end are to push the lookatPos farther out as we get closer
				Vector3 lookatPos = target.position + (target.forward * lookatDistance);
				transform.LookAt(lookatPos);
				//transform.LookAt(lookatPos, transform.up);
			}
			*/
			//Debug.Log("Slerping");
			// slerp to the player's rotation so that our rotation is similar when we change scripts
			transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, Time.deltaTime * slerpSpeed);
			
			/*
			// once we get close enough just set the rotations to be the same
			// this should eliminate the jump at the end...hopefully
			if (rDot > 0.95f)
			{
				Debug.Log("Set the camera rotation to be the player's rotation");
				transform.rotation = target.rotation;
				slerpFinished = true;
			}
			*/
		}

    }

    public void reset()
    {
        distance = 1.0f;
        lastPercentMark = .75f;
        heightDamping = 2.0f;
        positionDamping = 2.0f;
		arriving = false;
		//slerpFinished = false;
    }
}