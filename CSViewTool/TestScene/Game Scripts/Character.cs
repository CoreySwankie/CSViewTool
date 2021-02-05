/// 
/// Created By Corey Swankie as part of dissertation project "FPS Multiplayer Game Development – Map Development Tools: Lines of Sight"
/// Contact at University email: 1402382@abertay.ac.uk
/// or personal email: thatscorey@gmail.com
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Character: MonoBehaviour
{
    [Header("Defualt Character Variables")]
    [SerializeField]
    [Range(0.0f, 100.0f)]
    protected float movementSpeed = 5.0f;

    [SerializeField]
    [Range(0.5f, 180.0f)]
    protected float rotationSpeed = 25.0f;

    [SerializeField]
    protected Camera characterCamera = null;
    public Camera CharacterCamera
    {
        get
        {
            return characterCamera;
        }
    }

    protected Collider characterCollider = null;
    public Collider CharacterCollider
    {
        get
        {
            return characterCollider;
        }
    }

    protected Rigidbody rb = null;
    
    protected virtual void Start()
    {
        characterCollider = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();

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
       
    protected virtual void OnCollisionEnter(Collision collision)
    {

    }

    protected virtual void OnTriggerEnter(Collider collider)
    {

    }
}
