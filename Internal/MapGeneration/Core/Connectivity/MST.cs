/* This is a static class to compute a minimum spanning tree out of a collection of connections (edges). 
 * This implementation uses Kruskal's algorithm which runs in what is effectively linear time.
 * It takes as input edges in a connected graph. It returns a smaller collection of edges such that the resulting
 * subgraph is still connected, but the total distance/weight of the sum of edges is minimized.*/

using System.Collections.Generic;

namespace CaveGeneration.MapGeneration.Connectivity
{
    /// <summary>
    /// Computes minimum spanning trees.
    /// </summary>
    static class MST
    {
        /* The following method imposes a lot of requirements on the input. This is done because the MST
         * algorithm is so fast that ensuring these requirements are met in a general manner would take longer
         * than the algorithm itself. e.g. a general sorting algorithm takes log-linear time, but depending
         * on application, additional information could be exploited to sort in linear time (e.g. if dealing
         * with integers in a narrow range). */

        /// <summary>
        /// Compute the minimum spanning tree from the given connections/egdes. Resulting connections
        /// are guaranteed to form a connected subgraph only if input connections form a connected graph. If
        /// input is unsorted, a spanning tree will still be produced but may not be optimal. 
        /// </summary>
        /// <param name="connections">Array of connections sorted in increasing order by weight.</param>
        /// <param name="numRooms">ConnectionInfo specifies room indices, and this parameters corresponds
        /// to the maximum room index possible.</param>
        /// <returns>Collection of </returns>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static ConnectionInfo[] ComputeMST(ConnectionInfo[] connections, int numRooms)
        {
            if (connections == null) throw new System.ArgumentNullException("connections");

            var components = new UnionFind(numRooms);
            var prunedConnections = new List<ConnectionInfo>();

            foreach (ConnectionInfo connection in connections)
            {
                if (components.Find(connection.roomIndexA) != components.Find(connection.roomIndexB))
                {
                    prunedConnections.Add(connection);
                    components.Union(connection.roomIndexA, connection.roomIndexB);
                }
            }

            return prunedConnections.ToArray();
        }
    } 
}
