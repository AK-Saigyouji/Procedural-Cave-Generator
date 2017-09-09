/* This class is responsible for finding a short path between two rooms / sets of edge tiles. 
 * The original idea was to consider all pairs of edge tiles from each room, selecting the pair with minimum distance.
 * This was responsible for over half the run time of the original cave generator for smaller maps, and completely 
 * dominated for larger maps. The current version is over 1000 times faster (8ms versus 18000ms) than the
 * original method for maps of size 300 by 300 and now scales linearly. 
 * 
 * To optimize, a small amount of error was accepted based on the following observation:
 * the larger the distance between the rooms, the more slack can be given in terms of finding a 
 * suboptimal connection. e.g. finding a 300 tile connection versus a 280 tile connection isn't an issue, but finding 
 * a 2 tile connection versus a 22 tile connection is problematic. So each time we compute a distance, we skip a 
 * number of tiles comparable to the computed distance. This ensures that we don't waste time at large distances but 
 * are more careful as the tiles get closer (even if we compute a large initial distance, it is possible that the 
 * rooms are very close elsewhere). The reason this results in minimal error is that in the optimal case 
 * (i.e. where the shortest connection is always found) the average distance of the selected
 * connections is independent of the map size, and is about 3-6 (at 50% map density). 
 * So if we compute a distance between a pair of tiles of 20, then we're almost guaranteed to be 20+ tiles away 
 * from a connection that would actually get chosen. Exceptions to this are statistically anomalous, and in those 
 * cases, the error is small.
 */

using AKSaigyouji.Maps;
using UnityEngine.Assertions;

namespace AKSaigyouji.GraphAlgorithms
{
    static class ConnectionFinder
    {
        public static ConnectionInfo FindConnection(TileRegion regionA, TileRegion regionB)
        {
            Assert.IsNotNull(regionA);
            Assert.IsNotNull(regionB);
            Assert.AreNotEqual(regionA, regionB);

            var bestConnection = new TempConnection(Coord.zero, Coord.zero, float.MaxValue);
            int indexA = 0;
            while (indexA < regionA.Count)
            {
                int indexB = 0;
                var bestThisLoop = new TempConnection(regionA[indexA], Coord.zero, float.MaxValue);
                while (indexB < regionB.Count)
                {
                    Coord tileB = regionB[indexB];
                    float distance = bestThisLoop.tileA.Distance(tileB);
                    if (distance < bestThisLoop.distance)
                    {
                        bestThisLoop = new TempConnection(bestThisLoop.tileA, tileB, distance);
                    }
                    indexB += (int)distance; // distance is never < 1, as minimum occurs with diagonally adjacent tiles
                }
                if (bestThisLoop.distance < bestConnection.distance)
                {
                    bestConnection = bestThisLoop;
                }
                indexA += (int)bestThisLoop.distance;
            }
            return new ConnectionInfo(bestConnection.tileA, bestConnection.tileB, regionA.Index, regionB.Index, bestConnection.distance);
        }

        // This is used exclusively by ConnectionFinder to group three pieces of related connection data together
        // without having to build a full ConnectionInfo each time. 
        struct TempConnection
        {
            public readonly float distance;
            public readonly Coord tileA;
            public readonly Coord tileB;

            public TempConnection(Coord tileA, Coord tileB, float distance)
            {
                this.distance = distance;
                this.tileA = tileA;
                this.tileB = tileB;
            }
        }
    } 
}
