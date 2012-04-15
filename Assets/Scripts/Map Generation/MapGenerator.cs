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

    private List<GameObject> ChunkList;
    private List<GameObject> GrandList;

	// Use this for initialization
	void Start () {
        ChunkList = new List<GameObject>();
        GrandList = new List<GameObject>();

        BuildMap();
	}

    public void BuildMap()
    {
        for (int iC = 0; iC < NumberOfCubeChunks; iC++)
        {
            Vector3 randPos = Random.insideUnitSphere * WorldSpace;
            ChunkList.Add(ChunkGen.GenerateFullChunk(randPos));
        }
        for (int iG = 0; iG < NumberOfGrandCubes; iG++)
        {
            Vector3 randPos = Random.insideUnitSphere * WorldSpace;
            GrandList.Add((GameObject)Instantiate(GrandCubePrefab, randPos, Quaternion.identity));
        }
    }
}
