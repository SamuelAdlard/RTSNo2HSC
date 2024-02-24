using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class CameraMovement : MonoBehaviour
{
    //Network manager
    public NetworkManager NetworkManager;
    //records the location of where the camera should be looking
    float yRotation = 0f;
    //sensitivity
    public float Mousesensitivity = 600f;
    //The movement speed of the camera
    public float horizontalSpeed = 4f;
    
    void FixedUpdate()
    {
        //Checks if the player is holding right-click
        if (Input.GetMouseButton(1))
        {
            //Gets the location of the mouse on the X axis
            float MouseX = Input.GetAxis("Mouse X") * Mousesensitivity * Time.deltaTime;

            //updates the player rotation variable
            yRotation -= MouseX;
            //Updates the the rotation
            transform.localRotation = Quaternion.Euler(0 , transform.localRotation.y + -yRotation, 0f);
        }
        
        //gets movement left and right
        float X = Input.GetAxis("Horizontal");
        //gets movement forward and backward
        float Z = Input.GetAxis("Vertical");
        //changes the location of the camera
        transform.Translate(new Vector3(X, 0, Z) * horizontalSpeed * Time.deltaTime);

    }
}
