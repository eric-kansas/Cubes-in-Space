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
	public float slerpSpeed = 2.0f;
	public float lookatDistance = 7.5f;
    

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
			// slerp to the player's rotation first so that our rotation is similar when we change scripts
			transform.rotation = Quaternion.Euler(Vector3.Slerp(transform.rotation.eulerAngles, target.rotation.eulerAngles, Time.deltaTime * slerpSpeed));
			// if we're still a little bit away then use lookat but stop once we're really close
			if (percent < 1.0f)
			{
				// we want to lookAt a point in front of the player as we arrive to the player's position
				// this will have us align our camera with the correct player view
				// the multiplications at the end are to push the lookatPos farther out as we get closer
				Vector3 lookatPos = target.position + (target.forward * lookatDistance * (slerpSpeed + percent));
				transform.LookAt(lookatPos);
			}
		}

    }

    public void reset()
    {
        distance = 1.0f;
        lastPercentMark = .75f;
        heightDamping = 2.0f;
        positionDamping = 2.0f;
    }
}