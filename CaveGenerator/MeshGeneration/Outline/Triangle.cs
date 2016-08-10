using System.Collections.Generic;
using System;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// A simple container for three distinct vertex indices. Since there's a 16 bit vertex limit for meshes, 
    /// the allowed range for triangle vertices is from 0 to 65535.
    /// </summary>
    struct Triangle
    {
        public readonly ushort a;
        public readonly ushort b;
        public readonly ushort c;

        public IEnumerable<int> vertices
        {
            get
            {
                yield return a;
                yield return b;
                yield return c;
            }
        }

        public Triangle(int a, int b, int c)
        {
            this.a = (ushort)a;
            this.b = (ushort)b;
            this.c = (ushort)c;
        }

        /// <summary>
        /// Does the triangle contain this vertex?
        /// </summary>
        /// <returns>Returns whether this triangle contains the vertex.</returns>
        public bool Contains(int vertex)
        {
            return (vertex == a) || (vertex == b) || (vertex == c);
        }

        /// <summary>
        /// Get the first vertex not equal to the arguments. Does not check that the vertices passed in are actually in the 
        /// triangle. Can use the Contains method to check.
        /// </summary>  
        /// <param name="vertexOne">An index already in the triangle.</param>
        /// <param name="vertexTwo">Another distinct index already in the triangle.</param>
        /// <returns>Returns the first vertex not equal to either of the arguments.</returns>
        public int GetThirdPoint(int vertexOne, int vertexTwo)
        {
            foreach (int vertexIndex in vertices)
            {
                if (vertexIndex != vertexOne && vertexIndex != vertexTwo)
                {
                    return vertexIndex;
                }
            }
            throw new ArgumentException("Distinct third point not found in triangle.");
        }
    }
}
