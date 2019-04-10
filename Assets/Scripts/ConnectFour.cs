using System.Collections;
using System.Collections.Generic;
using BeardedManStudios.Forge.Networking;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;

public class ConnectFour : ConnectFourBehavior
{

    public Transform LeftSpawn;
    public GameObject Player1Piece;
    public GameObject Player2Piece;

    private int turn;
    private int inTrigger; // 0 if they are not in either, 1 for player 1, 2 for player 2

    private Dictionary<KeyCode, int> inputs = new Dictionary<KeyCode, int>()
    {
        {KeyCode.Alpha1, 1 },
        {KeyCode.Alpha2, 2 },
        {KeyCode.Alpha3, 3 },
        {KeyCode.Alpha4, 4 },
        {KeyCode.Alpha5, 5 },
        {KeyCode.Alpha6, 6 },
        {KeyCode.Alpha7, 7 },
    };

	// Use this for initialization
	void Start ()
	{
	    turn = 1;
	    inTrigger = 0;
	}
	
	// Update is called once per frame
	void Update () {
        // Check each of the inputs in the dictionary
	    foreach (var pair in inputs)
	    {
            // If one of those keys are pressed and the player is in the correct zone and turn
	        if (Input.GetKeyDown(pair.Key) && turn == inTrigger)
	        {
                // Then tell everyone to spawn a piece
	            bool player1Turn = turn == 1 ? true : false; // convert int turn to a bool
	            turn = turn == 1 ? 2 : 1; // switch the turns :)
	            int position = turn == 1 ? pair.Value : 7 - pair.Value; // reverse controls for player 2
                networkObject.SendRpc(RPC_PLACE_PIECE, Receivers.AllBuffered, player1Turn, pair.Value);
	        }
	    }
	}

    public void OnChildTriggerEnter(Collider col, int playerNumber)
    {
        Player playerRef = col.GetComponent<Player>();
        if (playerRef != null)
        {
            inTrigger = playerNumber;
        }
    }

    public void OnChildTriggerExit(Collider col)
    {
        Player playerRef = col.GetComponent<Player>();
        if (playerRef != null)
        {
            inTrigger = 0;
        }
    }

    // Returns the spawn location for the given horizontal index
    private Transform GetSpawn(int position)
    {
        GameObject emptyGO = new GameObject();
        Transform newTransform = emptyGO.transform;
        float boardOffset = -6.50f;
        newTransform.position = LeftSpawn.position + transform.rotation * new Vector3(0, 0, boardOffset * (position - 1));
        newTransform.rotation = LeftSpawn.rotation;
        return newTransform;
    }

    public override void PlacePiece(RpcArgs args)
    {
        bool player1Turn = args.GetNext<bool>();
        int position = args.GetNext<int>();
        GameObject objectToCreate = player1Turn ? Player1Piece : Player2Piece;
        Instantiate(objectToCreate, GetSpawn(position), false);
    }

    public override void ResetPieces(RpcArgs args)
    {
        throw new System.NotImplementedException();
    }
}
