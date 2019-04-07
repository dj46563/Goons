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

    private Rigidbody rigidbodyRef;
    private Collider colliderRef;

    public string Name { get; private set; }
    
    public GameObject MyCamera;
    public bool IsOwner
    {
        get { return networkObject.IsOwner; }
    }
    public TextMesh Nametag;

    protected override void NetworkStart()
    {
        base.NetworkStart();

        if (networkObject.IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            MyCamera.gameObject.SetActive(true);
            Name = PlayerPrefs.GetString("Name");
            networkObject.SendRpc(RPC_SET_NAME, Receivers.OthersBuffered, Name);
        }

        Nametag.text = Name;
        rigidbodyRef = GetComponent<Rigidbody>();
        colliderRef = GetComponent<Collider>();

        rigidbodyRef.velocity = Vector3.zero;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (networkObject.IsOwner)
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
            
            rigidbodyRef.MovePosition(transform.position + translation * speed * Time.deltaTime);
            //rigidbodyRef.velocity = translation * speed;

            // Camera movement with mouse
            CameraRotation();

            // Use key
            if (Input.GetKeyDown(KeyCode.E))
            {
                RaycastHit hit;
                if (Physics.Raycast(MyCamera.transform.position, MyCamera.transform.forward,
                    out hit, useRange))
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
        }
    }

    private void Update()
    {
        if (networkObject.IsOwner)
        {
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
        //transform.Rotate(Vector3.up * mouseX);
        Quaternion deltaRotation = Quaternion.Euler(Vector3.up * mouseX);
        rigidbodyRef.MoveRotation(rigidbodyRef.rotation * deltaRotation);
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
}