﻿using System.Collections.Generic;
using System;

namespace MeshHelpers
{
    /// <summary>
    /// A simple container for three distinct integers. 
    /// </summary>
    struct Triangle
    {
        public int a;
        public int b;
        public int c;

        public Triangle(int a, int b, int c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        /// <summary>
        /// Does the triangle contain this vertex?
        /// </summary>
        /// <returns>Returns whether this triangle contains the vertex.</returns>
        public bool Contains(int vertex)
        {
            return (vertex == a) || (vertex == b) || (vertex == c);
        }

        public IEnumerable<int> vertices
        {
            get
            {
                yield return a;
                yield return b;
                yield return c;
            }
        }

        /// <summary>
        /// Get the first vertex not equal to the arguments. Does not check that the vertices passed in are actually in the 
        /// triangle. Contains method can be used to check.
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
