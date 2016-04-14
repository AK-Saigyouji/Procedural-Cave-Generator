using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using MeshHelpers;

public class MeshGenerator : MonoBehaviour {

    GameObject ceiling;
    [SerializeField]
    Material ceilingMaterial;

    GameObject walls;
    [SerializeField]
    Material wallMaterial;

    [SerializeField]
    bool is2D;

    float squareSize;
    List<Vector3> baseVertices = new List<Vector3>();
    List<int> meshTriangles = new List<int>();
    Dictionary<int, List<Triangle>> triangleMap = new Dictionary<int, List<Triangle>>();
    List<Outline> outlines = new List<Outline>();
    bool[] checkedVertices;

    int wallHeight;
    Map map;

    int TEXTURE_REPETITION_FACTOR = 1;

    internal void GenerateMesh(Map map, int wallHeight = 1)
    {
        DestroyChildren();
        this.map = map;
        this.wallHeight = wallHeight;

        IList<Map> subMaps = map.SubdivideMap();
        foreach (Map subMap in subMaps)
        {
            triangleMap.Clear();
            meshTriangles.Clear();
            baseVertices.Clear();
            outlines.Clear();

            TriangulateSquares(subMap);
            Mesh baseMesh = CreateBaseMesh();
            SetBaseMesh(baseMesh);

            CalculateMeshOutlines();

            if (is2D)
            {
                Generate2DColliders();
            }
            else
            {
                Mesh wallMesh = CreateWallMesh();
                SetWallMesh(wallMesh);
                MeshCollider wallCollider = walls.gameObject.AddComponent<MeshCollider>();
                wallCollider.sharedMesh = wallMesh;
            }
        }
    }

    void TriangulateSquares(Map map)
    {
        SquareGrid squareGrid = new SquareGrid(map);
        for (int x = 0; x < squareGrid.GetLength(0); x++)
            for (int y = 0; y < squareGrid.GetLength(1); y++)
                TriangulateSquare(squareGrid[x, y]);
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

    Mesh CreateBaseMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = baseVertices.ToArray();
        mesh.triangles = meshTriangles.ToArray();
        mesh.RecalculateNormals();
        Vector2[] uv = new Vector2[baseVertices.Count];
        for (int i = 0; i < baseVertices.Count; i++)
        {
            float percentX = Mathf.InverseLerp(0, map.scaledLength, baseVertices[i].x) * TEXTURE_REPETITION_FACTOR;
            float percentY = Mathf.InverseLerp(0, map.scaledWidth, baseVertices[i].z) * TEXTURE_REPETITION_FACTOR;
            uv[i] = new Vector2(percentX, percentY);
        }
        mesh.uv = uv;
        return mesh;
    }

    void SetBaseMesh(Mesh mesh)
    {
        ceiling = new GameObject("ceiling", typeof(MeshRenderer), typeof(MeshFilter));
        ceiling.transform.parent = transform;
        if (is2D)
        {
            ceiling.transform.localRotation = Quaternion.Euler(270f, 0f, 0f);
        }
        ceiling.GetComponent<MeshFilter>().mesh = mesh;
        ceiling.GetComponent<MeshRenderer>().material = ceilingMaterial;
    }

    Mesh CreateWallMesh()
    {
        int outlineParameter = outlines.Select(x => x.Size() - 1).Sum();
        Vector3[] wallVertices = new Vector3[4 * outlineParameter];
        int[] wallTriangles = new int[6 * outlineParameter];

        int vertexCount = 0;
        int triangleCount = 0;
        foreach (Outline outline in outlines)
        {
            for (int i = 0; i < outline.Size() - 1; i++)
            {
                wallVertices[vertexCount] = baseVertices[outline[i]];
                wallVertices[vertexCount + 1] = baseVertices[outline[i+1]];
                wallVertices[vertexCount + 2] = baseVertices[outline[i]] - Vector3.up * wallHeight;
                wallVertices[vertexCount + 3] = baseVertices[outline[i + 1]] - Vector3.up * wallHeight;

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
        return wallMesh;
    }

    void SetWallMesh(Mesh mesh)
    {
        walls = new GameObject("wall", typeof(MeshRenderer), typeof(MeshFilter));
        walls.transform.parent = gameObject.transform;
        walls.GetComponent<MeshFilter>().mesh = mesh;
        walls.GetComponent<MeshRenderer>().material = wallMaterial;
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
                if (outline.Size() > 2)
                {
                    outlines.Add(outline);
                }
            }
        }
    }

    Outline GenerateOutlineFromPoint(int startVertexIndex)
    {
        int nextVertexIndex = GetConnectedOutlineVertex(startVertexIndex, 0);
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

    int GetConnectedOutlineVertex(int indexOne, int outlineSize)
    {
        List<Triangle> trianglesContainingVertex = triangleMap[indexOne];
        foreach (Triangle triangle in trianglesContainingVertex)
        {
            for (int j = 0; j < 3; j++)
            {
                int indexTwo = triangle[j];
                bool foundNewOutlineEdge = !checkedVertices[indexTwo] && IsOutlineEdge(indexOne, indexTwo);
                if (foundNewOutlineEdge)
                {
                    if (outlineSize > 0 || IsCorrectOrientation(indexOne, indexTwo, triangle))
                    {
                        return indexTwo;
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
        return IsRight(baseVertices[indexOne], baseVertices[indexTwo], baseVertices[indexThree]);
    }

    bool IsRight(Vector3 a, Vector3 b, Vector3 c)
    {
        return ((b.x - a.x) * (c.z - a.z) - (b.z - a.z) * (c.x - a.x)) < 0;
    }

    void DestroyChildren()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    void Generate2DColliders()
    {

        EdgeCollider2D[] currentColliders = gameObject.GetComponents<EdgeCollider2D>();
        foreach (EdgeCollider2D collider in currentColliders)
        {
            Destroy(collider);
        }
        foreach (Outline outline in outlines)
        {
            EdgeCollider2D edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
            Vector2[] edgePoints = new Vector2[outline.Size()];
            for (int i = 0; i < outline.Size(); i++)
            {
                edgePoints[i] = new Vector2(baseVertices[outline[i]].x, baseVertices[outline[i]].z);
            }
            edgeCollider.points = edgePoints;
        }
    }
}
