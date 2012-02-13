using UnityEngine;
using System.Collections;
using System;

// This class receive the updated transform from server for the remote player model
public class NetworkLaunchMessageReceiver : MonoBehaviour {
	LaunchPacket thisLaunchPacket;

    public void ReceiveLaunchData(LaunchPacket launchMessage)
    {
        //thisLaunchPacket = launchMessage;
        Debug.Log("ReceiveLaunchData Function... ");
		
		
		//calculate the time to move to target
		float moveSpeed = .75f;
		float distance = Vector3.Distance(launchMessage.LaunchPosition, launchMessage.LaunchDestination);
		
		//time = distance / speed
		float moveTime = distance / moveSpeed;
		//Debug.Log("\t-Distance and Time calculated");
		
		//parse the packet and move the enemy avatar
		//this.transform.position = Vector3.Lerp(launchMessage.LaunchPosition, launchMessage.LaunchDestination, moveTime);
		this.transform.position = launchMessage.LaunchDestination;
		this.transform.LookAt(launchMessage.LaunchDestination);
		//Debug.Log("\t-User: " + this.name + " is moving towards the destination now");
		
        //thisLaunchPacket.position = Vector3.Lerp(transform.position, ntransform.LaunchPosition, 10.0f);
		//thisTransform.localEulerAngles = ntransform.AngleRotation;	
	}
		
}
