using UnityEngine;
using System.Collections;

public class ArenaColorChanger : MonoBehaviour {

    public Material ArenaMaterial;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        float shininess = ((Mathf.Sin(Time.time)/32)+ 0.03125f);
        ArenaMaterial.SetFloat("_Shininess", shininess);
	}
}
