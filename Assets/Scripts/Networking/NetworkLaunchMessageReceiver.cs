using UnityEngine;
using System.Collections;
using System;

// This class receive the updated transform from server for the remote player model
public class NetworkLaunchMessageReceiver : MonoBehaviour {
	LaunchPacket thisLaunchPacket;

    public void ReceiveLaunchData(LaunchPacket launchMessage)
    {
        thisLaunchPacket = launchMessage;
        Debug.Log("got a launchPAcket");
        //thisLaunchPacket.position = Vector3.Lerp(transform.position, ntransform.LaunchPosition, 10.0f);
		//thisTransform.localEulerAngles = ntransform.AngleRotation;	
	}
		
}
