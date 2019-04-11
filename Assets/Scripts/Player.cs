using System.Collections;
using System.Collections.Generic;
using BeardedManStudios.Forge.Networking;
using UnityEngine;
using UnityEngine.UI;

using BeardedManStudios.Forge.Networking.Generated;

public class Player : PlayerBehavior
{

    private float speed = 50f;
    private float mouseSensitivity = 1f;
    private float xAxisClamp;
    private float useRange = 30f;
    private float smoothing = 2.0f;
    private Vector2 smoothV;
    private Vector2 mouseLook;

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

    // Rotate the player and camera based on mouse movement
    private void CameraRotation()
    {
        // Get the change in mouse position on the x and y
        Vector2 mouseDirection = new Vector2(Input.GetAxis("Mouse X") * mouseSensitivity, Input.GetAxis("Mouse Y") * mouseSensitivity);
        // Multiply smoothing and mouse sensitivity
        mouseDirection = Vector2.Scale(mouseDirection, new Vector2(mouseSensitivity * smoothing, mouseSensitivity * smoothing));
        // Smoothly interpolate towards these values
        smoothV.x = Mathf.Lerp(smoothV.x, mouseDirection.x, 1f / smoothing);
        smoothV.y = Mathf.Lerp(smoothV.y, mouseDirection.y, 1f / smoothing);

        // Apply these changes to our rotation
        MyCamera.transform.localRotation *= Quaternion.AngleAxis(-smoothV.y, Vector3.right);
        transform.localRotation *= Quaternion.AngleAxis(smoothV.x, transform.up);
        // Clamp the camera rotation
        MyCamera.transform.localRotation = ClampRotationAroundXAxis(MyCamera.transform.localRotation);
    }

    // Function stolen from Unity's MouseLook.cs
    Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, -90f, 90f);
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
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