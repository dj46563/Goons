using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunRotation : MonoBehaviour
{

    public float AnglesPerSecond;

    private Light light;
    private float dot;

    public bool isNight
    {
        get
        {
            // between 220 and 330
            return dot > -0.2;
        }
    }

    // Use this for initialization
	void Start ()
	{
	    light = GetComponent<Light>();
	}
	
	// Update is called once per frame
	void Update ()
	{
        // Rotate the directional light on its x axis
        transform.Rotate(Vector3.right, Time.deltaTime * AnglesPerSecond);
	    dot = Vector3.Dot(transform.forward, Vector3.up);

        // Lower the intensity to zero at night so there is no weird lighting on vertical objects
	    light.intensity = Mathf.Clamp(Mathf.Sin(-dot * Mathf.PI / 2) + 0.5f, 0, 1);
	}
}
