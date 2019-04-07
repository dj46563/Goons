using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BeardedManStudios.Forge.Networking.Generated;

public class Player : PlayerBehavior
{

    private float speed = 5f;
    private float mouseSensitivity = 150f;
    private float xAxisClamp;

    public string Name { get; private set; }
    public GameObject MyCamera;
    public bool IsOwner
    {
        get { return networkObject.IsOwner; }
    }

    protected override void NetworkStart()
    {
        base.NetworkStart();

        if (networkObject.IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            MyCamera.gameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (networkObject.IsOwner)
        {
            Vector3 translation = new Vector3();
            translation += transform.forward * Input.GetAxisRaw("Vertical");
            translation += transform.right * Input.GetAxisRaw("Horizontal");
            transform.position += translation * speed * Time.deltaTime;

            CameraRotation();

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
}