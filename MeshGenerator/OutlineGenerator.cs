using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeshHelpers
{
    /// <summary>
    /// Generates a list of outlines for a triangulated map, based on the vertices and the triangles containing each vertex.
    /// Note that it may skip extremely small outlines. As an example, it will skip the case
    /// 1 1 1
    /// 1 0 1
    /// 1 1 1
    /// </summary>
    class OutlineGenerator
    {
        readonly IList<Vector3> vertices;
        readonly IDictionary<int, List<Triangle>> vertexIndexToContainingTriangles;

        List<Outline> outlines;
        bool[] checkedVertices;

        /// <summary>
        /// Initialize the generator using the output of an appropriate triangulator. 
        /// </summary>
        public OutlineGenerator(IList<Vector3> vertices, IDictionary<int, List<Triangle>> vertexIndexToContainingTriangles)
        {
            this.vertices = vertices;
            this.vertexIndexToContainingTriangles = vertexIndexToContainingTriangles;
            outlines = new List<Outline>();
        }

        private OutlineGenerator() { }

        /// <summary>
        /// Generate and return the outlines based on the data passed in during instantiation.
        /// </summary>
        /// <returns></returns>
        public List<Outline> GenerateOutlines()
        {
            checkedVertices = new bool[vertices.Count];
            for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
            {
                if (!checkedVertices[vertexIndex] && MustBeOutlineVertex(vertexIndex))
                {
                    checkedVertices[vertexIndex] = true;
                    GenerateOutlineFromPoint(vertexIndex);
                }
            }
            return outlines;
        }

        /// <summary>
        /// Sufficient (but not necessary) test for the vertex index to be on the outline. For each outline, at least one
        /// vertex will have an index that returns true according to this method. As such, indices failing this test
        /// can be ignored while still ensuring every outline is discovered.
        /// </summary>
        bool MustBeOutlineVertex(int vertexIndex)
        {
            int MAX_CONTAINING_TRIANGLES_ENSURING_OUTLINE_INDEX = 3;
            return vertexIndexToContainingTriangles[vertexIndex].Count <= MAX_CONTAINING_TRIANGLES_ENSURING_OUTLINE_INDEX;
        }

        void GenerateOutlineFromPoint(int startVertexIndex)
        {
            Outline outline = new Outline(startVertexIndex);
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
            checkedVertices[vertexIndex] = true;
            int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex, outline.Size);
            FollowOutline(nextVertexIndex, outline);
        }

        int GetInitialConnectedOutlineVertex(int startIndex)
        {
            foreach (Triangle triangle in vertexIndexToContainingTriangles[startIndex])
            {
                foreach (int nextIndex in triangle.vertices)
                {
                    if (IsOutlineEdge(startIndex, nextIndex) && IsCorrectOrientation(startIndex, nextIndex, triangle))
                    {
                        return nextIndex;
                    }
                }
            }
            throw new Exception("Failed to initiate outline during mesh generation.");
        }

        int GetConnectedOutlineVertex(int currentIndex, int outlineSize)
        {
            foreach (Triangle triangle in vertexIndexToContainingTriangles[currentIndex])
            {
                foreach (int nextIndex in triangle.vertices)
                {
                    if (!checkedVertices[nextIndex] && IsOutlineEdge(currentIndex, nextIndex))
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
            foreach (Triangle triangle in vertexIndexToContainingTriangles[vertexA])
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
        /// <returns>Returns whether using the second index will result in a correctly oriented Outline.</returns>
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
