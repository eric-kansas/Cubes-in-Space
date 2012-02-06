using UnityEngine;
using System.Collections;
using System;

// This class receive the updated transform from server for the remote player model
public class NetworkTransformReceiver : MonoBehaviour {
	Transform thisTransform;

	void Awake() {
		thisTransform = this.transform;
	}
		
	public void ReceiveTransform(NetworkTransform ntransform) {
		thisTransform.position = Vector3.Lerp(transform.position, ntransform.Position, 10.0f);
		thisTransform.localEulerAngles = ntransform.AngleRotation;	
	}
		
}
