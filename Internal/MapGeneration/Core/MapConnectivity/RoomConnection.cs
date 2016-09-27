using System;

namespace CaveGeneration.MapGeneration
{
    /// <summary>
    /// Represents a possible connection between two rooms. Keeps track of the rooms it's connecting, the pair
    /// of tiles corresponding to the shortest distance between them, and the corresponding distance, rounded down
    /// to the nearest integer.
    /// </summary>
    sealed class RoomConnection : IComparable<RoomConnection>
    {
        public Room roomA { get; private set; }
        public Room roomB { get; private set; }
        public Coord tileA { get; private set; }
        public Coord tileB { get; private set; }
        public int distanceBetweenRooms { get; private set; }

        public int indexA { get; private set; }
        public int indexB { get; private set; }

        public RoomConnection(Room roomA, Room roomB, int indexRoomA, int indexRoomB)
        {
            this.roomA = roomA;
            this.roomB = roomB;
            indexA = indexRoomA;
            indexB = indexRoomB;
            distanceBetweenRooms = int.MaxValue;
        }

        /* Ensuring room connectivity is the most asymptotically expensive part of the map generator, and this method
         * is the reason. The original idea was to consider every pair of edge tiles from each room,
         * settling on the pair with the lowest distance (ensuring optimality). This single method was 
         * responsible for over half the run time of the original cave generator for smaller maps, and completely 
         * dominated for larger maps. The current version is over 1000 times faster (8ms versus 18000ms) than the
         * original method for map of size 300 by 300. 
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

        public void FindShortConnection()
        {
            TileRegion edgeTilesA = roomA.edgeTiles;
            TileRegion edgeTilesB = roomB.edgeTiles;
            int indexA = 0;
            while (indexA < edgeTilesA.Count)
            {
                Coord tileA = edgeTilesA[indexA];
                int shortestDistanceToTileA = SearchConnectionsToRegion(tileA, edgeTilesB);
                indexA += shortestDistanceToTileA;
            }
        }

        // Find the optimal connection from the given tile to the corresponding region.
        int SearchConnectionsToRegion(Coord tile, TileRegion otherTiles)
        {
            int indexB = 0;
            int bestDistanceThisLoop = int.MaxValue;
            Coord bestTileBThisLoop = tile;
            while (indexB < otherTiles.Count) // Loop over other tiles, skipping tiles based on distance computed
            {
                Coord tileB = otherTiles[indexB];
                int distance = (int)tile.Distance(tileB);
                if (distance < bestDistanceThisLoop)
                {
                    bestDistanceThisLoop = distance;
                    bestTileBThisLoop = tileB;
                }
                indexB += distance;
            }
            if (bestDistanceThisLoop < distanceBetweenRooms)
            {
                UpdateOptimalConnection(tile, bestTileBThisLoop, bestDistanceThisLoop);
            }
            return bestDistanceThisLoop;
        }

        public int CompareTo(RoomConnection other)
        {
            return distanceBetweenRooms.CompareTo(other.distanceBetweenRooms);
        }

        void UpdateOptimalConnection(Coord tileA, Coord tileB, int distance)
        {
            this.tileA = tileA;
            this.tileB = tileB;
            distanceBetweenRooms = distance;
        }
    } 
}