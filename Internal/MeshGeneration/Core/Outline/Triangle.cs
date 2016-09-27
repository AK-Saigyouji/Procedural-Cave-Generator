using System.Collections.Generic;
using System;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Highly optimized struct for referencing a mesh triangle. The three vertices represent three consecutive indices
    /// in a triangle array for a mesh. 
    /// </summary>
    struct Triangle
    {
        readonly ushort index;

        public int a { get { return index * 3; } }
        public int b { get { return index * 3 + 1; } }
        public int c { get { return index * 3 + 2; } }

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
        /// Create a new triangle based on the first index is passed in. Other indices will be inferred.
        /// </summary>
        public Triangle(int triangleIndex)
        {
            index = (ushort)(triangleIndex / 3);
        }
    }
}
