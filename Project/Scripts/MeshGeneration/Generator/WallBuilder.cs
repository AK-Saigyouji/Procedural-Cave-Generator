using AKSaigyouji.HeightMaps;
using AKSaigyouji.Modules.CaveWalls;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace AKSaigyouji.MeshGeneration
{
    sealed class WallBuilder
    {
        const float UV_SCALE = 10f;
        readonly int extraVertsPerCorner;
        readonly int totalVertsPerCorner;

        readonly List<Vector3[]> outlines;
        readonly IHeightMap floorHeightMap;
        readonly IHeightMap ceilingHeightMap;
        readonly CaveWallModule wallModule;

        public WallBuilder(List<Vector3[]> outlines, IHeightMap floor, IHeightMap ceiling, CaveWallModule walls)
        {
            this.outlines = outlines;
            floorHeightMap = floor;
            ceilingHeightMap = ceiling;
            wallModule = walls;
            extraVertsPerCorner = wallModule.ExtraVerticesPerCorner;
            totalVertsPerCorner = extraVertsPerCorner + 2;
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
            int numWallVertices = totalVertsPerCorner * outlines.Sum(outline => outline.Length);
            var vertices = new Vector3[numWallVertices];
            var context = new VertexContext(floorHeightMap, ceilingHeightMap, totalVertsPerCorner);

            int vertexIndex = 0;
            foreach (Vector3[] outline in outlines)
            {
                for (int i = 0; i < outline.Length; i++)
                {
                    Vector3 vertex = outline[i];
                    Vector3 normal = ComputeNormal(outline, i);
                    float x = vertex.x;
                    float z = vertex.z;
                    float floorHeight = floorHeightMap.GetHeight(x, z);
                    float ceilingHeight = ceilingHeightMap.GetHeight(x, z);
                    float interpolationScale = 1 / (totalVertsPerCorner - 1f);

                    for (int j = 0; j < totalVertsPerCorner; j++)
                    {
                        float interpolation = j * interpolationScale;
                        vertex.y = Mathf.Lerp(ceilingHeight, floorHeight, interpolation);
                        vertex.y = interpolation * floorHeight + (1 - interpolation) * ceilingHeight;
                        context.Update(vertex, normal, j);
                        vertices[vertexIndex++] = wallModule.GetAdjustedCorner(context);
                    }

                }
            }
            return vertices;
        }

        // a corner does not have a well-defined tangent and consequently lacks a normal. But we can define
        // a reasonable normal as being the average of the normals of the two adjacent panels.
        Vector3 ComputeNormal(Vector3[] outline, int index)
        {
            // modulo operator would be simpler but slower.
            int finalIndex = outline.Length - 1;
            Vector3 left = index > 0 ? outline[index - 1] : outline[finalIndex];
            Vector3 mid = outline[index];
            Vector3 right = index < finalIndex ? outline[index + 1] : outline[0];
            return (Vector3.Cross(mid - left, Vector3.up) + Vector3.Cross(right - mid, Vector3.up)) / 2;
        }

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
                    for (int j = 0; j < totalVertsPerCorner; j++, vertexIndex++)
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
            int numTriangles = 6 * (totalVertsPerCorner - 1) * outlines.Sum(outline => outline.Length - 1);
            var triangles = new int[numTriangles];

            int triangleIndex = 0;
            int vertexIndex = 0;
            foreach (Vector3[] outline in outlines)
            {
                for (int i = 0; i < outline.Length - 1; i++)
                {
                    for (int j = 0; j < totalVertsPerCorner - 1; j++, triangleIndex += 6, vertexIndex++)
                    {
                        AddQuadAtIndex(triangles, triangleIndex, vertexIndex);
                    }
                    vertexIndex++;
                }
                vertexIndex += totalVertsPerCorner;
            }
            return triangles;
        }

        void AddQuadAtIndex(int[] triangles, int triangleIndex, int vertexIndex)
        {
            triangles[triangleIndex++] = vertexIndex;
            triangles[triangleIndex++] = vertexIndex + 1;
            triangles[triangleIndex++] = vertexIndex + 1 + totalVertsPerCorner;

            triangles[triangleIndex++] = vertexIndex + 1 + totalVertsPerCorner;
            triangles[triangleIndex++] = vertexIndex + totalVertsPerCorner;
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