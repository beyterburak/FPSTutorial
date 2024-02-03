using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 400f;
    [SerializeField] private float xRotation = 0f;
    [SerializeField] private float yRotation = 0f;

    [SerializeField] float topClamp = -90f;
    [SerializeField] float bottomClamp = 90f;
    void Start()
    {
        //Locking the cursor to the middle of the screen and making it invisible.
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        //Getting the mouse inputs
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        //Rotation around the x axis (Looking up and down)
        xRotation -= mouseY;

        //Clamp the rotation (90 dereceye kitledi)
        xRotation = Mathf.Clamp(xRotation, topClamp, bottomClamp);

        //Rotation around the y axis (Looking right and left)
        yRotation += mouseX;

        //Apply rotations to our transform
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }
}
