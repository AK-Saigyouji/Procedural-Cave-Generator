/* The motivation behind this struct was the huge memory allocation associated with data structures containing triangles. 
 * The naive approach of storing three Vector3s would take 36 bytes per triangle. Even just storing three ints corresponding
 * to indices in a Vector3 array would take 12 bytes. This approach takes just 2 bytes, by storing a short corresponding
 * to the first index in an triangle array associated with a mesh. */

using System.Collections.Generic;
using System;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Highly optimized struct for referencing a mesh triangle. The three vertices represent three consecutive indices
    /// in a triangle array for a mesh. 
    /// </summary>
    struct Triangle : IEquatable<Triangle>, IComparable<Triangle>
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
        /// Create a new triangle based on the first index is passed in. Other indices will be inferred. Index must be a 
        /// multiple of 3.
        /// </summary>
        public Triangle(int triangleIndex)
        {
            index = (ushort)(triangleIndex / 3);
        }

        public override int GetHashCode()
        {
            return index;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Triangle))
            {
                return false;
            }
            return Equals((Triangle)obj);
        }

        public bool Equals(Triangle other)
        {
            return index == other.index;
        }

        public int CompareTo(Triangle other)
        {
            return index.CompareTo(other.index);
        }

        public static bool operator ==(Triangle a, Triangle b)
        {
            return a.index == b.index;
        }

        public static bool operator !=(Triangle a, Triangle b)
        {
            return a.index != b.index;
        }
    }
}
