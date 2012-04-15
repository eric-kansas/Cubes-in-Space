using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CubeChunk : MonoBehaviour {

    private List<GameObject> CubeArray;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void BindCubeArray(List<GameObject> array)
    {
        if (CubeArray == null)
        {
            CubeArray = array;
            for (int i = 0; i < CubeArray.Count; i++)
            {
                CubeArray[i].transform.parent = this.transform;
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
}
