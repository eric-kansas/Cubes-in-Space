using UnityEngine;
using System.Collections;


/// <summary>
/// SCRIPT FOR ALL NETWORKED CHARACTERS THAT ARE NOT THE PLAYER
/// </summary>
public class Avatar : MonoBehaviour {
	
	public Vector3 targetPosition;
	public Color color;
	public float lag; //ping
	
	
	
	
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
	
	}
}
