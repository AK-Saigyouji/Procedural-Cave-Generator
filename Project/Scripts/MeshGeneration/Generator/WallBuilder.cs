/* The task of building the walls is sufficiently complicated that it warranted a separate class. The basic
 idea is simple: taking a 2D outline of the walls, build a quad on each edge of the outline. The difficult
 part is the task of assigning texture coordinates.

 The task of determining the outlines for a grid is delegated to a separate class.
*/

using AKSaigyouji.HeightMaps;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace AKSaigyouji.MeshGeneration
{
    sealed class WallBuilder
    {
        const float UV_SCALE = 10f;
        const int VERTS_PER_CORNER = 2;

        readonly List<Vector3[]> outlines;
        readonly IHeightMap floorHeightMap;
        readonly IHeightMap ceilingHeightMap;

        public WallBuilder(List<Vector3[]> outlines, IHeightMap floor, IHeightMap ceiling)
        {
            this.outlines = outlines;
            floorHeightMap = floor;
            ceilingHeightMap = ceiling;
        }

        public MeshData Build()
        {
            MeshData mesh = new MeshData();
            mesh.vertices = GetVertices();
            mesh.uv = GetUVs(mesh.vertices);
            mesh.triangles = GetTriangles();
            return mesh;
        }

        Vector3[] GetVertices()
        {
            int numWallVertices = VERTS_PER_CORNER * outlines.Sum(outline => outline.Length);
            var vertices = new Vector3[numWallVertices];

            int vertexIndex = 0;
            foreach (Vector3 vertex in outlines.SelectMany(outline => outline))
            {
                float x = vertex.x;
                float z = vertex.z;
                float floorHeight = floorHeightMap.GetHeight(x, z);
                float ceilingHeight = ceilingHeightMap.GetHeight(x, z);
                for (int i = 0; i < VERTS_PER_CORNER; i++)
                {
                    float interpolation = (float)i / (VERTS_PER_CORNER - 1);
                    float interpolatedHeight = interpolation * floorHeight + (1 - interpolation) * ceilingHeight;
                    vertices[vertexIndex++] = new Vector3(x, interpolatedHeight, z);
                }
            }
            return vertices;
        }

        /* Computing the UV array for the walls proves to be a tricky matter. Unlike other meshes such as the floor,
         * it is not possible to consistently determine the UV for walls based purely on global coordinates. On top of that,
         * there are two extra challenges. Walls are built on top of outline edges. But the length of 
         * outline edges is inconsistent. So it's necessary to incorporate length in computing the u coordinate. 
         * The second challenge is ensuring that the start and end of an outline line up.
         * 
         * The following implementation addresses both issues. To address the first, we base the u coordinate
         * for each point in the outline on the length of the outline so far. To address the second, we scale
         * this value by a quantity (uvIncrement) which ensures that the final u coordinate will be an integer,
         * so that the texture will tile seamlessly. uvIncrement also serves the purpose of reducing the growth of u
         * by a constant factor so that it doesn't tile too rapidly.
         */

        Vector2[] GetUVs(Vector3[] vertices)
        {
            var uv = new Vector2[vertices.Length];
            int vertexIndex = 0;

            foreach (Vector3[] outline in outlines)
            {
                float u = 0f;
                float increment = ComputeUVIncrement(outline);
                for (int i = 0; i < outline.Length; i++)
                {
                    u += ComputeEdgeLength(outline, i) * increment;
                    for (int j = 0; j < VERTS_PER_CORNER; j++, vertexIndex++)
                    {
                        uv[vertexIndex] = new Vector2(u, vertices[vertexIndex].y / UV_SCALE);
                    }
                }
            }
            return uv;
        }

        float ComputeUVIncrement(Vector3[] outline)
        {
            float perimeter = ComputeLength(outline);
            float uvIncrement = Mathf.Round(perimeter / UV_SCALE) / perimeter;
            uvIncrement = Mathf.Max(uvIncrement, 1f / UV_SCALE); // In case uvIncrement = 0 (can happen for tiny outlines)
            return uvIncrement;
        }

        int[] GetTriangles()
        {
            int numTriangles = 6 * (VERTS_PER_CORNER - 1) * outlines.Sum(outline => outline.Length - 1);
            var triangles = new int[numTriangles];

            int triangleIndex = 0;
            int vertexIndex = 0;
            foreach (Vector3[] outline in outlines)
            {
                for (int i = 0; i < outline.Length - 1; i++)
                {
                    for (int j = 0; j < VERTS_PER_CORNER - 1; j++, triangleIndex += 6, vertexIndex++)
                    {
                        AddQuadAtIndex(triangles, triangleIndex, vertexIndex);
                    }
                    vertexIndex++;
                }
                vertexIndex += VERTS_PER_CORNER;
            }
            return triangles;
        }

        void AddQuadAtIndex(int[] triangles, int triangleIndex, int vertexIndex)
        {
            triangles[triangleIndex++] = vertexIndex;
            triangles[triangleIndex++] = vertexIndex + 1;
            triangles[triangleIndex++] = vertexIndex + 1 + VERTS_PER_CORNER;

            triangles[triangleIndex++] = vertexIndex + 1 + VERTS_PER_CORNER;
            triangles[triangleIndex++] = vertexIndex + VERTS_PER_CORNER;
            triangles[triangleIndex++] = vertexIndex;
        }

        static float ComputeLength(Vector3[] outline)
        {
            float length = 0;
            for (int i = 1; i < outline.Length; i++)
            {
                length += Vector3.Distance(outline[i], outline[i - 1]);
            }
            return length;
        }

        static float ComputeEdgeLength(Vector3[] outline, int i)
        {
            if (i == 0) return 0;
            return Vector3.Distance(outline[i], outline[i - 1]);
        }
    }
}