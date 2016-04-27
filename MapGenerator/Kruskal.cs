using System.Collections.Generic;

namespace MapHelpers
{
    static class Kruskal
    {
        static public List<RoomConnection> GetPrunedConnections(List<RoomConnection> connections, int numRooms)
        {
            DisjointSet components = new DisjointSet();
            connections.Sort();
            List<RoomConnection> prunedConnections = new List<RoomConnection>();

            for (int i = 0; i < numRooms; i++)
            {
                components.MakeSet(i);
            }
            foreach (RoomConnection connection in connections)
            {
                int indexA = connection.indexA;
                int indexB = connection.indexB;
                if (components.Find(indexA) != components.Find(indexB))
                {
                    prunedConnections.Add(connection);
                    components.Union(indexA, indexB);
                }
            }
            return prunedConnections;
        }

        public class DisjointSet
        {
            List<int> parent = new List<int>();

            public int Find(int i)
            {
                return (parent[i] == i) ? i : Find(parent[i]);
            }

            public void MakeSet(int i)
            {
                parent.Add(i);
            }

            public int Count()
            {
                return parent.Count;
            }

            public void Union(int i, int j)
            {
                int iRepresentative = Find(i);
                int jRepresentative = Find(j);
                parent[iRepresentative] = jRepresentative;
            }
        }
    } 
}

