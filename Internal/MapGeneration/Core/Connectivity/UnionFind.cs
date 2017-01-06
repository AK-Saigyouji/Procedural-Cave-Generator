/* This is a general implementation of the UnionFind (also known as Disjoint Set) data structure.*/

namespace CaveGeneration.MapGeneration.Connectivity
{
    sealed class UnionFind
    {
        readonly int[] parents;
        readonly int[] ranks;

        public UnionFind(int numNodes)
        {
            parents = GetRange(numNodes + 1);
            ranks = new int[numNodes + 1];
        }

        /// <exception cref="System.IndexOutOfRangeException"></exception>
        public void Union(int nodeA, int nodeB)
        {
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

        /// <exception cref="System.IndexOutOfRangeException"></exception>
        public int Find(int node)
        {
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
        int[] GetRange(int count)
        {
            int[] range = new int[count];
            for (int i = 0; i < count; i++)
            {
                range[i] = i;
            }
            return range;
        }
    }
}