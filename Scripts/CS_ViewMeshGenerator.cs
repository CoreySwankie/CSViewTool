/// 
/// Created By Corey Swankie as part of dissertation project "FPS Multiplayer Game Development – Map Development Tools: Lines of Sight"
/// Contact at Personal email: thatscorey@gmail.com
/// Find more of my work at: CoreySwankie.com
///

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class CS_ViewMeshGenerator : MonoBehaviour
{
    protected Mesh mesh;
    protected MeshFilter meshFilter;
    protected MeshRenderer meshRenderer;

    protected Vector3[] vertices = Array.Empty<Vector3>();
    protected int[] triangles = Array.Empty<int>();
    protected Color[] colors = Array.Empty<Color>();
    protected float transparency = 0.0f;

    protected CS_ViewToolManager viewToolManager = null;
    protected CS_ToolUserSettings toolUserSettings = null;
    protected CS_RaycastChecker connectedRaycastChecker = null;

    protected Color meshColor = Color.white;
    public void SetupViewMeshGenerator(CS_ViewToolManager vtManager, CS_RaycastChecker rChecker, Color mColor)
    {
        viewToolManager = vtManager;
        toolUserSettings = viewToolManager.ToolUserSettings;
        connectedRaycastChecker = rChecker;

        // get the mesh filter
        meshFilter = GetComponent<MeshFilter>();
        // create a new mesh
        mesh = new Mesh();

        // set the mesh to the mesh filter
        meshFilter.mesh = mesh;

        meshRenderer = GetComponent<MeshRenderer>();
        // set mesh transparency
        transparency = meshRenderer.material.color.a;
        // set the mesh color
        meshColor = mColor;
        meshColor.a = transparency;
        meshRenderer.material.color = meshColor;

        // send some quick mesh data to make a triangle for testing
        Vector3[] testTriangleVerts = new Vector3[]
        {
            new Vector3(0.0f, 1.0f, 0.0f),
            new Vector3(0.0f, 1.0f, 1.0f),
            new Vector3(1.0f, 1.0f, 0.0f)
        };

        int[] testTriangleTris = new int[]
        {
            0, 1, 2
        };

        UpdateMesh(testTriangleVerts, testTriangleTris);
        ClearMesh();
    }

    /// <summary>
    /// Clear mesh and clear and empty vert and tri arrays
    /// </summary>
    public void ClearMesh()
    {
        // clear the mesh
        mesh.Clear();

        // clear and empty the verticies and triangles arrays.
        Array.Clear(vertices, 0, vertices.Length);
        Array.Clear(triangles, 0, triangles.Length);
        Array.Clear(colors, 0, colors.Length);

        vertices = Array.Empty<Vector3>();
        triangles = Array.Empty<int>();
        colors = Array.Empty<Color>();
    }

    protected void UpdateMesh(Vector3[] newVertices, int[] newTriangles)
    {
        // clear mesh and vert/tri arrays
        ClearMesh();

        // set new vert and tri arrays
        vertices = newVertices;
        triangles = newTriangles;

        // set the mesh vert and tries
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        // recalculate normals
        mesh.RecalculateNormals();
    }

    protected void UpdateMesh(Vector3[] newVertices, int[] newTriangles, Color[] newColors)
    {
        // clear mesh and vert/tri arrays
        ClearMesh();

        // set new vert and tri arrays
        vertices = newVertices;
        triangles = newTriangles;
        colors = newColors;

        // set the mesh vert and tries
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;

        // recalculate normals
        mesh.RecalculateNormals();
    }

    /// <summary>
    /// Triangle Data just holds 3 ints that are the vertex ids that make up a complete triangle
    /// </summary>
    protected class TriangleData
    {
        protected int firstVertID = 0;
        public int FirstVertID
        {
            get
            {
                return firstVertID;
            }
            set
            {
                if (value >= 0)
                {
                    firstVertID = value;
                }
                else
                {
                    firstVertID = 0;
                }
            }
        }

        protected int secondVertID = 0;
        public int SecondVertID
        {
            get
            {
                return secondVertID;
            }
            set
            {
                if (value >= 0)
                {
                    secondVertID = value;
                }
                else
                {
                    secondVertID = 0;
                }
            }
        }

        protected int thirdVertID = 0;
        public int ThirdVertID
        {
            get
            {
                return thirdVertID;
            }
            set
            {
                if (value >= 0)
                {
                    thirdVertID = value;
                }
                else
                {
                    thirdVertID = 0;
                }
            }
        }

        public TriangleData()
        {
            FirstVertID = 0;
            SecondVertID = 0;
            ThirdVertID = 0;
        }

        public TriangleData(int firstID, int secondID, int thirdID)
        {
            FirstVertID = firstID;
            SecondVertID = secondID;
            ThirdVertID = thirdID;
        }

        public void SetVertIDS(int firstID, int secondID, int thirdID)
        {
            // set the first vert id
            FirstVertID = firstID;
            // set the second vert id
            SecondVertID = secondID;
            // set the third vert id
            ThirdVertID = thirdID;
        }

    }

    struct LastVertInfo
    {
        public bool wasHit;
        public int vertID;
    }

    public void ReceiveRaycastData(CheckerInfo checkerInfo)
    {

        switch (viewToolManager.VTMMode)
        {
            case VTMMode.Default3D:
                ProcessRCData3DMeshMultipleLayers(checkerInfo);
                break;
            case VTMMode.Default2D:
                ProcessRCData2DMesh(checkerInfo);
                break;
            case VTMMode.ConsolidateCast3D:
                ProcessRCDataConsolidateTo2DMesh(checkerInfo);
                break;
            case VTMMode.IgnoreVertical2D:
                ProcessRCData2DMesh(checkerInfo);
                break;
            case VTMMode.ViewCentreCast3D:
                ProcessRCData2DMesh(checkerInfo);
                break;
            case VTMMode.SpecificSlice2D:
                // using the new specific slice checker info
                ProcessRCData2DMesh(checkerInfo);
                break;
            case VTMMode.SpecificSliceOLD3D:
                // using the old method of having a full 3d raycast check and then only getting the specific slice
                ProcessRCDataSpecificSliceOld(checkerInfo);
                break;
            default:
                break;
        }
    }

    protected void ProcessRCData3DMeshMultipleLayers(CheckerInfo checkerInfo)
    {
        // get the max number of verts
        // max number of verts will be the number of horizontal casts multiplied by the vertical casts plus 1 more vert for the origin vertex
        int maxNumberOfVerts = (checkerInfo.horizontalCasts * checkerInfo.verticalCasts) + 1;
        Vector3[] finalVerts = new Vector3[maxNumberOfVerts];
        int[] finalTris = new int[0];
        Color[] finalColors = new Color[maxNumberOfVerts];

        // bring the vertex position back to the origin instead of being built from the position of the raycast checker
        // this is done as the view mesh generator is parented to the raycast checker which moves with the attached player
        // when creating a mesh it will be built around the origin so moving it requires building around the origin and not an world position
        //Vector3 offsetPosition = this.transform.position;
        Vector3 offsetPosition = Vector3.zero;

        // create a list of complete triangles to convert to the final triangles array
        List<TriangleData> listOfCompleteTriangles = new List<TriangleData>();

        // go through all the checker cast infos and add them to the verts
        // current vertex index will be increased before adding to each vertex id in the array of vertex
        int currentVertexIndex = 0;

        // add the origin point vert
        finalVerts[0] = checkerInfo.castOrigin - offsetPosition;
        finalColors[0] = toolUserSettings.VertexColorModeDefaultColor;

        // used for only showing collider hits
        LastVertInfo lastCheckedVert = new LastVertInfo();
        lastCheckedVert.wasHit = false;
        lastCheckedVert.vertID = 0;

        // add all cast hit points as vertexs
        for (int verticalCastCount = 0; verticalCastCount < checkerInfo.verticalCasts; verticalCastCount++)
        {
            for (int horizontalCastCount = 0; horizontalCastCount < checkerInfo.horizontalCasts; horizontalCastCount++)
            {
                // check that not passed the end of the max number of verts
                if (currentVertexIndex >= maxNumberOfVerts)
                {
                   //Debug.Log("Current vert id is larger than max number of Verts: " + maxNumberOfVerts);
                    break;
                }

                // get the cast info that we are checking
                CastInfo checkingInfo = checkerInfo.castInfos[horizontalCastCount, verticalCastCount];

                // only showing hits
                if (viewToolManager.OnlyShowColliderHits)
                {
                    // if point was a hit
                    if (checkingInfo.HitID != HitID.Miss)
                    {
                        // increase vertex index before updating vert information
                        currentVertexIndex++;

                        // add new vert to the array of verts
                        finalVerts[currentVertexIndex] = checkingInfo.HitPosition - offsetPosition;

                        // set vertex color according to if the cast info was a hit or not
                        // hit player
                        if (checkingInfo.HitID == HitID.HitPlayer)
                        {
                            finalColors[currentVertexIndex] = toolUserSettings.VertexColorModeHitPlayerColor;
                        }
                        // hit object
                        else if (checkingInfo.HitID == HitID.HitObject)
                        {
                            finalColors[currentVertexIndex] = toolUserSettings.VertexColorModeHitObjectColor;
                        }

                        // if there are atleast 3 vertexes already in the list of verts
                        // and the vert checked before this was also a hit
                        // and is atleast the second horizontal cast
                        if (currentVertexIndex >= 2 && lastCheckedVert.wasHit && horizontalCastCount > 0)
                        {
                            // create new triangle data to fill
                            TriangleData newTriangle = new TriangleData();
                            newTriangle.FirstVertID = 0;
                            newTriangle.SecondVertID = currentVertexIndex - 1;
                            newTriangle.ThirdVertID = currentVertexIndex;
                            // add the new triangle to the list of triangles
                            listOfCompleteTriangles.Add(newTriangle);
                        }

                        // set last checked vert info
                        lastCheckedVert.wasHit = true;
                        lastCheckedVert.vertID = currentVertexIndex;
                    }
                    // point checking was not a hit
                    else
                    {
                        // set the last checked vert info
                        lastCheckedVert.wasHit = false;
                        lastCheckedVert.vertID = 0; // there isnt a vertex created for the miss so will just be set as the origin
                    }
                }
                // showing all casts
                else
                {
                    // increase vertex index before updating vert information
                    currentVertexIndex++;

                    finalVerts[currentVertexIndex] = checkingInfo.HitPosition - offsetPosition;

                    // set vertex color according to if the cast info was a hit or not
                    // hit player
                    if (checkingInfo.HitID == HitID.HitPlayer)
                    {
                        finalColors[currentVertexIndex] = toolUserSettings.VertexColorModeHitPlayerColor;
                    }
                    // hit object
                    else if (checkingInfo.HitID == HitID.HitObject)
                    {
                        finalColors[currentVertexIndex] = toolUserSettings.VertexColorModeHitObjectColor;
                    }
                    // missed
                    else
                    {
                        finalColors[currentVertexIndex] = toolUserSettings.VertexColorModeDefaultColor;
                    }

                    // if there are atleast 3 vertexes already in the list of verts
                    // also make sure on atleast the second horizontal cast for each vertical increase 
                    if (currentVertexIndex >=2 && horizontalCastCount > 0)
                    {
                        TriangleData newTriangle = new TriangleData();
                        newTriangle.FirstVertID = 0;
                        newTriangle.SecondVertID = currentVertexIndex - 1;
                        newTriangle.ThirdVertID = currentVertexIndex;

                        // add the new triangle to the list of triangles
                        listOfCompleteTriangles.Add(newTriangle);
                    }
                }
            }
        }

        // check if needing to resize the vertex array
        // checking the current vertex index will show which was the last array element edited (+1 as arrays start at 0)
        // if the last array element edited wasnt the end of the array then resize it
        if (currentVertexIndex < finalVerts.Length)
        {
           //Debug.Log("Resizing the final vert array");
            // resize to the size of the current vertex index plus 1
            Array.Resize(ref finalVerts, currentVertexIndex + 1);
            Array.Resize(ref finalColors, currentVertexIndex + 1);
        }

        // convert complete triangles to the final tris array
        finalTris = new int[listOfCompleteTriangles.Count * 3];
        int currentBaseTriID = 0;
        foreach (TriangleData triData in listOfCompleteTriangles)
        {
            // add all the triangle data to the final tri array
            finalTris[currentBaseTriID + 0] = triData.FirstVertID;
            finalTris[currentBaseTriID + 1] = triData.SecondVertID;
            finalTris[currentBaseTriID + 2] = triData.ThirdVertID;

            // increase the base tri id
            currentBaseTriID += 3;
        }

        // finally update the mesh
        UpdateMesh(finalVerts, finalTris, finalColors);
    }

    protected void ProcessRCData2DMesh(CheckerInfo checkerInfo)
    {
        // get the max number of verts
        // max number of verts will be the number of horizontal casts plus 1 more vert for the origin vertex
        int maxNumberOfVerts = checkerInfo.horizontalCasts + 1;
        Vector3[] finalVerts = new Vector3[maxNumberOfVerts];
        int[] finalTris = new int[0];
        Color[] finalColors = new Color[maxNumberOfVerts];

        // bring the vertex position back to the origin instead of being built from the position of the raycast checker
        // this is done as the view mesh generator is parented to the raycast checker which moves with the attached player
        // when creating a mesh it will be built around the origin so moving it requires building around the origin and not an world position
        Vector3 offsetPosition = Vector3.zero;

        // create a list of complete triangles to convert to the final triangles array
        List<TriangleData> listOfCompleteTriangles = new List<TriangleData>();

        // go through all the checker cast infos and add them to the verts
        // current vertex index will be increased before adding to each vertex id in the array of vertex
        int currentVertexIndex = 0;

        // add the origin point vert
        finalVerts[0] = checkerInfo.castOrigin - offsetPosition;
        finalColors[0] = toolUserSettings.VertexColorModeDefaultColor;

        // used for only showing collider hits
        LastVertInfo lastCheckedVert = new LastVertInfo();
        lastCheckedVert.wasHit = false;
        lastCheckedVert.vertID = 0;

        // add all the hit points as verts
        for (int horizontalCastCount = 0; horizontalCastCount < checkerInfo.horizontalCasts; horizontalCastCount++)
        {
            // check that not passed the end of the max number of verts
            if (currentVertexIndex >= maxNumberOfVerts)
            {
               //Debug.Log("Current vert id is larger than max number of Verts: " + maxNumberOfVerts);
                break;
            }

            // get the cast info that we are checking
            CastInfo checkingInfo = checkerInfo.castInfos[horizontalCastCount, 0];

            // only showing hits
            if (viewToolManager.OnlyShowColliderHits)
            {
                if (checkingInfo.HitID != HitID.Miss)
                {
                    // increase vertex index before updating vert information
                    currentVertexIndex++;

                    // add new vert to the array of verts
                    finalVerts[currentVertexIndex] = checkingInfo.HitPosition - offsetPosition;

                    // set vertex color according to if the cast info was a hit or not
                    // hit player
                    if (checkingInfo.HitID == HitID.HitPlayer)
                    {
                        finalColors[currentVertexIndex] = toolUserSettings.VertexColorModeHitPlayerColor;
                    }
                    // hit object
                    else if (checkingInfo.HitID == HitID.HitObject)
                    {
                        finalColors[currentVertexIndex] = toolUserSettings.VertexColorModeHitObjectColor;
                    }

                    // if there are atleast 3 vertexes already in the list of verts
                    // and the vert checked before this was also a hit
                    if (currentVertexIndex >= 2 && lastCheckedVert.wasHit)
                    {
                        // create new triangle data to fill
                        TriangleData newTriangle = new TriangleData();
                        newTriangle.FirstVertID = 0;
                        newTriangle.SecondVertID = currentVertexIndex - 1;
                        newTriangle.ThirdVertID = currentVertexIndex;
                        // add the new triangle to the list of triangles
                        listOfCompleteTriangles.Add(newTriangle);
                    }

                    // set last checked vert info
                    lastCheckedVert.wasHit = true;
                    lastCheckedVert.vertID = currentVertexIndex;
                }
                // point checking was not a hit
                else
                {
                    // set the last checked vert info
                    lastCheckedVert.wasHit = false;
                    lastCheckedVert.vertID = 0; // there isnt a vertex created for the miss so will just be set as the origin
                }
            }
            // showing everything that is visable
            else
            {
                // increase vertex index before updating vert information
                currentVertexIndex++;

                finalVerts[currentVertexIndex] = checkingInfo.HitPosition - offsetPosition;

                // set vertex color according to if the cast info was a hit or not
                // hit player
                if (checkingInfo.HitID == HitID.HitPlayer)
                {
                    finalColors[currentVertexIndex] = toolUserSettings.VertexColorModeHitPlayerColor;
                }
                // hit object
                else if (checkingInfo.HitID == HitID.HitObject)
                {
                    finalColors[currentVertexIndex] = toolUserSettings.VertexColorModeHitObjectColor;
                }
                // missed
                else
                {
                    finalColors[currentVertexIndex] = toolUserSettings.VertexColorModeDefaultColor;
                }

                // if there are atleast 3 vertexes already in the array of verts
                if (currentVertexIndex >= 2)
                {
                    TriangleData newTriangle = new TriangleData();
                    newTriangle.FirstVertID = 0;
                    newTriangle.SecondVertID = currentVertexIndex - 1;
                    newTriangle.ThirdVertID = currentVertexIndex;

                    // add the new triangle to the list of triangles
                    listOfCompleteTriangles.Add(newTriangle);
                }
            }
        }

        // check if needing to resize the vertex array
        // checking the current vertex index will show which was the last array element edited (+1 as arrays start at 0)
        // if the last array element edited wasnt the end of the array then resize it
        if (currentVertexIndex < finalVerts.Length)
        {
           //Debug.Log("Resizing the final vert array");
            // resize to the size of the current vertex index plus 1
            Array.Resize(ref finalVerts, currentVertexIndex + 1);
            Array.Resize(ref finalColors, currentVertexIndex + 1);
        }

        // convert complete triangles to the final tris array
        finalTris = new int[listOfCompleteTriangles.Count * 3];
        int currentBaseTriID = 0;
        foreach (TriangleData triData in listOfCompleteTriangles)
        {
            // add all the triangle data to the final tri array
            finalTris[currentBaseTriID + 0] = triData.FirstVertID;
            finalTris[currentBaseTriID + 1] = triData.SecondVertID;
            finalTris[currentBaseTriID + 2] = triData.ThirdVertID;

            // increase the base tri id
            currentBaseTriID += 3;
        }

        // finally update the mesh
        UpdateMesh(finalVerts, finalTris, finalColors);
    }

    protected void ProcessRCDataSpecificSliceOld(CheckerInfo checkerInfo)
    {
        // get the max number of verts
        // max number of verts will be the number of horizontal casts plus 1 more vert for the origin vertex
        int maxNumberOfVerts = checkerInfo.horizontalCasts + 1;
        Vector3[] finalVerts = new Vector3[maxNumberOfVerts];
        int[] finalTris = new int[0];
        Color[] finalColors = new Color[maxNumberOfVerts];

        // bring the vertex position back to the origin instead of being built from the position of the raycast checker
        // this is done as the view mesh generator is parented to the raycast checker which moves with the attached player
        // when creating a mesh it will be built around the origin so moving it requires building around the origin and not an world position
        Vector3 offsetPosition = Vector3.zero;

        // create a list of complete triangles to convert to the final triangles array
        List<TriangleData> listOfCompleteTriangles = new List<TriangleData>();

        // go through all the checker cast infos and add them to the verts
        // current vertex index will be increased before adding to each vertex id in the array of vertex
        int currentVertexIndex = 0;

        // add the origin point vert
        finalVerts[0] = checkerInfo.castOrigin - offsetPosition;
        finalColors[0] = toolUserSettings.VertexColorModeDefaultColor;

        // used for only showing collider hits
        LastVertInfo lastCheckedVert = new LastVertInfo();
        lastCheckedVert.wasHit = false;
        lastCheckedVert.vertID = 0;

        // add all the hit points as verts
        for (int horizontalCastCount = 0; horizontalCastCount < checkerInfo.horizontalCasts; horizontalCastCount++)
        {
            // check that not passed the end of the max number of verts
            if (currentVertexIndex >= maxNumberOfVerts)
            {
                //Debug.Log("Current vert id is larger than max number of Verts: " + maxNumberOfVerts);
                break;
            }

            // get the cast info that we are checking
            CastInfo checkingInfo = checkerInfo.castInfos[horizontalCastCount, viewToolManager.SpecificSlice];

            // only showing hits
            if (viewToolManager.OnlyShowColliderHits)
            {
                if (checkingInfo.HitID != HitID.Miss)
                {
                    // increase vertex index before updating vert information
                    currentVertexIndex++;

                    // add new vert to the array of verts
                    finalVerts[currentVertexIndex] = checkingInfo.HitPosition - offsetPosition;

                    // set vertex color according to if the cast info was a hit or not
                    // hit player
                    if (checkingInfo.HitID == HitID.HitPlayer)
                    {
                        finalColors[currentVertexIndex] = toolUserSettings.VertexColorModeHitPlayerColor;
                    }
                    // hit object
                    else if (checkingInfo.HitID == HitID.HitObject)
                    {
                        finalColors[currentVertexIndex] = toolUserSettings.VertexColorModeHitObjectColor;
                    }

                    // if there are atleast 3 vertexes already in the list of verts
                    // and the vert checked before this was also a hit
                    if (currentVertexIndex >= 2 && lastCheckedVert.wasHit)
                    {
                        // create new triangle data to fill
                        TriangleData newTriangle = new TriangleData();
                        newTriangle.FirstVertID = 0;
                        newTriangle.SecondVertID = currentVertexIndex - 1;
                        newTriangle.ThirdVertID = currentVertexIndex;
                        // add the new triangle to the list of triangles
                        listOfCompleteTriangles.Add(newTriangle);
                    }

                    // set last checked vert info
                    lastCheckedVert.wasHit = true;
                    lastCheckedVert.vertID = currentVertexIndex;
                }
                // point checking was not a hit
                else
                {
                    // set the last checked vert info
                    lastCheckedVert.wasHit = false;
                    lastCheckedVert.vertID = 0; // there isnt a vertex created for the miss so will just be set as the origin
                }
            }
            // showing everything that is visable
            else
            {
                // increase vertex index before updating vert information
                currentVertexIndex++;

                finalVerts[currentVertexIndex] = checkingInfo.HitPosition - offsetPosition;

                // set vertex color according to if the cast info was a hit or not
                // hit player
                if (checkingInfo.HitID == HitID.HitPlayer)
                {
                    finalColors[currentVertexIndex] = toolUserSettings.VertexColorModeHitPlayerColor;
                }
                // hit object
                else if (checkingInfo.HitID == HitID.HitObject)
                {
                    finalColors[currentVertexIndex] = toolUserSettings.VertexColorModeHitObjectColor;
                }
                // missed
                else
                {
                    finalColors[currentVertexIndex] = toolUserSettings.VertexColorModeDefaultColor;
                }

                // if there are atleast 3 vertexes already in the array of verts
                if (currentVertexIndex >= 2)
                {
                    TriangleData newTriangle = new TriangleData();
                    newTriangle.FirstVertID = 0;
                    newTriangle.SecondVertID = currentVertexIndex - 1;
                    newTriangle.ThirdVertID = currentVertexIndex;

                    // add the new triangle to the list of triangles
                    listOfCompleteTriangles.Add(newTriangle);
                }
            }
        }

        // check if needing to resize the vertex array
        // checking the current vertex index will show which was the last array element edited (+1 as arrays start at 0)
        // if the last array element edited wasnt the end of the array then resize it
        if (currentVertexIndex < finalVerts.Length)
        {
            //Debug.Log("Resizing the final vert array");
            // resize to the size of the current vertex index plus 1
            Array.Resize(ref finalVerts, currentVertexIndex + 1);
            Array.Resize(ref finalColors, currentVertexIndex + 1);
        }

        // convert complete triangles to the final tris array
        finalTris = new int[listOfCompleteTriangles.Count * 3];
        int currentBaseTriID = 0;
        foreach (TriangleData triData in listOfCompleteTriangles)
        {
            // add all the triangle data to the final tri array
            finalTris[currentBaseTriID + 0] = triData.FirstVertID;
            finalTris[currentBaseTriID + 1] = triData.SecondVertID;
            finalTris[currentBaseTriID + 2] = triData.ThirdVertID;

            // increase the base tri id
            currentBaseTriID += 3;
        }

        // finally update the mesh
        UpdateMesh(finalVerts, finalTris, finalColors);
    }

    // this mode will also ignore the hits only option and just act as if it is true
    protected void ProcessRCDataConsolidateTo2DMesh(CheckerInfo checkerInfo)
    {
        // get the max number of verts
        // max number of verts will be the number of horizontal casts multiplied by the vertical casts plus 1 more vert for the origin vertex
        int maxNumberOfVerts = (checkerInfo.horizontalCasts * checkerInfo.verticalCasts) + 1;
        Vector3[] finalVerts = new Vector3[maxNumberOfVerts];
        int[] finalTris = new int[0];
        Color[] finalColors = new Color[maxNumberOfVerts];

        // bring the vertex position back to the origin instead of being built from the position of the raycast checker
        // this is done as the view mesh generator is parented to the raycast checker which moves with the attached player
        // when creating a mesh it will be built around the origin so moving it requires building around the origin and not an world position
        //Vector3 offsetPosition = this.transform.position;
        Vector3 offsetPosition = Vector3.zero;

        // create a list of complete triangles to convert to the final triangles array
        List<TriangleData> listOfCompleteTriangles = new List<TriangleData>();

        // go through all the checker cast infos and add them to the verts
        // current vertex index will be increased before adding to each vertex id in the array of vertex
        int currentVertexIndex = 0;

        // add the origin point vert
        finalVerts[0] = checkerInfo.castOrigin - offsetPosition;
        finalColors[0] = toolUserSettings.VertexColorModeDefaultColor;

        // used for only showing collider hits
        LastVertInfo lastCheckedVert = new LastVertInfo();
        lastCheckedVert.wasHit = false;
        lastCheckedVert.vertID = 0;

        // move all the the same height as the cast origin
        float consolidateHeight = checkerInfo.castOrigin.y;

        // add all cast hit points as vertexs
        for (int verticalCastCount = 0; verticalCastCount < checkerInfo.verticalCasts; verticalCastCount++)
        {
            for (int horizontalCastCount = 0; horizontalCastCount < checkerInfo.horizontalCasts; horizontalCastCount++)
            {
                // check that not passed the end of the max number of verts
                if (currentVertexIndex >= maxNumberOfVerts)
                {
                    //Debug.Log("Current vert id is larger than max number of Verts: " + maxNumberOfVerts);
                    break;
                }

                // get the cast info that we are checking
                CastInfo checkingInfo = checkerInfo.castInfos[horizontalCastCount, verticalCastCount];
               

                // if point was a hit
                if (checkingInfo.HitID != HitID.Miss)
                {
                    // increase vertex index before updating vert information
                    currentVertexIndex++;

                    // adjust the position by the offset position
                    Vector3 adjustedPosition = checkingInfo.HitPosition - offsetPosition;
                    // move all positions to the same y height
                    adjustedPosition.y = consolidateHeight;

                    // add new vert to the array of verts
                    finalVerts[currentVertexIndex] = adjustedPosition;

                    // set vertex color according to if the cast info was a hit or not
                    // hit player
                    if (checkingInfo.HitID == HitID.HitPlayer)
                    {
                        finalColors[currentVertexIndex] = toolUserSettings.VertexColorModeHitPlayerColor;
                    }
                    // hit object
                    else if (checkingInfo.HitID == HitID.HitObject)
                    {
                        finalColors[currentVertexIndex] = toolUserSettings.VertexColorModeHitObjectColor;
                    }

                    // if there are atleast 3 vertexes already in the list of verts
                    // and the vert checked before this was also a hit
                    // and is atleast the second horizontal cast
                    if (currentVertexIndex >= 2 && lastCheckedVert.wasHit && horizontalCastCount > 0)
                    {
                        // create new triangle data to fill
                        TriangleData newTriangle = new TriangleData();
                        newTriangle.FirstVertID = 0;
                        newTriangle.SecondVertID = currentVertexIndex - 1;
                        newTriangle.ThirdVertID = currentVertexIndex;
                        // add the new triangle to the list of triangles
                        listOfCompleteTriangles.Add(newTriangle);
                    }

                    // set last checked vert info
                    lastCheckedVert.wasHit = true;
                    lastCheckedVert.vertID = currentVertexIndex;
                }
                // point checking was not a hit
                else
                {
                    // set the last checked vert info
                    lastCheckedVert.wasHit = false;
                    lastCheckedVert.vertID = 0; // there isnt a vertex created for the miss so will just be set as the origin
                }

            }
        }

        // check if needing to resize the vertex array
        // checking the current vertex index will show which was the last array element edited (+1 as arrays start at 0)
        // if the last array element edited wasnt the end of the array then resize it
        if (currentVertexIndex < finalVerts.Length)
        {
           //Debug.Log("Resizing the final vert array");
            // resize to the size of the current vertex index plus 1
            Array.Resize(ref finalVerts, currentVertexIndex + 1);
            Array.Resize(ref finalColors, currentVertexIndex + 1);
        }

        // convert complete triangles to the final tris array
        finalTris = new int[listOfCompleteTriangles.Count * 3];
        int currentBaseTriID = 0;
        foreach (TriangleData triData in listOfCompleteTriangles)
        {
            // add all the triangle data to the final tri array
            finalTris[currentBaseTriID + 0] = triData.FirstVertID;
            finalTris[currentBaseTriID + 1] = triData.SecondVertID;
            finalTris[currentBaseTriID + 2] = triData.ThirdVertID;

            // increase the base tri id
            currentBaseTriID += 3;
        }

        // finally update the mesh
        UpdateMesh(finalVerts, finalTris, finalColors);
    }
   
    private void OnDrawGizmos()
    {
        // if using show gizmos
        if (viewToolManager && viewToolManager.ShowGizmos)
        {
            if (vertices == null)
            {
                return;
            }

            // draw some spheres for easily seeing the vertices
            for (int i = 0; i < vertices.Length; i++)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(vertices[i], 0.05f);
            }
        }
        
    }

    private int Get2DArrayIndex(int xValue, int yValue, int yMax)
    {
        int returnIndex = 0;

        returnIndex = (yValue * yMax) + xValue;
        returnIndex += 1; // add 1 since index 0 is the origin index

        return returnIndex;
    }

    public void UpdateMaterial()
    {
        // vertex color mode
        if (viewToolManager.UseVertexColorMode)
        {
            meshRenderer.material = toolUserSettings.VertexColorViewMeshMaterial;
        }
        // default view mode
        else
        {
            meshRenderer.material = toolUserSettings.DefualtViewMeshMaterial;
            meshRenderer.material.color = meshColor;
        }
    }


}
