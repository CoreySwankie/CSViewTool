/// 
/// Created By Corey Swankie as part of dissertation project "FPS Multiplayer Game Development – Map Development Tools: Lines of Sight"
/// Contact at Personal email: thatscorey@gmail.com
/// Find more of my work at: CoreySwankie.com
///

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum HitID
{
    Miss,
    HitPlayer,
    HitObject
}

[System.Serializable]
public class CastInfo
{
    protected string rayName = "Cast # ";
    public string RayName
    {
        get
        {
            return rayName;
        }
        set
        {
            rayName = value;
        }
    }

    protected int rayID = 0;
    public int RayID
    {
        get
        {
            return rayID;
        }
        set
        {
            rayID = value;
        }
    }

    protected int horizontalID = 0;
    public int HorizontalID
    {
        get
        {
            return horizontalID;
        }
        set
        {
            horizontalID = value;
        }
    }

    protected int verticalID = 0;
    public int VerticalID
    {
        get
        {
            return verticalID;
        }
        set
        {
            verticalID = value;
        }
    }

    protected float horizontalAngle = 0.0f;
    public float HorizontalAngle
    {
        get
        {
            return horizontalAngle;
        }
        set
        {
            horizontalAngle = value;
        }
    }

    protected float verticalAngle = 0.0f;
    public float VerticalAngle
    {
        get
        {
            return verticalAngle;
        }
        set
        {
            verticalAngle = value;
        }
    }

    protected Vector3 castDirection = new Vector3();
    public Vector3 CastDirection
    {
        get
        {
            return castDirection;
        }
        set
        {
            castDirection = value;
        }
    }

    //protected bool hit = false;
    //public bool Hit
    //{
    //    get
    //    {
    //        return hit;
    //    }
    //    set
    //    {
    //        hit = value;
    //    }
    //}

    protected HitID hitID = HitID.Miss;
    public HitID HitID
    {
        get
        {
            return hitID;
        }
        set
        {
            hitID = value;
        }
    }

    protected Vector3 hitPosition = new Vector3();
    public Vector3 HitPosition
    {
        get
        {
            return hitPosition;
        }
        set

        {
            hitPosition = value;
        }
    }

    protected Collider hitCollider = null;
    public Collider HitCollider
    {
        get
        {
            return hitCollider;
        }
        set
        {
            hitCollider = value;

        }
    }

}

public struct CheckerInfo
{
    public Vector3 castOrigin;
    public Vector3 facingDirection;
    public CastInfo[,] castInfos;
    public int horizontalCasts;
    public int verticalCasts;
    public float horizontalFieldOfView;
    public float verticalFieldOfView;

    public CheckerInfo(Vector3 castOrigin, Vector3 facingDirection, CastInfo[,] castInfos, int horizontalCasts, int verticalCasts, float horizontalFieldOfView, float verticalFieldOfView)
    {
        this.castOrigin = castOrigin;
        this.facingDirection = facingDirection;
        this.castInfos = castInfos ?? throw new ArgumentNullException(nameof(castInfos));
        this.horizontalCasts = horizontalCasts;
        this.verticalCasts = verticalCasts;
        this.horizontalFieldOfView = horizontalFieldOfView;
        this.verticalFieldOfView = verticalFieldOfView;
    }
}

public class CS_RaycastChecker : MonoBehaviour
{
    [Header("Default Variables")]
    [SerializeField]
    protected bool raycasterActive = true;
    public bool RaycasterActive
    {
        get
        {
            return raycasterActive;
        }
    }

    [SerializeField]
    protected bool viewFrustumCheck = true;

    protected int raycasterID = 0;
    public int RaycasterID
    {
        get
        {
            return raycasterID;
        }
    }

    [Header("Override Variables")]
    [SerializeField]
    [Range(1.0f, 10000.0f)]
    protected float rayRange = 80.0f;

    [SerializeField]
    protected bool overridePrecision = false;

    [SerializeField]
    [Range(3, 500)]
    [Tooltip("Number of rays to check across the horizontal angle increases")]
    protected int overrideHorizontalPrecision = 100;

    [SerializeField]
    [Range(3, 500)]
    [Tooltip("Number of rays to check across the vertical angle increases")]
    protected int overrideVerticalPrecision = 100;

    [SerializeField]
    protected bool useOverrideFieldOfView = false;

    [SerializeField]
    [Range(1.0f, 180.0f)]
    protected float overrideVerticalFieldOfView = 90.0f;
    [SerializeField]
    [Range(1.0f, 180.0f)]
    protected float overrideHorizontalFieldOfView = 90.0f;

    [SerializeField]
    protected bool useOverrideSpecificSlice = false;
    [SerializeField]
    [Min(0)]
    protected int overrideSpecificSlice = 0;

    protected Camera connectedCamera = null;
    protected Plane[] camFrustumPlanes;
    protected List<Collider> objectsPotentiallyVisableToCamera = new List<Collider>();

    protected CastInfo[,] currentCastInfoArray;

    protected bool currentlyCasting = false;
    public bool CurrentlyCasting
    {
        get
        {
            return currentlyCasting;
        }
    }

    protected CS_ViewToolManager viewToolManager = null;
    protected CS_ToolUserSettings toolUserSettings = null;

    protected CS_ViewMeshGenerator viewMeshGenerator = null;
    public CS_ViewMeshGenerator ViewMeshGenerator
    {
        get
        {
            return viewMeshGenerator;
        }
    }

    [Header("Visable Colliders and Raycast Checkers")]
    [SerializeField]
    protected List<Collider> visableColliders = new List<Collider>();
    public List<Collider> VisableColliders
    {
        get
        {
            return visableColliders;
        }
    }
    [SerializeField]
    protected List<CS_RaycastChecker> visableRaycastCheckers = new List<CS_RaycastChecker>();
    public List<CS_RaycastChecker> VisableRaycastCheckers
    {
        get
        {
            return visableRaycastCheckers;
        }
    }
    [SerializeField]
    protected List<CS_RaycastChecker> raycastCheckersCanSeeMe = new List<CS_RaycastChecker>();
    public List<CS_RaycastChecker> RaycastCheckersCanSeeMe
    {
        get
        {
            return raycastCheckersCanSeeMe;
        }
    }


