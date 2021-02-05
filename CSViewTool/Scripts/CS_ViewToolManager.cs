/// 
/// Created By Corey Swankie as part of dissertation project "FPS Multiplayer Game Development – Map Development Tools: Lines of Sight"
/// Contact at Personal email: thatscorey@gmail.com
/// Find more of my work at: CoreySwankie.com
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum VTMMode
{
    Default3D, // 3D casting check, with all layers displayed
    Default2D, // 2D casting check, will check the centre of the view frustrum
    ConsolidateCast3D, // 3D casting check, consolidate the mesh to show all the hits in 2D
    IgnoreVertical2D, // 2D casting check, will ignore vertical rotation of the view frustrum
    ViewCentreCast3D, // 2/3D casting check, will only check and display the centre cast
    SpecificSlice2D, // 2D casting check, will only check the specific slice rather than raycasting on all then displaying just the specific slice
    SpecificSliceOLD3D // 3D casting check, will show the specified slice // OLD version, better to use the 2D version
}

public enum AngleMode
{
    ViewportPointPercentage, // using the precision values dividing up the viewport values and using camera.viewporttoray for ray cast calculation
    ForwardToAngleConversion // using a rudementary conversion for taking the forward vector of the camera, converting to eular angles and working out ray direction from that
}

[System.Serializable]
public class CS_ViewToolManager : MonoBehaviour
{
    [SerializeField]
    [HideInInspector]
    protected bool showGizmos = false;
    public bool ShowGizmos
    {
        get
        {
            return showGizmos;
        }
    }

    [SerializeField]
    [HideInInspector]
    protected bool showDebugRaycast = false;
    public bool ShowDebugRaycast
    {
        get
        {
            return showDebugRaycast;
        }
    }

    [SerializeField]
    [HideInInspector]
    protected bool constantCheck = false;
    public bool ConstantCheck
    {
        get
        {
            return constantCheck;
        }
    }

    [SerializeField]
    [HideInInspector]
    protected float clearMeshesTimerLength = 3.0f;
    protected float clearMeshesTimer = 0.0f;

    [SerializeField]
    [HideInInspector]
    protected bool onlyShowColliderHits = false;
    public bool OnlyShowColliderHits
    {
        get
        {
            return onlyShowColliderHits;
        }
    }

    [SerializeField]
    [HideInInspector]
    [Min(0)]
    protected int specificSlice = 0;
    public int SpecificSlice
    {
        get
        {
            return specificSlice;
        }
    }

    [SerializeField]
    [HideInInspector]
    protected int precisionMin = 2;
    [SerializeField]
    [HideInInspector]
    protected int precisionMax = 500;
    [SerializeField]
    [HideInInspector]
    protected int horizontalPrecision = 100;
    public int HorizontalPrecision
    {
        get
        {
            return horizontalPrecision;
        }
    }
    [SerializeField]
    [HideInInspector]
    protected int verticalPrecision = 100;
    public int VerticalPrecision
    {
        get
        {
            return verticalPrecision;
        }
    }

    [SerializeField]
    [HideInInspector]
    protected bool useVertexColorMode = false;
    public bool UseVertexColorMode
    {
        get
        {
            return useVertexColorMode;
        }
    }

    [SerializeField]
    [HideInInspector]
    protected VTMMode vTMMode = VTMMode.Default2D;
    public VTMMode VTMMode
    {
        get
        {
            return vTMMode;
        }
    }

    [SerializeField]
    [HideInInspector]
    protected AngleMode angleMode = AngleMode.ViewportPointPercentage;
    public AngleMode AngleMode
    {
        get
        {
            return angleMode;
        }
    }

    protected int currentRaycastCheckerIndex = 0;

    protected List<CS_RaycastChecker> raycastCheckers = new List<CS_RaycastChecker>();
    public List<CS_RaycastChecker> RaycastCheckers
    {
        get
        {
            return raycastCheckers;
        }
    }

    protected List<Collider> allSceneColliders = new List<Collider>();
    public List<Collider> AllSceneColliders
    {
        get
        {
            return allSceneColliders;
        }
    }

