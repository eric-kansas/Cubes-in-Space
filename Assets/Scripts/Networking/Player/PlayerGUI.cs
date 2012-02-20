using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerGUI : MonoBehaviour
{

    public Texture2D crosshair;
    public int crosshairSize = 20;

    //list of colors, activated color, decativated color
    private Color deactivated = new Color(1f, 1f, 1f, .5f);
    private Color clear = new Color(1f, 1f, 1f, 1f);
    private List<Color> gameColors = new List<Color>();

    public List<GUITexture> crosshairSections;

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
        GameManager mScript = manager.GetComponent<GameManager>();

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

    /*void OnGUI()
    {
        GUI.DrawTexture(new Rect(Screen.width/2 - crosshairSize/2, Screen.height/2 - crosshairSize * .80f, crosshairSize, crosshairSize), crosshair, ScaleMode.ScaleToFit);
    }*/
}