    public void SetupRaycastChecker(CS_ViewToolManager vtManager, int newID)
    {
        viewToolManager = vtManager;
        toolUserSettings = viewToolManager.ToolUserSettings;

        // name the raycast checker
        raycasterID = newID;

        SetupViewMeshGenerator(toolUserSettings.GetMeshObjectColor(raycasterID));

        // get a camera on the connected object
        if (!connectedCamera)
        {
            // check for connected camera
            connectedCamera = gameObject.GetComponent<Camera>();
            if (!connectedCamera)
            {
               //UnityEngine.Debug.Log("Couldnt find Camera on: (" + gameObject.name + ")");

                // check parents
                connectedCamera = gameObject.GetComponentInParent<Camera>();
                if (!connectedCamera)
                {
                   //UnityEngine.Debug.Log("Couldnt find Camera in parents of : (" + gameObject.name + ")");

                    // check children
                    connectedCamera = gameObject.GetComponentInChildren<Camera>();
                    if (!connectedCamera)
                    {
                       //UnityEngine.Debug.Log("Couldnt find Camera in children of : (" + gameObject.name + ")");

                        // check the parents children
                        connectedCamera = gameObject.transform.parent.GetComponentInChildren<Camera>();
                        if (!connectedCamera)
                        {
                            //UnityEngine.Debug.LogError("Couldnt find Camera in children of parent of: (" + gameObject.name + ")");
                        }
                    }
                }
            }
        }

        // get camera view frustum planes
        camFrustumPlanes = GeometryUtility.CalculateFrustumPlanes(connectedCamera);

        currentCastInfoArray = new CastInfo[,]
        {

        };

        currentlyCasting = false;
    }

    protected void SetupViewMeshGenerator(Color meshColor)
    {
        // create the view mesh generator object
        GameObject newViewMeshGeneratorGameObject = Instantiate(toolUserSettings.ViewMeshGeneratorPrefab);
        //newViewMeshGeneratorGameObject.transform.parent = gameObject.transform;
        newViewMeshGeneratorGameObject.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f); // make sure position is zeroed out

