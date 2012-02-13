using UnityEngine;
using System.Collections;


/// <summary>
/// SCRIPT FOR ALL NETWORKED CHARACTERS THAT ARE NOT THE PLAYER
/// </summary>
public class Avatar : MonoBehaviour {
	
	public Vector3 targetPosition;
	public Color color;
	public float lag; //ping
	public float moveSpeed = 1.25f;
	
	
	
	// Use this for initialization
	void Start () 
	{
	}
	
	void init(Vector3 spawnPos, Color playerColor)
	{
		transform.position = spawnPos;
		color = playerColor;
	}
	
	// LERP THE CHARACTER TO THE TARGET POSITION
	void Update () 
	{
		//Debug.Log("Avatar Update Loop...");
		if (Vector3.Distance(transform.position, targetPosition) <= 5)
		{
			//Debug.Log("\tHas Arrived at " +targetPosition);
			transform.position = targetPosition;	
		}
		else
		{
			//Debug.Log("\tMoving. Current Position = " + transform.position);
			//move towards the target position
			Vector3 distanceVector = targetPosition - transform.position;
			transform.position += (distanceVector.normalized * moveSpeed);
		}
	}
}
