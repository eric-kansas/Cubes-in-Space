using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cube : MonoBehaviour {

    public GameObject sidePrefab;

    public int id = -1;

    public bool locked = false;
    private ParticleEmitter emitter;
    private ParticleRenderer pRenderer;
    private GameManager manager;
    private List<Material> partMaterials;

    private List<GameObject> sides;
    public List<GameObject> Sides
    {
        get { return sides; }
    }

	// Use this for initialization
	void Awake () {
        emitter = this.GetComponentInChildren<ParticleEmitter>();
        emitter.emit = false;
        pRenderer = this.GetComponentInChildren<ParticleRenderer>();
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        partMaterials = manager.paintMaterials;

        Debug.Log("Building Sides _____________________________");
		sides = new List<GameObject>();
        Quaternion cubeRot = transform.rotation;
		
        if (sidePrefab)
        {
            for (int i = 0; i < 6; i++)
            {
                GameObject side = new GameObject();
				
				//position the side relative to the cube
                switch (i)
                {
                    case 0:
                        side = (GameObject)Instantiate(sidePrefab, transform.position + new Vector3(0f, 0f, 2.61f), Quaternion.identity);
                        side.transform.parent = transform;
						//add the side to the list
						//sides.Add(side);
                        break;
                    case 1:
                        side = (GameObject)Instantiate(sidePrefab, transform.position + new Vector3(2.61f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f));
                        side.transform.parent = transform;
						//add the side to the list
						//sides.Add(side);
                        break;
                    case 2:
                        side = (GameObject)Instantiate(sidePrefab, transform.position + new Vector3(0f, 0f, -2.61f), Quaternion.Euler(0f, 180f, 0f));
                        side.transform.parent = transform;
						//add the side to the list
						//sides.Add(side);
                        break;
                    case 3:
                        side = (GameObject)Instantiate(sidePrefab, transform.position + new Vector3(-2.61f, 0f, 0f), Quaternion.Euler(0f, 270f, 0f));
                        side.transform.parent = transform;
						//add the side to the list
						//sides.Add(side);
                        break;
                    case 4:
                        side = (GameObject)Instantiate(sidePrefab, transform.position + new Vector3(0f, 2.61f, 0f), Quaternion.Euler(270f, 0f, 0f));
                        side.transform.parent = transform;
                        
						//add the side to the list
						//sides.Add(side);
                        break;
                    case 5:
                        side = (GameObject)Instantiate(sidePrefab, transform.position + new Vector3(0f, -2.61f, 0f), Quaternion.Euler(90f, 0f, 0f));
                        side.transform.parent = transform;
						//add the side to the list
						//sides.Add(side);
                        break;
                }
				
				//add the side to the list
                side.GetComponent<Side>().id = i;
                side.GetComponent<Side>().cube = transform.gameObject;
				sides.Add(side);
            }
            Vector3 randRot = new Vector3(
                                          UnityEngine.Random.value * 45,
                                          UnityEngine.Random.value * 45,
                                          UnityEngine.Random.value * 45
                                         );
            this.transform.rotation = Quaternion.Euler(randRot);
            Debug.Log("Building Sides ________________________DONE");
        }
        else
        {
            Debug.LogError("No sidePrefab given");
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	private int getSideIndex(GameObject aSide)
	{
		//loop through the sides of the cube and return the index of the selected side
		for (int i = 0; i < 6; i ++)
		{
			if (sides[i].Equals(aSide)) { return i; }
		}
		
		//side not found
		return -1;
	}

    public void lockCube(int teamNum)
    {
        Debug.Log(teamNum);
        for (var i = 0; i < sides.Count; i++)
        {
            sides[i].GetComponent<Side>().locked = true;
            sides[i].GetComponent<Side>().teamOwnedBy = teamNum;
            setSideColor(sides[i], sides[i].GetComponent<Side>().Color[teamNum], teamNum, false);
        }
    }

	/// <summary>
	/// A FUNCTION TO CHANGE THE COLOR OF A SIDE OF A CUBE 
	/// </summary>
	public void setSideColor(GameObject sideHit, Color color, int teamOwnedBy, bool score = true)
	{
		int index = getSideIndex(sideHit);
		if (index == -1) { return; } //not a proper side
		
		
		MeshRenderer mesh = sides[index].GetComponent<MeshRenderer>();
		if (mesh)
		{
			mesh.renderer.material.color = color;
		}
		
		//check for lock
		for(int i = 0; i < 6; i++)
		{
			if(sides[i].GetComponent<MeshRenderer>().renderer.material.color != color)
			{
				return;	
			}
		}
		
		int curTeamIndex = -1;
		List<Color> colors = GameManager.Instance.colors;
		
		for(int i = 0; i < colors.Count; i++)
		{
			if(colors[i].Equals(color))
			{
				curTeamIndex = i;
			}
		}
		
        if(score)
		    GameManager.Instance.UpdateCubeLock(curTeamIndex, this.transform.position);
		                                    
		for(int i = 0; i < 6; i++)
		{
			sides[i].GetComponent<Side>().locked = true;
		}
		
        locked = true;
		pRenderer.material = partMaterials[curTeamIndex];
        emitter.emit = true;
	}
}
