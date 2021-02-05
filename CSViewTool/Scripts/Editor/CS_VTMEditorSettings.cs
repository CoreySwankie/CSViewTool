/// 
/// Created By Corey Swankie as part of dissertation project "FPS Multiplayer Game Development – Map Development Tools: Lines of Sight"
/// Contact at Personal email: thatscorey@gmail.com
/// Find more of my work at: CoreySwankie.com
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "ToolEditorSettings", menuName = "CS Tool/Tool Editor Settings")]
[System.Serializable]
public class CS_VTMEditorSettings : ScriptableObject
{
    [SerializeField]
    protected GameObject raycastCheckerPrefab = null;
    public GameObject RaycastCheckerPrefab
    {
        get
        {
            return raycastCheckerPrefab;
        }
    }

    [SerializeField]
    protected GameObject vTMScenePrefab = null;
    public GameObject VTMScenePrefab
    {
        get
        {
            return vTMScenePrefab;
        }
    }
}
