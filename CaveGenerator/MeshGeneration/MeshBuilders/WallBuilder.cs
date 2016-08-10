﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CaveGeneration.MeshGeneration
{
    class WallBuilder : IMeshBuilder
    {
        Vector3[] vertices;
        List<Outline> outlines;
        MeshData mesh;

        const float UVSCALE = 3f;
        const string name = "Wall Mesh";

        public WallBuilder(Vector3[] vertices, List<Outline> outlines)
        {
            this.vertices = vertices;
            this.outlines = outlines;
        }

        public MeshData Build()
        {
            CreateMesh();
            return mesh;
        }

        void CreateMesh()
        {
            int outlineEdgeCount = outlines.Select(outline => outline.Length - 1).Sum();
            int outlineVertexCount = outlineEdgeCount + outlines.Count;

            mesh = new MeshData();
            mesh.name = name;
            mesh.vertices = GetVertices(outlineVertexCount);
            mesh.triangles = GetTriangles(outlineEdgeCount);
            mesh.uv = GetUVs(outlineVertexCount);
        }

        Vector3[] GetVertices(int outlineVertexCount)
        {
            Vector3[] vertices = new Vector3[2 * outlineVertexCount];
            int vertexIndex = 0;
            foreach (Outline outline in outlines)
            {
                for (int i = 0; i < outline.Length; i++)
                {
                    Vector3 vertex = this.vertices[outline[i]];
                    vertices[vertexIndex] = vertex;
                    vertices[vertexIndex + 1] = new Vector3(vertex.x, 0f, vertex.z);
                    vertexIndex += 2;
                }
            }
            return vertices;
        }

        Vector2[] GetUVs(int outlineEdgeCount)
        {
            Vector2[] uv = new Vector2[2 * outlineEdgeCount];
            int vertexIndex = 0;

            foreach (Outline outline in outlines)
            {
                float xPercentage = 0f;
                float increment = ComputeUVIncrement(outline);
                for (int i = 0; i < outline.Length; i++, vertexIndex += 2)
                {
                    xPercentage += ComputeDistanceTo(outline, i) * increment;
                    float yPercentage = vertices[outline[i]].y / UVSCALE;
                    uv[vertexIndex] = new Vector2(xPercentage, yPercentage);
                    uv[vertexIndex + 1] = new Vector2(xPercentage, 0f);
                }
            }
            return uv;
        }

        float ComputeUVIncrement(Outline outline)
        {
            float outlineDistance = ComputeOutlineDistance(outline);
            float increment = ((int)(outlineDistance / UVSCALE)) / outlineDistance;
            return increment;
        }

        float ComputeDistanceTo(Outline outline, int index)
        {
            if (index == 0)
            {
                return 0;
            }
            Vector3 vectorA = vertices[outline[index]];
            Vector3 vectorB = vertices[outline[index - 1]];
            return Distance2D(vectorA, vectorB);
        }

        // Compute the distance between the projections of the vectors along the y-axis.
        float Distance2D(Vector3 a, Vector3 b)
        {
            a.y = 0;
            b.y = 0;
            return Vector3.Distance(a, b);
        }

        float ComputeOutlineDistance(Outline outline)
        {
            float distance = 0;
            for (int i = 0; i < outline.Length; i++)
            {
                distance += ComputeDistanceTo(outline, i);
            }
            return distance;
        }

        int[] GetTriangles(int outlineEdgeCount)
        {
            int[] triangles = new int[6 * outlineEdgeCount];
            int triangleIndex = 0;
            int currentVertex = 0;

            foreach (Outline outline in outlines)
            {
                int numEdges = outline.Length - 1;
                for (int i = 0; i < numEdges; i++, triangleIndex += 6, currentVertex += 2)
                {
                    AddQuadAtIndex(triangles, triangleIndex, currentVertex);
                }
                currentVertex += 2; // skip the two vertices at the end of an outline for the seam
            }
            return triangles;
        }

        void AddQuadAtIndex(int[] triangles, int triangleIndex, int vertexIndex)
        {
            triangles[triangleIndex] = vertexIndex;
            triangles[triangleIndex + 1] = vertexIndex + 1;
            triangles[triangleIndex + 2] = vertexIndex + 3;

            triangles[triangleIndex + 3] = vertexIndex + 3;
            triangles[triangleIndex + 4] = vertexIndex + 2;
            triangles[triangleIndex + 5] = vertexIndex;
        }
    }
}