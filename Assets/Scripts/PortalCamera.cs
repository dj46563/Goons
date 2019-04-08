using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCamera : MonoBehaviour
{
    public Transform Portal1;
    public Transform Portal2;
    public Transform Player;
    public GameLogic GameLogicRef;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        Player = GameLogicRef.PlayerCamera.transform;
        if (Player != null)
        {
            Vector3 playerOffsetFromPortal1 = Player.position - Portal1.position;
            Vector3 playerOffsetFromPortal2 = Player.position - Portal2.position;
            Vector3 offsetFromPortal1ToPortal2 = Portal1.position - Portal2.position;

            float angularDifferenceBetweenPortals = Quaternion.Angle(Portal1.rotation, Portal2.rotation);
            Quaternion portalRotationDifference = Quaternion.AngleAxis(180f - angularDifferenceBetweenPortals, Vector3.up);
            Vector3 newCameraDirection = portalRotationDifference * Player.forward;

            transform.position = Portal2.position + playerOffsetFromPortal1;
            transform.rotation = Quaternion.LookRotation(newCameraDirection);
        }    
	}
}
