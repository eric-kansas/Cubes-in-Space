using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Side : MonoBehaviour {

    public int id = -1;
    public GameObject cube;
    double timeToBeTaken = -1;
    double timeLastTaken;
    int currentTeamClaim = -1;
    int teamOwnedBy = -1;
    private List<Color> colors = new List<Color>() { Color.red, Color.magenta, Color.yellow, Color.green, Color.blue, Color.cyan };
    const double LOCKINTERVAL = 5000;
    

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        //if there is a time to be taken set && the current time is passed the time to take
        if (timeToBeTaken > 0 && TimeManager.Instance.ClientTimeStamp > timeToBeTaken)
        {
            Debug.Log("taken at: " + TimeManager.Instance.ClientTimeStamp);
            teamOwnedBy = currentTeamClaim;
            cube.GetComponent<Cube>().setSideColor(transform.gameObject, colors[teamOwnedBy]);
            timeToBeTaken = -1;
        }
	}

    public void TakeSide(LaunchPacket info, int team)
    {
        
        if (timeToBeTaken < 0 || // if there is no time to be taken
            info.GameTimeETA < timeToBeTaken && timeLastTaken + LOCKINTERVAL < info.GameTimeETA)//if eta is less then the current time to be taken and we can take it becuase its not locked
        {
            timeToBeTaken = info.GameTimeETA;
            Debug.Log("TAke at: " + timeToBeTaken);
            currentTeamClaim = team;
        }

    }
}
