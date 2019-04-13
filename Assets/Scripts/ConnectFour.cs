using System;
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
    public GameObject BottomCollider;
    public GameObject FourBoard;
    public GameObject NumbersText;
    public GameObject HelpText;

    public ICollection<GameObject> pieces;
    // Keeps tracks of existing pieces for win checking and instantiating pieces
    // for joining players. player1 = 1, player2 = 2, none = 0
    private int[,] pieceMatrix = new int[7,6];
    private Transform target;

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
        {KeyCode.R, 0 } // Reset button
    };

	// Use this for initialization
	void Start ()
	{
	    turn = 1;
	    inTrigger = 0;
        pieces = new List<GameObject>();
	    target = GameLogic.Instance.PlayerCamera.transform;
	}
	
	// Update is called once per frame
	void Update () {
        // Check each of the inputs in the dictionary
	    foreach (var pair in inputs)
	    {
            // If one of those keys are pressed and the player is in the correct zone and turn
	        if (Input.GetKeyDown(pair.Key) && inTrigger != 0)
	        {
                networkObject.SendRpc(RPC_SEND_INPUT, Receivers.Server, pair.Value, inTrigger);
	        }
	    }
	}

    public void OnChildTriggerEnter(Collider col, int playerNumber)
    {
        Player playerRef = col.GetComponent<Player>();
        if (playerRef != null && playerRef.IsOwner)
        {
            inTrigger = playerNumber;

            // Turn the help texts on
            if (inTrigger == 1)
            {
                NumbersText.transform.localRotation = Quaternion.AngleAxis(90f, Vector3.up);
                HelpText.transform.localRotation = Quaternion.AngleAxis(90f, Vector3.up);
            }
            else
            {
                NumbersText.transform.localRotation = Quaternion.AngleAxis(90f, Vector3.up) * Quaternion.AngleAxis(180f, Vector3.up);
                HelpText.transform.localRotation = Quaternion.AngleAxis(90f, Vector3.up) * Quaternion.AngleAxis(180f, Vector3.up);
            }
            NumbersText.SetActive(true);
            HelpText.SetActive(true);
        }
    }

    public void OnChildTriggerExit(Collider col)
    {
        Player playerRef = col.GetComponent<Player>();
        if (playerRef != null && playerRef.IsOwner)
        {
            inTrigger = 0;
            // Turn the help texts off
            NumbersText.SetActive(false);
            HelpText.SetActive(false);
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
        GameObject piece = Instantiate(objectToCreate, GetSpawn(position), false);
        pieces.Add(piece);
    }

    public override void ResetPieces(RpcArgs args)
    {
        if (BottomCollider != null)
        {
            // Release the position and rotation constraints on the piece's rigidbodies
            foreach (GameObject piece in pieces)
            {
                Rigidbody pieceRb = piece.GetComponent<Rigidbody>();
                pieceRb.constraints = RigidbodyConstraints.None;
            }

            // Spin the board!!
            StartCoroutine(Spin());
            StartCoroutine(FadePieces());
            Array.Clear(pieceMatrix, 0, pieceMatrix.Length);
        }
    }

    IEnumerator Spin()
    {
        Quaternion startRotation = FourBoard.transform.rotation;
        for (float f = 0; f < 1f; f += Time.deltaTime)
        {
            FourBoard.transform.Rotate(Vector3.forward, 360f * Time.deltaTime);
            yield return null;
        }

        FourBoard.transform.rotation = startRotation;
    }

    // Wait for 0.8seconds then fade all the pieces in a second
    // Then delete them all
    IEnumerator FadePieces()
    {
        yield return new WaitForSeconds(0.8f);
        for (float f = 0; f < 1; f += Time.deltaTime)
        {
            foreach (var piece in pieces)
            {
                Renderer renderer = piece.GetComponent<Renderer>();
                Color newColor = renderer.material.color;
                newColor.a -= Time.deltaTime;
                renderer.material.color = newColor;
            }

            yield return null;
        }

        foreach (var piece in pieces)
        {
            Destroy(piece);
        }
        pieces.Clear();
    }

    public override void SendInput(RpcArgs args)
    {
        if (networkObject.IsServer)
        {
            int code = args.GetNext<int>();
            int trigger = args.GetNext<int>();
            if (code == 0)
            {
                // Reset
                networkObject.SendRpc(RPC_RESET_PIECES, Receivers.All);
            }
            else if (trigger == turn)
            {
                // Attemp to place the piece in the piece matrix, if it works then continue
                if (PlacePieceInMatrix(code, trigger))
                {
                    int position = trigger == 1 ? code : 8 - code;
                    // Then tell everyone to spawn a piece
                    bool player1Turn = turn == 1 ? true : false; // convert int turn to a bool
                    networkObject.SendRpc(RPC_PLACE_PIECE, Receivers.All, player1Turn, position);
                    turn = turn == 1 ? 2 : 1; // switch the turns :)
                }
            } 
        }
    }

    // calculates where a piece will go in the matrix
    // given an x position (player 1 pov 1-7)
    public bool PlacePieceInMatrix(int xPosition, int player)
    {
        if (xPosition >= 1 && xPosition <= 7)
        {
            // Check all of the position on the y value of this array for a non zero
            for (int i = 0, value = 1; i < 6 && value != 0; i++)
            {
                value = pieceMatrix[xPosition, i];
                // Once a none zero is reached, place the piece in the matrix above it
                if (value == 0)
                {
                    pieceMatrix[xPosition, i] = player;
                    return true;
                }
            }
        }

        // If we got this far then a piece was not placed either because
        // there was no verical space or the position was invalid
        return false;
    }
}
