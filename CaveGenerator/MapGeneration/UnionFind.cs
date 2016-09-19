namespace CaveGeneration.MapGeneration
{
    public class UnionFind
    {
        int[] parents;
        int[] ranks;

        public UnionFind(int numNodes)
        {
            parents = GetRange(numNodes + 1);
            ranks = new int[numNodes + 1];
        }

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
                ranks[parentA] += 1;
                parents[parentB] = parentA;
            }
        }

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