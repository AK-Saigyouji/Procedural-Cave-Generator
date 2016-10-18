/* This class is responsible for finding a short path between two rooms / sets of edge tiles. 
 * The original idea was to consider all pairs of edge tiles from each room, selecting the pair with minimum distance.
 * This was responsible for over half the run time of the original cave generator for smaller maps, and completely 
 * dominated for larger maps. The current version is over 1000 times faster (8ms versus 18000ms) than the
 * original method for map of size 300 by 300 and now scales linearly. 
 * 
 * To optimize, a small amount of error was accepted, based on the following observation:
 * the larger the distance between the rooms, the more slack can be given in terms of finding a 
 * suboptimal connection. e.g. finding a 300 tile connection versus a 280 tile connection isn't an issue, but finding 
 * a 2 tile connection versus a 22 tile connection is problematic. So each time we compute a distance, we skip a 
 * number of tiles comparable to the computed distance. This ensures that we don't waste time at large distances but 
 * are more careful as the tiles get closer (even if we compute a large initial distance, it is possible that the 
 * rooms are very close elsewhere else). The reason this results in minimal error is that in the optimal case 
 * (i.e. where the shortest connection is always found) the average distance of the selected
 * connections is independent of the map size, and is about 3-6. So if we compute a distance between a pair of 
 * tiles of 20, then we're almost guaranteed to be 20+ tiles away from a connection that would actually get chosen.
 * Exceptions to this are statistically anomalous, and in those cases, the error is small.
 */

namespace CaveGeneration.MapGeneration.Connectivity
{
    static class ConnectionFinder
    {
        public static ConnectionInfo FindConnection(TileRegion regionA, TileRegion regionB, int roomIndexA, int roomIndexB)
        {
            Coord bestTileA = new Coord();
            Coord bestTileB = new Coord();
            float bestDistance = float.MaxValue;

            int indexA = 0;
            while (indexA < regionA.Count)
            {
                int indexB = 0;
                Coord tileA = regionA[indexA];
                Coord bestTileBThisLoop = new Coord();
                float bestDistanceThisLoop = float.MaxValue;
                while (indexB < regionB.Count)
                {
                    Coord tileB = regionB[indexB];
                    float distance = tileA.Distance(tileB);
                    if (distance < bestDistanceThisLoop)
                    {
                        bestDistanceThisLoop = distance;
                        bestTileBThisLoop = tileB;
                    }
                    indexB += (int)distance;
                }
                if (bestDistanceThisLoop < bestDistance)
                {
                    bestTileA = tileA;
                    bestTileB = bestTileBThisLoop;
                    bestDistance = bestDistanceThisLoop;
                }
                indexA += (int)bestDistanceThisLoop;
            }
            return new ConnectionInfo(bestTileA, bestTileB, roomIndexA, roomIndexB, bestDistance);
        }
    } 
}
