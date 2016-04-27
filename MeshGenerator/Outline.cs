using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MeshHelpers
{
    class Outline
    {
        List<int> indices = new List<int>();

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

        public int Size()
        {
            return indices.Count;
        }

    }
}
