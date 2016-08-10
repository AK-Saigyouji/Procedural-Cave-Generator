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
        public int Length { get { return indices.Length; } }

        int[] indices;
        
        public Outline(List<int> indexList)
        {
            indices = new int[indexList.Count];
            for (int i = 0; i < indexList.Count; i++)
            {
                indices[i] = indexList[i];
            }
        }

        public void Reverse()
        {
            int length = indices.Length;
            for (int i = 0; i < length; i++)
            {
                int temp = indices[i];
                indices[i] = indices[length - i - 1];
                indices[length - i - 1] = temp;
            }
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
