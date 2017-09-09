/* This is a static class to compute a minimum spanning tree out of a collection of connections (edges). 
 * This implementation uses Kruskal's algorithm which runs in what is effectively linear time.
 * It takes as input edges in a connected graph. It returns a smaller collection of edges such that the resulting
 * subgraph is still connected, but the total distance/weight of the sum of edges is minimized.*/

using System;
using System.Collections.Generic;
using AKSaigyouji.DataStructures;

namespace AKSaigyouji.GraphAlgorithms
{
    /// <summary>
    /// Computes minimum spanning trees.
    /// </summary>
    static class MST
    {
        /// <summary>
        /// Compute the minimum spanning tree from the given connections/edges. Resulting connections
        /// are guaranteed to form a connected subgraph only if input connections form a connected graph. If
        /// input is unsorted, a spanning tree will still be produced but may not be optimal. 
        /// </summary>
        /// <param name="connections">Array of connections sorted in increasing order by weight.</param>
        /// <returns>Connections specifying a minimum spanning tree.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static ConnectionInfo[] ComputeMST(ConnectionInfo[] connections)
        {
            if (connections == null) throw new ArgumentNullException("connections");

            int numRooms = CountNumberOfRooms(connections);
            var components = new UnionFind(numRooms);
            var prunedConnections = new List<ConnectionInfo>(numRooms);

            foreach (ConnectionInfo connection in connections)
            {
                int indexA = connection.roomIndexA;
                int indexB = connection.roomIndexB;
                if (components.Find(indexA) != components.Find(indexB))
                {
                    prunedConnections.Add(connection);
                    components.Union(indexA, indexB);
                }
            }
            return prunedConnections.ToArray();
        }

        static int CountNumberOfRooms(ConnectionInfo[] connections)
        {
            int maxRoomIndex = -1;
            foreach (ConnectionInfo connection in connections)
            {
                maxRoomIndex = Math.Max(connection.roomIndexA, maxRoomIndex);
                maxRoomIndex = Math.Max(connection.roomIndexB, maxRoomIndex);
            }
            // if indices are 0, 1, 2, 3, then maxRoomIndex is 3 but the number of rooms is 4.
            return maxRoomIndex + 1;
        }
    } 
}
