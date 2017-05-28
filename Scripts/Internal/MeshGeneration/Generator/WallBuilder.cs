/* The task of building the walls is sufficiently complicated that it warranted a separate class. The basic
 idea is simple: taking a 2D outline of the walls, build a quad on each edge of the outline. The difficult
 part is the task of assigning texture coordinates.

 The task of determining the outlines for a grid is delegated to a separate class.
*/

using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace CaveGeneration.MeshGeneration
{
    static class WallBuilder
    {
        const float UVSCALE = 10f;

        public static MeshData Build(List<Vector3[]> outlines, IHeightMap floorHeightMap, IHeightMap ceilingHeightMap)
        {
            MeshData mesh = new MeshData();
            mesh.vertices  = GetVertices(outlines, floorHeightMap, ceilingHeightMap);
            mesh.uv        = GetUVs(outlines, mesh.vertices);
            mesh.triangles = GetTriangles(outlines);
            return mesh;
        }

        static Vector3[] GetVertices(List<Vector3[]> outlines, IHeightMap floorHeightMap, IHeightMap ceilingHeightMap)
        {
            int numWallVertices = 2 * outlines.Sum(outline => outline.Length);
            var vertices = new Vector3[numWallVertices];

            int vertexIndex = 0;
            foreach (Vector3[] outline in outlines)
            {
                for (int i = 0; i < outline.Length; i++)
                {
                    Vector3 vertex = outline[i];
                    float x = vertex.x;
                    float z = vertex.z;
                    vertices[vertexIndex++] = new Vector3(x, ceilingHeightMap.GetHeight(x, z), z);
                    vertices[vertexIndex++] = new Vector3(x, floorHeightMap.GetHeight(x, z), z);
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

        static Vector2[] GetUVs(List<Vector3[]> outlines, Vector3[] vertices)
        {
            var uv = new Vector2[vertices.Length];
            int vertexIndex = 0;

            foreach (Vector3[] outline in outlines)
            {
                float u = 0f;
                float increment = ComputeUVIncrement(outline);
                for (int i = 0; i < outline.Length; i++, vertexIndex += 2)
                {
                    u += ComputeEdgeLength(outline, i) * increment;
                    float vTop = vertices[vertexIndex].y / UVSCALE;
                    float vBot = vertices[vertexIndex + 1].y / UVSCALE;
                    uv[vertexIndex] = new Vector2(u, vTop);
                    uv[vertexIndex + 1] = new Vector2(u, vBot);
                }
            }
            return uv;
        }

        static float ComputeUVIncrement(Vector3[] outline)
        {
            float perimeter = ComputeLength(outline);
            float uvIncrement = Mathf.Round(perimeter / UVSCALE) / perimeter;
            uvIncrement = Mathf.Max(uvIncrement, 1f / UVSCALE); // In case uvIncrement = 0 (can happen for tiny outlines)
            return uvIncrement;
        }

        static int[] GetTriangles(List<Vector3[]> outlines)
        {
            int numTriangles = 6 * outlines.Sum(outline => outline.Length - 1);
            var triangles = new int[numTriangles];

            int triangleIndex = 0;
            int vertexIndex = 0;
            foreach (Vector3[] outline in outlines)
            {
                for (int i = 0; i < outline.Length - 1; i++, triangleIndex += 6, vertexIndex += 2)
                {
                    AddQuadAtIndex(triangles, triangleIndex, vertexIndex);
                }
                vertexIndex += 2; // skip the two vertices at the end of an outline
            }
            return triangles;
        }

        static void AddQuadAtIndex(int[] triangles, int triangleIndex, int vertexIndex)
        {
            triangles[triangleIndex++] = vertexIndex;
            triangles[triangleIndex++] = vertexIndex + 1;
            triangles[triangleIndex++] = vertexIndex + 3;

            triangles[triangleIndex++] = vertexIndex + 3;
            triangles[triangleIndex++] = vertexIndex + 2;
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