using System.Collections.Generic;
using System.Linq;

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
            DisjointSet components = new DisjointSet(numRooms);
            List<RoomConnection> prunedConnections = new List<RoomConnection>();

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

        public class DisjointSet
        {
            int[] parents;
            int[] ranks;

            public DisjointSet(int numNodes)
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

        static List<RoomConnection> BucketSort(List<RoomConnection> connections)
        {
            var buckets = InitializeBuckets(connections);
            PopulateBuckets(buckets, connections);
            return UnpackBuckets(buckets);
        }

        static List<RoomConnection>[] InitializeBuckets(List<RoomConnection> connections)
        {
            int longestLength = GetLongestLength(connections);
            List<RoomConnection>[] buckets = new List<RoomConnection>[longestLength + 1];
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
            List<RoomConnection> roomConnections = new List<RoomConnection>();
            for (int i = 0; i < buckets.Length; i++)
            {
                foreach (var item in buckets[i])
                {
                    roomConnections.Add(item);
                }
            }
            return roomConnections;
        }

        static int GetLongestLength(List<RoomConnection> connections)
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

