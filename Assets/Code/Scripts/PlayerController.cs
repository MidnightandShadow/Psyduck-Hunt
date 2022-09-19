using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Insert character controller")]
    private CharacterController controller;
    
    [FormerlySerializedAs("camera")]
    [SerializeField]
    [Tooltip("Insert main camera")]
    private Camera mainCamera;

    [SerializeField]
    private float speed = 10f;

    private Vector3 velocity;
    private float gravity = -9.82f;
    private float jumpHeight = 10f;
    private float groundCastDistance = 0.05f;
    private bool grounded;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Grab transforms
        Transform playerTransform = transform;
        Transform cameraTransform = mainCamera.transform;
        
        // grounded (ray-casting to see if on ground or not)
        // if (grounded)
        // {
        //     Debug.DrawRay(playerTransform.position, Vector3.down, Color.blue);
        // }
        // else
        // {
        //     Debug.DrawRay(playerTransform.position, Vector3.down, Color.red);
        // }

        grounded = Physics.Raycast(playerTransform.position, Vector3.down, groundCastDistance);
        
        // wasd/left right up down ground movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 movement = (playerTransform.right * x) + (playerTransform.forward * z);

        controller.Move(movement * (speed * Time.deltaTime));
        
        // Rotate player alongside camera
        playerTransform.rotation = Quaternion.AngleAxis(cameraTransform.rotation.eulerAngles.y, Vector3.up);
        
        // gravity and jumping
        velocity.y += gravity * Time.deltaTime;

        if (Input.GetButtonDown("Jump") && this.grounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight);
        }
        
        controller.Move(velocity * Time.deltaTime);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
        
    }
}
