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
    public int teamOwnedBy = -1;
    private List<Color> colors;
    public List<Color> Color
    {
        get { return colors; }
    }
    private List<Material> partMaterials;
    const double LOCKINTERVAL = 5000;
    private ParticleEmitter emitter;
    private ParticleRenderer pRenderer;
    private GameManager manager;
	public bool locked = false;
    public bool _isPlayer;

    public bool _isRefuel = false;
    private bool willPaint;
    

	// Use this for initialization
	void Awake () {
        emitter = this.GetComponentInChildren<ParticleEmitter>();
        pRenderer = this.GetComponentInChildren<ParticleRenderer>();
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        colors = manager.colors;
        partMaterials = manager.materials;
	}
	
	// Update is called once per frame
	void Update () {

        //if there is a time to be taken set && the current time is passed the time to take
        if (timeToBeTaken > 0 && TimeManager.Instance.ClientTimeStamp > timeToBeTaken)
        {

            //already owned
            if (teamOwnedBy == currentTeamClaim)
            {
                emitter.emit = true;
                if (_isPlayer)
                {
                    if (locked)
                    {
                        Debug.Log("REFUEL!!!");
                        manager.myAvatar.GetComponent<Player>().refuel();
                    }
                }
            }
            else
            {
                if (willPaint && !locked)
                {
                    teamLastOwnedBy = teamOwnedBy;
                    teamOwnedBy = currentTeamClaim;
                    cube.GetComponent<Cube>().setSideColor(transform.gameObject, colors[teamOwnedBy]);
                    Debug.Log("team owned by: " + teamOwnedBy);
                    pRenderer.material = partMaterials[teamOwnedBy];
                    emitter.emit = true;
                    //UPDATE SCORE *****/
                    GameManager.Instance.UpdateTeamScore(teamLastOwnedBy, teamOwnedBy);
                    /*****/
                }
                if (_isPlayer)
                {
                    if (willPaint && !locked)
                    {
                        manager.myAvatar.GetComponent<Player>().subtractPaint();
                    }
                }

            }

            timeLastTaken = timeToBeTaken;
            timeToBeTaken = -1;
			
            
        }
        if (emitter.emit == true && timeLastTaken + LOCKINTERVAL < TimeManager.Instance.ClientTimeStamp)
        {
            emitter.emit = false;

        }
	}

    public void TakeSide(LaunchPacket info, int team, bool isPlayer = false)
    {
        //owned by same team

        if (info.Paint)
        {
            if (timeToBeTaken < 0 || // if there is no time to be taken
            info.GameTimeETA < timeToBeTaken && timeLastTaken + LOCKINTERVAL < info.GameTimeETA)//if eta is less then the current time to be taken and we can take it becuase its not locked
            {
                willPaint = true;
            }
        }

        timeToBeTaken = info.GameTimeETA;
        currentTeamClaim = team;
        _isPlayer = isPlayer;
        
    }
}
