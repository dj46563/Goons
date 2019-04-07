using System.Collections;
using System.Collections.Generic;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;

public class GameLogic : GameLogicBehavior {

	// Use this for initialization
	void Start ()
	{
	    NetworkManager.Instance.InstantiatePlayer(position: new Vector3(0, 5, 0));
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