        viewMeshGenerator = newViewMeshGeneratorGameObject.GetComponent<CS_ViewMeshGenerator>();
        // if missing the view mesh generator script
        if (viewMeshGenerator == null)
        {
            //UnityEngine.Debug.LogError("Created View Mesh Generator Game Object is missing the View Mesh Generator Script");
        }
        // not missing view mess generator script
        else
        {
            // setup the view mesh generator
            viewMeshGenerator.SetupViewMeshGenerator(viewToolManager, this, meshColor);
        }
    }

    public void StartRaycastCheck()
    {
        // if raycaster is active and not currently casting
        if (raycasterActive && !currentlyCasting)
        {
            // update the view check frustrum info
            UpdateViewFrustumCheck();
            
            switch (viewToolManager.VTMMode)
            {
                case VTMMode.Default3D:
                    StartCoroutine(RaycastCheck3DNEW());
                    break;
                case VTMMode.Default2D:
                    StartCoroutine(RaycastCheck2DNEW());
                    break;
                case VTMMode.ConsolidateCast3D:
                    StartCoroutine(RaycastCheck3DNEW());
                    break;
                case VTMMode.IgnoreVertical2D:
                    StartCoroutine(RaycastCheckIgnoreVertical());
                    break;
                case VTMMode.ViewCentreCast3D:
                    // get the central slice 
                    int centreSlice = 0;
                    if (overridePrecision)
                    {
                        centreSlice = overrideVerticalPrecision / 2;
                    }
                    else
                    {
                        centreSlice = viewToolManager.VerticalPrecision / 2;
                    }
                    StartCoroutine(RaycastCheckSpecificSliceNEW(centreSlice));
                    break;
                case VTMMode.SpecificSlice2D:
                    StartCoroutine(RaycastCheckSpecificSliceNEW(viewToolManager.SpecificSlice));
                    break;
                case VTMMode.SpecificSliceOLD3D:
                    StartCoroutine(RaycastCheck3DNEW());
                    break;
                default:
                    break;
            }
        }
    }

    protected void UpdateViewFrustumCheck()
    {
        if (connectedCamera && viewFrustumCheck)
        {
            // clear the potential visable objects list
            objectsPotentiallyVisableToCamera.Clear();

            // calculate camera view frustum planes
            camFrustumPlanes = GeometryUtility.CalculateFrustumPlanes(connectedCamera);

            foreach (Collider potentialCollider in viewToolManager.AllSceneColliders)
            {
                // check if object can potentially be seen by the camera view frustum
                if (GeometryUtility.TestPlanesAABB(camFrustumPlanes, potentialCollider.bounds))
                {
                    // add to the list of potentially visable colliders
                    if (!objectsPotentiallyVisableToCamera.Contains(potentialCollider))
                    {
                        objectsPotentiallyVisableToCamera.Add(potentialCollider);
                    }
                }
            }

        }

    }

    // 3D field of view checking
    protected virtual IEnumerator RaycastCheck3DNEW()
    {
        // clear the current view mesh
        viewMeshGenerator.ClearMesh();

        // set casting started
        currentlyCasting = true;

        // set checking precisions
        int horizontalPrecision = viewToolManager.HorizontalPrecision;
        int verticalPrecision = viewToolManager.VerticalPrecision;
        if (overridePrecision)
        {
            horizontalPrecision = overrideHorizontalPrecision;
            verticalPrecision = overrideVerticalPrecision;
        }

        // adjust the current cast array to the size of the horizontal and vertical precisions
        currentCastInfoArray = new CastInfo[horizontalPrecision + 1, verticalPrecision + 1];

        // set checking field of view
        float checkVFOV = 0.0f;
        float checkHFOV = 0.0f;
        if (useOverrideFieldOfView)
        {
            checkVFOV = overrideVerticalFieldOfView;
            checkHFOV = overrideHorizontalFieldOfView;
        }
        // not overriding this specific value use the value in the view tool manager
        else
        {
            checkVFOV = connectedCamera.fieldOfView; // connected camera fov is the vertical angle size, need to convert it for horizontal via aspect ratio

            float radAngle = connectedCamera.fieldOfView * Mathf.Deg2Rad;
            float radHFOV = 2 * Mathf.Atan(Mathf.Tan(radAngle / 2) * connectedCamera.aspect);
            checkHFOV = radHFOV * Mathf.Rad2Deg;
        }


        // angle change
        float angleIncreaseHorizontal = 0.0f;
        float startingHorizontalAngle = 0.0f;
        float angleIncreaseVertical = 0.0f;
        float startingVerticalAngle = 0.0f;
        // check the angle mode
        switch (viewToolManager.AngleMode)
        {
            case AngleMode.ViewportPointPercentage:
                // calculate the angle increases
                angleIncreaseHorizontal = 1.0f / horizontalPrecision;
                angleIncreaseVertical = 1.0f / verticalPrecision;
                // starting and current angles can stay at 0.0f
                break;
            case AngleMode.ForwardToAngleConversion:
                // calculate the angle increases
                angleIncreaseHorizontal = checkHFOV / horizontalPrecision;
                angleIncreaseVertical = checkVFOV / verticalPrecision;

                // vertical starting angle adjusted from current facing direction by the checking fov
                startingVerticalAngle = GetVerticalAngleFromDirectionVector(connectedCamera.transform.forward) - (checkVFOV / 2.0f);
                // horizontal starting angle adjusted from current facing direction by the checking fov
                startingHorizontalAngle = GetHorizontalAngleFromDirectionVector(connectedCamera.transform.forward) + (checkHFOV / 2.0f);
                break;
            default:
                break;
        }

        // set the cast origin to the position of the camera
        Vector3 castOrigin = connectedCamera.transform.position;

        // clear current list of cast infos
        Array.Clear(currentCastInfoArray, 0, currentCastInfoArray.Length);

        // create a list of the new visable colliders
        List<Collider> newVisableColliders = new List<Collider>();
        // create a list of the new visable raycasters
        List<CS_RaycastChecker> newVisableRaycastCheckers = new List<CS_RaycastChecker>();

        int castCount = 0;
        for (int yCount = 0; yCount < (verticalPrecision + 1); yCount++)
        {
            for (int xCount = 0; xCount < (horizontalPrecision + 1); xCount++)
            {
                // proccess the raycast and create a cast info
                CastInfo newCastInfo = ProcessRaycast(castOrigin, castCount, xCount, yCount, startingHorizontalAngle, angleIncreaseHorizontal, startingVerticalAngle, angleIncreaseVertical);

                // check if the raycaster hit any new colliders
                if (newCastInfo.HitCollider != null)
                {
                    // add hit collider to the new visable list if its not already there
                    if (!newVisableColliders.Contains(newCastInfo.HitCollider))
                    {
                        // add new visable collider to the list of colliders
                        newVisableColliders.Add(newCastInfo.HitCollider);

                        // check if that collider was a raycast checker
                        CS_RaycastChecker foundRaycastChecker = CheckIfColliderHasARaycaster(newCastInfo.HitCollider);

                        // found raycaster isnt null and not in the new visable raycast checkers
                        if (foundRaycastChecker != null && !newVisableRaycastCheckers.Contains(foundRaycastChecker))
                        {
                            newVisableRaycastCheckers.Add(foundRaycastChecker);

                            // tell the found raycast checker that this raycast checker can see it
                            foundRaycastChecker.AddToCanSeeMeList(this);
                        }
                    }
                }                

                // add new cast info to the list of cast infos
                currentCastInfoArray[xCount, yCount] = newCastInfo;

                //// add a wait to test the scanning
                //yield return new WaitForSeconds(0.005f);

                castCount++;
            }
        }

        currentlyCasting = false;

        // send the cast data to the view mesh generator
        CheckerInfo infoToPass = new CheckerInfo();
        infoToPass.castOrigin = castOrigin;
        infoToPass.facingDirection = connectedCamera.transform.forward;
        infoToPass.castInfos = currentCastInfoArray;
        infoToPass.horizontalCasts = horizontalPrecision + 1;
        infoToPass.verticalCasts = verticalPrecision + 1;
        infoToPass.horizontalFieldOfView = checkHFOV;
        infoToPass.verticalFieldOfView = checkVFOV;

        // send the raycast info to the view mesh generator
        viewMeshGenerator.ReceiveRaycastData(infoToPass);

        // update the visable colliders list
        UpdateVisableLists(newVisableColliders, newVisableRaycastCheckers);

        yield return true;
    }

    // 3D field of view check, only casting on a specific layer
    protected virtual IEnumerator RaycastCheckSpecificSliceNEW(int specificSlice)
    {
        // clear the current view mesh
        viewMeshGenerator.ClearMesh();

        // set casting started
        currentlyCasting = true;

        // set the specific slice to check
        int finalSpecificSlice = specificSlice;
        if (useOverrideSpecificSlice && viewToolManager.VTMMode == VTMMode.SpecificSlice2D)
        {
            finalSpecificSlice = overrideSpecificSlice;
        }

        // set checking precisions
        int horizontalPrecision = viewToolManager.HorizontalPrecision;
        int verticalPrecision = viewToolManager.VerticalPrecision;
        if (overridePrecision)
        {
            horizontalPrecision = overrideHorizontalPrecision;
            verticalPrecision = overrideVerticalPrecision;
        }

        // adjust the current cast array to the size of the horizontal and vertical precisions
        currentCastInfoArray = new CastInfo[horizontalPrecision + 1, verticalPrecision + 1];

        // set checking field of view
        float checkVFOV = 0.0f;
        float checkHFOV = 0.0f;
        if (useOverrideFieldOfView)
        {
            checkVFOV = overrideVerticalFieldOfView;
            checkHFOV = overrideHorizontalFieldOfView;
        }
        // not overriding this specific value use the value in the view tool manager
        else
        {
            checkVFOV = connectedCamera.fieldOfView; // connected camera fov is the vertical angle size, need to convert it for horizontal via aspect ratio

            float radAngle = connectedCamera.fieldOfView * Mathf.Deg2Rad;
            float radHFOV = 2 * Mathf.Atan(Mathf.Tan(radAngle / 2) * connectedCamera.aspect);
            checkHFOV = radHFOV * Mathf.Rad2Deg;
        }

        // angle change
        float angleIncreaseHorizontal = 0.0f;
        float startingHorizontalAngle = 0.0f;
        float angleIncreaseVertical = 0.0f;
        float bottomVerticalAngle = 0.0f;
        // check the angle mode
        switch (viewToolManager.AngleMode)
        {
            case AngleMode.ViewportPointPercentage:
                // to have the viewport mode mimic the forward to angle set to 1 and have the angle be subtracted away instead of added when increasing angle
                // otherwise set the bottom vertical angle to 0.0f and keep the vertical increase angle positive
                // calculate the angle increases
                angleIncreaseHorizontal = 1.0f / horizontalPrecision;
                angleIncreaseVertical = -(1.0f / verticalPrecision);
                // starting and current angles can stay at 0.0f
                bottomVerticalAngle = 1.0f;
                break;
            case AngleMode.ForwardToAngleConversion:
                // calculate the angle increases
                angleIncreaseHorizontal = checkHFOV / horizontalPrecision;
                angleIncreaseVertical = checkVFOV / verticalPrecision;

                // vertical bottom angle adjusted from current facing direction by the checking fov
                // adjusting to the bottom of the vertical angle before adjusting to the specific angle later
                bottomVerticalAngle = GetVerticalAngleFromDirectionVector(connectedCamera.transform.forward) - (checkVFOV / 2.0f);
                // horizontal starting angle adjusted from current facing direction by the checking fov
                startingHorizontalAngle = GetHorizontalAngleFromDirectionVector(connectedCamera.transform.forward) + (checkHFOV / 2.0f);
                break;
            default:
                break;
        }

        // set the cast origin to the position of the camera
        Vector3 castOrigin = connectedCamera.transform.position;

        // clear current list of cast infos
        Array.Clear(currentCastInfoArray, 0, currentCastInfoArray.Length);

        // create a list of the new visable colliders
        List<Collider> newVisableColliders = new List<Collider>();
        // create a list of the new visable raycasters
        List<CS_RaycastChecker> newVisableRaycastCheckers = new List<CS_RaycastChecker>();

        int castCount = 0;
        for (int xCount = 0; xCount < (horizontalPrecision + 1); xCount++)
        {
            // proccess the raycast and create a cast info
            CastInfo newCastInfo = ProcessRaycast(castOrigin, castCount, xCount, finalSpecificSlice, startingHorizontalAngle, angleIncreaseHorizontal, bottomVerticalAngle, angleIncreaseVertical);

            // check the new raycast info for if they collided with an object
            if (newCastInfo.HitCollider != null)
            {
                // add hit collider to the new visable list if its not already there
                if (!newVisableColliders.Contains(newCastInfo.HitCollider))
                {
                    // add new visable collider to the list of colliders
                    newVisableColliders.Add(newCastInfo.HitCollider);

                    // check if that collider was a raycast checker
                    CS_RaycastChecker foundRaycastChecker = CheckIfColliderHasARaycaster(newCastInfo.HitCollider);

                    // found raycaster isnt null and not in the new visable raycast checkers
                    if (foundRaycastChecker != null && !newVisableRaycastCheckers.Contains(foundRaycastChecker))
                    {
                        newVisableRaycastCheckers.Add(foundRaycastChecker);

                        // tell the found raycast checker that this raycast checker can see it
                        foundRaycastChecker.AddToCanSeeMeList(this);
                    }
                }
            }

            // add new cast info to the list of cast infos
            currentCastInfoArray[xCount, 0] = newCastInfo;

            //// add a wait to test the scanning
            //yield return new WaitForSeconds(0.005f);

            castCount++;
        }
        
        // send the cast data to the view mesh generator
        CheckerInfo infoToPass = new CheckerInfo();
        infoToPass.castOrigin = castOrigin;
        infoToPass.facingDirection = connectedCamera.transform.forward;
        infoToPass.castInfos = currentCastInfoArray;
        infoToPass.horizontalCasts = horizontalPrecision + 1;
        infoToPass.verticalCasts = verticalPrecision + 1;
        infoToPass.horizontalFieldOfView = checkHFOV;
        infoToPass.verticalFieldOfView = checkVFOV;

        // send the raycast info to the view mesh generator
        viewMeshGenerator.ReceiveRaycastData(infoToPass);

        // update the visable colliders list
        UpdateVisableLists(newVisableColliders, newVisableRaycastCheckers);

        currentlyCasting = false;

        yield return true;
    }

    // 2d field of view checking
    protected IEnumerator RaycastCheck2DNEW()
    {
        // clear the current view mesh
        viewMeshGenerator.ClearMesh();

        // set casting started
        currentlyCasting = true;

        // set checking precisions
        int horizontalPrecision = viewToolManager.HorizontalPrecision;
        int verticalPrecision = viewToolManager.VerticalPrecision;
        if (overridePrecision)
        {
            horizontalPrecision = overrideHorizontalPrecision;
            verticalPrecision = overrideVerticalPrecision;
        }

        // adjust the current cast array to the size of the horizontal and vertical precisions
        currentCastInfoArray = new CastInfo[horizontalPrecision + 1, 1];

        // set checking field of view
        float checkVFOV = 0.0f;
        float checkHFOV = 0.0f;
        if (useOverrideFieldOfView)
        {
            checkVFOV = overrideVerticalFieldOfView;
            checkHFOV = overrideHorizontalFieldOfView;
        }
        // not overriding this specific value use the value in the view tool manager
        else
        {
            checkVFOV = connectedCamera.fieldOfView; // connected camera fov is the vertical angle size, need to convert it for horizontal via aspect ratio

            float radAngle = connectedCamera.fieldOfView * Mathf.Deg2Rad;
            float radHFOV = 2 * Mathf.Atan(Mathf.Tan(radAngle / 2) * connectedCamera.aspect);
            checkHFOV = radHFOV * Mathf.Rad2Deg;
        }

        // angle change
        float angleIncreaseHorizontal = 0.0f;
        float startingHorizontalAngle = 0.0f;
        float angleIncreaseVertical = 0.0f;
        float centredVerticalAngle = 0.0f;
        // check the angle mode
        switch (viewToolManager.AngleMode)
        {
            case AngleMode.ViewportPointPercentage:
                // calculate the angle increases
                angleIncreaseHorizontal = 1.0f / horizontalPrecision;
                angleIncreaseVertical = 1.0f / verticalPrecision;
                // starting and current angles can stay at 0.0f
                centredVerticalAngle = 0.5f; // 0.5f should be centred on the viewport
                break;
            case AngleMode.ForwardToAngleConversion:
                // calculate the angle increases
                angleIncreaseHorizontal = checkHFOV / horizontalPrecision;
                angleIncreaseVertical = checkVFOV / verticalPrecision;

                // vertical starting angle adjusted from current facing direction by the checking fov
                centredVerticalAngle = GetVerticalAngleFromDirectionVector(connectedCamera.transform.forward); // not adjusting for the check vfov will give the centre casting
                // horizontal starting angle adjusted from current facing direction by the checking fov
                startingHorizontalAngle = GetHorizontalAngleFromDirectionVector(connectedCamera.transform.forward) + (checkHFOV / 2.0f);
                break;
            default:
                break;
        }

        // set the cast origin to the position of the camera
        Vector3 castOrigin = connectedCamera.transform.position;

        // clear current list of cast infos
        Array.Clear(currentCastInfoArray, 0, currentCastInfoArray.Length);

        // create a list of the new visable colliders
        List<Collider> newVisableColliders = new List<Collider>();
        // create a list of the new visable raycasters
        List<CS_RaycastChecker> newVisableRaycastCheckers = new List<CS_RaycastChecker>();


        int castCount = 0;
        for (int xCount = 0; xCount < (horizontalPrecision + 1); xCount++)
        {
            // proccess the raycast and create a cast info
            CastInfo newCastInfo = ProcessRaycast(castOrigin, castCount, xCount, 0, startingHorizontalAngle, angleIncreaseHorizontal, centredVerticalAngle, 0.0f);

            // check if the raycaster hit any new colliders
            if (newCastInfo.HitCollider != null)
            {
                // add hit collider to the new visable list if its not already there
                if (!newVisableColliders.Contains(newCastInfo.HitCollider))
                {
                    // add new visable collider to the list of colliders
                    newVisableColliders.Add(newCastInfo.HitCollider);

                    // check if that collider was a raycast checker
                    CS_RaycastChecker foundRaycastChecker = CheckIfColliderHasARaycaster(newCastInfo.HitCollider);

                    // found raycaster isnt null and not in the new visable raycast checkers
                    if (foundRaycastChecker != null && !newVisableRaycastCheckers.Contains(foundRaycastChecker))
                    {
                        newVisableRaycastCheckers.Add(foundRaycastChecker);

                        // tell the found raycast checker that this raycast checker can see it
                        foundRaycastChecker.AddToCanSeeMeList(this);
                    }
                }
            }

            // add new cast info to the list of cast infos
            currentCastInfoArray[xCount, 0] = newCastInfo;

            //// add a wait to test the scanning
            //yield return new WaitForSeconds(0.005f);

            castCount++;
        }


        

        // send the cast data to the view mesh generator
        CheckerInfo infoToPass = new CheckerInfo();
        infoToPass.castOrigin = castOrigin;
        infoToPass.facingDirection = connectedCamera.transform.forward;
        infoToPass.castInfos = currentCastInfoArray;
        infoToPass.horizontalCasts = horizontalPrecision + 1;
        infoToPass.verticalCasts = verticalPrecision + 1;
        infoToPass.horizontalFieldOfView = checkHFOV;
        infoToPass.verticalFieldOfView = checkVFOV;

        // send the raycast info to the view mesh generator
        viewMeshGenerator.ReceiveRaycastData(infoToPass);

        // update the visable colliders list
        UpdateVisableLists(newVisableColliders, newVisableRaycastCheckers);

        currentlyCasting = false;

        yield return true;
    }

    // 2d field of view checking but ignoring vertical rotation
    protected IEnumerator RaycastCheckIgnoreVertical()
    {
        // clear the current view mesh
        viewMeshGenerator.ClearMesh();

        // set casting started
        currentlyCasting = true;

        // set checking precisions
        int horizontalPrecision = viewToolManager.HorizontalPrecision;
        int verticalPrecision = viewToolManager.VerticalPrecision;
        if (overridePrecision)
        {
            horizontalPrecision = overrideHorizontalPrecision;
            verticalPrecision = overrideVerticalPrecision;
        }

        // adjust the current cast array to the size of the horizontal and vertical precisions
        currentCastInfoArray = new CastInfo[horizontalPrecision + 1, verticalPrecision + 1];

        // set checking field of view
        float checkVFOV = 0.0f;
        float checkHFOV = 0.0f;
        if (useOverrideFieldOfView)
        {
            checkVFOV = overrideVerticalFieldOfView;
            checkHFOV = overrideHorizontalFieldOfView;
        }
        // not overriding this specific value use the value in the view tool manager
        else
        {
            checkVFOV = connectedCamera.fieldOfView; // connected camera fov is the vertical angle size, need to convert it for horizontal via aspect ratio

            float radAngle = connectedCamera.fieldOfView * Mathf.Deg2Rad;
            float radHFOV = 2 * Mathf.Atan(Mathf.Tan(radAngle / 2) * connectedCamera.aspect);
            checkHFOV = radHFOV * Mathf.Rad2Deg;
        }

        // angle change
        float angleIncreaseHorizontal = 0.0f;
        float startingHorizontalAngle = 0.0f;

        // calculate the angle increases
        angleIncreaseHorizontal = checkHFOV / horizontalPrecision;

        // horizontal starting angle adjusted from current facing direction by the checking fov
        startingHorizontalAngle = GetHorizontalAngleFromDirectionVector(connectedCamera.transform.forward) + (checkHFOV / 2.0f);


        // set the cast origin to the position of the camera
        Vector3 castOrigin = connectedCamera.transform.position;

        // clear current list of cast infos
        Array.Clear(currentCastInfoArray, 0, currentCastInfoArray.Length);

        // create a list of the new visable colliders
        List<Collider> newVisableColliders = new List<Collider>();
        // create a list of the new visable raycasters
        List<CS_RaycastChecker> newVisableRaycastCheckers = new List<CS_RaycastChecker>();

        int castCount = 0;
        for (int xCount = 0; xCount < (horizontalPrecision + 1); xCount++)
        {
            // create new cast info
            CastInfo newCastInfo = new CastInfo();
            newCastInfo.RayID = castCount;

            // fill out horizontal and vertical ids
            newCastInfo.HorizontalID = xCount;
            newCastInfo.VerticalID = 0;

            // set the current angles to the starting angles
            float currentHorizontalAngle = startingHorizontalAngle;


            // set up a new cast ray
            Ray castRay = new Ray();
            Vector3 rayDirection = new Vector3();

            // set the current horizontal angle of the ray to cast
            currentHorizontalAngle = startingHorizontalAngle - (xCount * angleIncreaseHorizontal);

            // get the ray direction by converting the current angles
            rayDirection = GetDirectionVectorFromAngleHorizontalOnly(currentHorizontalAngle) * rayRange;
            rayDirection.Normalize();

            // create a new ray starting from the origin heading in the now calculated ray direction
            castRay = new Ray(castOrigin, rayDirection);

            // update the cast infos ray direction
            newCastInfo.CastDirection = rayDirection;


            // set the cast info values of the horizontal and vertical angles
            newCastInfo.HorizontalAngle = currentHorizontalAngle;
            newCastInfo.VerticalAngle = 0;

            // create new hit information
            RaycastHit hit = new RaycastHit();
            // preform physics raycast
            if (Physics.Raycast(castRay, out hit, rayRange, toolUserSettings.GetLayerMask()))
            {
                // if collides with an object
                if (hit.collider)
                {
                    // convert hit position to direction
                    Vector3 hitDirection = hit.point - castOrigin;

                    // check if meant to show debug rays
                    if (viewToolManager.ShowDebugRaycast)
                    {
                        // draw ray to collided position
                       UnityEngine.Debug.DrawRay(castOrigin, hitDirection, Color.red, 2.0f);
                    }

                    // fill out cast info
                    // hit player
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer(toolUserSettings.PlayersLayerString))
                    {
                        newCastInfo.HitID = HitID.HitPlayer;
                    }
                    // hit object
                    else if (hit.collider.gameObject.layer == LayerMask.NameToLayer(toolUserSettings.WorldObjectsLayerString))
                    {
                        newCastInfo.HitID = HitID.HitObject;
                    }
                    newCastInfo.HitPosition = hit.point;
                    newCastInfo.HitCollider = hit.collider;

                    // add hit collider to the new visable list if its not already there
                    if (!newVisableColliders.Contains(newCastInfo.HitCollider))
                    {
                        // add new visable collider to the list of colliders
                        newVisableColliders.Add(newCastInfo.HitCollider);

                        // check if that collider was a raycast checker
                        CS_RaycastChecker foundRaycastChecker = CheckIfColliderHasARaycaster(newCastInfo.HitCollider);

                        // found raycaster isnt null and not in the new visable raycast checkers
                        if (foundRaycastChecker != null && !newVisableRaycastCheckers.Contains(foundRaycastChecker))
                        {
                            newVisableRaycastCheckers.Add(foundRaycastChecker);

                            // tell the found raycast checker that this raycast checker can see it
                            foundRaycastChecker.AddToCanSeeMeList(this);
                        }
                    }
                }

                

            }
            // didnt collide with checking objects
            else
            {
                // check if too show debug rays
                if (viewToolManager.ShowDebugRaycast)
                {
                    //// draw ray to max direction length.
                    UnityEngine.Debug.DrawRay(castOrigin, rayDirection * rayRange, Color.cyan, 2.0f);
                }

                // fill out cast info
                newCastInfo.HitID = HitID.Miss;
                // even though didnt hit, set hit position to the end of the ray cast
                newCastInfo.HitPosition = castOrigin + rayDirection * rayRange;// * rayRange;
                newCastInfo.HitCollider = null;
            }


            // update the cast ray info name for easier editor debugging
            newCastInfo.RayName = "Ray #" + newCastInfo.RayID + " Hit: " + newCastInfo.HitID;

            // add new cast info to the list of cast infos
            currentCastInfoArray[xCount, 0] = newCastInfo;

            //// add a wait to test the scanning
            //yield return new WaitForSeconds(0.005f);

            castCount++;
        }



        // send the cast data to the view mesh generator
        CheckerInfo infoToPass = new CheckerInfo();
        infoToPass.castOrigin = castOrigin;
        infoToPass.facingDirection = connectedCamera.transform.forward;
        infoToPass.castInfos = currentCastInfoArray;
        infoToPass.horizontalCasts = horizontalPrecision + 1;
        infoToPass.verticalCasts = verticalPrecision + 1;
        infoToPass.horizontalFieldOfView = checkHFOV;
        infoToPass.verticalFieldOfView = checkVFOV;

        // send the raycast info to the view mesh generator
        viewMeshGenerator.ReceiveRaycastData(infoToPass);

        // update the visable colliders list
        UpdateVisableLists(newVisableColliders, newVisableRaycastCheckers);

        currentlyCasting = false;

        yield return true;
    }

    protected CastInfo ProcessRaycast(Ray castRay, Vector3 castOrigin, Vector3 rayDirection, int castCount, int xCount, int yCount, float horizontalAngle, float verticalAngle)
    {
        // create new cast info
        CastInfo newCastInfo = new CastInfo();
        // fill out the cast info
        newCastInfo.RayID = castCount;

        // fill out horizontal and vertical ids
        newCastInfo.HorizontalID = xCount;
        newCastInfo.VerticalID = yCount;

        // set the cast info values of the horizontal and vertical angles
        newCastInfo.HorizontalAngle = horizontalAngle;
        newCastInfo.VerticalAngle = verticalAngle;

        // set the cast infos ray direction
        newCastInfo.CastDirection = rayDirection;

        // create new hit information
        RaycastHit hit = new RaycastHit();
        // preform physics raycast
        if (Physics.Raycast(castRay, out hit, rayRange, toolUserSettings.GetLayerMask()))
        {
            // if collides with an object
            if (hit.collider)
            {
                // convert hit position to direction
                Vector3 hitDirection = hit.point - castOrigin;

                // check if meant to show debug rays
                if (viewToolManager.ShowDebugRaycast)
                {
                    // draw ray to collided position
                    UnityEngine.Debug.DrawRay(castOrigin, hitDirection, Color.red, 2.0f);
                }

                // fill out cast info
                // hit player
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer(toolUserSettings.PlayersLayerString))
                {
                    newCastInfo.HitID = HitID.HitPlayer;
                }
                // hit object
                else if (hit.collider.gameObject.layer == LayerMask.NameToLayer(toolUserSettings.WorldObjectsLayerString))
                {
                    newCastInfo.HitID = HitID.HitObject;
                }
                newCastInfo.HitPosition = hit.point;
                newCastInfo.HitCollider = hit.collider;
                                
            }

        }
        // didnt collide with checking objects
        else
        {
            // check if too show debug rays
            if (viewToolManager.ShowDebugRaycast)
            {
                //// draw ray to max direction length.
                UnityEngine.Debug.DrawRay(castOrigin, rayDirection * rayRange, Color.cyan, 2.0f);
            }

            // fill out cast info
            newCastInfo.HitID = HitID.Miss;
            // even though didnt hit, set hit position to the end of the ray cast
            newCastInfo.HitPosition = castOrigin + rayDirection * rayRange;// * rayRange;
            newCastInfo.HitCollider = null;
        }

        // update the cast ray info name for easier editor debugging
        newCastInfo.RayName = "Ray #" + newCastInfo.RayID + " Hit: " + newCastInfo.HitID;

        return newCastInfo;
    }

    // cast a ray and fill out its information according to the provided information
    protected CastInfo ProcessRaycast(Vector3 castOrigin, int castCount, int xCount, int yCount, 
        float startingHorizontalAngle, float angleIncreaseHorizontal, float startingVerticalAngle, float angleIncreaseVertical)
    {
        // set the final vertical angle of the ray to cast
        float finalVerticalAngle = startingVerticalAngle + (yCount * angleIncreaseVertical);
        // set to 0 for now, will be worked out once the ray is ready to be created
        float finalHorizontalAngle = 0.0f;

        // set up a new cast ray
        Ray castRay = new Ray();
        Vector3 rayDirection = new Vector3();
        switch (viewToolManager.AngleMode)
        {
            case AngleMode.ViewportPointPercentage:
                // set the current horizontal angle of the ray to cast
                finalHorizontalAngle = startingHorizontalAngle + (xCount * angleIncreaseHorizontal);
                // using the unity camera class, convert the viewport point to a ray using the "angle" percentage of the screen
                castRay = connectedCamera.ViewportPointToRay(new Vector3(finalHorizontalAngle, finalVerticalAngle, 0.0f));
                // get the ray direction from the cast ray info now that the viewport point conversion has been done
                rayDirection = castRay.direction;
                break;
            case AngleMode.ForwardToAngleConversion:
                // set the current horizontal angle of the ray to cast
                finalHorizontalAngle = startingHorizontalAngle - (xCount * angleIncreaseHorizontal);

                // get the ray direction by converting the current angles
                rayDirection = GetDirectionVectorFromAngles(finalHorizontalAngle, finalVerticalAngle) * rayRange;
                rayDirection.Normalize();

                // create a new ray starting from the origin heading in the now calculated ray direction
                castRay = new Ray(castOrigin, rayDirection);
                break;
            default:
                break;
        }

        // create new cast info
        CastInfo newCastInfo = new CastInfo();
        // fill out the cast info
        newCastInfo.RayID = castCount;

        // fill out horizontal and vertical ids
        newCastInfo.HorizontalID = xCount;
        newCastInfo.VerticalID = yCount;

        // set the cast info values of the horizontal and vertical angles
        newCastInfo.HorizontalAngle = finalHorizontalAngle;
        newCastInfo.VerticalAngle = finalVerticalAngle;

        // set the cast infos ray direction
        newCastInfo.CastDirection = rayDirection;

        // create new hit information
        RaycastHit hit = new RaycastHit();
        // preform physics raycast
        if (Physics.Raycast(castRay, out hit, rayRange, toolUserSettings.GetLayerMask()))
        {
            // if collides with an object
            if (hit.collider)
            {
                // convert hit position to direction
                Vector3 hitDirection = hit.point - castOrigin;

                // check if meant to show debug rays
                if (viewToolManager.ShowDebugRaycast)
                {
                    // draw ray to collided position
                    UnityEngine.Debug.DrawRay(castOrigin, hitDirection, Color.red, 2.0f);
                }

                // fill out cast info
                // hit player
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer(toolUserSettings.PlayersLayerString))
                {
                    newCastInfo.HitID = HitID.HitPlayer;
                }
                // hit object
                else if (hit.collider.gameObject.layer == LayerMask.NameToLayer(toolUserSettings.WorldObjectsLayerString))
                {
                    newCastInfo.HitID = HitID.HitObject;
                }
                newCastInfo.HitPosition = hit.point;
                newCastInfo.HitCollider = hit.collider;

            }

        }
        // didnt collide with checking objects
        else
        {
            // check if too show debug rays
            if (viewToolManager.ShowDebugRaycast)
            {
                //// draw ray to max direction length.
                UnityEngine.Debug.DrawRay(castOrigin, rayDirection * rayRange, Color.cyan, 2.0f);
            }

            // fill out cast info
            newCastInfo.HitID = HitID.Miss;
            // even though didnt hit, set hit position to the end of the ray cast
            newCastInfo.HitPosition = castOrigin + rayDirection * rayRange;// * rayRange;
            newCastInfo.HitCollider = null;
        }

        // update the cast ray info name for easier editor debugging
        newCastInfo.RayName = "Ray #" + newCastInfo.RayID + " Hit: " + newCastInfo.HitID;

        return newCastInfo;
    }

    protected Vector3 GetDirectionVectorFromAngles(float hAngle, float vAngle)
    {
        float horizontalAngleInRadians = hAngle * (Mathf.PI / 180.0f);
        float verticalAngleInRadians = vAngle * (Mathf.PI / 180.0f);

        //Vector3 directionVector = new Vector3(Mathf.Cos(horizontalAngleInRadians), 0.0f, Mathf.Sin(horizontalAngleInRadians));
        Vector3 directionVector = new Vector3(Mathf.Cos(horizontalAngleInRadians), Mathf.Cos(verticalAngleInRadians), Mathf.Sin(horizontalAngleInRadians));

        return directionVector;
    }

    protected Vector3 GetDirectionVectorFromAngleHorizontalOnly(float hAngle)
    {
        float angleInRadians = hAngle * (Mathf.PI / 180.0f);

        Vector3 directionVector = new Vector3(Mathf.Cos(angleInRadians), 0.0f, Mathf.Sin(angleInRadians)); // horizontal
        
        return directionVector;
    }

    protected float GetHorizontalAngleFromDirectionVector(Vector3 direction)
    {
        // normalize the direction vector
        direction = direction.normalized;

        float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;

        if (angle < 0)
        {
            angle += 360.0f;
        }

        return angle;
    }

    protected float GetVerticalAngleFromDirectionVector(Vector3 direction)
    {
        // normalize the direction vector
        direction = direction.normalized;

        float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;

        if (angle < 0)
        {
            angle += 360.0f;
        }

        return angle;
    }

    public void ClearViewMesh()
    {
        if (viewMeshGenerator)
        {
            viewMeshGenerator.ClearMesh();
        }
    }

    public bool ActivateRaycastChecker(bool isActive)
    {
        raycasterActive = isActive;
        ClearViewMesh();
        return raycasterActive;
    }

    protected void ClearVisableLists()
    {
        visableColliders.Clear();
        visableRaycastCheckers.Clear();
        raycastCheckersCanSeeMe.Clear();
    }

    protected void UpdateVisableLists(List<Collider> newVisableCollidersList, List<CS_RaycastChecker> newVisableRaycastCheckersList)
    {
        // if there are previously visable raycast checkers
        if (visableRaycastCheckers.Count > 0)
        {
            // empty new visable raycasters list
            if (newVisableRaycastCheckersList.Count == 0)
            {
                // update all the previously visable raycast checkers that this raycast checker can no longer see them
                foreach (CS_RaycastChecker previouslyVisableRaycastChecker in visableRaycastCheckers)
                {
                    previouslyVisableRaycastChecker.RemoveFromCanSeeMeList(this);
                }
            }
            // new visable raycast checkers list is not empty
            else
            {
                // check for any previously visable raycast checkers that are not in the new list of raycast checkers
                foreach (CS_RaycastChecker previouslyVisableRaycastChecker in visableRaycastCheckers)
                {
                    // if previously visable raycast checker is not in the list of now visable raycast checkers
                    if (!newVisableRaycastCheckersList.Contains(previouslyVisableRaycastChecker))
                    {
                        // let the previously visable raycast checker know that it isnt visable to this raycaster any more
                        previouslyVisableRaycastChecker.RemoveFromCanSeeMeList(this);
                    }
                }
            }
        }   
        

        // replace visable lists with the new lists
        visableColliders = newVisableCollidersList;
        visableRaycastCheckers = newVisableRaycastCheckersList;

        //// check if any of visable raycast checkers that this can see can see this back
        //foreach (RaycastChecker visableRC in visableRaycastCheckers)
        //{
        //    // search the visable RCs list of visable RCs to see if it contains this raycast checker
        //    bool canSeeMe = visableRC.CanRCSeeMe(this);
        //    if (canSeeMe)
        //    {
        //        string debugMessage = "RC: " + gameObject.transform.parent.name + " & RC: " + visableRC.gameObject.transform.parent.name + " can see eachother";
        //        //UnityEngine.Debug.Log(debugMessage);
        //        viewToolManager.UpdateDebugMessageDisplay(debugMessage);
        //    }
        //}

    }

    public void AddToCanSeeMeList(CS_RaycastChecker raycastChecker)
    {
        // check if already in the list of raycast checkers that can see this raycast checker
        if (!raycastCheckersCanSeeMe.Contains(raycastChecker))
        {
            raycastCheckersCanSeeMe.Add(raycastChecker);
        }
    }

    public void RemoveFromCanSeeMeList(CS_RaycastChecker raycastChecker)
    {
        if (raycastCheckersCanSeeMe.Contains(raycastChecker))
        {
            raycastCheckersCanSeeMe.Remove(raycastChecker);
        }
    }

    protected CS_RaycastChecker CheckIfColliderHasARaycaster(Collider checkCollider)
    {
        CS_RaycastChecker foundRaycastChecker = null;

        // check through children for raycast checker
        foundRaycastChecker = checkCollider.GetComponentInChildren<CS_RaycastChecker>();
        // didnt find raycast checker in children
        if (!foundRaycastChecker)
        {
            foundRaycastChecker = checkCollider.GetComponentInParent<CS_RaycastChecker>();
            // didnt find raycast checker in parents
            if (!foundRaycastChecker)
            {
                //UnityEngine.Debug.Log("Didnt find Raycast Checker in parents or children of: " + checkCollider);
            }
        }

        return foundRaycastChecker;
    }

    public void UpdateViewMeshGeneratorMaterial()
    {
        viewMeshGenerator.UpdateMaterial();
    }

}
