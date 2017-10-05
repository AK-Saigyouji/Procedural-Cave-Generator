using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace AKSaigyouji.Modules.Outlines
{
    public sealed class Outline : IList<Vector3>
    {
        readonly Vector3[] outline;

        public Vector3 this[int index]
        {
            get { return outline[index]; }
            set { ((IList<Vector3>)outline)[index] = value; }
        }

        public int Count { get { return outline.Length; } }

        public bool IsReadOnly { get { return true; } }

        public Outline(IEnumerable<Vector3> outline)
        {
            if (outline == null)
                throw new ArgumentNullException("outline");

            if (!outline.Any())
                throw new ArgumentException("Outline is empty.");

            this.outline = outline.ToArray();
        }

        public IEnumerable<Edge> GetEdges()
        {
            if (Count > 1)
            {
                for (int i = 0; i < outline.Length - 1; i++)
                {
                    yield return new Edge(outline[i], outline[i + 1]);
                }
            }
        }

        public void Add(Vector3 item)
        {
            ((IList<Vector3>)outline).Add(item);
        }

        public void Clear()
        {
            ((IList<Vector3>)outline).Clear();
        }

        public bool Contains(Vector3 item)
        {
            return outline.Contains(item);
        }

        public void CopyTo(Vector3[] array, int arrayIndex)
        {
            outline.CopyTo(array, arrayIndex);
        }

        public IEnumerator<Vector3> GetEnumerator()
        {
            return (IEnumerator<Vector3>)outline.GetEnumerator();
        }

        public int IndexOf(Vector3 item)
        {
            return Array.IndexOf(outline, item);
        }

        public void Insert(int index, Vector3 item)
        {
            ((IList<Vector3>)outline).Insert(index, item);
        }

        public bool Remove(Vector3 item)
        {
            return ((IList<Vector3>)outline).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<Vector3>)outline).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return outline.GetEnumerator();
        }
    } 
}