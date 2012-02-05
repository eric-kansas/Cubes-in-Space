using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    public int numberOfCubes = 1;
    public GameObject GrandCube;
    private List<GameObject> cubeList;

	// Use this for initialization
	void Start () {

        if (GrandCube)
            BuildCubes();
        else
            Debug.LogError("No GrandCube prefab assigned");
        
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    private void BuildCubes()
    {
        for (int i = 0; i < numberOfCubes; i++)
        {
            Debug.Log("adding cube " + i);
            Instantiate(GrandCube, Vector3.zero, Quaternion.identity);
        }
    }
}
