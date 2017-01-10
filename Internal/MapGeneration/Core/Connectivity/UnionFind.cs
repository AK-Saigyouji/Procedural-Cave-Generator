﻿/* This is a general implementation of the UnionFind (also known as Disjoint Set) data structure.*/

namespace CaveGeneration.MapGeneration.Connectivity
{
    sealed class UnionFind
    {
        readonly int[] parents;
        readonly int[] ranks;

        readonly int maxIndex;

        /// <param name="numNodes">Must be at least 1.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public UnionFind(int numNodes)
        {
            if (numNodes < 1)
                throw new System.ArgumentOutOfRangeException("numNodes");

            parents = GetRange(numNodes + 1);
            ranks = new int[numNodes + 1];
            maxIndex = numNodes;
        }

        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public void Union(int nodeA, int nodeB)
        {
            if (0 > nodeA || nodeA > maxIndex)
                throw GetArgumentRangeException("nodeA", nodeA);

            if (0 > nodeB || nodeB > maxIndex)
                throw GetArgumentRangeException("nodeB", nodeB);

            if (nodeA == nodeB)
                return;

            int parentA = Find(nodeA);
            int parentB = Find(nodeB);
            if (ranks[parentA] > ranks[parentB])
            {
                parents[parentB] = parentA;
            }
            else if (ranks[parentA] < ranks[parentB])
            {
                parents[parentA] = parentB;
            }
            else
            {
                ranks[parentA]++;
                parents[parentB] = parentA;
            }
        }

        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public int Find(int node)
        {
            if (0 > node || node > maxIndex)
                throw GetArgumentRangeException("node", node);

            int parent = parents[node];
            if (parent != node)
            {
                parent = Find(parent);
                parents[node] = parent;
            }
            return parent;
        }

        /// <summary>
        /// Create an array of integers from 0 inclusive to count exclusive.
        /// </summary>
        static int[] GetRange(int count)
        {
            var range = new int[count];
            for (int i = 0; i < range.Length; i++)
            {
                range[i] = i;
            }
            return range;
        }

        System.ArgumentOutOfRangeException GetArgumentRangeException(string paramName, int value)
        {
            string errorMessage = string.Format("Must be between 0 and {0}. Actual: {1}.", maxIndex, value);
            return new System.ArgumentOutOfRangeException(paramName, errorMessage);
        }
    }
}