using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CubeChunkGenerator : MonoBehaviour {

    public GameObject ChunkPeice;

    public bool centerCubeGeneration = true;
    public int MinAddChunkSize = 2;
    public int MaxAddChunkSize = 6;
    public int MinAdditivePasses = 1;
    public int MaxAdditivePasses = 5;
    /*
    public bool SubtractivePass = true;
    public int MinSubChunkSize = 2;
    public int MaxSubChunkSize = 6;
    public int MinSubtractivePasses = 1;
    public int MaxSubtractivePasses = 5;
    */

    private List<GameObject> pieceList;
    private List<GameObject> centersList;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public GameObject GenerateFullChunk(Vector3 startPos, int myId)
    {
        pieceList = new List<GameObject>();
        centersList = new List<GameObject>();

        GameObject Chunk = new GameObject();
        Chunk.name = "Cube Chunk";
        CubeChunk ChunkScript = Chunk.AddComponent<CubeChunk>();

        int addCounter = Random.Range(MinAdditivePasses, MaxAdditivePasses);
        for (int iA = 0; iA < addCounter; iA++)
        {
            if (pieceList.Count > 0)
            {
                startPos = pieceList[(int)Mathf.Floor(Random.Range(0, pieceList.Count))].transform.position;
                List<GameObject> currentChunk = AddChunk(startPos);
                RemoveDuplicates(currentChunk);
            }
            else
            {
                AddChunk(startPos);
            }
        }

        MarkCenterCubes();
        /*
        if (SubtractivePass)
        {
            //List<GameObject> outersList = new List<GameObject>();
            int subtractCounter = Random.Range(MinSubtractivePasses, MaxSubtractivePasses);


            for (int iS = 0; iS < subtractCounter; iS++)
            {
                for (int jA = 0; jA < pieceList.Count; jA++)
                {
                    if
                }
                startPos = pieceList[(int)Mathf.Floor(Random.Range(0, pieceList.Count))].transform.position;
                SubtractChunk(startPos);
            }

            MarkCenterCubes();
        }
        */
        DeleteCenterCubes();

        Debug.Log("Final cube chunk size is " + pieceList.Count);
        ChunkScript.BindCubeArray(pieceList, myId);
        return Chunk;
    }

    private void MarkCenterCubes()
    {
        centersList = new List<GameObject>();
        for (int i = 0; i < pieceList.Count; i++)
        {
            //up
            RaycastHit hit = new RaycastHit();
            if(Physics.Raycast(pieceList[i].transform.position,Vector3.up,out hit, 2))
                //down
                if(Physics.Raycast(pieceList[i].transform.position,Vector3.down,out hit, 2))
                    //front
                    if(Physics.Raycast(pieceList[i].transform.position,Vector3.forward,out hit, 2))
                        //back
                        if(Physics.Raycast(pieceList[i].transform.position,Vector3.back,out hit, 2))
                            //left
                            if(Physics.Raycast(pieceList[i].transform.position,Vector3.left,out hit, 2))
                                //right
                                if(Physics.Raycast(pieceList[i].transform.position,Vector3.right,out hit, 2))
                                    centersList.Add(pieceList[i]);

        }
    }

    private void DeleteCenterCubes()
    {
        for (int i = 0; i < centersList.Count; i++)
        {
            Destroy(centersList[i]);
        }
    }

    private void RemoveDuplicates(List<GameObject> currentChunk)
    {
        int removed = 0;
        for (int iC = 0; iC < currentChunk.Count; iC++)
        {
            for (int iA = 0; iA < pieceList.Count; iA++)
            {
                if (currentChunk[iC] != pieceList[iA])
                {
                    if (currentChunk[iC].transform.position == pieceList[iA].transform.position)
                    {
                        Destroy(currentChunk[iC]);
                        removed++;
                    }
                }
            }
        }
    }

    private List<GameObject> AddChunk(Vector3 startPos)
    {
        Debug.Log("++++ Adding Chunk ++++");
        int genX = (int)Mathf.Floor(Random.Range(MinAddChunkSize, MaxAddChunkSize+1));
        int genY = (int)Mathf.Floor(Random.Range(MinAddChunkSize, MaxAddChunkSize+1));
        int genZ = (int)Mathf.Floor(Random.Range(MinAddChunkSize, MaxAddChunkSize+1));
        List<GameObject> currentChunk = new List<GameObject>();

        //Debug.Log("generated addition chunk dimensions " + genX + ", " + genY + ", " + genZ);

        //If checked, this will try to change the start position to make the chunks generate around the originally chosen center position
        if (centerCubeGeneration)
        {
            startPos = new Vector3(
                                startPos.x - Mathf.Floor(genX / 2),
                                startPos.y - Mathf.Floor(genY / 2),
                                startPos.z - Mathf.Floor(genZ / 2)
                                  );
            // fix centered generation moving cube chunks into invalid locations
            if (startPos.x % 2 != 0)
                startPos.x --;
            if (startPos.y % 2 != 0)
                startPos.y --;
            if (startPos.z % 2 != 0)
                startPos.z --;

            Debug.Log("new start position is: " + startPos);
        }

        for (int iX = 0; iX < genX; iX++)
        {
            for (int iY = 0; iY < genY; iY++)
            {
                for (int iZ = 0; iZ < genZ; iZ++)
                {

                    GameObject temp = (GameObject)Instantiate(ChunkPeice, (startPos + new Vector3(2 * iX, 2 * iY, 2 * iZ)), Quaternion.identity);
                    pieceList.Add(temp);
                    currentChunk.Add(temp);
                }
            }
        }
        return currentChunk;
    }
    /*
    private void SubtractChunk(Vector3 startPos)
    {
        Debug.Log("---- Subtracting Chunk ----");
        int genX = (int)Mathf.Floor(Random.Range(MinSubChunkSize, MaxSubChunkSize + 1));
        int genY = (int)Mathf.Floor(Random.Range(MinSubChunkSize, MaxSubChunkSize + 1));
        int genZ = (int)Mathf.Floor(Random.Range(MinSubChunkSize, MaxSubChunkSize + 1));

        Debug.Log("generated subtraction chunk dimensions " + genX + ", " + genY + ", " + genZ);

        if (centerCubeGeneration)
        {
            startPos = new Vector3(
                                startPos.x - Mathf.Floor(genX / 2),
                                startPos.y - Mathf.Floor(genY / 2),
                                startPos.z - Mathf.Floor(genZ / 2)
                                  );
            // fix centered generation moving cube chunks into invalid locations
            if (startPos.x % 2 != 0)
                startPos.x--;
            if (startPos.y % 2 != 0)
                startPos.y--;
            if (startPos.z % 2 != 0)
                startPos.z--;

            Debug.Log("new start position is: " + startPos);

            for (int iX = 0; iX < genX; iX++)
            {
                for (int iY = 0; iY < genY; iY++)
                {
                    for (int iZ = 0; iZ < genZ; iZ++)
                    {
                        for (int iA = 0; iA < pieceList.Count; iA++)
                        {
                            Vector3 checkPos = new Vector3(iX, iY, iZ);
                            if (pieceList[iA].transform.position == checkPos)
                            {
                                Debug.Log("removing");
                                pieceList.RemoveAt(iA);
                            }
                        }
                    }
                }
            }
        }
    }
    */
}
