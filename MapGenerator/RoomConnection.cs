using System;
namespace MapHelpers
{
    /// <summary>
    /// Class representing a possible connection between two rooms. Keeps track of the rooms it's connecting and the pair
    /// of tiles corresponding to the shortest distance between them.
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
                while (indexB < edgeTilesB.Count)
                {
                    Coord tileB = edgeTilesB[indexB];
                    int distance = tileA.SupNormDistance(tileB);
                    if (distance < distanceBetweenRooms)
                    {
                        Update(tileA, tileB, distance);
                        if (distance < thresholdToTerminateSearch)
                            return;
                    }
                    indexB += distance;
                }
                indexA += distanceBetweenRooms;
            }
        }

        public int CompareTo(RoomConnection other)
        {
            return distanceBetweenRooms.CompareTo(other.distanceBetweenRooms);
        }

        void Update(Coord tileA, Coord tileB, int distance)
        {
            this.tileA = tileA;
            this.tileB = tileB;
            distanceBetweenRooms = distance;
        }
    } 
}