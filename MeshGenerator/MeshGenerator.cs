using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using MeshHelpers;

public class MeshGenerator : MonoBehaviour {

    [SerializeField]
    MeshFilter walls;

    float squareSize;
    List<Vector3> vertices;
    List<int> triangles;
    Dictionary<int, List<Triangle>> triangleMap;
    List<Outline> outlines;
    bool[] checkedVertices;

    int WALL_HEIGHT = 5;

    void Awake()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        triangleMap = new Dictionary<int, List<Triangle>>();
        outlines = new List<Outline>();
    }

    public void generateMesh(int[,] map, float squareSize = 1f)
    {
        TriangulateSquares(map, squareSize);
        SetMesh();
        CalculateMeshOutlines();
        CreateWallMesh();
        vertices.Clear();
        triangles.Clear();
        triangleMap.Clear();
        outlines.Clear();
    }

    void TriangulateSquares(int[,] map, float squareSize)
    {
        SquareGrid squareGrid = new SquareGrid(map, squareSize);
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
                points[i].vertexIndex = vertices.Count;
                vertices.Add(points[i].position);
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
            triangles.Add(triangle[i]);
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

    void SetMesh()
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    void CreateWallMesh()
    {
        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangles = new List<int>();
        Mesh wallMesh = new Mesh();

        foreach (Outline outline in outlines)
        {
            for (int i = 0; i < outline.Count - 1; i++)
            {
                int startIndex = wallVertices.Count;
                wallVertices.Add(vertices[outline[i]]);
                wallVertices.Add(vertices[outline[i+1]]);
                wallVertices.Add(vertices[outline[i]] - Vector3.up * WALL_HEIGHT);
                wallVertices.Add(vertices[outline[i + 1]] - Vector3.up * WALL_HEIGHT);

                wallTriangles.Add(startIndex);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);

                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex);
            }
        }
        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();
        walls.mesh = wallMesh;
    }

    void CalculateMeshOutlines()
    {
        checkedVertices = new bool[vertices.Count];
        for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
        {
            if (!checkedVertices[vertexIndex])
            {
                checkedVertices[vertexIndex] = true;
                int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
                if (newOutlineVertex != -1)
                {
                    Outline outline = new Outline(vertexIndex);
                    outlines.Add(outline);
                    FollowOutline(newOutlineVertex, outline);
                    outline.Add(vertexIndex);
                }
            }
        }
    }

    void FollowOutline(int vertexIndex, Outline outline)
    {
        if (vertexIndex == -1)
            return;
        outline.Add(vertexIndex);
        checkedVertices[vertexIndex] = true;
        int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);
        FollowOutline(nextVertexIndex, outline);
    }

    int GetConnectedOutlineVertex(int vertexIndex)
    {
        List<Triangle> trianglesContainingVertex = triangleMap[vertexIndex];
        foreach (Triangle triangle in trianglesContainingVertex)
        {
            for (int j = 0; j < 3; j++)
            {
                int vertexB = triangle[j];
                if ((vertexB != vertexIndex) && !checkedVertices[vertexB] && IsOutlineEdge(vertexIndex, vertexB))
                {
                    return vertexB;
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

    class Outline : List<int>
    {
        internal Outline(int vertexIndex)
        {
            Add(vertexIndex);
        }
    }

    struct Triangle
    {
        public int a;
        public int b;
        public int c;

        internal Triangle(int a, int b, int c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        internal bool Contains(int vertex)
        {
            return (vertex == a) || (vertex == b) || (vertex == c);
        }

        internal int this[int i]
        {
            get
            {
                if (i == 0)
                    return a;
                else if (i == 1)
                    return b;
                else if (i == 2)
                    return c;
                else
                    throw new System.ArgumentOutOfRangeException();
            }
        }
    }
}
