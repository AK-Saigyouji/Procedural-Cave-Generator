/* Originally, information about the triangles containing each vertex was handled by keeping an array, indexed by 
 * vertex indices, of lists of triangles. The problem with this approach is that lists have 
 * 40 bytes of overhead, and with each list containing an average of 10 bytes of data, this was resulting in wasteful 
 * allocation. This implementation using a multidimensional array is far messier, but trades the 40 bytes of overhead 
 * per vertex for an average of 8 bytes per vertex for a pair of byte arrays and wasted space 
 * in the multidimensional array. 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Highly optimized table for retrieving information about which triangles in a mesh contain a given vertex.
    /// </summary>
    class TriangleLookup
    {
        // How many triangles contain each vertex
        byte[] numTrianglesByIndex;
        // First dimension is the index, second gives the triangles
        Triangle[,] triangleLookup;
        // This reusable list holds the triangles that get returned upon query
        List<Triangle> triangles = new List<Triangle>(MAX_TRIANGLES_CONTAINING_VERTEX);

        const int MAX_TRIANGLES_CONTAINING_VERTEX = 8;

        public TriangleLookup(Vector3[] vertices, int[] triangles)
        {
            triangleLookup = DetermineContainingTriangles(vertices, triangles);
        }

        /// <summary>
        /// Get the list of triangles containing the given vertex. 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public List<Triangle> GetTrianglesContainingVertex(VertexIndex index)
        {
            triangles.Clear();
            for (int i = 0; i < numTrianglesByIndex[index]; i++)
            {
                triangles.Add(triangleLookup[index, i]);
            }
            return triangles;
        }

        /// <summary>
        /// How many triangles contain the given vertex? 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int CountTrianglesContainingVertex(VertexIndex index)
        {
            return numTrianglesByIndex[index];
        }

        Triangle[,] DetermineContainingTriangles(Vector3[] vertices, int[] triangles)
        {
            numTrianglesByIndex = CountNumberOfTrianglesContainingEachVertex(triangles, vertices.Length);
            return PopulateTriangleLookup(triangles, numTrianglesByIndex);
        }

        byte[] CountNumberOfTrianglesContainingEachVertex(int[] triangles, int numVertices)
        {
            byte[] numTrianglesForIndex = new byte[numVertices];
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int a = triangles[i];
                int b = triangles[i + 1];
                int c = triangles[i + 2];
                numTrianglesForIndex[a]++;
                numTrianglesForIndex[b]++;
                numTrianglesForIndex[c]++;
            }
            return numTrianglesForIndex;
        }

        Triangle[,] PopulateTriangleLookup(int[] triangles, byte[] numTrianglesForEachIndex)
        {
            Triangle[,] triangleLookup = new Triangle[numTrianglesForEachIndex.Length, MAX_TRIANGLES_CONTAINING_VERTEX];
            // This array keeps track of where to insert triangles
            byte[] currentNumberOfTrianglesAtIndex = new byte[numTrianglesForEachIndex.Length];
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int a = triangles[i];
                int b = triangles[i + 1];
                int c = triangles[i + 2];

                Triangle triangle = new Triangle(i);
                triangleLookup[a, currentNumberOfTrianglesAtIndex[a]++] = triangle;
                triangleLookup[b, currentNumberOfTrianglesAtIndex[b]++] = triangle;
                triangleLookup[c, currentNumberOfTrianglesAtIndex[c]++] = triangle;
            }
            return triangleLookup;
        }
    } 
}