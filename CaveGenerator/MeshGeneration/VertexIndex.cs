/* In meshes, integers are used to represent a position in a vertex array. But since vertex arrays have length at most
 * 65535, integers waste 16 bits. By using unsigned shorts we can cut in half the GC allocations of any temporary arrays
 * used to hold vertex indices. Having a named struct also promotes type safety, since an int can refer to many things and
 * offers many operations not appropriate for a vertex index.
 */

/// <summary>
/// Represents a vertex in an array of mesh vertices. Meshes can have up to 65535 vertices: thus VertexIndex goes up to
/// 65534. Supports incrementation and direct comparison, as well as conversion between integers, but not arithmetic.
/// </summary>
public struct VertexIndex : System.IEquatable<VertexIndex> {

    readonly ushort value;
    public const ushort VoidValue = 65535;

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
