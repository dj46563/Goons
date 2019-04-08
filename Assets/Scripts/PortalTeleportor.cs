using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTeleportor : MonoBehaviour
{
    public Transform Reciever;
    public GameLogic GameLogicRef;

    private bool playerIsOverlapping = false;

	// Use this for initialization
	void Start ()
	{
	    GameLogicRef = FindObjectOfType<GameLogic>();
	}
	
	// Update is called once per frame
	void Update ()
	{
	    Transform player = GameLogicRef.PlayerObject.transform;

	    if (playerIsOverlapping)
	    {
            // Use the dot product to determine exactly when we finish going through
            // and to make sure we are entering in the correct direction
	        Vector3 portalToPlayer = player.position - transform.position;
	        float dotProduct = Vector3.Dot(transform.up, portalToPlayer);
	        Debug.Log("Dot: " + dotProduct.ToString());
            // If this is true the player has moved across the portal
            if (dotProduct < 0)
	        {
                // Teleport!
                Debug.Log("Teleport");

                // Calculate the rotation difference and apply it
	            float rotationDiff = -Quaternion.Angle(transform.rotation, Reciever.rotation);
                player.Rotate(Vector3.up, 180f - rotationDiff);

	            Vector3 positionOffset = Quaternion.Euler(0f, rotationDiff, 0f) * portalToPlayer;
	            player.position = Reciever.position + positionOffset;

	            playerIsOverlapping = false;
            }
	        
        }
	}

    void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null && player.networkObject.IsOwner)
        {
            // Only set the overlapping if they entered from the correct side
            Vector3 portalToPlayer = player.transform.position - transform.position;
            float dotProduct = Vector3.Dot(transform.up, portalToPlayer);
            if (dotProduct > 0)
            {
                playerIsOverlapping = true;
                Debug.Log("Trigger enter");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null && player.networkObject.IsOwner)
        {
            playerIsOverlapping = false;
            Debug.Log("Trigger exit");
        }
    }
}
