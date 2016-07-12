using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CaveGeneration.MeshGeneration
{
    class WallBuilder
    {
        Vector3[] ceilingVertices;
        int wallsPerTextureTile;
        int wallHeight;
        List<Outline> outlines;

        public MeshData mesh { get; private set; }

        public WallBuilder(Vector3[] ceilingVertices, List<Outline> outlines, int wallsPerTextureTile, int wallHeight)
        {
            this.ceilingVertices = ceilingVertices;
            this.wallsPerTextureTile = wallsPerTextureTile;
            this.wallHeight = wallHeight;
            this.outlines = outlines;
        }

        /// <summary>
        /// Generate the data necessary to produce the wall mesh. Must first run GenerateCeiling. Note that this will
        /// raise the ceiling to accommodate the walls. 
        /// </summary>
        public void Build()
        {
            int outlineEdgeCount = outlines.Select(outline => outline.Count - 1).Sum();
            RaiseCeiling(wallHeight);

            mesh = new MeshData();

            mesh.vertices = GetVertices(outlineEdgeCount);
            mesh.triangles = GetTriangles(outlineEdgeCount);
            mesh.uv = GetUVs(outlineEdgeCount, wallsPerTextureTile);
        }

        void RaiseCeiling(int height)
        {
            for (int i = 0; i < ceilingVertices.Length; i++)
            {
                ceilingVertices[i] += height * Vector3.up;
            }
        }

        Vector3[] GetVertices(int outlineEdgeCount)
        {
            Vector3[] vertices = new Vector3[4 * outlineEdgeCount];
            int vertexIndex = 0;
            foreach (Outline outline in outlines)
            {
                int numEdges = outline.Count - 1;
                for (int i = 0; i < numEdges; i++)
                {
                    Vector3 vertexA = ceilingVertices[outline[i]];
                    Vector3 vertexB = ceilingVertices[outline[i + 1]];
                    AddQuadAtIndex(vertices, vertexIndex, vertexA, vertexB);
                    vertexIndex += 4;
                }
            }
            return vertices;
        }

        void AddQuadAtIndex(Vector3[] vertices, int vertexIndex, Vector3 vertexA, Vector3 vertexB)
        {
            vertices[vertexIndex] = vertexA;
            vertices[vertexIndex + 1] = vertexB;
            vertices[vertexIndex + 2] = new Vector3(vertexA.x, 0f, vertexA.z);
            vertices[vertexIndex + 3] = new Vector3(vertexB.x, 0f, vertexB.z);
        }

        Vector2[] GetUVs(int outlineEdgeCount, int wallsPerTextureTile)
        {
            Vector2[] uv = new Vector2[4 * outlineEdgeCount];
            int vertexIndex = 0;

            foreach (Outline outline in outlines)
            {
                int numEdges = outline.Count - 1;
                for (int i = 0; i < numEdges; i++)
                {
                    float uLeft = i / (float)wallsPerTextureTile;
                    float uRight = (i + 1) / (float)wallsPerTextureTile;
                    AddUVAtIndex(uv, vertexIndex, uLeft, uRight);
                    vertexIndex += 4;
                }
            }

            return uv;
        }

        void AddUVAtIndex(Vector2[] uv, int vertexIndex, float uLeft, float uRight)
        {
            uv[vertexIndex] = new Vector2(uLeft, 1f);
            uv[vertexIndex + 1] = new Vector2(uRight, 1f);
            uv[vertexIndex + 2] = new Vector2(uLeft, 0f);
            uv[vertexIndex + 3] = new Vector2(uRight, 0f);
        }

        int[] GetTriangles(int outlineEdgeCount)
        {
            int[] triangles = new int[6 * outlineEdgeCount];
            int triangleIndex = 0;

            foreach (Outline outline in outlines)
            {
                int numEdges = outline.Count - 1;
                for (int i = 0; i < numEdges; i++)
                {
                    AddTrianglesAtIndex(triangles, triangleIndex);
                    triangleIndex += 6;
                }
            }

            return triangles;
        }

        void AddTrianglesAtIndex(int[] triangles, int index)
        {
            int vertexCount = (index / 6) * 4;

            triangles[index] = vertexCount;
            triangles[index + 1] = vertexCount + 2;
            triangles[index + 2] = vertexCount + 3;

            triangles[index + 3] = vertexCount + 3;
            triangles[index + 4] = vertexCount + 1;
            triangles[index + 5] = vertexCount;
        }
    }
}