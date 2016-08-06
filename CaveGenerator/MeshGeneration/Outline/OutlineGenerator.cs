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
        Dictionary<int, List<Triangle>> trianglesContainingIndex;

        List<Outline> outlines;
        bool[] visited;

        const int MAX_CONTAINING_TRIANGLES_ENSURING_OUTLINE_INDEX = 3;

        public OutlineGenerator(MeshData mesh)
        {
            vertices = mesh.vertices;
            trianglesContainingIndex = DetermineContainingTriangles(mesh.triangles);
            outlines = new List<Outline>();
        }

        private OutlineGenerator() { }

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

        Dictionary<int, List<Triangle>> DetermineContainingTriangles(int[] triangles)
        {
            var indexToTriangles = new Dictionary<int, List<Triangle>>();
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int a = triangles[i];
                int b = triangles[i + 1];
                int c = triangles[i + 2];

                Triangle triangle = new Triangle(a, b, c);
                AddTriangleToTable(indexToTriangles, a, triangle);
                AddTriangleToTable(indexToTriangles, b, triangle);
                AddTriangleToTable(indexToTriangles, c, triangle);
            }
            return indexToTriangles;
        }

        void AddTriangleToTable(Dictionary<int, List<Triangle>> table, int index, Triangle triangle)
        {
            List<Triangle> triangles;
            if (table.TryGetValue(index, out triangles))
            {
                triangles.Add(triangle);
            }
            else
            {
                triangles = new List<Triangle> { triangle };
                table[index] = triangles;
            }
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
            Outline outline = new Outline();

            outline.Add(startVertexIndex);
            int nextVertexIndex = GetInitialConnectedOutlineVertex(startVertexIndex);
            FollowOutline(nextVertexIndex, outline);
            outline.Add(startVertexIndex);

            outlines.Add(outline);
        }

        void FollowOutline(int vertexIndex, Outline outline)
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
            foreach (Triangle triangle in trianglesContainingIndex[startIndex])
            {
                foreach (int nextIndex in triangle.vertices)
                {
                    if (IsOutlineEdge(startIndex, nextIndex) && IsCorrectOrientation(startIndex, nextIndex, triangle))
                    {
                        return nextIndex;
                    }
                }
            }
            throw new Exception("Failed to initialize outline during mesh generation.");
        }

        int GetConnectedOutlineVertex(int currentIndex, int outlineSize)
        {
            foreach (Triangle triangle in trianglesContainingIndex[currentIndex])
            {
                foreach (int nextIndex in triangle.vertices)
                {
                    if (!visited[nextIndex] && IsOutlineEdge(currentIndex, nextIndex))
                    {
                        return nextIndex;
                    }
                }
            }
            return -1;
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
            foreach (Triangle triangle in trianglesContainingIndex[vertexA])
            {
                if (triangle.Contains(vertexB))
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
            int indexThree = triangle.GetThirdPoint(startIndex, otherIndex);
            return IsRightOf(vertices[startIndex], vertices[otherIndex], vertices[indexThree]);
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
