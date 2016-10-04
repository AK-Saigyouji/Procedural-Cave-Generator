using System;
using System.Collections;
using System.Collections.Generic;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Outline represents an ordered collection of indices corresponding to points that separate a region of walls 
    /// from a region of space.
    /// </summary>
    sealed class Outline: IEnumerable<VertexIndex>
    {
        public int Length { get; private set; }

        VertexIndex[] indices;
        
        public Outline(List<VertexIndex> indexList)
        {
            indices = new VertexIndex[indexList.Count];
            for (int i = 0; i < indexList.Count; i++)
            {
                indices[i] = indexList[i];
            }
            Length = indices.Length;
        }
        
        public void Reverse()
        {
            Array.Reverse(indices);
        }

        public IEnumerator<VertexIndex> GetEnumerator()
        {
            return ((IEnumerable<VertexIndex>)indices).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<VertexIndex>)indices).GetEnumerator();
        }

        public int this[int i]
        {
            get { return indices[i]; }
        }
    }
}
