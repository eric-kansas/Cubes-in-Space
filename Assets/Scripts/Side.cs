using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Side : MonoBehaviour {

    public int id = -1;
    public GameObject cube;
    double timeToBeTaken = -1;
    double timeLastTaken;
    int currentTeamClaim = -1;
	/*****/
	int teamLastOwnedBy = -1;
	/*****/
    int teamOwnedBy = -1;
    private List<Color> colors;
    private List<Material> partMaterials;
    const double LOCKINTERVAL = 5000;
    private ParticleEmitter emitter;
    private ParticleRenderer pRenderer;
    private GameObject manager;
    

	// Use this for initialization
	void Start () {
        emitter = this.GetComponentInChildren<ParticleEmitter>();
        pRenderer = this.GetComponentInChildren<ParticleRenderer>();
        manager = GameObject.Find("GameManager");
        GameManager managerScript = manager.GetComponent<GameManager>();
        colors = managerScript.colors;
        partMaterials = managerScript.materials;
	}
	
	// Update is called once per frame
	void Update () {

        //if there is a time to be taken set && the current time is passed the time to take
        if (timeToBeTaken > 0 && TimeManager.Instance.ClientTimeStamp > timeToBeTaken)
        {
			teamLastOwnedBy = teamOwnedBy;
            Debug.Log("taken at: " + TimeManager.Instance.ClientTimeStamp);
            teamOwnedBy = currentTeamClaim;
            cube.GetComponent<Cube>().setSideColor(transform.gameObject, colors[teamOwnedBy]);
            timeLastTaken = timeToBeTaken;
            timeToBeTaken = -1;
            pRenderer.material = partMaterials[teamOwnedBy];
			
			
			//UPDATE SCORE *****/
			GameManager.Instance.UpdateTeamScore(teamLastOwnedBy, teamOwnedBy);
			/*****/
			
            emitter.emit = true;
        }
        if (emitter.emit == true && timeLastTaken + LOCKINTERVAL < TimeManager.Instance.ClientTimeStamp)
        {
            emitter.emit = false;

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
