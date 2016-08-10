using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Generates a list of outlines for a triangulated map, based on the vertices and the triangles containing each vertex.
    /// Note that it may skip extremely small outlines. As an example, it will skip the case of a single floor tile surrounded
    /// by walls. As such, it is recommended that very small rooms are pruned (filled in) before generating outlines.
    /// </summary>
    class OutlineGenerator
    {
        Vector3[] vertices;
        int[] triangles;
        List<Triangle>[] trianglesContainingIndex;

        List<Outline> outlines;
        bool[] visited;
        List<int> outlineTemp;
        int[] currentTriangle = new int[3]; // Allocated at instance level to avoid allocations or expensive foreach loops

        const int MAX_CONTAINING_TRIANGLES = 8;
        const int MAX_CONTAINING_TRIANGLES_ENSURING_OUTLINE_INDEX = 3;

        public OutlineGenerator(MeshData mesh)
        {
            vertices = mesh.vertices;
            triangles = mesh.triangles;
            trianglesContainingIndex = DetermineContainingTriangles(mesh.triangles);
            outlines = new List<Outline>();
            outlineTemp = new List<int>(mesh.vertices.Length);
        }

        /// <summary>
        /// Generate and return the outlines based on the data passed in during instantiation.
        /// </summary>
        public List<Outline> GenerateOutlines()
        {
            visited = new bool[vertices.Length];
            for (int vertexIndex = 0; vertexIndex < vertices.Length; vertexIndex++)
            {
                if (!visited[vertexIndex] && MustBeOutlineVertex(vertexIndex))
                {
                    GenerateOutlineFromPoint(vertexIndex);
                }
            }
            return outlines;
        }

        List<Triangle>[] DetermineContainingTriangles(int[] triangles)
        {
            var indexToTriangles = new List<Triangle>[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                indexToTriangles[i] = new List<Triangle>(MAX_CONTAINING_TRIANGLES);
            }
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int a = triangles[i];
                int b = triangles[i + 1];
                int c = triangles[i + 2];

                Triangle triangle = new Triangle(i);
                indexToTriangles[a].Add(triangle);
                indexToTriangles[b].Add(triangle);
                indexToTriangles[c].Add(triangle);
            }
            return indexToTriangles;
        }

        /// <summary>
        /// Sufficient (but not necessary) test for the vertex index to be on an outline. For each outline, at least one
        /// vertex will have an index that returns true according to this method. As such, indices failing this test
        /// can be ignored while still ensuring every outline is discovered.
        /// </summary>
        bool MustBeOutlineVertex(int vertexIndex)
        {
            int numTrianglesContainingVertex = trianglesContainingIndex[vertexIndex].Count;
            return numTrianglesContainingVertex <= MAX_CONTAINING_TRIANGLES_ENSURING_OUTLINE_INDEX;
        }

        void GenerateOutlineFromPoint(int startVertexIndex)
        {
            visited[startVertexIndex] = true;
            outlineTemp.Clear();

            outlineTemp.Add(startVertexIndex);
            int nextVertexIndex = GetInitialConnectedOutlineVertex(startVertexIndex);
            FollowOutline(nextVertexIndex, outlineTemp);
            outlineTemp.Add(startVertexIndex);

            outlines.Add(new Outline(outlineTemp));
        }

        void FollowOutline(int vertexIndex, List<int> outline)
        {
            if (vertexIndex == -1)
                return;
            outline.Add(vertexIndex);
            visited[vertexIndex] = true;
            int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex, outline.Count);
            FollowOutline(nextVertexIndex, outline);
        }

        int GetInitialConnectedOutlineVertex(int startIndex)
        {
            List<Triangle> containingTriangles = trianglesContainingIndex[startIndex];
            for (int i = 0; i < containingTriangles.Count; i++)
            {
                Triangle triangle = containingTriangles[i];
                int[] indices = ExtractIndices(triangle);
                foreach (int nextIndex in indices)
                {
                    int vertexIndex = triangles[nextIndex];
                    if (IsOutlineEdge(startIndex, vertexIndex) && IsCorrectOrientation(startIndex, vertexIndex, triangle))
                    {
                        return vertexIndex;
                    }
                }
            }
            throw new InvalidOperationException("Failed to initialize outline during mesh generation.");
        }

        int GetConnectedOutlineVertex(int currentIndex, int outlineSize)
        {
            List<Triangle> containingTriangles = trianglesContainingIndex[currentIndex];
            for (int i = 0; i < containingTriangles.Count; i++)
            {
                int[] indices = ExtractIndices(containingTriangles[i]);
                foreach (int nextIndex in indices)
                {
                    int vertexIndex = triangles[nextIndex];
                    if (!visited[vertexIndex] && IsOutlineEdge(currentIndex, vertexIndex))
                    {
                        return vertexIndex;
                    }
                }
            }
            return -1;
        }

        int[] ExtractIndices(Triangle triangle)
        {
            currentTriangle[0] = triangle.a;
            currentTriangle[1] = triangle.b;
            currentTriangle[2] = triangle.c;
            return currentTriangle;
        }

        bool IsOutlineEdge(int vertexA, int vertexB)
        {
            if (vertexA == vertexB)
                return false;

            return !DoVerticesShareMultipleTriangles(vertexA, vertexB);
        }

        bool DoVerticesShareMultipleTriangles(int vertexA, int vertexB)
        {
            int sharedTriangleCount = 0;
            List<Triangle> containingTriangles = trianglesContainingIndex[vertexA];
            for (int i = 0; i < containingTriangles.Count; i++)
            {
                if (IsVertexContainedInTriangle(containingTriangles[i], vertexB))
                {
                    sharedTriangleCount++;
                    if (sharedTriangleCount > 1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        bool IsVertexContainedInTriangle(Triangle triangle, int vertex)
        {
            return triangles[triangle.a] == vertex || triangles[triangle.b] == vertex || triangles[triangle.c] == vertex;
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
        bool IsCorrectOrientation(int startIndex, int otherIndex, Triangle triangle)
        {
            int outsideIndex = GetThirdPoint(startIndex, otherIndex, triangle);
            return IsRightOf(vertices[startIndex], vertices[otherIndex], vertices[outsideIndex]);
        }
        
        int GetThirdPoint(int indexA, int indexB, Triangle triangle)
        {
            foreach (int triangleIndex in triangle.vertices)
            {
                int vertexIndex = triangles[triangleIndex];
                if (vertexIndex != indexA && vertexIndex != indexB)
                {
                    return vertexIndex;
                }
            }
            return -1;
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
