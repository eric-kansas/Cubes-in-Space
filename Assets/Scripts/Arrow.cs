using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Arrow : MonoBehaviour
{
    GameManager mScript;
    Player pScript;

    // Use this for initialization
    void Start()
    {
        GameObject manager = GameObject.Find("GameManager");
        mScript = manager.GetComponent<GameManager>();
        for (int i = 0; i < transform.GetChildCount(); i++)
        {
            transform.GetChild(i).renderer.material.color = mScript.colors[GameValues.teamNum];
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (mScript.myRefillingStations.Count > 0)
        {
            int index = -1;
            float closestDist = 100000000000000;
            Vector3 dv = Vector3.zero;
			//Debug.Log(mScript.myRefillingStations.Count + "++++++++++++++++++++++++++++++++++++++++++++++++++");
            for (int i = 0; i < mScript.myRefillingStations.Count; i++)
            {
                if (mScript.myRefillingStations[i] != null)
                {
                    float tempDist = Vector3.Distance(transform.position, mScript.myRefillingStations[i]);
                    if (tempDist < closestDist && tempDist != 0)
                    {
                        dv = mScript.myRefillingStations[i] - transform.position;
						closestDist = tempDist;
                        index = i;
                    }
                }
            }

            if (index != -1)
            {
                transform.forward = dv;
            }
        }
    }
}
