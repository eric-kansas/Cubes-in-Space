using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {

    public int NumberOfGrandCubes = 10;
    public int NumberOfCubeChunks = 30;

    public int WorldSpace = 80;
    public float Spacing = 5;
    public GameObject GrandCubePrefab;
    public CubeChunkGenerator ChunkGen;

    public List<GameObject> ChunkList = new List<GameObject>();
    public List<GameObject> GrandList = new List<GameObject>();

	// Use this for initialization
	void Start () {

	}

    public void BuildMap()
    {
        int idCount = 0;

        for (int iC = 0; iC < NumberOfCubeChunks; iC++)
        {
            Vector3 randPos = Random.insideUnitSphere * WorldSpace;
            ChunkList.Add(ChunkGen.GenerateFullChunk(randPos, idCount));
            idCount++;
        }
        idCount = 0;
        for (int iG = 0; iG < NumberOfGrandCubes; iG++)
        {
            Debug.Log("GrandList: " + GrandList.Count);
            Vector3 randPos = Random.insideUnitSphere * WorldSpace;
            GrandList.Add((GameObject)Instantiate(GrandCubePrefab, randPos, Quaternion.identity));
            GrandList[iG].GetComponent<Cube>().id = idCount;
            idCount++;
        }

    }

    public void BuildInitialStations(int numberOfTeams)
    {
        for (int iT = 0; iT < numberOfTeams; iT++)
        {
            GrandList[iT].GetComponent<Cube>().lockCube(iT);
        }
    }
}
