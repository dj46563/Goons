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
            // If this is true the player has moved across the portal
            if (dotProduct < 0)
	        {
                // Teleport!

                // Calculate the rotation difference and apply it
	            // Get the difference in rotation from exit to entrance
	            Quaternion portalRotationDifference = Reciever.rotation * Quaternion.Inverse(transform.rotation);
	            // Rotate that angle 180 degrees on the up axis because my portals face each other
	            portalRotationDifference = portalRotationDifference * Quaternion.AngleAxis(180f, Vector3.up);
                //player.Rotate(Vector3.up, 180f - rotationDiff);
	            player.rotation = player.rotation * portalRotationDifference;

	            Vector3 positionOffset = portalRotationDifference * portalToPlayer;
	            Vector3 sourceToReceiver = Reciever.position - transform.position;
                player.position = player.position + positionOffset + sourceToReceiver;
	            //player.position = player.position + sourceToReceiver;

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
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null && player.networkObject.IsOwner)
        {
            playerIsOverlapping = false;
        }
    }
}
