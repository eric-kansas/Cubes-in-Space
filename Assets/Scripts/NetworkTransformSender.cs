using UnityEngine;
using System.Collections;
using System;

// Sends the transform of the local player to server
public class NetworkTransformSender : MonoBehaviour {

	// We will send transform each 0.1 second. To make transform 
	//synchronization smoother consider writing interpolation algorithm 
	//instead of making smaller period.
	public static float sendingPeriod = 0.1f; 
	
	private  float accuracy = 0.002f;
	private float timeLastSending = 0.0f;
	private bool send = false;
	private NetworkTransform lastState;
	private Transform thisTransform;
	
	void Start ()
	{
		thisTransform = this.transform;
		lastState = NetworkTransform.FromTransform (thisTransform);
		SendTransformOnRequest ();

	}
		
	// We call it on local player to start sending his transform
	public void StartSendTransform() {
		send = true;
	}
		
	void FixedUpdate ()
	{
		if (send) {
			SendTransform ();
		}
	}
	
	public void SendTransformOnRequest ()
	{
		Debug.Log (GameManager.Instance.ClientName + " sent transform on request");
		GameManager.Instance.SendTransform (lastState);
		timeLastSending = 0;
	}
	
	void SendTransform() {
		if (lastState.IsDifferent(thisTransform, accuracy)) {
			if (timeLastSending >= sendingPeriod) {
				lastState = NetworkTransform.FromTransform(thisTransform);
				GameManager.Instance.SendTransform(lastState);
				timeLastSending = 0;
				return;
			}
		}
		timeLastSending += Time.deltaTime;
	}
		
}
