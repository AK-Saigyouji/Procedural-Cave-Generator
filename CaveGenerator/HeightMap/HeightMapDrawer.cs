using UnityEngine;
using System.Collections;
using System;

public class HeightMapDrawer {

    const int width = 100;
    const int length = 100;

    Vector3[] vertices = new Vector3[(length - 1) * (width - 1) * 4];
    int[] triangles = new int[(length - 1) * (width - 1) * 6];
    Func<float, float, float> heightMap;

    public Mesh mesh { get; private set; }

    public HeightMapDrawer(Func<float, float, float> heightMap)
    {
        this.heightMap = heightMap;
        mesh = new Mesh();
        UpdateVertices();
        UpdateTriangles();
    }

    public void BuildMesh(Func<float, float, float> heightMap)
    {
        this.heightMap = heightMap;
        UpdateVertices();
        mesh.RecalculateNormals();
    }

    void UpdateVertices()
    {
        int vertexIndex = 0;
        for (int x = 0; x < length - 1; x++)
        {
            for (int y = 0; y < width - 1; y++)
            {
                vertices[vertexIndex] = GetVertex(x, y);
                vertices[vertexIndex + 1] = GetVertex(x, y + 1);
                vertices[vertexIndex + 2] = GetVertex(x + 1, y + 1);
                vertices[vertexIndex + 3] = GetVertex(x + 1, y);
                vertexIndex += 4;
            }
        }
        mesh.vertices = vertices;
    }

    Vector3 GetVertex(int x, int y)
    {
        return new Vector3(x, heightMap(x, y), y);
    }

    void UpdateTriangles()
    {
        int triangleIndex = 0;
        int vertexIndex = 0;
        for (int x = 0; x < length - 1; x++)
        {
            for (int y = 0; y < width - 1; y++)
            {
                triangles[triangleIndex] = vertexIndex;
                triangles[triangleIndex + 1] = vertexIndex + 1;
                triangles[triangleIndex + 2] = vertexIndex + 2;

                triangles[triangleIndex + 3] = vertexIndex;
                triangles[triangleIndex + 4] = vertexIndex + 2;
                triangles[triangleIndex + 5] = vertexIndex + 3;
                triangleIndex += 6;
                vertexIndex += 4;
            }
        }
        mesh.triangles = triangles;
    }
}
