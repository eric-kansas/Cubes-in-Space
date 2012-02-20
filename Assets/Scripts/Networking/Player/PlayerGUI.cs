using UnityEngine;
using System.Collections;

public class PlayerGUI : MonoBehaviour {
	
	public Texture2D crosshair;
	public int crosshairSize = 20;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnGUI()
	{
		GUI.DrawTexture(new Rect(Screen.width/2 - crosshairSize/2, Screen.height/2 - crosshairSize * .80f, crosshairSize, crosshairSize), crosshair, ScaleMode.ScaleToFit);
	}
}
