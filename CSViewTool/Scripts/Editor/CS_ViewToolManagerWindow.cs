/// 
/// Created By Corey Swankie as part of dissertation project "FPS Multiplayer Game Development – Map Development Tools: Lines of Sight"
/// Contact at Personal email: thatscorey@gmail.com
/// Find more of my work at: CoreySwankie.com
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CS_ViewToolManagerWindow : EditorWindow
{
    [SerializeField]
    protected CS_VTMEditorSettings vTMEditorSettings = null;

    public List<CS_RaycastChecker> raycastCheckers = new List<CS_RaycastChecker>();
    public List<CS_RaycastChecker> inactiveRaycastCheckers = new List<CS_RaycastChecker>();

    protected static string editorRootFilePath = "Assets/Plugin Files/Scripts/Editor";

    Vector2 scrollPos;

    protected CS_ViewToolManager viewToolManager = null;

    protected bool EditorApplicationRunning
    {
        get
        {
            if (Application.isEditor && Application.isPlaying)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    [MenuItem("Tools/CS View Tool Manager")]
    public static void ShowWindow()
    {
        GetWindow<CS_ViewToolManagerWindow>("CS View Tool Manager");
    }

    private void Awake()
    {
        // check for editor settings
        CheckForEditorSettings();
        // check for the scene view tool manager
        CheckForVTManager();
    }

    private void OnFocus()
    {
        // check for editor settings
        CheckForEditorSettings();
        // check for the scene view tool manager
        CheckForVTManager();

        FindRaycastCheckersOnScene();
    }

    private void CheckForEditorSettings()
    {
        if (!vTMEditorSettings)
        {
            Debug.LogError("View Tool Manager Window is missing a Editor Settings File, looking for Editor Settings file.");

            // locate file at specific location that holds editor files
            vTMEditorSettings = (CS_VTMEditorSettings)AssetDatabase.LoadAssetAtPath(editorRootFilePath + "/ToolEditorSettings.asset", typeof(CS_VTMEditorSettings));
            if (!vTMEditorSettings)
            {
                Debug.Log("No Editor Setting named 'ToolEditorSettings' found. Looking for any editor settings file in the root folder.");
                
                // not found a file named specifically "ToolEditorSettings.asset", look through the files in the editor files folder for any files of the VTMEditorSettings type
                string[] foundEditorSettingAssets = AssetDatabase.FindAssets("t:CS_VTMEditorSettings", new[] { editorRootFilePath });
                if (foundEditorSettingAssets.Length > 0)
                {
                    //foreach (string fesas in foundEditorSettingAssets)
                    //{
                    //    Debug.Log("Found asset ID: " + fesas);
                    //}
                    // set editor settings to the first found editor settings file in root folder
                    vTMEditorSettings = (CS_VTMEditorSettings)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(foundEditorSettingAssets[0]), typeof(CS_VTMEditorSettings));
                }
                else
                {
                    Debug.Log("No Editor Setting files found in the root folder");
                }
            }

            // still not found the file
            if (!vTMEditorSettings)
            {
                Debug.LogError("No Editor Setting file found. Please close and reopen the View Tool Window.");
            }
            else
            {
                Debug.Log("Editor Settings file found.");
            }
        }
    }

    private void CheckForVTManager()
    {
        if (!viewToolManager)
        {
            Debug.Log("View Tool Manager Window is missing a referance to the View Tool Manager on the Scene");

            viewToolManager = FindObjectOfType<CS_ViewToolManager>();
            // cant find a vtm on the scene
            if (!viewToolManager)
            {
                // if there is a linked VTMEditorSettings file
                if (vTMEditorSettings)
                {
                    GameObject newVTMSceneGO = Instantiate(vTMEditorSettings.VTMScenePrefab);
                    viewToolManager = newVTMSceneGO.GetComponent<CS_ViewToolManager>();
                    Debug.Log("No View Tool Manager on the scene, adding one from the Editor Settings file");
                }
                else
                {
                    Debug.LogError("No View Tool Manager on the scene and VTMEditorSettings file is missing");
                }
            }
            else
            {
                Debug.Log("Found View Tool Manager on the scene");
            }
        }

        // check for raycast checkers on view tool manager
        CheckForRaycastersOnVTM();
    }

    private void OnGUI()
    {
        // if not set up correctly exit here
        if (!viewToolManager ||  !vTMEditorSettings)
        {
            return;
        }

        // create a scroll view to encapsulate the entire view
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        // serialize the window and its properties
        ScriptableObject windowTarget = this;
        SerializedObject serializedWindow = new SerializedObject(windowTarget);

        // serialize the view tool manager and its properties
        SerializedObject serializedVTM = new SerializedObject(viewToolManager);

        // add a label for the grouping
        GUILayout.Label("CS View Tool Manager", EditorStyles.centeredGreyMiniLabel);


        //////////////////////////////// RAYCAST CHECKERS DISPLAY START
        // label area for all raycasters
        GUILayout.Space(8.0f);
        GUILayout.Label("Raycast Checkers", EditorStyles.boldLabel);


        Rect raycastCheckersRect = EditorGUILayout.BeginHorizontal();

        /////////////// display default raycast checkers list
        // find the raycast checker array property of the serialized window
        SerializedProperty raycastCheckersProperty = serializedWindow.FindProperty("raycastCheckers");
        // display the raycast checkers array
        EditorGUILayout.PropertyField(raycastCheckersProperty, new GUIContent("All Raycast Checkers"), true);

        ///////// display buttons for activating and deactivating raycast checkers
        Rect verticalRCButtons = EditorGUILayout.BeginVertical();

        // activate selected raycast checkers
        if (GUILayout.Button("Activate Selected"))
        {
            ActivateSelecetedRaycastCheckersNEW(Selection.gameObjects);
        }
        
        // deactivate raycast checkers
        if (GUILayout.Button("Deactivate Selected >>"))
        {
            DeactivateSelectedRaycastCheckersNEW(Selection.gameObjects);
        }

        // activate all raycast checkers
        if (GUILayout.Button("Activate All"))
        {
            ActivateAllRaycastCheckersNEW();
        }


        // deactivate raycast checkers
        if (GUILayout.Button("Deactivate All >>"))
        {
            DeactivateAllRaycastCheckersNEW();
        }
        ///////// end vertical grouping of the buttons
        EditorGUILayout.EndVertical();


        ////////////// display active raycast checkers list
        // find the active raycast checker array property of the serialized window
        SerializedProperty activeRaycastCheckersProperty = serializedWindow.FindProperty("inactiveRaycastCheckers");
        // display the raycast checkers array
        EditorGUILayout.PropertyField(activeRaycastCheckersProperty, new GUIContent("Inactive Raycast Checkers"), true);

        EditorGUILayout.EndHorizontal();

        // Find any raycast checkers on the scene
        if (GUILayout.Button("Find Raycast Checkers on Scene"))
        {
            FindRaycastCheckersOnScene();
        }

        // add Raycast Checkers to selected objects
        if (GUILayout.Button("Add Raycast Checkers To Selected Scene Objects"))
        {
            // pass on the selected game objects and added raycast checkers if possible
            AddRaycastChecker(Selection.gameObjects);
        }

        // clear raycast checker list
        if (GUILayout.Button("Clear Raycast Checkers List"))
        {
            ClearRaycastCheckerslist();
        }
        ////////////////////////////// RAYCAST CHECKERS DISPLAY END

        ////////////////////////////// EXTRA OPTIONS DISPLAY START
        // constant casting bool
        // label area for all raycasters
        GUILayout.Space(8.0f);
        GUILayout.Label("Casting Settings", EditorStyles.boldLabel);
        SerializedProperty constantCastProperty = serializedVTM.FindProperty("constantCheck");
        bool constantCasting = constantCastProperty.boolValue;
        bool previousCC = constantCasting;
        constantCasting = (bool)EditorGUILayout.Toggle("Constant Casting: ", constantCasting);
        constantCastProperty.boolValue = constantCasting;

        // if not in constant casting mode display the single cast button
        if (!constantCasting)
        {
            // cast button
            if (GUILayout.Button("Check View Meshes"))
            {
                if (EditorApplicationRunning)
                {
                    viewToolManager.ActivateRaycastCheckers();
                }
                else
                {
                    Debug.Log("'Check View Meshes' requires the editor game to be playing");
                }
            }
            // cast timer length slider
            SerializedProperty singleCastMeshTimerLengthProperty = serializedVTM.FindProperty("clearMeshesTimerLength");
            float scmtLengthValue = singleCastMeshTimerLengthProperty.floatValue;
            // create a slider for the single cast mesh timer length value
            scmtLengthValue = (float)EditorGUILayout.Slider("Single Cast Mesh Timer Length Value: ", scmtLengthValue, 0.1f, 60.0f);
            // set the adjusted timer length value to VTM property
            singleCastMeshTimerLengthProperty.floatValue = scmtLengthValue;
        }

        // label area for all raycasters
        GUILayout.Space(8.0f);
        GUILayout.Label("View Mode Settings", EditorStyles.boldLabel);


        // create enum menu for the View Tool angle mode
        SerializedProperty angleModeProperty = serializedVTM.FindProperty("angleMode");
        AngleMode angleMode = (AngleMode)angleModeProperty.enumValueIndex;
        angleMode = (AngleMode)EditorGUILayout.EnumPopup("Angle Mode", angleMode);
        angleModeProperty.enumValueIndex = (int)angleMode;

        // create enum menu for the View Tool Mode
        SerializedProperty vtmModeProperty = serializedVTM.FindProperty("vTMMode");
        VTMMode editingVTMMode = (VTMMode)vtmModeProperty.enumValueIndex;
        editingVTMMode = (VTMMode)EditorGUILayout.EnumPopup("View Tool Mode", editingVTMMode);
        vtmModeProperty.enumValueIndex = (int)editingVTMMode;

        // if using the vtm mode for specific slice create a slider below the enum drop down that has teh specific slice
        if (editingVTMMode == VTMMode.SpecificSlice2D || editingVTMMode == VTMMode.SpecificSliceOLD3D)
        {
            // get the specific slice value
            SerializedProperty ssProperty = serializedVTM.FindProperty("specificSlice");
            int ssValue = ssProperty.intValue;
            // get the specific slice max value by getting the vertical precision value
            SerializedProperty maxSliceProperty = serializedVTM.FindProperty("verticalPrecision");
            int maxSliceValue = maxSliceProperty.intValue;
            // create a slider for the specific slice value
            ssValue = (int)EditorGUILayout.IntSlider("Specific Slice: ", ssValue, 0, maxSliceValue);
            // set the adjusted specific slice value to VTM property
            ssProperty.intValue = ssValue;
        }

        // show gizmos bool
        SerializedProperty showGizmosProperty = serializedVTM.FindProperty("showGizmos");
        bool showGizmos = showGizmosProperty.boolValue;
        showGizmos = (bool)EditorGUILayout.Toggle("Show Gizmos: ", showGizmos);
        showGizmosProperty.boolValue = showGizmos;

        // show raycasts bool
        SerializedProperty showRaycastsProperty = serializedVTM.FindProperty("showDebugRaycast");
        bool showRaycasts = showRaycastsProperty.boolValue;
        showRaycasts = (bool)EditorGUILayout.Toggle("Show Debug Raycasts: ", showRaycasts);
        showRaycastsProperty.boolValue = showRaycasts;

        // only show hits bool
        SerializedProperty showColliderHitsOnlyProperty = serializedVTM.FindProperty("onlyShowColliderHits");
        bool showColliderHitsOnly = showColliderHitsOnlyProperty.boolValue;
        showColliderHitsOnly = (bool)EditorGUILayout.Toggle("Show Collider Hits Only: ", showColliderHitsOnly);
        showColliderHitsOnlyProperty.boolValue = showColliderHitsOnly;

        // vertex color mode bool
        SerializedProperty vertexColorModeProperty = serializedVTM.FindProperty("useVertexColorMode");
        bool vertexColorMode = vertexColorModeProperty.boolValue;
        bool previousVCM = vertexColorMode;
        vertexColorMode = (bool)EditorGUILayout.Toggle("Use Vertex Color Mode: ", vertexColorMode);
        vertexColorModeProperty.boolValue = vertexColorMode;



        // label precision
        GUILayout.Space(8.0f);
        GUILayout.Label("Precision Settings", EditorStyles.boldLabel);
        // get the min precision value
        SerializedProperty precisionMinProperty = serializedVTM.FindProperty("precisionMin");
        int precisionMinValue = precisionMinProperty.intValue;
        // get the max precision value
        SerializedProperty precisionMaxProperty = serializedVTM.FindProperty("precisionMax");
        int precisionMaxValue = precisionMaxProperty.intValue;

        // horizontal precision slider
        SerializedProperty horizontalPrecisionProperty = serializedVTM.FindProperty("horizontalPrecision");
        int horizontalPrecisionValue = horizontalPrecisionProperty.intValue;
        // create a slider for the horizontal precision value
        horizontalPrecisionValue = (int)EditorGUILayout.IntSlider("Horizontal Precision Value: ", horizontalPrecisionValue, precisionMinValue, precisionMaxValue);
        // set the adjusted horizontal precision value to VTM property
        horizontalPrecisionProperty.intValue = horizontalPrecisionValue;

        // vertical precision slider
        SerializedProperty verticalPrecisionProperty = serializedVTM.FindProperty("verticalPrecision");
        int verticalPrecisionValue = verticalPrecisionProperty.intValue;
        int previousVP = verticalPrecisionValue;
        // create a slider for the vertical precision value
        verticalPrecisionValue = (int)EditorGUILayout.IntSlider("Vertical Precision Value: ", verticalPrecisionValue, precisionMinValue, precisionMaxValue);
        // set the adjusted vertical precision value to VTM property
        verticalPrecisionProperty.intValue = verticalPrecisionValue;

        /////////////////////////////////// EXTRA OPTIONS DISPLAY END

        ///////////////////////////////////// DISPLAY WHAT EACH RAYCAST CHECKER CAN SEE
        if (EditorApplicationRunning)
        { 
            // label the raycaster data
            GUILayout.Space(8.0f);
            GUILayout.Label("Raycaster Visability Data", EditorStyles.boldLabel);
            //// for each raycast checker
            foreach (CS_RaycastChecker raycastChecker in viewToolManager.RaycastCheckers)
            {
                // create a box hold all the displaying data
                Rect entireRacasterDetailsRect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // serialize the raycast checker and its properties
                SerializedObject serializedRC = new SerializedObject(raycastChecker);
                // find the can see raycast checker array property of the serialized raycast checker
                SerializedProperty seenRCProperty = serializedRC.FindProperty("visableRaycastCheckers");
                // find the can see raycast checker array property of the serialized raycast checker
                SerializedProperty seenByRCProperty = serializedRC.FindProperty("raycastCheckersCanSeeMe");

                // add a label for the grouping
                GUILayout.Label(raycastChecker.gameObject.name + " Visablilty Data:", EditorStyles.boldLabel);

                // create a horizontal grouping
                Rect rcDetatilsRect = EditorGUILayout.BeginHorizontal();

                // create a vertical grouping for the raycast checkers that can be seen by this raycast checker
                Rect rcCanSeeRect = EditorGUILayout.BeginVertical();
                GUILayout.Label("Can See " + raycastChecker.VisableRaycastCheckers.Count + " Raycasters", EditorStyles.boldLabel);
                // display the seen raycast checkers array
                EditorGUILayout.PropertyField(seenRCProperty, true);
                EditorGUILayout.EndVertical();

                // create a vertical grouping for the raycast checkers that can see this raycast checker
                Rect rcSeenByRect = EditorGUILayout.BeginVertical();
                GUILayout.Label("Can Be Seen By " + raycastChecker.RaycastCheckersCanSeeMe.Count + " Raycasters", EditorStyles.boldLabel);
                // display the seen raycast checkers array
                EditorGUILayout.PropertyField(seenByRCProperty, true);
                EditorGUILayout.EndVertical();

                // end the horizontal grouping
                EditorGUILayout.EndHorizontal();

                // end the main box
                EditorGUILayout.EndVertical();
            }
        }
        ///////////////////////////////////// END DISPLAYING WHAT EACH RAYCAST CHECKER CAN SEE

        // apply any modifications to window properties
        serializedWindow.ApplyModifiedProperties();
        // apply any modifications to the vtm properties
        serializedVTM.ApplyModifiedProperties();

        // preform any updates that call to scripts during runtime
        if (EditorApplicationRunning)
        {
            // if constantly casting has changed update the constant casting state
            if (previousCC != constantCasting)
            {
                viewToolManager.ConstantCheckChange();
            }

            // if vertex color mode has changed update the vertex color mode state
            if (previousVCM != vertexColorMode)
            {
                viewToolManager.UpdateRaycastCheckersVertexColorMode();
            }
        }


        // end the window scroll view
        EditorGUILayout.EndScrollView();

    }

    protected void AddRaycastChecker(GameObject[] selectedGameObjects)
    {
        Debug.Log("Adding Raycast Checkers to all Currently Selected GameObjects");

        // cycle through selected game objects and add raycast checkers if possible
        foreach (GameObject obj in selectedGameObjects)
        {
            // check if object already has a raycast checker
            // look through children
            CS_RaycastChecker attachedRaycastChecker = obj.GetComponentInChildren<CS_RaycastChecker>();
            // doesnt contain a raycast checker in children
            if (attachedRaycastChecker == null)
            {
                // look through parents
                attachedRaycastChecker = obj.GetComponentInParent<CS_RaycastChecker>();

                // if object contains a raycast checker, move on
                if (attachedRaycastChecker)
                {
                    Debug.Log(obj.name + " Already contains a raycast checker");
                    break;
                }

            }
            // if object contains a raycast checker, move on
            else
            {
                Debug.Log(obj.name + " Already contains a raycast checker");
                break;
            }

            // check if object meets the criteria to work with the raycast checker
            // look for a camera
            Camera attachedCamera = obj.GetComponentInChildren<Camera>();
            // no attached camera in objects children
            if (attachedCamera == null)
            {
                attachedCamera = obj.GetComponentInParent<Camera>();
                // no camera attached to any of the objects parents contains a camera
                if (attachedCamera == null)
                {
                    Debug.Log(obj.name + " doesnt contain a camera, raycast checker will not be added");
                    break;
                }
            }

            // add raycast checker to the raycast checker
            CS_RaycastChecker newRC = obj.AddComponent<CS_RaycastChecker>();
            //newRC.SetupRaycastChecker()

            // add raycast checker to the list of raycast checkers
            raycastCheckers.Add(newRC);

            // if the application is running
            if (EditorApplicationRunning)
            {
                // link and setup the raycast checker
                viewToolManager.LinkAndSetupRaycastChecker(newRC);
            }

        }


    }

    protected void FindRaycastCheckersOnScene()
    {
        ClearRaycastCheckerslist();

        // initially check for any raycasters on the view tool manager
        CheckForRaycastersOnVTM();

        CS_RaycastChecker[] foundRaycastCheckers = FindObjectsOfType<CS_RaycastChecker>();
        foreach (CS_RaycastChecker foundRC in foundRaycastCheckers)
        {
            // check if found rc is in the raycast checkers list
            if (!raycastCheckers.Contains(foundRC))
            {
                // add raycast checker to the list of raycast checkers
                raycastCheckers.Add(foundRC);
                Debug.Log("Raycast Checker: " + foundRC.name + " added to raycaster checker list");
            }
            else
            {
                Debug.Log("Raycast Checker: " + foundRC.name + " already in raycast checker list");
            }

            // if the found raycast checker not active check if needing to add to the inactive list
            if (!foundRC.RaycasterActive && !inactiveRaycastCheckers.Contains(foundRC))
            {
                inactiveRaycastCheckers.Add(foundRC);
            }
        }

    }

    protected void ClearRaycastCheckerslist()
    {
        raycastCheckers.Clear();
        inactiveRaycastCheckers.Clear();
    }

    protected void CheckForRaycastersOnVTM()
    {
        // if the view tool manager is connected
        if (viewToolManager)
        {
            // go through the list of raycast checkers on the view tool manager and check if they are in this version of the view tool manager list
            foreach (CS_RaycastChecker rc in viewToolManager.RaycastCheckers)
            {
                // check if in list of raycast checkers
                if (!raycastCheckers.Contains(rc))
                {
                    raycastCheckers.Add(rc);
                }

                // if the found raycast checker not active check if needing to add to the inactive list
                if (!rc.RaycasterActive && !inactiveRaycastCheckers.Contains(rc))
                {
                    inactiveRaycastCheckers.Add(rc);
                }
            }
            
        }
    }

    protected void ActivateSelecetedRaycastCheckers(GameObject[] selectedGameObjects)
    {
        List<CS_RaycastChecker> raycastCheckersToRemoveFromInactiveList = new List<CS_RaycastChecker>();

        // cycle selected objects and get their raycast checkers
        foreach (GameObject goRaycastChecker in selectedGameObjects)
        {
            CS_RaycastChecker selectedRC = goRaycastChecker.GetComponent<CS_RaycastChecker>();
            // if the selected object is a raycast checker, activate it
            if (selectedRC)
            {
                // activate the raycast checker
                selectedRC.ActivateRaycastChecker(true);

                raycastCheckersToRemoveFromInactiveList.Add(selectedRC);
            }
        }

        // if raycast checkers are to be moved lists
        if (raycastCheckersToRemoveFromInactiveList.Count > 0)
        {
            foreach (CS_RaycastChecker activeRaycastChecker in raycastCheckersToRemoveFromInactiveList)
            {
                // if the raycast checker is in the inactive raycast checkers list
                if (inactiveRaycastCheckers.Contains(activeRaycastChecker))
                {
                    inactiveRaycastCheckers.Remove(activeRaycastChecker);
                }
            }

            raycastCheckersToRemoveFromInactiveList.Clear();
        }
    }

    protected void ActivateSelecetedRaycastCheckersNEW(GameObject[] selectedGameObjects)
    {
        List<CS_RaycastChecker> raycastCheckersToRemoveFromInactiveList = new List<CS_RaycastChecker>();

        // cycle selected objects and get their raycast checkers
        foreach (GameObject goRaycastChecker in selectedGameObjects)
        {
            CS_RaycastChecker selectedRC = goRaycastChecker.GetComponent<CS_RaycastChecker>();
            // if the selected object is a raycast checker, activate it
            if (selectedRC)
            {
                // serialize the raycast checker
                SerializedObject serializedRC = new SerializedObject(selectedRC);
                // get the raycaster active property
                SerializedProperty raycasterActiveProperty = serializedRC.FindProperty("raycasterActive");

                bool prevRCActiveValue = raycasterActiveProperty.boolValue;
                raycasterActiveProperty.boolValue = true; // activate the raycast checker

                // apply the serialized object property changes
                serializedRC.ApplyModifiedProperties();

                // if the editor is running
                if (EditorApplicationRunning)
                {
                    // if value has changed
                    if (prevRCActiveValue != raycasterActiveProperty.boolValue)
                    {
                        // clear the view mesh
                        selectedRC.ClearViewMesh();
                    }
                }

                raycastCheckersToRemoveFromInactiveList.Add(selectedRC);
            }
        }

        // if raycast checkers are to be moved lists
        if (raycastCheckersToRemoveFromInactiveList.Count > 0)
        {
            foreach (CS_RaycastChecker activeRaycastChecker in raycastCheckersToRemoveFromInactiveList)
            {
                // if the raycast checker is in the inactive raycast checkers list
                if (inactiveRaycastCheckers.Contains(activeRaycastChecker))
                {
                    inactiveRaycastCheckers.Remove(activeRaycastChecker);
                }
            }

            raycastCheckersToRemoveFromInactiveList.Clear();
        }
    }

    protected void DeactivateSelectedRaycastCheckers(GameObject[] selectedGameObjects)
    {
        // cycle selected objects and get their raycast checkers
        foreach (GameObject goRaycastChecker in selectedGameObjects)
        {
            CS_RaycastChecker selectedRC = goRaycastChecker.GetComponent<CS_RaycastChecker>();
            // if the selected object is a raycast checker, activate it
            if (selectedRC)
            {
                // deactivate the raycast checker
                selectedRC.ActivateRaycastChecker(false);

                // if the selected raycast checker is not in the inactive raycast checker list, add it
                if (!inactiveRaycastCheckers.Contains(selectedRC))
                {
                    inactiveRaycastCheckers.Add(selectedRC);
                }
            }
        }
    }

    protected void DeactivateSelectedRaycastCheckersNEW(GameObject[] selectedGameObjects)
    {
        // cycle selected objects and get their raycast checkers
        foreach (GameObject goRaycastChecker in selectedGameObjects)
        {
            CS_RaycastChecker selectedRC = goRaycastChecker.GetComponent<CS_RaycastChecker>();
            // if the selected object is a raycast checker, activate it
            if (selectedRC)
            {
                // serialize the raycast checker
                SerializedObject serializedRC = new SerializedObject(selectedRC);
                // get the raycaster active property
                SerializedProperty raycasterActiveProperty = serializedRC.FindProperty("raycasterActive");
                bool prevRCActiveValue = raycasterActiveProperty.boolValue;
                raycasterActiveProperty.boolValue = false;

                // if the selected raycast checker is not in the inactive raycast checker list, add it
                if (!inactiveRaycastCheckers.Contains(selectedRC))
                {
                    inactiveRaycastCheckers.Add(selectedRC);
                }

                // apply the serialized object property changes
                serializedRC.ApplyModifiedProperties();

                // if the editor is running
                if (EditorApplicationRunning)
                {
                    // if value has changed
                    if (prevRCActiveValue != raycasterActiveProperty.boolValue)
                    {
                        // clear the view mesh
                        selectedRC.ClearViewMesh();
                    }
                }
            }
        }
    }
    
    protected void ActivateAllRaycastCheckers()
    {
        foreach (CS_RaycastChecker raycastChecker in inactiveRaycastCheckers)
        {
            raycastChecker.ActivateRaycastChecker(true);
        }

        // clear the inactive list
        inactiveRaycastCheckers.Clear();
    }

    protected void ActivateAllRaycastCheckersNEW()
    {
        foreach (CS_RaycastChecker raycastChecker in inactiveRaycastCheckers)
        {
            // serialize the raycast checker
            SerializedObject serializedRC = new SerializedObject(raycastChecker);
            // get the raycaster active property
            SerializedProperty raycasterActiveProperty = serializedRC.FindProperty("raycasterActive");

            bool prevRCActiveValue = raycasterActiveProperty.boolValue;
            raycasterActiveProperty.boolValue = true; // activate the raycast checker

            // apply the serialized object property changes
            serializedRC.ApplyModifiedProperties();

            // if the editor is running
            if (EditorApplicationRunning)
            {
                // if value has changed
                if (prevRCActiveValue != raycasterActiveProperty.boolValue)
                {
                    // clear the view mesh
                    raycastChecker.ClearViewMesh();
                }
            }

        }

        // clear the inactive list
        inactiveRaycastCheckers.Clear();
    }

    protected void DeactivateAllRaycastCheckers()
    {
        foreach (CS_RaycastChecker rcToDeactivate in raycastCheckers)
        {
            rcToDeactivate.ActivateRaycastChecker(false);

            // if not in the inactive list
            if (!inactiveRaycastCheckers.Contains(rcToDeactivate))
            {
                inactiveRaycastCheckers.Add(rcToDeactivate);
            }
        }
        
    }

    protected void DeactivateAllRaycastCheckersNEW()
    {
        foreach (CS_RaycastChecker rcToDeactivate in raycastCheckers)
        {
            // serialize the raycast checker
            SerializedObject serializedRC = new SerializedObject(rcToDeactivate);
            // get the raycaster active property
            SerializedProperty raycasterActiveProperty = serializedRC.FindProperty("raycasterActive");

            bool prevRCActiveValue = raycasterActiveProperty.boolValue;
            raycasterActiveProperty.boolValue = false; // set the active property to false

            // if the selected raycast checker is not in the inactive raycast checker list, add it
            if (!inactiveRaycastCheckers.Contains(rcToDeactivate))
            {
                inactiveRaycastCheckers.Add(rcToDeactivate);
            }

            // apply the serialized object property changes
            serializedRC.ApplyModifiedProperties();

            // if the editor is running
            if (EditorApplicationRunning)
            {
                // if value has changed
                if (prevRCActiveValue != raycasterActiveProperty.boolValue)
                {
                    // clear the view mesh
                    rcToDeactivate.ClearViewMesh();
                }
            }
        }

    }

    // This will only get called 10 times per second.
    public void OnInspectorUpdate()
    {
        // will re do the ongui call without having to have it in focus
        Repaint();
    }
}
