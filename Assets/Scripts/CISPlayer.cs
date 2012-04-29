using UnityEngine;
using System.Collections;

public class CISPlayer {
	
	public string userName;
	public int sidesCaptured;
	public int sidesLocked;
	public int sidesStolen;
	public int score;
	
	// Use this for initialization
	public CISPlayer (string n, int c, int l, int s, int myScore) 
	{
		userName = n;
		sidesCaptured = c;
		sidesLocked = l;
		sidesStolen = s;
		score = myScore;
	}
}
