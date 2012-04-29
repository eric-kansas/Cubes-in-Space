using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sfs2X.Entities.Data;

public class PlayerGUI : MonoBehaviour
{

    public Texture2D crosshair;
    public int crosshairSize = 20;

    //list of colors, activated color, decativated color
    private Color deactivated = new Color(1f, 1f, 1f, .5f);
    private Color clear = new Color(1f, 1f, 1f, 1f);
    private List<Color> gameColors = new List<Color>();

    public List<GUITexture> crosshairSections;
	
	// CIS GUIStyles
	public GUIStyle gScoreboardBannerStyle;
	public GUIStyle gHeaderBannerStyle;
	public GUIStyle gTeamBannerStyle;
	public GUIStyle gPlayerBannerStyle;
	
	// CIS Textures for GUI
	public Texture2D t_redBackground; // red
	public Texture2D t_blueBackground;	// blue	
	public Texture2D t_greenBackground; // green					
	public Texture2D t_yellowBackground; // yellow
	public Texture2D t_purpleBackground; // purple
	public Texture2D t_orangeBackground; // orange
	public Texture2D t_pinkBackground; // pink
	public Texture2D t_tealBackground; // teal
	public Texture2D t_blackBackground; // black
	public Texture2D t_darkGrayBackground; // darkGray
	public Texture2D t_lightGrayBackground; // lightGray
	
