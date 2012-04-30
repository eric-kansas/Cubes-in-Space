using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CubeChunk : MonoBehaviour {

    public List<GameObject> CubeArray;
    public int id;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void BindCubeArray(List<GameObject> array,  int myId)
    {
        id = myId;
        if (CubeArray == null)
        {
            CubeArray = array;
            for (int i = 0; i < CubeArray.Count; i++)
            {
                CubeArray[i].transform.parent = this.transform;
                ChunkSide chunkSideScript = CubeArray[i].AddComponent<ChunkSide>();
                chunkSideScript.id = i;
                chunkSideScript.cubeChunk = this.gameObject;
            }
            RotateChunk();
        }
        else
        {
            Debug.LogError("Something tried to set a Cube Chunk array after the chunk was created");
        }
    }

    private void RotateChunk()
    {
        Vector3 randRot = new Vector3(
                                          UnityEngine.Random.value * 45,
                                          UnityEngine.Random.value * 45,
                                          UnityEngine.Random.value * 45
                                         );
        this.transform.rotation = Quaternion.Euler(randRot);
    }

    private int getSideIndex(GameObject aSide)
    {
        //loop through the sides of the cube and return the index of the selected side
        for (int i = 0; i < CubeArray.Count; i++)
        {
            if (CubeArray[i].Equals(aSide)) { return i; }
        }

        //side not found
        return -1;
    }

    public void splashColor(GameObject startObject, Color color)
    {
        setSideColor(startObject, color);

        RaycastHit hit;
        if (Physics.Raycast(startObject.transform.position, Vector3.up, out hit, 4.0f))
        {
            GameObject targetObject = hit.transform.gameObject;
            if (targetObject.transform.parent == this.transform)
            {
                setSideColor(targetObject, color);
            }
        }
        if (Physics.Raycast(startObject.transform.position, Vector3.down, out hit, 4.0f))
        {
            GameObject targetObject = hit.transform.gameObject;
            if (targetObject.transform.parent == this.transform)
            {
                setSideColor(targetObject, color);
            }
        }
        if (Physics.Raycast(startObject.transform.position, Vector3.left, out hit, 4.0f))
        {
            GameObject targetObject = hit.transform.gameObject;
            if (targetObject.transform.parent == this.transform)
            {
                setSideColor(targetObject, color);
            }
        }
        if (Physics.Raycast(startObject.transform.position, Vector3.right, out hit, 4.0f))
        {
            GameObject targetObject = hit.transform.gameObject;
            if (targetObject.transform.parent == this.transform)
            {
                setSideColor(targetObject, color);
            }
        }
        if (Physics.Raycast(startObject.transform.position, Vector3.forward, out hit, 4.0f))
        {
            GameObject targetObject = hit.transform.gameObject;
            if (targetObject.transform.parent == this.transform)
            {
                setSideColor(targetObject, color);
            }
        }
        if (Physics.Raycast(startObject.transform.position, Vector3.back, out hit, 4.0f))
        {
            GameObject targetObject = hit.transform.gameObject;
            if (targetObject.transform.parent == this.transform)
            {
                setSideColor(targetObject, color);
            }
        }
    }

    /// <summary>
    /// A FUNCTION TO CHANGE THE COLOR OF A SIDE OF A CUBE 
    /// </summary>
    public void setSideColor(GameObject sideHit, Color color)
    {
        int index = getSideIndex(sideHit);
        if (index == -1) { return; } //not a proper side


        MeshRenderer mesh = CubeArray[index].GetComponent<MeshRenderer>();
        if (mesh)
        {
            mesh.renderer.material.color = color;
        }

        //check for lock
        for (int i = 0; i < 6; i++)
        {
            if (CubeArray[i].GetComponent<MeshRenderer>().renderer.material.color != color)
            {
                return;
            }
        }

        int curTeamIndex = -1;
        List<Color> colors = GameManager.Instance.colors;

        for (int i = 0; i < colors.Count; i++)
        {
            if (colors[i].Equals(color))
            {
                curTeamIndex = i;
            }
        }

        //GameManager.Instance.UpdateCubeLock(curTeamIndex);

        for (int i = 0; i < 6; i++)
        {
            CubeArray[i].GetComponent<ChunkSide>().locked = true;
        }

    }
}
