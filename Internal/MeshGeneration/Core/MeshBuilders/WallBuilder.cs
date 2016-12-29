/* The task of building the walls is sufficiently complicated that it warranted a separate class. The basic
 idea is simple: taking a 2D outline of the walls, build a quad on each edge of the outline. The difficult
 part is the task of assigning texture coordinates.
*/

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CaveGeneration.MeshGeneration
{
    static class WallBuilder
    {
        const float UVSCALE = 10f;

        public static MeshData Build(Outline[] outlines, IHeightMap floorHeightMap, IHeightMap ceilingHeightMap)
        {
            MeshData mesh = new MeshData();
            mesh.vertices = GetVertices(outlines, floorHeightMap, ceilingHeightMap);
            mesh.triangles = GetTriangles(outlines);
            mesh.uv = GetUVs(outlines, mesh.vertices);
            return mesh;
        }

        static Vector3[] GetVertices(Outline[] outlines, IHeightMap floorHeightMap, IHeightMap ceilingHeightMap)
        {
            int numWallVertices = 2 * outlines.Sum(outline => outline.NumVertices);
            var vertices = new Vector3[numWallVertices];

            int vertexIndex = 0;
            foreach (Outline outline in outlines)
            {
                for (int i = 0; i < outline.NumVertices; i++)
                {
                    Vector3 vertex = outline[i];
                    float x = vertex.x;
                    float z = vertex.z;
                    vertices[vertexIndex] = new Vector3(x, ceilingHeightMap.GetHeight(x, z), z);
                    vertices[vertexIndex + 1] = new Vector3(x, floorHeightMap.GetHeight(x, z), z);
                    vertexIndex += 2;
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
         * 
         * The one issue not addressed by this implementation is that the uv coordinates for a wall in one map chunk
         * will not in general match up with the wall in an adjacent map chunk. 
         */

        static Vector2[] GetUVs(Outline[] outlines, Vector3[] vertices)
        {
            var uv = new Vector2[vertices.Length];
            int vertexIndex = 0;

            foreach (Outline outline in outlines)
            {
                float u = 0f;
                float increment = ComputeUVIncrement(outline);
                for (int i = 0; i < outline.NumVertices; i++, vertexIndex += 2)
                {
                    u += ComputeEdgeLength(outline, i) * increment;
                    float vTop = vertices[vertexIndex].y / UVSCALE;
                    float vBot = vertices[vertexIndex + 1].y / UVSCALE;
                    uv[vertexIndex]     = new Vector2(u, vTop);
                    uv[vertexIndex + 1] = new Vector2(u, vBot);
                }
            }
            return uv;
        }

        static float ComputeEdgeLength(Outline outline, int i)
        {
            if (i == 0) return 0;
            return Vector3.Distance(outline[i], outline[i - 1]);
        }

        static float ComputeUVIncrement(Outline outline)
        {
            // The following number is meant to be approximately 1/UVSCALE, but adjusted slightly
            // to ensure that the final vector in the outline gets a u-value that is an integer multiple. 
            // Otherwise, the texture at the beginning of the outline won't match the texture at the end.
            // i.e. we require that outline.PerimeterLength * uvIncrement is an integer. 
            float uvIncrement = Mathf.Round(outline.PerimeterLength / UVSCALE) / outline.PerimeterLength;
            uvIncrement = Mathf.Max(uvIncrement, 1f / UVSCALE); // In case uvIncremenet = 0
            return uvIncrement;
        }

        static int[] GetTriangles(Outline[] outlines)
        {
            int numTriangles = 6 * outlines.Sum(outline => outline.NumEdges);
            var triangles = new int[numTriangles];

            int triangleIndex = 0;
            int currentVertex = 0;
            foreach (Outline outline in outlines)
            {
                for (int i = 0; i < outline.NumEdges; i++, triangleIndex += 6, currentVertex += 2)
                {
                    AddQuadAtIndex(triangles, triangleIndex, currentVertex);
                }
                currentVertex += 2; // skip the two vertices at the end of an outline for the seam
            }
            return triangles;
        }

        static void AddQuadAtIndex(int[] triangles, int triangleIndex, int vertexIndex)
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