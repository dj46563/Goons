using System.Collections;
using System.Collections.Generic;
using BeardedManStudios.Forge.Networking;
using UnityEngine;
using UnityEngine.UI;

using BeardedManStudios.Forge.Networking.Generated;

public class Player : PlayerBehavior
{

    private float speed = 50f;
    private float mouseSensitivity = 150f;
    private float xAxisClamp;
    private float useRange = 30f;

    private CharacterController ccRef;
    private Collider colliderRef;

    public string Name { get; private set; }
    
    public GameObject MyCamera;
    public bool IsOwner
    {
        get { return networkObject.IsOwner; }
    }
    public TextMesh Nametag;

    private bool isOwner;

    protected override void NetworkStart()
    {
        base.NetworkStart();

        isOwner = networkObject.IsOwner;

        if (isOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            MyCamera.gameObject.SetActive(true);
            Name = PlayerPrefs.GetString("Name");
            networkObject.SendRpc(RPC_SET_NAME, Receivers.OthersBuffered, Name);
            Destroy(transform.GetComponentInChildren<Nametag>().gameObject); 
        }

        Nametag.text = Name;
        
        ccRef = GetComponent<CharacterController>();
        colliderRef = ccRef.GetComponent<Collider>();
    }

    public void CalculateMovement()
    {
        Vector3 translation = Vector3.zero;

        translation += transform.forward * Input.GetAxisRaw("Vertical");
        translation += transform.right * Input.GetAxisRaw("Horizontal");
        translation = translation.normalized;

        // Bit layer mask to ignore Player colliders on layer 8
        int layerMask = 1 << 8; // mask to only collide with player
        layerMask = ~layerMask; // inverse the mask
        // Use a capsule to see if I am on the ground
        if (!Physics.CheckCapsule(colliderRef.bounds.center, new Vector3(colliderRef.bounds.center.x,
            colliderRef.bounds.min.y - 0.1f, colliderRef.bounds.center.z), 0.18f, layerMask))
        {
            translation += Vector3.down * 2;
            Debug.Log("Not grounded");
        }

        ccRef.Move(translation * speed * Time.deltaTime);

        // Camera movement with mouse
        CameraRotation();
    }

    // Update is called once per frame
    void Update()
    {
        if (isOwner)
        {
            CalculateMovement();

            // Use key
            // Bit layer mask to ignore Player colliders on layer 8
            int layerMask = 1 << 8; // mask to only collide with player
            layerMask = ~layerMask; // inverse the mask
            if (Input.GetKeyDown(KeyCode.E))
            {
                RaycastHit hit;
                if (Physics.Raycast(MyCamera.transform.position, MyCamera.transform.forward,
                    out hit, useRange, layerMask))
                {
                    ColorShift colorShift = hit.collider.gameObject.GetComponent<ColorShift>();
                    if (colorShift != null)
                    {
                        Color color = colorShift.Color;
                        GetComponent<Renderer>().material.color = color;
                        networkObject.SendRpc(RPC_SET_COLOR, Receivers.OthersBuffered, color);
                    }
                }
            }

            networkObject.position = transform.position;
            networkObject.rotation = transform.rotation;
        }
        else
        {
            transform.position = networkObject.position;
            transform.rotation = networkObject.rotation;
        }
    }

    private void CameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xAxisClamp += mouseY;
        if (xAxisClamp > 90f)
        {
            xAxisClamp = 90f;
            mouseY = 0;
        }
        else if (xAxisClamp < -90f)
        {
            xAxisClamp = -90f;
            mouseY = 0;
        }

        MyCamera.transform.Rotate(Vector3.left * mouseY);
        transform.Rotate(Vector3.up * mouseX);
    }

    public override void SetName(RpcArgs args)
    {
        Name = args.GetNext<string>();
        Nametag.text = Name;
    }

    public override void SetColor(RpcArgs args)
    {
        GetComponent<Renderer>().material.color = args.GetNext<Color>();
    }

    public override void Teleport(RpcArgs args)
    {
        Vector3 position = args.GetNext<Vector3>();
        networkObject.positionInterpolation.current = position;
        networkObject.positionInterpolation.target = position;
    }
}