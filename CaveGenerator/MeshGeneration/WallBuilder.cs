using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CaveGeneration.MeshGeneration
{
    class WallBuilder : IMeshBuilder
    {
        Vector3[] ceilingVertices;
        int wallHeight;
        List<Outline> outlines;
        MeshData mesh;

        const string name = "Wall Mesh";

        public WallBuilder(Vector3[] ceilingVertices, List<Outline> outlines, int wallHeight)
        {
            this.ceilingVertices = ceilingVertices;
            this.wallHeight = wallHeight;
            this.outlines = outlines;
        }

        /// <summary>
        /// Generate the data necessary to produce the wall mesh. Must first run GenerateCeiling. Note that this will
        /// raise the ceiling to accommodate the walls. 
        /// </summary>
        public MeshData Build()
        {
            int outlineEdgeCount = outlines.Select(outline => outline.Count - 1).Sum();
            int outlineVertexCount = outlineEdgeCount + outlines.Count;
            RaiseCeiling(wallHeight);

            mesh = new MeshData();
            mesh.name = name;
            mesh.vertices = GetVertices(outlineVertexCount);
            mesh.triangles = GetTriangles(outlineEdgeCount);
            mesh.uv = GetUVs(outlineVertexCount);
            return mesh;
        }

        void RaiseCeiling(int height)
        {
            for (int i = 0; i < ceilingVertices.Length; i++)
            {
                ceilingVertices[i].y += height;
            }
        }

        Vector3[] GetVertices(int outlineVertexCount)
        {
            Vector3[] vertices = new Vector3[2 * outlineVertexCount];
            int vertexIndex = 0;
            foreach (Outline outline in outlines)
            {
                for (int i = 0; i < outline.Count; i++)
                {
                    Vector3 vertex = ceilingVertices[outline[i]];
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
                for (int i = 0; i < outline.Count; i++)
                {
                    xPercentage += ComputeDistanceTo(outline, i) * increment;
                    uv[vertexIndex] = new Vector2(xPercentage, 1f);
                    uv[vertexIndex + 1] = new Vector2(xPercentage, 0f);
                    vertexIndex += 2;
                }
            }
            return uv;
        }

        float ComputeUVIncrement(Outline outline)
        {
            int scale = wallHeight;
            float outlineDistance = ComputeOutlineDistance(outline);
            float increment = (1f / (outlineDistance)) * (int)(outlineDistance / scale);
            return increment;
        }

        float ComputeDistanceTo(Outline outline, int index)
        {
            if (index == 0)
            {
                return 0;
            }
            return Vector3.Distance(ceilingVertices[outline[index]], ceilingVertices[outline[index - 1]]);
        }

        float ComputeOutlineDistance(Outline outline)
        {
            float distance = 0;
            for (int i = 0; i < outline.Count; i++)
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
                int numEdges = outline.Count - 1;
                for (int i = 0; i < numEdges; i++, triangleIndex+=6, currentVertex+=2) 
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