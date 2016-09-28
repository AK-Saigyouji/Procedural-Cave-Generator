using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Generates a list of outlines for a triangulated map, based on the vertices and the triangles containing each vertex.
    /// Note that it may skip extremely small outlines. As an example, it will skip the case of a single floor tile surrounded
    /// by walls. As such, it is recommended that very small rooms are pruned (filled in) before generating outlines.
    /// </summary>
    sealed class OutlineGenerator
    {
        Vector3[] vertices;
        int[] triangles;

        TriangleLookup triangleLookup;
        bool[] visited;
        List<VertexIndex> outlineTemp;
        int[] currentTriangle = new int[3];

        const int MAX_CONTAINING_TRIANGLES_ENSURING_OUTLINE_INDEX = 3;

        public OutlineGenerator(MeshData mesh)
        {
            vertices = mesh.vertices;
            triangles = mesh.triangles;
            outlineTemp = new List<VertexIndex>(mesh.vertices.Length);
            triangleLookup = new TriangleLookup(mesh.vertices, mesh.triangles);
        }

        /// <summary>
        /// Generate and return the outlines based on the data passed in during instantiation.
        /// </summary>
        public IList<Outline> GenerateOutlines()
        {
            var outlines = new List<Outline>();
            visited = new bool[vertices.Length];
            for (VertexIndex index = 0; index < vertices.Length; index++)
            {
                if (!visited[index] && MustBeOutlineVertex(index))
                {
                    outlines.Add(GenerateOutlineFromPoint(index));
                }
            }
            return outlines.AsReadOnly();
        }

        /// <summary>
        /// Sufficient (but not necessary) test for the vertex index to be on an outline. For each outline, at least one
        /// vertex will have an index that returns true according to this method. As such, indices failing this test
        /// can be ignored while still ensuring every outline is discovered.
        /// </summary>
        bool MustBeOutlineVertex(VertexIndex vertexIndex)
        {
            int numTrianglesContainingVertex = triangleLookup.CountTrianglesContainingVertex(vertexIndex);
            return numTrianglesContainingVertex <= MAX_CONTAINING_TRIANGLES_ENSURING_OUTLINE_INDEX;
        }

        Outline GenerateOutlineFromPoint(VertexIndex startVertexIndex)
        {
            visited[startVertexIndex] = true;
            outlineTemp.Clear();

            outlineTemp.Add(startVertexIndex);
            VertexIndex nextVertexIndex = GetInitialConnectedOutlineVertex(startVertexIndex);
            FollowOutline(nextVertexIndex, outlineTemp);
            outlineTemp.Add(startVertexIndex);

            return new Outline(outlineTemp);
        }

        void FollowOutline(VertexIndex vertexIndex, List<VertexIndex> outline)
        {
            outline.Add(vertexIndex);
            visited[vertexIndex] = true;
            VertexIndex nextVertexIndex;
            if (TryGetConnectedOutlineVertex(vertexIndex, out nextVertexIndex))
            {
                FollowOutline(nextVertexIndex, outline);
            }
        }

        VertexIndex GetInitialConnectedOutlineVertex(VertexIndex startIndex)
        {
            List<Triangle> containingTriangles = triangleLookup.GetTrianglesContainingVertex(startIndex);
            for (int i = 0; i < containingTriangles.Count; i++)
            {
                Triangle triangle = containingTriangles[i];
                int[] triangleIndices = ExtractTriangleIndices(triangle);
                foreach (int nextIndex in triangleIndices)
                {
                    VertexIndex vertexIndex = triangles[nextIndex];
                    if (IsOutlineEdge(startIndex, vertexIndex) && IsCorrectOrientation(startIndex, vertexIndex, triangle))
                    {
                        return vertexIndex;
                    }
                }
            }
            throw new InvalidOperationException("Failed to initialize outline during mesh generation.");
        }

        bool TryGetConnectedOutlineVertex(VertexIndex currentIndex, out VertexIndex nextIndex)
        {
            List<Triangle> containingTriangles = triangleLookup.GetTrianglesContainingVertex(currentIndex);
            for (int i = 0; i < containingTriangles.Count; i++)
            {
                int[] triangleIndices = ExtractTriangleIndices(containingTriangles[i]);
                foreach (int index in triangleIndices)
                {
                    VertexIndex vertexIndex = triangles[index];
                    if (!visited[vertexIndex] && IsOutlineEdge(currentIndex, vertexIndex))
                    {
                        nextIndex = vertexIndex;
                        return true;
                    }
                }
            }
            nextIndex = new VertexIndex();
            return false;
        }

        int[] ExtractTriangleIndices(Triangle triangle)
        {
            currentTriangle[0] = triangle.a;
            currentTriangle[1] = triangle.b;
            currentTriangle[2] = triangle.c;
            return currentTriangle;
        }

        bool IsOutlineEdge(VertexIndex vertexA, VertexIndex vertexB)
        {
            return vertexA != vertexB && !triangleLookup.DoVerticesShareMultipleTriangles(vertexA, vertexB);
        }

        /// <summary>
        /// Will these indices produce an Outline going in the right direction? The direction of the Outline will determine
        /// whether the walls are visible. 
        /// </summary>
        /// <param name="startIndex">The starting index.</param>
        /// <param name="otherIndex">The discovered index in question.</param>
        /// <param name="triangle">A triangle containing both indices.</param>
        /// <returns>Returns whether going from the start index to the discovered index will result in a correctly 
        /// oriented Outline.</returns>
        bool IsCorrectOrientation(VertexIndex startIndex, VertexIndex otherIndex, Triangle triangle)
        {
            VertexIndex outsideIndex = GetThirdPoint(startIndex, otherIndex, triangle);
            return IsRightOf(vertices[startIndex], vertices[otherIndex], vertices[outsideIndex]);
        }
        
        VertexIndex GetThirdPoint(VertexIndex indexA, VertexIndex indexB, Triangle triangle)
        {
            foreach (int triangleIndex in triangle.vertices)
            {
                VertexIndex vertexIndex = triangles[triangleIndex];
                if (vertexIndex != indexA && vertexIndex != indexB)
                {
                    return vertexIndex;
                }
            }
            throw new ArgumentException("Unexpected error: outline generation failed.");
        }

        /// <summary>
        /// Is the vector c positioned "to the right of" the line formed by a and b, when looking down the y-axis?
        /// </summary>
        /// <returns>Returns whether the vector c is positioned to the right of the line formed by a and b.</returns>
        bool IsRightOf(Vector3 a, Vector3 b, Vector3 c)
        {
            return ((b.x - a.x) * (c.z - a.z) - (b.z - a.z) * (c.x - a.x)) < 0;
        }
    }
}
