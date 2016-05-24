using System.Collections.Generic;

namespace MapHelpers
{
    /// <summary>
    /// Class representing a possible connection between two rooms. Keeps track of the rooms it's connecting and the pair
    /// of tiles corresponding to the shortest distance between them.
    /// </summary>
    class RoomConnection : System.IComparable<RoomConnection>
    {
        public Room roomA { get; private set; }
        public Room roomB { get; private set; }
        public Coord tileA { get; private set; }
        public Coord tileB { get; private set; }
        public int squaredDistance { get; private set; }
        public int indexA { get; private set; }
        public int indexB { get; private set; }

        int MAX_ERROR_IN_SHORTEST_CONNECTION = 6;

        public RoomConnection(Room roomA, Room roomB, int indexRoomA, int indexRoomB)
        {
            this.roomA = roomA;
            this.roomB = roomB;
            indexA = indexRoomA;
            indexB = indexRoomB;
            squaredDistance = int.MaxValue;
            FindShortestConnection();
        }

        void FindShortestConnection()
        {
            var edgeTilesA = GetOptimizedEdgeTileList(roomA.edgeTiles);
            var edgeTilesB = GetOptimizedEdgeTileList(roomB.edgeTiles);
            //var edgeTilesA = roomA.edgeTiles;
            //var edgeTilesB = roomB.edgeTiles;
            foreach (Coord tileA in edgeTilesA)
            {
                foreach (Coord tileB in edgeTilesB)
                {
                    int distance = tileA.SquaredDistance(tileB);
                    if (distance < this.squaredDistance)
                    {
                        Update(tileA, tileB, distance);
                    }
                }
            }
        }

        IEnumerable<Coord> GetOptimizedEdgeTileList(TileRegion edgeTiles)
        {
            int incrementor = 1;
            if (edgeTiles.Count > MAX_ERROR_IN_SHORTEST_CONNECTION)
            {
                incrementor += MAX_ERROR_IN_SHORTEST_CONNECTION;
            }
            for (int i = 0; i < edgeTiles.Count; i += incrementor)
            {
                yield return edgeTiles[i];
            }
        }
        
        public int CompareTo(RoomConnection other)
        {
            return squaredDistance.CompareTo(other.squaredDistance);
        }

        void Update(Coord tileA, Coord tileB, int distance)
        {
            this.tileA = tileA;
            this.tileB = tileB;
            this.squaredDistance = distance;
        }
    } 
}