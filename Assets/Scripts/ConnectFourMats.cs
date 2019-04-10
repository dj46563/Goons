using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectFourMats : MonoBehaviour
{
    private ConnectFour parent;
    public int PlayerNumber;

    void Start()
    {
        parent = transform.parent.GetComponent<ConnectFour>();
    }

    void OnTriggerEnter(Collider col)
    {
        parent.OnChildTriggerEnter(col, PlayerNumber);
    }

    void OnTriggerExit(Collider col)
    {
        parent.OnChildTriggerExit(col);
    }
}
