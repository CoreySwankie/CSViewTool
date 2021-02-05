/// 
/// Created By Corey Swankie as part of dissertation project "FPS Multiplayer Game Development – Map Development Tools: Lines of Sight"
/// Contact at University email: 1402382@abertay.ac.uk
/// or personal email: thatscorey@gmail.com
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotator : MonoBehaviour
{
    [SerializeField]
    protected float rotationSpeed = 10.0f;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newRotation = gameObject.transform.eulerAngles;
        newRotation.y += rotationSpeed * Time.deltaTime;
        gameObject.transform.eulerAngles = newRotation;
    }
}
