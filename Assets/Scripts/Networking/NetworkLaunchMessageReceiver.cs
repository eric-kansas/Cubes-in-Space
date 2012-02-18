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
        Debug.Log("CubeID: " + launchMessage.CubeID);
        Debug.Log("SideID: " + launchMessage.SideID);
		avatarScript.TargetPosition = launchMessage.LaunchDestination;
        GameObject side = GameManager.Instance.GetSide(launchMessage.CubeID, launchMessage.SideID);
        side.GetComponent<Side>().TakeSide(launchMessage, avatarScript.team);

        //Debug.Log(this.transform.position)		this.transform.LookAt(launchMessage.LaunchDestination);

	}
		
	
	
}
