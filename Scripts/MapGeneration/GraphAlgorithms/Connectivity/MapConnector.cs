/* This class is responsible for producing a set of connections that will efficiently ensure every region is 
 * reachable from every other region. 
 *
 * The algorithm takes a list of regions, and extracts their edge tiles, since the shortest connection between
 * two regions will always be between a pair of edges. A list of near-shortest connections between every pair of 
 * rooms (collections of edge tiles) is produced, effectively forming a dense graph where rooms are vertices and
 * potential connections are edges. A minimum spanning tree algorithm is used to efficiently produce a subgraph
 * that connects all rooms while minimizing the amount of tunneling that needs to be done. 
 *
 * Run-time analysis:
 * This component of the map generator (and in fact, of the whole cavegenerator) has the worst worst-case run-time.
 * This part explains the run-time of this algorithm ultimately in terms of the number of tiles in the map, which
 * is equal to length * width. We'll denote this number as n. The rest of the cavegenerator
 * is linear in the number of tiles.
 * This algorithm has four components:
 * 
 * 1. Convert each region to a collection of edge tiles (room).
 * 2. Compute a connection for every pair of rooms.
 * 3. Sort the connections.
 * 4. Run a minimum spanning tree algorithm on the connections. 
 * 
 * 1 is linear in n. The total size of all the regions cannot exceed the number of tiles in the map, and we're doing
 * a linear depth-first search on each region.
 * 
 * 2 is avgWorkPerConnection * numConnections, equivalent to avgWorkPerConnection * numRooms ^ 2. In the worst case,
 * this is avgWorkPerConnection * n ^ 2, as the number of rooms can in principle be linear in n. The average work
 * per connection is difficult to compute analytically, but measured empirically, it converges to 1 as the number
 * of tiles grows large. Thus the run-time of part 2 is given by numRooms ^ 2. One of the processing steps for the
 * map generator can put a sharp upper bound on the number of rooms: namely, the minimum floor size. If the min floor
 * size is k, then numRooms <= n / k. If k is on the order of sqrt(n), then numRooms is O(sqrt(n)), and the run-time
 * becomes O(n). To give an example to make this concrete, if length = 1000, width = 1000, and min floor size = 1000,
 * then the run time will be linear.
 * 
 * 3 is simpler to compute: a general sorting algorithm would be numConnections * log(numConnections), but we can do
 * better. By truncating the floats representing distances, we can use a linear-time sorting algorithm instead. During 
 * tests, using the built-in sorting algorithm (n * logn) took half the time of the entire cave generator
 * on a very large map (1000 by 1000) with min wall size set to 0, while a bucket sort took about 5%. With a min
 * wall size of 1000 they both take 3-4 ms.
 * 
 * 4 is given by the run-time for Kruskal's MST algorithm without sorting (since we did it in 3), which is 
 * numConnections * a(numConnections), where a is the inverse ackermann function. For all intents and purposes, this
 * is O(numConnections), as the inverse ackermann function is one of (if not THE) slowest growing unbounded functions
 * known to computer science which appears in a practical application. 
 * 
 * So in total, the run-time is O(numConnections * a(numConnections)), which by our computation for 2, is 
 * O(n * a(n)) given a suitable choice for the min floor size. Profiling the run-time for very large maps reveals
 * that the run-time indeed scales about linearly, and in proportion with the other parts of the cave generator.
 */

using AKSaigyouji.Maps;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

namespace AKSaigyouji.GraphAlgorithms
{
    static class MapConnector
    { 
        /// <summary>
        /// Determine the connections that need to be made in order to ensure that every region is reachable
        /// from every other region. Returns a minimal set of connections, in the sense that the sum of all distances
        /// in the connections is minimized. 
        /// </summary>
        public static ConnectionInfo[] GetConnections(Map map, List<TileRegion> regions)
        {
            if (regions.Count < 2) return new ConnectionInfo[0]; // nothing to connect

            TileRegion[] rooms = ExtractEdges(map, regions);
            ConnectionInfo[] allConnections = GetAllRoomConnections(rooms);
            ConnectionInfo[] sortedConnections = SortConnections(allConnections);
            return PruneConnections(sortedConnections);
        }

        static TileRegion[] ExtractEdges(Map map, List<TileRegion> regions)
        {
            var extractor = new EdgeExtractor(map);
            var rooms = new TileRegion[regions.Count];
            for (int i = 0; i < rooms.Length; i++)
            {
                rooms[i] = extractor.Extract(regions[i]);
            }
            return rooms;
        }

        static ConnectionInfo[] GetAllRoomConnections(TileRegion[] room)
        {
            Assert.IsTrue(room.Length > 1);
            int numRooms = room.Length;
            int numConnections = numRooms * (numRooms - 1) / 2; // The number of pairs (a,b) for a < b < numRooms
            var allConnections = new ConnectionInfo[numConnections];
            int connectionIndex = 0;
            for (int a = 0; a < numRooms; a++)
            {
                for (int b = a + 1; b < numRooms; b++)
                {
                    allConnections[connectionIndex] = ConnectionFinder.FindConnection(room[a], room[b]);
                    connectionIndex++;
                }
            }
            return allConnections;
        }

        static ConnectionInfo[] SortConnections(ConnectionInfo[] connections)
        {
            List<int>[] buckets = CreateBuckets(connections);
            ConnectionInfo[] sortedConnections = ExtractSortedFromBuckets(connections, buckets);
            return sortedConnections;
        }

        static List<int>[] CreateBuckets(ConnectionInfo[] connections)
        {
            int maxDistance = connections.Max(con => (int)con.distance);
            var buckets = new List<int>[maxDistance + 1];
            for (int i = 0; i < connections.Length; i++)
            {
                int distance = (int)connections[i].distance;
                if (buckets[distance] == null)
                {
                    buckets[distance] = new List<int>();
                }
                buckets[distance].Add(i);
            }
            return buckets;
        }

        static ConnectionInfo[] ExtractSortedFromBuckets(ConnectionInfo[] connections, List<int>[] buckets)
        {
            int currentIndex = 0;
            var sortedConnections = new ConnectionInfo[connections.Length];
            for (int bucketIndex = 0; bucketIndex < buckets.Length; bucketIndex++)
            {
                List<int> currentBucket = buckets[bucketIndex];
                if (currentBucket != null)
                {
                    for (int connIndex = 0; connIndex < currentBucket.Count; connIndex++, currentIndex++)
                    {
                        sortedConnections[currentIndex] = connections[currentBucket[connIndex]];
                    }
                }
            }
            return sortedConnections;
        }

        static ConnectionInfo[] PruneConnections(ConnectionInfo[] connections)
        {
            return MST.ComputeMST(connections);
        }
    } 
}
