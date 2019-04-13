using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunRotation : MonoBehaviour
{

    public float AnglesPerSecond;
    public float NightStartAngle;
    public float NightEndAngle;

    public bool isNight
    {
        get
        {
            // between 220 and 330
            return transform.rotation.eulerAngles.x > NightStartAngle && 
                   transform.rotation.eulerAngles.x < NightEndAngle;
        }
    }

    // Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.rotation *= Quaternion.AngleAxis(AnglesPerSecond * Time.deltaTime, Vector3.right);
        Debug.Log(isNight);
	}
}
