using MeshHelpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Produces meshes and colliders for Map objects using the marching squares algorithm. 
/// Break large maps (max of 100 by 100 recommended - beyond 200 by 200 likely to produde exceptions) into 
/// smaller maps before generating meshes.
/// </summary>
public class MeshGenerator
{
    List<Vector3> ceilingVertices = new List<Vector3>();
    List<int> ceilingTriangles = new List<int>();
    Vector2[] ceilingUV;

    Vector3[] wallVertices;
    int[] wallTriangles;
    Vector2[] wallUV;

    List<Triangle>[] vertexIndexToTriangleMap;
    List<Outline> outlines = new List<Outline>();
    bool[] checkedOutlineVertices;

    Map map;

    /// <summary>
    /// Generate the data necessary to produce the ceiling mesh. Safe to run on background threads.
    /// </summary>
    public void GenerateCeiling(Map map, Vector2 ceilingTextureDimensions)
    {
        this.map = map;
        TriangulateMap();
        CalculateMeshOutlines();
        ComputeCeilingUVArray(ceilingTextureDimensions);
    }

    /// <summary>
    /// Create and return the ceiling mesh. Must first run GenerateCeiling to populate the data.
    /// </summary>
    /// <returns></returns>
    public Mesh CreateCeilingMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = ceilingVertices.ToArray();
        mesh.triangles = ceilingTriangles.ToArray();
        mesh.RecalculateNormals();
        mesh.uv = ceilingUV;
        mesh.name = "Ceiling Mesh" + map.index;
        return mesh;
    }

    /// <summary>
    /// Create and return the wall 3D wall mesh. Must first run GenerateWalls.
    /// </summary>
    public Mesh CreateWallMesh()
    {
        Mesh wallMesh = new Mesh();
        wallMesh.vertices = wallVertices;
        wallMesh.triangles = wallTriangles;
        wallMesh.RecalculateNormals();
        wallMesh.uv = wallUV;
        wallMesh.name = "Wall Mesh" + map.index;
        return wallMesh;
    }

    /// <summary>
    /// Generates a list of 2D points for the creation of edge colliders along 2D boundaries in the cave.
    /// </summary>
    /// <returns>Returns a list of Vector2 points indicating where edge colliders should be placed.</returns>
    public List<Vector2[]> GenerateColliderEdges()
    {
        List<Vector2[]> edgePointLists = new List<Vector2[]>();
        foreach (Outline outline in outlines)
        {
            Vector2[] edgePoints = new Vector2[outline.size];
            for (int i = 0; i < outline.size; i++)
            {
                edgePoints[i] = new Vector2(ceilingVertices[outline[i]].x, ceilingVertices[outline[i]].z);
            }
            edgePointLists.Add(edgePoints);
        }
        return edgePointLists;
    }

    /// <summary>
    /// Generate the data necessary to produce the wall mesh. Must first run GenerateCeiling. 
    /// </summary>
    public void GenerateWalls(int height, int wallsPerTextureTile)
    {
        int outlineParameter = outlines.Select(x => x.size - 1).Sum();
        Vector3[] wallVertices = new Vector3[4 * outlineParameter];
        Vector2[] uv = new Vector2[4 * outlineParameter];
        int[] wallTriangles = new int[6 * outlineParameter];

        int vertexCount = 0;
        int triangleCount = 0;
        // Run along each outline, and create a quad between each pair of points in the outline.
        foreach (Outline outline in outlines)
        {
            for (int i = 0; i < outline.size - 1; i++)
            {
                wallVertices[vertexCount] = ceilingVertices[outline[i]];
                wallVertices[vertexCount + 1] = ceilingVertices[outline[i + 1]];
                wallVertices[vertexCount + 2] = ceilingVertices[outline[i]] - Vector3.up * height;
                wallVertices[vertexCount + 3] = ceilingVertices[outline[i + 1]] - Vector3.up * height;

                // This uv configuration ensures that the texture gets tiled once every wallsPerTextureTile quads in the 
                // horizontal direction.
                float uLeft = i / (float)wallsPerTextureTile;
                float uRight = (i + 1) / (float)wallsPerTextureTile;
                uv[vertexCount] = new Vector2(uLeft, 1f);
                uv[vertexCount + 1] = new Vector2(uRight, 1f);
                uv[vertexCount + 2] = new Vector2(uLeft, 0f);
                uv[vertexCount + 3] = new Vector2(uRight, 0f);

                wallTriangles[triangleCount] = vertexCount;
                wallTriangles[triangleCount + 1] = vertexCount + 2;
                wallTriangles[triangleCount + 2] = vertexCount + 3;

                wallTriangles[triangleCount + 3] = vertexCount + 3;
                wallTriangles[triangleCount + 4] = vertexCount + 1;
                wallTriangles[triangleCount + 5] = vertexCount;
                vertexCount += 4;
                triangleCount += 6;
            }
        }
        this.wallVertices = wallVertices;
        this.wallTriangles = wallTriangles;
        this.wallUV = uv;
    }

    /// <summary>
    /// Triangulates the squares according to the marching squares algorithm.
    /// In the process, this method populates the baseVertices, triangleMap and and meshTriangles collections.
    /// </summary>
    void TriangulateMap()
    {
        MapTriangulator mapTriangulator = new MapTriangulator();
        mapTriangulator.Triangulate(map);

        ceilingVertices = mapTriangulator.vertices;
        ceilingTriangles = mapTriangulator.triangles;
        vertexIndexToTriangleMap = mapTriangulator.vertexIndexToTriangles;
    }

    void ComputeCeilingUVArray(Vector2 textureDimensions)
    {
        Vector2[] uv = new Vector2[ceilingVertices.Count];
        float xMax = textureDimensions.x;
        float yMax = textureDimensions.y;
        for (int i = 0; i < ceilingVertices.Count; i++)
        {
            float percentX = ceilingVertices[i].x / xMax;
            float percentY = ceilingVertices[i].z / yMax;
            uv[i] = new Vector2(percentX, percentY);
        }
        ceilingUV = uv;
    }

    void CalculateMeshOutlines()
    {
        checkedOutlineVertices = new bool[ceilingVertices.Count];
        for (int startVertexIndex = 0; startVertexIndex < ceilingVertices.Count; startVertexIndex++)
        {
            if (!checkedOutlineVertices[startVertexIndex])
            {
                checkedOutlineVertices[startVertexIndex] = true;
                Outline outline = GenerateOutlineFromPoint(startVertexIndex);
                if (outline != null)
                {
                    outlines.Add(outline);
                }
            }
        }
    }

    Outline GenerateOutlineFromPoint(int startVertexIndex)
    {
        int nextVertexIndex = GetConnectedOutlineVertex(startVertexIndex, 0);
        if (nextVertexIndex == -1)
            return null;

        Outline outline = new Outline(startVertexIndex);
        FollowOutline(nextVertexIndex, outline);
        outline.Add(startVertexIndex);
        return outline;
    }

    void FollowOutline(int vertexIndex, Outline outline)
    {
        if (vertexIndex == -1)
            return;
        outline.Add(vertexIndex);
        checkedOutlineVertices[vertexIndex] = true;
        int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex, outline.size);
        FollowOutline(nextVertexIndex, outline);
    }

    int GetConnectedOutlineVertex(int currentIndex, int outlineSize)
    {
        List<Triangle> trianglesContainingVertex = vertexIndexToTriangleMap[currentIndex];
        foreach (Triangle triangle in trianglesContainingVertex)
        {
            for (int j = 0; j < 3; j++)
            {
                int nextIndex = triangle[j];
                bool foundNewOutlineEdge = !checkedOutlineVertices[nextIndex] && IsOutlineEdge(currentIndex, nextIndex);
                if (foundNewOutlineEdge)
                {
                    if (outlineSize > 0 || IsCorrectOrientation(currentIndex, nextIndex, triangle))
                    {
                        return nextIndex;
                    }
                }
            }
        }
        return -1;
    }

    bool IsOutlineEdge(int vertexA, int vertexB)
    {
        List<Triangle> trianglesContainingVertexA = vertexIndexToTriangleMap[vertexA];
        int sharedTriangleCount = 0;

        foreach (Triangle triangle in trianglesContainingVertexA)
        {
            if (triangle.Contains(vertexB))
            {
                sharedTriangleCount++;
                if (sharedTriangleCount > 1)
                    return false;
            }
        }
        return sharedTriangleCount == 1;
    }

    /// <summary>
    /// Will these indices produce an Outline going in the right direction? The direction of the Outline will determine
    /// whether the walls are visible.
    /// </summary>
    /// <param name="indexOne">The starting index.</param>
    /// <param name="indexTwo">The discovered index in question.</param>
    /// <param name="triangle">A triangle containing both indices.</param>
    /// <returns>Returns whether using the second index will result in a correctly oriented Outline.</returns>
    bool IsCorrectOrientation(int indexOne, int indexTwo, Triangle triangle)
    {
        int indexThree = triangle.GetThirdPoint(indexOne, indexTwo);
        return IsRightOf(ceilingVertices[indexOne], ceilingVertices[indexTwo], ceilingVertices[indexThree]);
    }

    /// <summary>
    /// Is the vector c positioned "to the right of" the line formed by a and b?
    /// </summary>
    /// <returns>Returns whether the vector c is positioned to the right of the line formed by a and b.</returns>
    bool IsRightOf(Vector3 a, Vector3 b, Vector3 c)
    {
        return ((b.x - a.x) * (c.z - a.z) - (b.z - a.z) * (c.x - a.x)) < 0;
    }
}