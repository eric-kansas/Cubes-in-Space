using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CISTeam {
	
	public string teamName;
	public List<CISPlayer> playerList = new List<CISPlayer>();
	public int score;
	
	// Use this for initialization
	public CISTeam (string name, List<CISPlayer> players, int sc) {
		teamName = name;
		playerList = players;
		score = sc;
	}
	
	public void AddPlayerToTeam(CISPlayer pl)
	{
		playerList.Add(pl);
	}
	
	public CISPlayer FindPlayer(string name)
	{
		for(int i = 0; i < playerList.Count; i++)
		{
			if(playerList[i].userName == name)
				return playerList[i];
		}
		
		return null;
	}
}