	GameManager mScript;

    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < 6; i++)
        {
            GameObject temp = GameObject.Find("Crosshair Inner " + (i + 1));
            GUITexture tempTex = temp.GetComponent<GUITexture>();
            crosshairSections.Add(tempTex);
        }
        GameObject manager = GameObject.Find("GameManager");
        mScript = manager.GetComponent<GameManager>();

        for (int i = 0; i < mScript.colors.Count; i++)
        {
            gameColors.Add(mScript.colors[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void updateColors(bool activated, List<int> Owners = null)
    {
        // set crosshair GUI to correct colors
        if (activated)
        {
            //sorting Owners list
            Owners.Sort();

            for (int i = 0; i < crosshairSections.Count; i++)
            {
                //Debug.Log(Owners[0] + ", " +Owners[1] + ", " +Owners[2] + ", " +Owners[3] + ", " +Owners[4] + ", " +Owners[5]);

                switch (Owners[i])
                {
                    case -1:
                        crosshairSections[i].color = clear;
                        break;
                    default:
                        crosshairSections[i].color = gameColors[Owners[i]];
                        break;
                }
            }
        }
        else
        {
            foreach (GUITexture i in crosshairSections)
            {
                i.color = deactivated;
            }
        }
    }

    void OnGUI()
    {
        //GUI.DrawTexture(new Rect(Screen.width/2 - crosshairSize/2, Screen.height/2 - crosshairSize * .80f, crosshairSize, crosshairSize), crosshair, ScaleMode.ScaleToFit);
		// CIS Code--------------------------------------------------------------------------
		if(Input.GetKey(KeyCode.Tab))
		{
			Debug.Log("holding down tab");
			
			//instance vars for Scoreboard Layout
			
			float scoreBoxWidth = Screen.width * .75f;
			float scoreBoxHeight = Screen.height * .75f;
			float scoreBoxLeft = Screen.width * .5f - scoreBoxWidth * .5f;
			float scoreBoxTop = Screen.height * .5f - scoreBoxHeight * .5f;
			
			//float scoreBoardBannerWidth = 
			
			float heightFromTop = 0.0f;
			
			GUI.BeginGroup(new Rect(scoreBoxLeft, scoreBoxTop, scoreBoxWidth, scoreBoxHeight));
			GUI.Box(new Rect(0, 0, scoreBoxWidth, scoreBoxHeight),"");
			GUI.Label(new Rect(0, 10.0f, scoreBoxWidth, 50), "Scoreboard", gScoreboardBannerStyle);
			heightFromTop = 60;
			// Header Labels ("Name, #Captured, #Locked, #Stolen, Score)
			// Left Side
			GUI.Box(new Rect(0, heightFromTop + 15.0f, scoreBoxWidth, 25.0f), "", gHeaderBannerStyle);
			gHeaderBannerStyle.alignment = TextAnchor.MiddleLeft;
			GUI.Label(new Rect(0, heightFromTop + 15.0f, scoreBoxWidth * .5f * .2f, 25.0f), "Name", gHeaderBannerStyle); // Name
			gHeaderBannerStyle.alignment = TextAnchor.MiddleCenter;
			gHeaderBannerStyle.fontSize = 16;
			GUI.Label(new Rect(scoreBoxWidth * .5f - (scoreBoxWidth * .5f * .30f) * 3, heightFromTop + 15.0f, scoreBoxWidth * .5f * .2f, 25.0f), "#Captured", gHeaderBannerStyle); // #Captured
			GUI.Label(new Rect(scoreBoxWidth * .5f - (scoreBoxWidth * .5f * .30f) * 2, heightFromTop + 15.0f, scoreBoxWidth * .5f * .2f, 25.0f), "#Locked", gHeaderBannerStyle); // #Locked
			GUI.Label(new Rect(scoreBoxWidth * .5f - (scoreBoxWidth * .5f * .35f) * 1, heightFromTop + 15.0f, scoreBoxWidth * .5f * .2f, 25.0f), "#Stolen", gHeaderBannerStyle); // #Stolen
			gHeaderBannerStyle.alignment = TextAnchor.MiddleRight;
			gHeaderBannerStyle.fontSize = 20;
			GUI.Label(new Rect(scoreBoxWidth * .5f - scoreBoxWidth * .5f * .2f, heightFromTop + 15.0f, scoreBoxWidth * .5f * .2f, 25.0f), "Score", gHeaderBannerStyle); // Score
			
			// Right Side
			gHeaderBannerStyle.alignment = TextAnchor.MiddleLeft;
			GUI.Label(new Rect(scoreBoxWidth * .5f, heightFromTop + 15.0f, scoreBoxWidth * .5f * .2f, 25.0f), "Name", gHeaderBannerStyle);
			gHeaderBannerStyle.alignment = TextAnchor.MiddleCenter;
			gHeaderBannerStyle.fontSize = 16;
			GUI.Label(new Rect(scoreBoxWidth - (scoreBoxWidth * .5f * .30f) * 3, heightFromTop + 15.0f, scoreBoxWidth * .5f * .25f, 25.0f), "#Captured", gHeaderBannerStyle);
			GUI.Label(new Rect(scoreBoxWidth - (scoreBoxWidth * .5f * .30f) * 2, heightFromTop + 15.0f, scoreBoxWidth * .5f * .2f, 25.0f), "#Locked", gHeaderBannerStyle); // #Locked
			GUI.Label(new Rect(scoreBoxWidth - (scoreBoxWidth * .5f * .35f) * 1, heightFromTop + 15.0f, scoreBoxWidth * .5f * .2f, 25.0f), "#Stolen", gHeaderBannerStyle); // #Stolen
			gHeaderBannerStyle.alignment = TextAnchor.MiddleRight;
			gHeaderBannerStyle.fontSize = 20;
			GUI.Label(new Rect(scoreBoxWidth - scoreBoxWidth * .5f * .2f, heightFromTop + 15.0f, scoreBoxWidth * .5f * .2f, 25.0f), "Score", gHeaderBannerStyle); // Score
			
			//Table of player scores
			heightFromTop += 45; // add labels height + padding
			//GUI.BeginScrollView(new Rect(0, 50, scoreBoxWidth, scoreBoxHeight-50), new Vector2(0,50), new Rect(0, 50, scoreBoxWidth, scoreBoxHeight-50));
			//Debug.Log("BUILDING TABLE");
			int heightMultiplier = -1;
			float listDivider = mScript.numberOfTeams/2.0f;
			//Debug.Log("listDivider before: " + listDivider);
			if(listDivider % 2 != 0)
				listDivider = Mathf.Ceil(listDivider);//listDivider+ (listDivider - (int)listDivider);
			//Debug.Log("listDivider after: " + listDivider);
			float teamScoreSpacing = ((scoreBoxHeight - heightFromTop) / listDivider);
			//Debug.Log("Team score spacing = " + teamScoreSpacing);
			string teamName = "";
			for(int i = 0; i < mScript.numberOfTeams; i++)
			{
				//Debug.Log("creating element" + i + ": " + TeamList[i].teamName);
				//Debug.Log("Height Multiplier = " + heightMultiplier);
				float labelPosX = 0.0f;
				float scorePos = scoreBoxWidth * .5f - 25;
				if(i % 2 != 0)
				{
					labelPosX = scoreBoxWidth * .5f;
				}
				else
				{
					heightMultiplier++;
				}
				switch(i)
				{
				case(0):
					teamName = "Red";
					gTeamBannerStyle.normal.background = t_redBackground;
					break;
				case(1):
					teamName = "Blue";
					gTeamBannerStyle.normal.background = t_blueBackground;
					break;
				case(2):
					teamName = "Green";
					gTeamBannerStyle.normal.background = t_greenBackground;
					break;
				case(3):
					teamName = "Yellow";
					gTeamBannerStyle.normal.background = t_yellowBackground;
					break;
				case(4):
					teamName = "Purple";
					gTeamBannerStyle.normal.background = t_purpleBackground;
					break;
				case(5):
					teamName = "Orange";
					gTeamBannerStyle.normal.background = t_orangeBackground;
					break;
				case(6):
					teamName = "Pink";
					gTeamBannerStyle.normal.background = t_pinkBackground;
					break;
				case(7):
					teamName = "Teal";
					gTeamBannerStyle.normal.background = t_tealBackground;
					break;
				default:
					teamName = "Error";
					gTeamBannerStyle.normal.background = t_blackBackground;
					break;
				}
				Debug.Log("Label X pos: " + labelPosX);
				gTeamBannerStyle.alignment = TextAnchor.MiddleLeft;
				GUI.Label(new Rect(labelPosX, heightFromTop + teamScoreSpacing * heightMultiplier, scoreBoxWidth * .5f, 50), teamName, gTeamBannerStyle);
				gTeamBannerStyle.alignment = TextAnchor.MiddleRight;
				//GUI.Label(new Rect(labelPosX, heightFromTop + teamScoreSpacing * heightMultiplier, scoreBoxWidth * .5f, 50), TeamList[i].score.ToString(), gTeamBannerStyle);
				GUI.Label(new Rect(labelPosX + scoreBoxWidth * .5f - scoreBoxWidth * .5f * .25f, heightFromTop + teamScoreSpacing * heightMultiplier, scoreBoxWidth * .5f * .25f, 50.0f), mScript.teamScores[i].ToString(), gTeamBannerStyle); // Score
				GUI.Box(new Rect(labelPosX, heightFromTop + teamScoreSpacing * heightMultiplier, scoreBoxWidth * .5f, 3f), "", gHeaderBannerStyle);
				for(int j = 0; j < mScript.teamList[i].playerList.Count; j++) //TeamList[i].playerList.Count
				{
					if(j % 2 == 0)
						gPlayerBannerStyle.normal.background = t_lightGrayBackground;
					else
						gPlayerBannerStyle.normal.background = t_darkGrayBackground;
					
					
					gPlayerBannerStyle.alignment = TextAnchor.MiddleLeft;
					GUI.Label(new Rect(labelPosX, heightFromTop + 50 + (teamScoreSpacing * heightMultiplier) + 50 * j, scoreBoxWidth * .5f, 50), mScript.teamList[i].playerList[j].userName, gPlayerBannerStyle); // Player Name
					gPlayerBannerStyle.alignment = TextAnchor.MiddleCenter;
					GUI.Label(new Rect(labelPosX + scoreBoxWidth * .5f - (scoreBoxWidth * .5f * .30f) * 3, heightFromTop + 50 + (teamScoreSpacing * heightMultiplier) + 50 * j, scoreBoxWidth * .5f * .15f, 50.0f), mScript.teamList[i].playerList[j].sidesCaptured.ToString(), gPlayerBannerStyle); // #Captured
					GUI.Label(new Rect(labelPosX + scoreBoxWidth * .5f - (scoreBoxWidth * .5f * .30f) * 2, heightFromTop + 50 + (teamScoreSpacing * heightMultiplier) + 50 * j, scoreBoxWidth * .5f * .15f, 50.0f), mScript.teamList[i].playerList[j].sidesLocked.ToString(), gPlayerBannerStyle); // #Locked
					GUI.Label(new Rect(labelPosX + scoreBoxWidth * .5f - (scoreBoxWidth * .5f * .35f) * 1, heightFromTop + 50 + (teamScoreSpacing * heightMultiplier) + 50 * j, scoreBoxWidth * .5f * .15f, 50.0f), mScript.teamList[i].playerList[j].sidesStolen.ToString(), gPlayerBannerStyle); // #Stolen
					gPlayerBannerStyle.alignment = TextAnchor.MiddleRight;
					GUI.Label(new Rect(labelPosX + scoreBoxWidth * .5f - (scoreBoxWidth * .5f * .25f), heightFromTop+50 + (teamScoreSpacing * heightMultiplier) + 50 * j, scoreBoxWidth * .5f * .25f, 50.0f), mScript.teamList[i].playerList[j].score.ToString(), gPlayerBannerStyle);	// Player Score
					//GUI.Box(new Rect(labelPosX, heightFromTop + 50 + (teamScoreSpacing * heightMultiplier) + 50 * j, scoreBoxWidth * .5f, 3f), "", gHeaderBannerStyle);
				}
			}
			GUI.Box(new Rect(scoreBoxWidth * .5f - 2.5f, 75, 5.0f, scoreBoxHeight), "", gHeaderBannerStyle);
			//GUI.EndScrollView();
			GUI.EndGroup();
			// End CIS code-----------------------------------------------------------------------------------------------------
		}// end area
    }
}
