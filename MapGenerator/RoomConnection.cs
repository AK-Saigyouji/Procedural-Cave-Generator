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
        public int squaredDistanceBetweenRooms { get; private set; }
        public int indexA { get; private set; }
        public int indexB { get; private set; }

        public RoomConnection(Room roomA, Room roomB, int indexRoomA, int indexRoomB)
        {
            this.roomA = roomA;
            this.roomB = roomB;
            indexA = indexRoomA;
            indexB = indexRoomB;
            squaredDistanceBetweenRooms = int.MaxValue;
            FindShortestConnection();
        }

        void FindShortestConnection()
        {
            int thresholdToTerminateSearch = 3;
            TileRegion edgeTilesA = roomA.edgeTiles;
            TileRegion edgeTilesB = roomB.edgeTiles;
            int indexA = 0;
            while (indexA < edgeTilesA.Count)
            {
                Coord tileA = edgeTilesA[indexA];
                int indexB = 0;
                while (indexB < edgeTilesB.Count)
                {
                    Coord tileB = edgeTilesB[indexB];
                    int distance = tileA.SquaredDistance(tileB);
                    if (distance < squaredDistanceBetweenRooms)
                    {
                        Update(tileA, tileB, distance);
                        if (distance < thresholdToTerminateSearch)
                            return;
                    }
                    indexB += GetIncrementBasedOnDistance(distance);
                }
                indexA += GetIncrementBasedOnDistance(squaredDistanceBetweenRooms);
            }
        }

        int GetIncrementBasedOnDistance(int distance)
        {
            return distance / 2;
        }

        public int CompareTo(RoomConnection other)
        {
            return squaredDistanceBetweenRooms.CompareTo(other.squaredDistanceBetweenRooms);
        }

        void Update(Coord tileA, Coord tileB, int distance)
        {
            this.tileA = tileA;
            this.tileB = tileB;
            squaredDistanceBetweenRooms = distance;
        }
    } 
}