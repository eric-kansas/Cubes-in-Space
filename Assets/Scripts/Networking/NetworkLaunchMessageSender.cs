using UnityEngine;
using System.Collections;
using System;

// Sends the transform of the local player to server
public class NetworkLaunchMessageSender : MonoBehaviour {

	// We will send transform each 0.1 second. To make transform 
	//synchronization smoother consider writing interpolation algorithm 
	//instead of making smaller period.
	public static float sendingPeriod = 0.1f; 
	
	//private  float accuracy = 0.002f;
	private float timeLastSending = 0.0f;
	private bool send = false;
	
	void Start ()
	{
		//thisTransform = this.transform;
        //lastState = LaunchPacket.FromTransform(thisTransform);
        //SendLaunchOnRequest();

	}
		
	// We call it on local player to start sending his transform
	public void StartSendTransform() {
		send = true;
	}
		
    /*
	void FixedUpdate ()
	{
		if (send) {
            SendLaunchData();
		}
	}
     */
	
	public void SendLaunchOnRequest (LaunchPacket message)
	{
		Debug.Log (GameManager.Instance.ClientName + " sent transform on request");
        Debug.Log(message.ToString());
        GameManager.Instance.SendLaunchMessage(message);
	}
	
	void SendLaunchData() {
        /*
		if (lastState.IsDifferent(thisTransform, accuracy)) {
			if (timeLastSending >= sendingPeriod) {
                lastState = LaunchPosition.FromTransform(thisTransform);
				GameManager.Instance.SendTransform(lastState);
				timeLastSending = 0;
				return;
			}
		}
         */
		timeLastSending += Time.deltaTime;
	}
		
}
