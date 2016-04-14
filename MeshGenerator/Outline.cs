using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeshHelpers
{
    class Outline
    {
        List<int> indices = new List<int>();

        internal Outline(int vertexIndex)
        {
            Add(vertexIndex);
        }

        internal void Add(int vertexIndex)
        {
            indices.Add(vertexIndex);
        }

        internal int this[int i]
        {
            get { return indices[i]; }
            private set { indices[i] = value; }
        }

        internal int Size()
        {
            return indices.Count;
        }

    }
}
