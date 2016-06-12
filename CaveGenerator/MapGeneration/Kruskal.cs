using System.Collections.Generic;

namespace CaveGeneration.MapGeneration
{
    static class Kruskal
    {
        /// <summary>
        /// Kruskal's minimum spanning tree algorithm, treating each RoomConnection as an weighted edge and each Room as 
        /// a vertex. Given a list of connections, this will find a minimal subset of connections with the shortest distances
        /// between rooms. 
        /// </summary>
        /// <param name="connections">A list of RoomConnection objects.</param>
        /// <param name="numRooms">Total number of Rooms (vertices).</param>
        /// <returns>Returns a minimum list of RoomConnection objects needed to connect rooms together.</returns>
        static public List<RoomConnection> GetMinimalConnections(List<RoomConnection> connections, int numRooms)
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

