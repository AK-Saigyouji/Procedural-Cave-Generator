using System.Collections.Generic;
using System;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Represents a mesh triangle. Its three vertices represent the three entries in a triangle array for a mesh, and are
    /// thus consecutive integers.
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

        public Triangle(int triangleIndex)
        {
            index = (ushort)(triangleIndex / 3);
        }
    }
}
