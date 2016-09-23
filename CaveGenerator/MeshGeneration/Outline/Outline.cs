using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Outline represents an ordered collection of indices corresponding to points that separate a region of walls 
    /// from a region of space.
    /// </summary>
    sealed class Outline: IEnumerable<VertexIndex>
    {
        public int Length { get { return indices.Length; } }

        VertexIndex[] indices;
        
        public Outline(List<VertexIndex> indexList)
        {
            indices = new VertexIndex[indexList.Count];
            for (int i = 0; i < indexList.Count; i++)
            {
                indices[i] = indexList[i];
            }
        }

        public void Reverse()
        {
            int length = indices.Length;
            int midPoint = length / 2;
            for (int i = 0; i < midPoint; i++)
            {
                VertexIndex temp = indices[i];
                indices[i] = indices[length - i - 1];
                indices[length - i - 1] = temp;
            }
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
