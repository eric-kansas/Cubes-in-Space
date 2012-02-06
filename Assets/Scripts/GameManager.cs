using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    public float WorldSpace = 50f;

    public int numberOfCubes = 10;
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
            Vector3 randPos = new Vector3(
                                          -WorldSpace + (Random.value * (WorldSpace*2f)),
                                          -WorldSpace + (Random.value * (WorldSpace*2f)),
                                          -WorldSpace + (Random.value * (WorldSpace*2f))
                                          );

            Instantiate(GrandCube, randPos, Quaternion.identity);
        }
    }
}
