/* Note: this implementation uses a dictionary to support removal operations. For applications that don't need this 
 operation, an implementation without the dictionary would offer significantly improved performance with no downsides,
 as dictionaries add a lot of overhead per element, especially if T is a small struct.*/

using System;
using System.Collections.Generic;

namespace AKSaigyouji.DataStructures
{
    /// <summary>
    /// Priority queue, implemented as a binary heap. Supports removal.
    /// </summary>
    public class PriorityQueue<T> where T : IComparable<T>
    {
        readonly List<T> data;
        readonly Dictionary<T, int> indices; // Needed for removal operations.

        public PriorityQueue()
        {
            data = new List<T>();
            indices = new Dictionary<T, int>();
        }

        public PriorityQueue(IEnumerable<T> data)
        {
            var dataList = new List<T>();
            var indices = new Dictionary<T, int>();
            int currentIndex = 0;
            foreach (T datum in data)
            {
                dataList.Add(datum);
                indices[datum] = currentIndex;
                currentIndex++;
            }
            this.indices = indices;
            this.data = dataList;
            for (int i = this.data.Count / 2; i >= 0; i--)
            {
                BubbleDown(i);
            }
        }

        public void Enqueue(T item)
        {
            data.Add(item);
            int itemIndex = data.Count - 1;
            indices[item] = itemIndex;
            while (IsItemOutOfPlace(itemIndex))
            {
                int parentIndex = GetParentIndex(itemIndex);
                SwapAtIndices(itemIndex, parentIndex);
                itemIndex = parentIndex;
            }
        }

        public T Dequeue()
        {
            if (data.Count == 0)
                throw new InvalidOperationException("Queue empty.");

            return RemoveAt(0);
        }

        public int Count { get { return data.Count; } }

        public void Remove(T item)
        {
            RemoveAt(indices[item]);
        }

        public T Peek()
        {
            if (data.Count == 0)
                throw new InvalidOperationException("Queue empty.");

            return data[0];
        }

        public void Clear()
        {
            indices.Clear();
            data.Clear();
        }

        T RemoveAt(int index)
        {
            indices.Remove(data[index]);
            int lastIndex = data.Count - 1;
            T removedItem = data[index];
            SwapAtIndices(index, lastIndex);
            data.RemoveAt(lastIndex);
            BubbleDown(index);
            return removedItem;
        }

        void BubbleDown(int index)
        {
            int lastIndex = data.Count - 1;
            while (GetLeftChildIndex(index) <= lastIndex)
            {
                int childIndex = GetSmallestChildIndex(index);
                if (IsCorrectOrder(childIndex, index))
                    break;

                SwapAtIndices(childIndex, index);
                index = childIndex;
            }
        }

        int GetSmallestChildIndex(int parentIndex)
        {
            int leftIndex = GetLeftChildIndex(parentIndex);
            int rightIndex = leftIndex + 1;
            if (rightIndex < data.Count && data[rightIndex].CompareTo(data[leftIndex]) < 0)
            {
                return rightIndex;
            }
            else
            {
                return leftIndex;
            }
        }

        void SwapAtIndices(int indexOne, int indexTwo)
        {
            T itemOne = data[indexOne];
            T itemTwo = data[indexTwo];
            data[indexOne] = itemTwo;
            data[indexTwo] = itemOne;
            indices[itemTwo] = indexOne;
            indices[itemOne] = indexTwo;
        }

        bool IsItemOutOfPlace(int itemIndex)
        {
            int parentIndex = GetParentIndex(itemIndex);
            return itemIndex > 0 && !IsCorrectOrder(itemIndex, parentIndex);
        }

        bool IsCorrectOrder(int childIndex, int parentIndex)
        {
            return data[childIndex].CompareTo(data[parentIndex]) >= 0;
        }

        static int GetLeftChildIndex(int itemIndex)
        {
            return itemIndex * 2 + 1;
        }

        static int GetParentIndex(int itemIndex)
        {
            return (itemIndex - 1) / 2;
        }
    } 
}