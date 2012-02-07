using UnityEngine;
using System.Collections;

using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;

public class Character : MonoBehaviour {
	
	private bool isMe = false;
	public bool IsMe{ 		get {return isMe;} 
		set {isMe = value;}}
	
	private User smartFoxUser;
	public User SmartFoxUser{ 
		get {return smartFoxUser;} 
		set {smartFoxUser = value;}}
	
	private Color bodyColor = new Color(0.6f,0.6f,0.6f);
	public Color BodyColor{ 
		get {return bodyColor;} 
		set {
			bodyColor = value;
			//find my capsule and color it this color 
			transform.renderer.material.color = bodyColor;
		}
	}
	
	
	
	// Use this for initialization
	void Start () {
		//Debug.Log("Character started.");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