    [SerializeField]
    protected CS_ToolUserSettings toolUserSettings = null;
    public CS_ToolUserSettings ToolUserSettings
    {
        get
        {
            return toolUserSettings;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (toolUserSettings == null)
        {
           //UnityEngine.Debug.LogError("Tool User Setting Scriptable Object is missing");
            return;
        }


        // check for raycast checkers
        CS_RaycastChecker[] foundRaycastCheckers = FindObjectsOfType<CS_RaycastChecker>();
        if (foundRaycastCheckers.Length > 0)
        {
            foreach (CS_RaycastChecker foundRaycastChecker in foundRaycastCheckers)
            {
                // link and setup the raycasters
                LinkAndSetupRaycastChecker(foundRaycastChecker);
            }
        }

        // initial set of the vertex color mode
        UpdateRaycastCheckersVertexColorMode();
                  

        // get all scene colliders
        Collider[] foundColliders = FindObjectsOfType<Collider>();
        allSceneColliders =  new List<Collider>(foundColliders);



        // constant raycasting at the start
        if (constantCheck)
        {
            ConstantCheckChange();
        }
    }

    public void LinkAndSetupRaycastChecker(CS_RaycastChecker raycastChecker)
    {
        // check if in the list of raycast checkers
        if (!raycastCheckers.Contains(raycastChecker))
        {
            // add to raycast checkers list
            raycastCheckers.Add(raycastChecker);
        }

        // setup the raycast checker
        raycastChecker.SetupRaycastChecker(this, currentRaycastCheckerIndex);
        currentRaycastCheckerIndex++;
    }

    public void ConstantCheckChange()
    {
        // if now set to constantly check
        if (constantCheck)
        {
            // start constantly raycasting and updating the created meshes
            StartCoroutine(ConstantCastAndMeshUpdate());
        }
        // now set to false
        else
        {
            // clear all the meshes
            ClearAllRaycastCheckerMeshes();
        }
    }


    public void ActivateRaycastCheckers()
    {
        // if constantly checking raycasts and updating meshes
        // exit here
        if (constantCheck)
        {
            return;
        }

        // clear all the meshes
        ClearAllRaycastCheckerMeshes();

        // raycast from all raycast checkers, they will update their own raycast meshes
        RaycastCheckFromAllRaycastCheckers();

        // start clear mesh timer
        StartCoroutine(ClearMeshTimer());
    }

    protected IEnumerator ClearMeshTimer()
    {
        // set the clear meshes timer back to 0
        clearMeshesTimer = 0.0f;

        // while the clear meshes timer is less than the timer length
        while (clearMeshesTimer < clearMeshesTimerLength)
        {
            // increase the timer
            clearMeshesTimer += Time.deltaTime;
            yield return null;
        }

        // timer complete
        // clear the raycast meshes
        ClearAllRaycastCheckerMeshes();

        // exit enumerator
        yield return true;
    }

    protected void ClearAllRaycastCheckerMeshes()
    {
        // check there are raycast checkers
        if (raycastCheckers.Count > 0)
        {
            // clear all the meshes
            foreach (CS_RaycastChecker raycastChecker in raycastCheckers)
            {
                raycastChecker.ClearViewMesh();
            }
        }

    }

    protected void RaycastCheckFromAllRaycastCheckers()
    {
        // check that there is raycast checkers
        if (raycastCheckers.Count > 0)
        {
            // start raycasting from all raycasters
            foreach (CS_RaycastChecker raycastChecker in raycastCheckers)
            {
                raycastChecker.StartRaycastCheck();   
            }
        }

    }

    protected IEnumerator ConstantCastAndMeshUpdate()
    {
        ClearAllRaycastCheckerMeshes();

        // continue casting while in constant check mode
        while (constantCheck)
        {
            RaycastCheckFromAllRaycastCheckers();
            yield return null;
        }

        // no longer constantly casting, exit
        yield return true;
    }

    public void UpdateRaycastCheckersVertexColorMode()
    {
        // update all the mesh generators
        foreach (CS_RaycastChecker raycastChecker in raycastCheckers)
        {
            raycastChecker.UpdateViewMeshGeneratorMaterial();
        }
    }

}
