using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cube : MonoBehaviour {

    public GameObject sidePrefab;

    private List<GameObject> sides;
    public List<GameObject> Sides
    {
        get { return sides; }
    }

	// Use this for initialization
	void Start () {
		sides = new List<GameObject>();
        Quaternion cubeRot = transform.rotation;
		
        if (sidePrefab)
        {
            for (int i = 0; i < 6; i++)
            {
                GameObject side;
				
				//position the side relative to the cube
                switch (i)
                {
                    case 0:
                        side = (GameObject)Instantiate(sidePrefab, transform.position + new Vector3(0f, 0f, 2.61f), Quaternion.identity);
                        side.transform.parent = transform;
						//add the side to the list
						sides.Add(side);
                        break;
                    case 1:
                        side = (GameObject)Instantiate(sidePrefab, transform.position + new Vector3(2.61f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f));
                        side.transform.parent = transform;
						//add the side to the list
						sides.Add(side);
                        break;
                    case 2:
                        side = (GameObject)Instantiate(sidePrefab, transform.position + new Vector3(0f, 0f, -2.61f), Quaternion.Euler(0f, 180f, 0f));
                        side.transform.parent = transform;
						//add the side to the list
						sides.Add(side);
                        break;
                    case 3:
                        side = (GameObject)Instantiate(sidePrefab, transform.position + new Vector3(-2.61f, 0f, 0f), Quaternion.Euler(0f, 270f, 0f));
                        side.transform.parent = transform;
						//add the side to the list
						sides.Add(side);
                        break;
                    case 4:
                        side = (GameObject)Instantiate(sidePrefab, transform.position + new Vector3(0f, 2.61f, 0f), Quaternion.Euler(270f, 0f, 0f));
                        side.transform.parent = transform;
						//add the side to the list
						sides.Add(side);
                        break;
                    case 5:
                        side = (GameObject)Instantiate(sidePrefab, transform.position + new Vector3(0f, -2.61f, 0f), Quaternion.Euler(90f, 0f, 0f));
                        side.transform.parent = transform;
						//add the side to the list
						sides.Add(side);
                        break;
                }
				
				//add the side to the list
				//sides.Add(side);
            }
            Vector3 randRot = new Vector3(
                                          UnityEngine.Random.value * 45,
                                          UnityEngine.Random.value * 45,
                                          UnityEngine.Random.value * 45
                                         );
            this.transform.rotation = Quaternion.Euler(randRot);
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
	
	
	/// <summary>
	/// A FUNCTION TO CHANGE THE COLOR OF A SIDE OF A CUBE 
	/// </summary>
	public void setSideColor(GameObject sideHit, Color color)
	{
		int index = getSideIndex(sideHit);
		if (index == -1) { return; } //not a proper side
		
		
		MeshRenderer mesh = sides[index].GetComponent<MeshRenderer>();
		if (mesh)
		{
			mesh.renderer.material.color = color;
		}
	}
}
