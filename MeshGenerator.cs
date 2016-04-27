using MeshHelpers;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class MeshGenerator : MonoBehaviour {

    List<Vector3> baseVertices = new List<Vector3>();
    List<int> meshTriangles = new List<int>();
    Dictionary<int, List<Triangle>> triangleMap = new Dictionary<int, List<Triangle>>();
    List<Outline> outlines = new List<Outline>();
    bool[] checkedVertices;

    Map map;

    [SerializeField]
    Vector2 ceilingTextureDimensions = new Vector2(100f, 100f);

    public MapMeshes Generate3D(Map map)
    {
        Mesh ceilingMesh = Generate(map);
        Mesh wallMesh = CreateWallMesh(map.wallHeight);
        return new MapMeshes(ceilingMesh, wallMesh);
    }

    public MapMeshes Generate2D(Map map)
    {
        Mesh ceilingMesh = Generate(map);
        return new MapMeshes(ceilingMesh);
    }

    Mesh Generate(Map map)
    {
        Clear();
        this.map = map;
        TriangulateSquares();
        Mesh ceilingMesh = CreateCeilingMesh();
        CalculateMeshOutlines();
        return ceilingMesh;
    }

    void TriangulateSquares()
    {
        SquareGrid squareGrid = new SquareGrid(map);
        {
            for (int x = 0; x < squareGrid.GetLength(0); x++)
            {
                for (int y = 0; y < squareGrid.GetLength(1); y++)
                {
                    TriangulateSquare(squareGrid[x, y]);
                }
            }
        }
    }

    void TriangulateSquare(Square square)
    {
        Node[] points = square.GetPoints();
        AssignVertices(points);
        MeshFromPoints(points);
    }

    void AssignVertices(Node[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].vertexIndex == -1)
            {
                points[i].vertexIndex = baseVertices.Count;
                baseVertices.Add(points[i].position);
            }
        }
    }

    void MeshFromPoints(Node[] points)
    {
        if (points.Length >= 3)
            CreateTriangle(points[0], points[1], points[2]);
        if (points.Length >= 4)
            CreateTriangle(points[0], points[2], points[3]);
        if (points.Length >= 5)
            CreateTriangle(points[0], points[3], points[4]);
        if (points.Length >= 6)
            CreateTriangle(points[0], points[4], points[5]);
    }

    void CreateTriangle(Node a, Node b, Node c)
    {
        Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
        for (int i = 0; i < 3; i++)
        {
            meshTriangles.Add(triangle[i]);
            AddTriangleToDictionary(triangle[i], triangle);
        }
    }

    void AddTriangleToDictionary(int vertexIndex, Triangle triangle)
    {
        if (triangleMap.ContainsKey(vertexIndex))
        {
            triangleMap[vertexIndex].Add(triangle);
        }
        else
        {
            triangleMap.Add(vertexIndex, new List<Triangle> { triangle });
        }
    }

    Mesh CreateCeilingMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = baseVertices.ToArray();
        mesh.triangles = meshTriangles.ToArray();
        mesh.RecalculateNormals();
        mesh.uv = ComputeCeilingUVArray();
        mesh.name = "Ceiling Mesh" + map.index;
        return mesh;
    }

    Vector2[] ComputeCeilingUVArray()
    {
        Vector2[] uv = new Vector2[baseVertices.Count];
        float xMax = ceilingTextureDimensions.x;
        float yMax = ceilingTextureDimensions.y;
        for (int i = 0; i < baseVertices.Count; i++)
        {
            float percentX = baseVertices[i].x / xMax;
            float percentY = baseVertices[i].z / yMax;
            uv[i] = new Vector2(percentX, percentY);
        }
        return uv;
    }

    Mesh CreateWallMesh(int height)
    {
        int outlineParameter = outlines.Select(x => x.Size() - 1).Sum();
        Vector3[] wallVertices = new Vector3[4 * outlineParameter];
        Vector2[] uv = new Vector2[4 * outlineParameter];
        int[] wallTriangles = new int[6 * outlineParameter];

        int vertexCount = 0;
        int triangleCount = 0;
        foreach (Outline outline in outlines)
        {
            for (int i = 0; i < outline.Size() - 1; i++)
            {
                wallVertices[vertexCount] = baseVertices[outline[i]];
                wallVertices[vertexCount + 1] = baseVertices[outline[i + 1]];
                wallVertices[vertexCount + 2] = baseVertices[outline[i]] - Vector3.up * height;
                wallVertices[vertexCount + 3] = baseVertices[outline[i + 1]] - Vector3.up * height;

                uv[vertexCount] = new Vector2(0f, 1f);
                uv[vertexCount + 1] = new Vector2(1f, 1f);
                uv[vertexCount + 2] = new Vector2(0f, 0f);
                uv[vertexCount + 3] = new Vector2(1f, 0f);

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

        Mesh wallMesh = new Mesh();
        wallMesh.vertices = wallVertices;
        wallMesh.triangles = wallTriangles;
        wallMesh.RecalculateNormals();
        wallMesh.uv = uv;
        wallMesh.name = "Wall Mesh" + map.index;
        return wallMesh;
    }

    void CalculateMeshOutlines()
    {
        checkedVertices = new bool[baseVertices.Count];
        for (int startVertexIndex = 0; startVertexIndex < baseVertices.Count; startVertexIndex++)
        {
            if (!checkedVertices[startVertexIndex])
            {
                checkedVertices[startVertexIndex] = true;
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
        checkedVertices[vertexIndex] = true;
        int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex, outline.Size());
        FollowOutline(nextVertexIndex, outline);
    }

    int GetConnectedOutlineVertex(int currentIndex, int outlineSize)
    {
        List<Triangle> trianglesContainingVertex = triangleMap[currentIndex];
        foreach (Triangle triangle in trianglesContainingVertex)
        {
            for (int j = 0; j < 3; j++)
            {
                int nextIndex = triangle[j];
                bool foundNewOutlineEdge = !checkedVertices[nextIndex] && IsOutlineEdge(currentIndex, nextIndex);
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
        List<Triangle> trianglesContainingVertexA = triangleMap[vertexA];
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

    bool IsCorrectOrientation(int indexOne, int indexTwo, Triangle triangle)
    {
        int indexThree = triangle.GetThirdPoint(indexOne, indexTwo);
        return IsRightOf(baseVertices[indexOne], baseVertices[indexTwo], baseVertices[indexThree]);
    }

    bool IsRightOf(Vector3 a, Vector3 b, Vector3 c)
    {
        return ((b.x - a.x) * (c.z - a.z) - (b.z - a.z) * (c.x - a.x)) < 0;
    }

    public List<Vector2[]> Generate2DColliders()
    {
        EdgeCollider2D[] currentColliders = gameObject.GetComponents<EdgeCollider2D>();
        foreach (EdgeCollider2D collider in currentColliders)
        {
            Destroy(collider);
        }
        List<Vector2[]> edgePointLists = new List<Vector2[]>();
        foreach (Outline outline in outlines)
        {
            Vector2[] edgePoints = new Vector2[outline.Size()];
            for (int i = 0; i < outline.Size(); i++)
            {
                edgePoints[i] = new Vector2(baseVertices[outline[i]].x, baseVertices[outline[i]].z);
            }
            edgePointLists.Add(edgePoints);
        }
        return edgePointLists;
    }

    void Clear()
    {
        triangleMap.Clear();
        meshTriangles.Clear();
        baseVertices.Clear();
        outlines.Clear();
    }
}
