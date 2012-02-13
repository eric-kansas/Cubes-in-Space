using UnityEngine;
using System.Collections;

public class SpawnPoint : MonoBehaviour {
	
	public Vector3 lookAt;
	
	
    void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "Spawn.tif");
    }
}
