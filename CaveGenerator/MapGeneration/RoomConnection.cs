using System;

namespace CaveGeneration.MapGeneration
{
    /// <summary>
    /// Computes a possible connection between two rooms. Keeps track of the rooms it's connecting, the pair
    /// of tiles corresponding to the shortest distance between them, and the corresponding distance.
    /// </summary>
    class RoomConnection : IComparable<RoomConnection>
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
            FindShortConnection();
        }

        /* Ensuring room connectivity is the most computationally involved part of the  of the map generator and this 
         * algorithm constitutes the majority of the work for this purpoes. The original idea was to consider every pair 
         * of edge tiles, settling on the pair with the lowest distance (ensuring optimality). This single method was 
         * responsible for over half the run time of the original cave generator. 
         * 
         * In order to optimize it, it was necessary either to lose the guarantee of connecting all rooms, or to give up 
         * optimality. The latter was chosen, using the following insight in order to dramatically improve performance in 
         * large maps: the larger the distance between the rooms, the more slack can be given in terms of finding a 
         * suboptimal connection. e.g.finding a 300 tile connection versus a 280 tile connection isn't an issue, but finding 
         * a 2 tile connection versus a 22 tile connection is problematic. So each time we compute a distance, we skip a 
         * number of tiles comparable to the computed distance. This ensures that we don't waste time at large distances but 
         * are more careful as the tiles get closer (even if we compute a large initial distance, it is possible that the 
         * rooms are very close elsewhere else). 
         */

        void FindShortConnection()
        {
            int thresholdToTerminateSearch = 3;
            TileRegion edgeTilesA = roomA.edgeTiles;
            TileRegion edgeTilesB = roomB.edgeTiles;
            int indexA = 0;
            while (indexA < edgeTilesA.Count)
            {
                Coord tileA = edgeTilesA[indexA];
                int indexB = 0;
                int bestDistanceThisLoop = int.MaxValue;
                Coord bestTileBThisLoop = tileA;
                while (indexB < edgeTilesB.Count)
                {
                    Coord tileB = edgeTilesB[indexB];
                    int distance = tileA.SupNormDistance(tileB);
                    if (distance < bestDistanceThisLoop)
                    {
                        bestDistanceThisLoop = distance;
                        bestTileBThisLoop = tileB;
                    }
                    indexB += distance;
                }
                if (bestDistanceThisLoop < distanceBetweenRooms)
                {
                    UpdateOptimalConnection(tileA, bestTileBThisLoop, bestDistanceThisLoop);
                    if (bestDistanceThisLoop < thresholdToTerminateSearch)
                        return;
                }
                indexA += bestDistanceThisLoop;
            }
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