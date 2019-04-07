using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nametag : MonoBehaviour
{
    public Player Target = null;

	// Use this for initialization
	void Start () {
        // Find the player that is the owner
	    foreach (Object obj in FindObjectsOfType(typeof(Player)))
	    {
	        Player player = obj as Player;
	        if (player != null && player.IsOwner)
	        {
	            Target = player;
	            break;
	        }
	    }
	}
	
	// Point at that player
	void Update () {
	    if (Target != null)
	    {
	        Vector3 lookDirection = Target.MyCamera.transform.position - transform.position;
	        transform.forward = -lookDirection;
	    }
	}
}
