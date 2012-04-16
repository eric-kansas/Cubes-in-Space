using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChunkSide : MonoBehaviour {

    public int id = -1;
    public GameObject cubeChunk;

    double timeToBeTaken = -1;
    double timeLastTaken;
    int currentTeamClaim = -1;
    int teamLastOwnedBy = -1;

    public int teamOwnedBy = -1;
    private List<Color> colors;
    private List<Material> partMaterials;
    const double LOCKINTERVAL = 5000;

    private GameManager manager;
    public bool locked = false;
    public bool _isPlayer;
    private bool willPaint;


    // Use this for initialization
    void Start()
    {
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        colors = manager.colors;
    }

    // Update is called once per frame
    void Update()
    {

        //if there is a time to be taken set && the current time is passed the time to take
        if (timeToBeTaken > 0 && TimeManager.Instance.ClientTimeStamp > timeToBeTaken)
        {

            //already owned
            if (teamOwnedBy == currentTeamClaim)
            {
                //emitter.emit = true;
            }
            else
            {
                if (willPaint)
                {
                    teamLastOwnedBy = teamOwnedBy;
                    teamOwnedBy = currentTeamClaim;
                    cubeChunk.GetComponent<CubeChunk>().splashColor(transform.gameObject, colors[teamOwnedBy]);
                    //pRenderer.material = partMaterials[teamOwnedBy];
                    //emitter.emit = true;
                    //UPDATE SCORE *****/
                    GameManager.Instance.UpdateTeamScore(teamLastOwnedBy, teamOwnedBy);
                    /*****/
                }
                if (_isPlayer)
                {
                    if (willPaint)
                    {
                        manager.myAvatar.GetComponent<Player>().subtractPaint();
                    }
                }

            }

            timeLastTaken = timeToBeTaken;
            timeToBeTaken = -1;


        }
        /*
        if (emitter.emit == true && timeLastTaken + LOCKINTERVAL < TimeManager.Instance.ClientTimeStamp)
        {
            emitter.emit = false;
        }
         */
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
