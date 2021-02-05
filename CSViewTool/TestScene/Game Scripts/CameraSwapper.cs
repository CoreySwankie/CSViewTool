/// 
/// Created By Corey Swankie as part of dissertation project "FPS Multiplayer Game Development – Map Development Tools: Lines of Sight"
/// Contact at University email: 1402382@abertay.ac.uk
/// or personal email: thatscorey@gmail.com
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwapper : MonoBehaviour
{
    [SerializeField]
    protected Camera godCamera = null;

    [SerializeField]
    protected List<Camera> allCameras = new List<Camera>();

    protected int currentCamID = 0;

    private void Start()
    {
        currentCamID = 0;

        // if there is already a god camera set, set it to the first entry in the all cameras list
        if (godCamera)
        {
            allCameras.Add(godCamera);
        }

        // set up cameras
        // find cameras
        Camera[] foundCameras = FindObjectsOfType<Camera>();
        foreach (Camera foundCamera in foundCameras)
        {
            if (!allCameras.Contains(foundCamera))
            {
                allCameras.Add(foundCamera);
            }
        }
        foundCameras = null;

        DisableAllCameras();

        // if there is a god camera set it as active
        if (godCamera)
        {
            godCamera.enabled = true;
        }
        // if no god camera set first found camera as active
        else if(allCameras.Count > 0)
        {
            allCameras[0].enabled = true;
        }
        // no cameras throw an error
        else
        {
           //UnityEngine.Debug.LogError("No Camera found on the scene");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            CycleCameras();
        }
    }

    protected void CycleCameras()
    {
        if (allCameras.Count > 0)
        {
            currentCamID++;
            if (currentCamID >= allCameras.Count)
            {
                currentCamID = 0;
            }

            // disable all cameras
            DisableAllCameras();

            // enable the specific camera
            allCameras[currentCamID].enabled = true;
        }



    }

    protected void DisableAllCameras()
    {
        // disable all cameras
        foreach (Camera disableCamera in allCameras)
        {
            disableCamera.enabled = false;
        }
    }
}
