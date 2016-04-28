using System.Collections.Generic;

namespace MeshHelpers
{
    /// <summary>
    /// Outline represents a collection of indices corresponding to points that separate a region of walls from a 
    /// region of space.
    /// </summary>
    class Outline
    {
        List<int> indices = new List<int>();
        public int size { get { return indices.Count; } }

        public Outline(int vertexIndex)
        {
            Add(vertexIndex);
        }

        public void Add(int vertexIndex)
        {
            indices.Add(vertexIndex);
        }

        public int this[int i]
        {
            get { return indices[i]; }
            private set { indices[i] = value; }
        }
    }
}
