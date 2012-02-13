using UnityEngine;
using System.Collections;
using System;

// This class receive the updated transform from server for the remote player model
public class NetworkLaunchMessageReceiver : MonoBehaviour {
	LaunchPacket thisLaunchPacket;

    public void ReceiveLaunchData(LaunchPacket launchMessage)
    {
        //thisLaunchPacket = launchMessage;
        //Debug.Log("ReceiveLaunchData Function... ");
		//Debug.Log("\t-Received Packet: " + launchMessage.ToString());
		
		//calculate the time to move to target
		float moveSpeed = 1.25f;
		//float distance = Vector3.Distance(launchMessage.LaunchPosition, launchMessage.LaunchDestination);
		
		//velocity = distance / time
		//velocity * time = distance
		//time = distance / velocity
		//time = distance / speed
		//float moveTime = distance / moveSpeed;
		//Debug.Log("\t-Distance: " + distance + "\t Time: " + moveTime);
		
		//parse the packet and move the enemy avatar
		//Debug.Log(launchMessage.LaunchDestination);
		//this.transform.position = launchMessage.LaunchDestination;
		//Debug.Log("\t-Setting the Avatar settings.");
		Avatar avatarScript = this.GetComponent<Avatar>();
		avatarScript.targetPosition = launchMessage.LaunchDestination;
		
        //Debug.Log(this.transform.position);
		this.transform.LookAt(launchMessage.LaunchDestination);
		//Debug.Log("\t-User: " + this.name + " is moving towards the destination now");
		
        //thisLaunchPacket.position = Vector3.Lerp(transform.position, ntransform.LaunchPosition, 10.0f);
		//thisTransform.localEulerAngles = ntransform.AngleRotation;	
	}
		
	
	
}
