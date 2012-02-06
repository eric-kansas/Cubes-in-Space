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
                GameObject side;
                switch (i)
                {
                    case 0:
                        side = (GameObject)Instantiate(sidePrefab, transform.position + new Vector3(0f, 0f, 2.61f), Quaternion.identity);
                        side.transform.parent = transform;
                        break;
                    case 1:
                        side = (GameObject)Instantiate(sidePrefab, transform.position + new Vector3(2.61f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f));
                        side.transform.parent = transform;
                        break;
                    case 2:
                        side = (GameObject)Instantiate(sidePrefab, transform.position + new Vector3(0f, 0f, -2.61f), Quaternion.Euler(0f, 180f, 0f));
                        side.transform.parent = transform;
                        break;
                    case 3:
                        side = (GameObject)Instantiate(sidePrefab, transform.position + new Vector3(-2.61f, 0f, 0f), Quaternion.Euler(0f, 270f, 0f));
                        side.transform.parent = transform;
                        break;
                    case 4:
                        side = (GameObject)Instantiate(sidePrefab, transform.position + new Vector3(0f, 2.61f, 0f), Quaternion.Euler(270f, 0f, 0f));
                        side.transform.parent = transform;
                        break;
                    case 5:
                        side = (GameObject)Instantiate(sidePrefab, transform.position + new Vector3(0f, -2.61f, 0f), Quaternion.Euler(90f, 0f, 0f));
                        side.transform.parent = transform;
                        break;
                }
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
