/* In meshes, integers are used to represent a position in a vertex array. But since vertex arrays have length at most
 * 65535, integers waste 2 bytes each. By using unsigned shorts we can cut in half the GC allocations of any temporary arrays
 * used to hold vertex indices. It also makes intent more clear, since integer collections have many purposes.*/
using System;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Represents a vertex in an array of mesh vertices. Meshes can have up to 65535 vertices: thus VertexIndex goes up to
    /// 65535. Supports incrementation and direct comparison, as well as conversion between integers, but not arithmetic.
    /// </summary>
    struct VertexIndex : IEquatable<VertexIndex>, IComparable<VertexIndex>
    {
        readonly ushort value;

        public VertexIndex(int index)
        {
            value = (ushort)index;
        }

        public static bool operator ==(VertexIndex a, VertexIndex b)
        {
            return a.value == b.value;
        }

        public static bool operator !=(VertexIndex a, VertexIndex b)
        {
            return a.value != b.value;
        }

        // This implicit conversion allows us to use VertexIndex to index into arrays.
        public static implicit operator int(VertexIndex index)
        {
            return index.value;
        }

        public static implicit operator VertexIndex(int number)
        {
            return new VertexIndex(number);
        }

        public static VertexIndex operator ++(VertexIndex index)
        {
            return index.value + 1;
        }

        public int CompareTo(VertexIndex other)
        {
            return value.CompareTo(other.value);
        }

        public static bool operator <(VertexIndex a, VertexIndex b)
        {
            return a.value < b.value;
        }

        public static bool operator >(VertexIndex a, VertexIndex b)
        {
            return a.value > b.value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is VertexIndex))
            {
                return false;
            }
            VertexIndex p = (VertexIndex)obj;
            return Equals(p);
        }

        public bool Equals(VertexIndex p)
        {
            return value == p.value;
        }

        public override int GetHashCode()
        {
            return value;
        }
    } 
}
