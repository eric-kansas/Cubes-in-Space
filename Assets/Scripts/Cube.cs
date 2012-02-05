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
        if (sidePrefab)
        {
            for (int i = 0; i < 6; i++)
            {
                Debug.Log("building side " + i);
            }
        }
        else
        {
            Debug.LogError("No sidePrefab given");
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
