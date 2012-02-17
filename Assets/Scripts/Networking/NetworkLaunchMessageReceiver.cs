using UnityEngine;
using System.Collections;
using System;

// This class receive the updated transform from server for the remote player model
public class NetworkLaunchMessageReceiver : MonoBehaviour {
	LaunchPacket thisLaunchPacket;

    public void ReceiveLaunchData(LaunchPacket launchMessage)
    {

		Avatar avatarScript = this.GetComponent<Avatar>();
        double deltaTime = launchMessage.LocalGameTime - TimeManager.Instance.ClientTimeStamp;
        Debug.Log("here in launch data: delta time: " + deltaTime);
		avatarScript.TargetPosition = launchMessage.LaunchDestination;
		
        //Debug.Log(this.transform.position)		this.transform.LookAt(launchMessage.LaunchDestination);

	}
		
	
	
}
