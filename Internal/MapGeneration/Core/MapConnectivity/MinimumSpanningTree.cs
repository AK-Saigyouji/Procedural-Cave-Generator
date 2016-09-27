using System.Collections.Generic;

namespace CaveGeneration.MapGeneration
{
    static class MinimumSpanningTree
    {
        /// <summary>
        /// Kruskal's minimum spanning tree algorithm, treating each RoomConnection as a weighted edge and each Room as 
        /// a vertex. Given a list of connections, returns the connections necessary to produce a connected subgraph
        /// with minimum total distance.
        /// </summary>
        /// <returns>Minimum spanning tree of RoomConnections with respect to distance.</returns>
        static public List<RoomConnection> GetMinimalConnections(List<RoomConnection> connections, int numRooms)
        {
            connections.Sort();
            return KruskalMinimumSpanningTree(connections, numRooms);
        }

        /// <summary>
        /// Kruskal's minimum spanning tree algorithm, treating each RoomConnection as a weighted edge and each Room as 
        /// a vertex. Given a list of connections, this will find a connected subset of connections with the shortest 
        /// total distance between rooms. Assumes distances are nonnegative integers with a reasonable upper bound 
        /// (if significantly larger than 10000, consider using the non-discrete method).
        /// </summary>
        /// <param name="connections">List of RoomConnection objects where each distance is a nonnegative integer.</param>
        /// /// <returns>Minimum spanning tree of RoomConnections with respect to distance.</returns>
        static public List<RoomConnection> GetMinimalConnectionsDiscrete(List<RoomConnection> connections, int numRooms)
        {
            connections = BucketSort(connections);
            return KruskalMinimumSpanningTree(connections, numRooms);
        }

        static List<RoomConnection> KruskalMinimumSpanningTree(List<RoomConnection> sortedConnections, int numRooms)
        {
            var components = new UnionFind(numRooms);
            var prunedConnections = new List<RoomConnection>();

            foreach (RoomConnection connection in sortedConnections)
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

        static List<RoomConnection> BucketSort(List<RoomConnection> connections)
        {
            var buckets = InitializeBuckets(connections);
            PopulateBuckets(buckets, connections);
            return UnpackBuckets(buckets);
        }

        static List<RoomConnection>[] InitializeBuckets(List<RoomConnection> connections)
        {
            int longestLength = GetLongestDistance(connections);
            var buckets = new List<RoomConnection>[longestLength + 1];
            for (int i = 0; i < buckets.Length; i++)
            {
                buckets[i] = new List<RoomConnection>();
            }
            return buckets;
        }

        static void PopulateBuckets(List<RoomConnection>[] buckets, List<RoomConnection> connections)
        {
            foreach (var connection in connections)
            {
                buckets[connection.distanceBetweenRooms].Add(connection);
            }
        }

        static List<RoomConnection> UnpackBuckets(List<RoomConnection>[] buckets)
        {
            var roomConnections = new List<RoomConnection>();
            for (int i = 0; i < buckets.Length; i++)
            {
                foreach (var item in buckets[i])
                {
                    roomConnections.Add(item);
                }
            }
            return roomConnections;
        }

        /// <summary>
        /// From the list of connections, retrieve the maximum distance.
        /// </summary>
        static int GetLongestDistance(List<RoomConnection> connections)
        {
            int bestSoFar = 0;
            for (int i = 0; i < connections.Count; i++)
            {
                if (bestSoFar < connections[i].distanceBetweenRooms)
                {
                    bestSoFar = connections[i].distanceBetweenRooms;
                }
            }
            return bestSoFar;
        }
    } 
}