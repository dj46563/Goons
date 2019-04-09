using System.Collections;
using System.Collections.Generic;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using UnityEngine;

public class PortalTeleportor : MonoBehaviour
{
    public Transform Reciever;
    public GameLogic GameLogicRef;

    private Player player;
    private bool playerIsOverlapping = false;

	// Use this for initialization
	void Start ()
	{
	    player = null;
	    GameLogicRef = FindObjectOfType<GameLogic>();
	}
	
	// Update is called once per frame
	void Update ()
	{
	    if (playerIsOverlapping)
	    {
	        Transform playerTransform = player.transform;

            // Use the dot product to determine exactly when we finish going through
            // and to make sure we are entering in the correct direction
            Vector3 portalToPlayer = playerTransform.position - transform.position;
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
	            playerTransform.rotation = playerTransform.rotation * portalRotationDifference;

                Vector3 positionOffset = portalRotationDifference * portalToPlayer;
                Vector3 sourceToReceiver = Reciever.position - transform.position;

	            playerTransform.position = Reciever.transform.position + positionOffset;
                player.networkObject.SendRpc(PlayerBehavior.RPC_TELEPORT, Receivers.Others, playerTransform.position);

                playerIsOverlapping = false;
            }  
        }
	}

    void OnTriggerEnter(Collider other)
    {
        player = other.GetComponent<Player>();
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
        player = other.GetComponent<Player>();
        if (player != null && player.networkObject.IsOwner)
        {
            playerIsOverlapping = false;
        }
    }
}
