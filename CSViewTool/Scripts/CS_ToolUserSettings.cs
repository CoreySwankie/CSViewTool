/// 
/// Created By Corey Swankie as part of dissertation project "FPS Multiplayer Game Development – Map Development Tools: Lines of Sight"
/// Contact at Personal email: thatscorey@gmail.com
/// Find more of my work at: CoreySwankie.com
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ToolUserSettings", menuName = "CS Tool/Tool User Settings")]
public class CS_ToolUserSettings : ScriptableObject
{
    [SerializeField]
    protected List<Color> meshObjectColors = new List<Color>();
    public List<Color> MeshObjectColors
    {
        get
        {
            return meshObjectColors;
        }
    }

    public Color GetMeshObjectColor(int colorIndex)
    {
        // set return color to magenta usually used when there is an error in mesh
        Color returnColor = Color.magenta;

        // there is colors set
        if (meshObjectColors.Count > 0)
        {
            // if the input index is within the count of object colors that have been set
            if (colorIndex < meshObjectColors.Count && colorIndex >= 0)
            {
                returnColor = meshObjectColors[colorIndex];
            }
            // if there is more than 1 color then we can use mod operations to set recurring/looping colors 
            else if (meshObjectColors.Count > 1 && colorIndex >= meshObjectColors.Count)
            {
                int newIndex = colorIndex % (meshObjectColors.Count - 1);
                returnColor = meshObjectColors[newIndex];
            }
        }

        return returnColor;
    }

    [SerializeField]
    protected Color vertexColorModeDefaultColor = Color.green;
    public Color VertexColorModeDefaultColor
    {
        get
        {
            return vertexColorModeDefaultColor;
        }
    }

    [SerializeField]
    protected Color vertexColorModeHitPlayerColor = Color.red;
    public Color VertexColorModeHitPlayerColor
    {
        get
        {
            return vertexColorModeHitPlayerColor;
        }
    }

    [SerializeField]
    protected Color vertexColorModeHitObjectColor = Color.blue;
    public Color VertexColorModeHitObjectColor
    {
        get
        {
            return vertexColorModeHitObjectColor;
        }
    }

    [SerializeField]
    protected string playersLayerString = "Players";
    public string PlayersLayerString
    {
        get
        {
            return playersLayerString;
        }
    }
    

    [SerializeField]
    protected string worldObjectsLayerString = "WorldObjects";
    public string WorldObjectsLayerString
    {
        get
        {
            return worldObjectsLayerString;
        }
    }

    public LayerMask GetLayerMask()
    {
        string[] acceptedLayerStrings =  {playersLayerString, worldObjectsLayerString};
        LayerMask returnLayerMask = LayerMask.GetMask(acceptedLayerStrings);
        return returnLayerMask;
    }

    [SerializeField]
    protected Material defualtViewMeshMaterial = null;
    public Material DefualtViewMeshMaterial
    {
        get
        {
            return defualtViewMeshMaterial;
        }
    }

    [SerializeField]
    protected Material vertexColorViewMeshMaterial = null;
    public Material VertexColorViewMeshMaterial
    {
        get
        {
            return vertexColorViewMeshMaterial;
        }
    }

    [SerializeField]
    protected GameObject viewMeshGeneratorPrefab = null;
    public GameObject ViewMeshGeneratorPrefab
    {
        get
        {
            if (viewMeshGeneratorPrefab == null)
            {
               //UnityEngine.Debug.LogError("ViewMeshGeneratorPrefab is not set");
            }
            return viewMeshGeneratorPrefab;
        }
    }
}
