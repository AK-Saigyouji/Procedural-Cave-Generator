/* The basic idea behind wallbuilder is to generate quads on top of 2d outlines. The main challenge is the uv array,
 * which is addressed more thoroughly in comments preceding the corresponding method. 
 */

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CaveGeneration.MeshGeneration
{
    sealed class WallBuilder : IMeshBuilder
    {
        Vector3[] outlineVertices;
        IList<Outline> outlines;
        MeshData mesh;

        const float UVSCALE = 10f;


        public WallBuilder(Vector3[] vertices, IList<Outline> outlines)
        {
            outlineVertices = vertices;
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
            mesh.vertices = GetVertices(outlineVertexCount);
            mesh.triangles = GetTriangles(outlineEdgeCount);
            mesh.uv = GetUVs(outlineVertexCount);
        }

        Vector3[] GetVertices(int outlineVertexCount)
        {
            Vector3[] outlineVertices = this.outlineVertices;
            Vector3[] vertices = new Vector3[2 * outlineVertexCount];
            int vertexIndex = 0;
            foreach (Outline outline in outlines)
            {
                for (int i = 0; i < outline.Length; i++)
                {
                    Vector3 vertex = outlineVertices[outline[i]];
                    vertices[vertexIndex] = vertex;
                    vertices[vertexIndex + 1] = new Vector3(vertex.x, 0f, vertex.z);
                    vertexIndex += 2;
                }
            }
            return vertices;
        }

        /* Computing the UV array for the walls proves to be a tricky matter. Unlike other meshes like the floor,
         * it is not possible to consistently determine the UV for walls based purely on global coordinates. On top of that,
         * there are two extra challenges. Walls are built on top of outline edges. But the distance between pairs of 
         * outline edges is inconsistent. So it's necessary to use distance in computing the u coordinate. The second
         * challenge is ensuring that the left edge of the beginning of an outline matches up with the right side of
         * the end of the outline. 
         * 
         * The following implementation addresses both issues. To address the first, we base the u coordinate
         * for each point in the outline on the length of the outline so far. To address the second, we scale
         * this value by a quantity (uvIncrement) which ensures that the final u coordinate will be an integer,
         * so that the texture will tile seamlessly. uvIncrement also serves the purpose of reducing the growth of u
         * by a constant factor so that it doesn't tile too rapidly.
         * 
         * The one issue not addressed by this implementation is that the uv coordinates for a wall in one map chunk
         * will not in general match up with the wall in an adjacent map chunk. It's not clear that there is a viable
         * way to do this locally: it might be necessary to 'correct' wall UVs globally. 
         */

        Vector2[] GetUVs(int outlineEdgeCount)
        {
            Vector2[] uv = new Vector2[2 * outlineEdgeCount];
            int vertexIndex = 0;

            foreach (Outline outline in outlines)
            {
                float u = 0f;
                float increment = ComputeUVIncrement(outline);
                for (int i = 0; i < outline.Length; i++, vertexIndex += 2)
                {
                    u += ComputeDistanceTo(outline, i) * increment;
                    float v = outlineVertices[outline[i]].y / UVSCALE;
                    uv[vertexIndex] = new Vector2(u, v);
                    uv[vertexIndex + 1] = new Vector2(u, 0f);
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
            Vector3 vectorA = outlineVertices[outline[index]];
            Vector3 vectorB = outlineVertices[outline[index - 1]];
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