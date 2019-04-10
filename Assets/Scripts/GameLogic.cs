using System.Collections;
using System.Collections.Generic;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

public class GameLogic : GameLogicBehavior {

    public GameObject PlayerCamera { get; private set; }
    public GameObject PlayerObject { get; private set; }

    private static GameLogic _instance;

    public static GameLogic Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameLogic>();
            }

            return _instance;
        }
    }

	// Use this for initialization
	void Start ()
	{
	    Player player = NetworkManager.Instance.InstantiatePlayer(position: new Vector3(0, 15, 0)) as Player;

        // Set the public properties for other objects to use
        PlayerObject = player.gameObject;
	    PlayerCamera = player.MyCamera;

        // Portal stuff
        GetComponent<PortalTextureSetup>().SetupPortalMaterial();

	    NetworkManager.Instance.Networker.playerDisconnected += PlayerDisconnected;
	}

    private void PlayerDisconnected(NetworkingPlayer networkingPlayer, NetWorker sender)
    {
        MainThreadManager.Run(() =>
        {
            // Destroy all players owned by the disonnected player
            foreach (var player in FindObjectsOfType<Player>())
            {
                if (player.networkObject.Owner == networkingPlayer)
                {
                    player.networkObject.Destroy();
                    return;
                }
            }
        }); 
    }
	
	// Update is called once per frame
	void FixedUpdate () {
	    // Portal stuff
        GetComponent<PortalTextureSetup>().SetupPortalMaterial();
    }
}
