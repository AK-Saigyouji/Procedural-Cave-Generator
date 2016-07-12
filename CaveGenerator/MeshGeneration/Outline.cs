using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Outline represents an ordered collection of indices corresponding to points that separate a region of walls 
    /// from a region of space.
    /// </summary>
    class Outline : IEnumerable<int>
    {
        public int Count { get { return indices.Count; } }

        List<int> indices = new List<int>();

        public void Add(int vertexIndex)
        {
            indices.Add(vertexIndex);
        }

        public IEnumerator<int> GetEnumerator()
        {
            return ((IEnumerable<int>)indices).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<int>)indices).GetEnumerator();
        }

        public int this[int i]
        {
            get { return indices[i]; }
            private set { indices[i] = value; }
        }
    }
}
