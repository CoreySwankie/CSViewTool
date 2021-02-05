/// 
/// Created By Corey Swankie as part of dissertation project "FPS Multiplayer Game Development – Map Development Tools: Lines of Sight"
/// Contact at University email: 1402382@abertay.ac.uk
/// or personal email: thatscorey@gmail.com
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : Character
{
    protected override void Start()
    {
        characterCollider = GetComponent<Collider>();

        if (characterCamera == null)
        {
            characterCamera = GetComponentInChildren<Camera>();
            // if no camera was found in the character throw an error
            if (characterCamera == null)
            {
               //UnityEngine.Debug.LogError("No camera attached to the character, add one and restart");
            }
        }
    }

    protected void Update()
    {
        UserInput();
    }

    private void UserInput()
    {

        #region Mouse Rotation // NOT IN USE
        ////// rotate character horizontal axis, x axis of mouse
        ////float yaw = transform.eulerAngles.y;
        ////yaw += (rotationSpeed * Time.deltaTime * Input.GetAxis("Mouse X")); ;
        ////transform.eulerAngles = new Vector3(transform.rotation.eulerAngles.x, yaw, transform.rotation.eulerAngles.z);

        ////// rotate camera vertical axis, y axis of mouse
        ////float pitch = characterCamera.transform.eulerAngles.x;
        //////invert the input so that moving the mouse up looks up, moving mouse down looks down
        ////pitch -= (rotationSpeed * Time.deltaTime * Input.GetAxis("Mouse Y"));
        ////characterCamera.transform.eulerAngles = new Vector3(pitch, characterCamera.transform.eulerAngles.y, characterCamera.transform.eulerAngles.z);
        #endregion

        #region Input Rotation // IN USE
        float yaw = transform.eulerAngles.y;

        if (Input.GetKey(KeyCode.Q))
        {
            yaw -= (rotationSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.E))
        {
            yaw += (rotationSpeed * Time.deltaTime);
        }

        transform.eulerAngles = new Vector3(transform.rotation.eulerAngles.x, yaw, transform.rotation.eulerAngles.z);
        #endregion

        // move left
        if (Input.GetKey(KeyCode.A))
        {
            transform.position += transform.right * (-movementSpeed * Time.deltaTime);
        }

        // move right
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right * (movementSpeed * Time.deltaTime);
        }

        // move forward
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * (movementSpeed * Time.deltaTime);
        }

        // move backwards
        if (Input.GetKey(KeyCode.S))
        {
            transform.position += transform.forward * (-movementSpeed * Time.deltaTime);
        }

        // pause the editor
        if (Application.isEditor && Input.GetKey(KeyCode.F1))
        {
            UnityEngine.Debug.Break();
        }

    }

    protected override void OnCollisionEnter(Collision collision)
    {

    }

    protected override void OnTriggerEnter(Collider collider)
    {

    }

}
