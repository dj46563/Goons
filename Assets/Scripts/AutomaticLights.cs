using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticLights : MonoBehaviour
{
    public SunRotation Sun;

    public Renderer EmissionRenderer;
    public Light Light;
	
	// Update is called once per frame
	void Update () {
	    if (Light != null && EmissionRenderer != null)
	    {
	        if (Sun != null && Sun.isNight)
	        {
	            Light.enabled = true;
                EmissionRenderer.material.EnableKeyword("_EMISSION");
	        }
	        else
	        {
	            Light.enabled = false;
	            EmissionRenderer.material.DisableKeyword("_EMISSION");
            }
        }
	}
}
