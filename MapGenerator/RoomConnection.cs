namespace MapHelpers
{
    class RoomConnection : System.IComparable<RoomConnection>
    {
        internal Room roomA { get; private set; }
        internal Room roomB { get; private set; }
        internal Coord tileA { get; private set; }
        internal Coord tileB { get; private set; }
        internal int distance { get; private set; }
        internal int indexA { get; private set; }
        internal int indexB { get; private set; }

        internal RoomConnection(Room roomA, Room roomB, int indexRoomA, int indexRoomB)
        {
            this.roomA = roomA;
            this.roomB = roomB;
            indexA = indexRoomA;
            indexB = indexRoomB;
            distance = int.MaxValue;
            FindShortestConnection();
        }

        void FindShortestConnection()
        {
            foreach (Coord tileA in roomA.edgeTiles)
            {
                foreach (Coord tileB in roomB.edgeTiles)
                {
                    int distance = tileA.SquaredDistance(tileB);
                    if (distance < this.distance)
                    {
                        Update(tileA, tileB, distance);
                    }
                }
            }
        }

        public int CompareTo(RoomConnection other)
        {
            return distance.CompareTo(other.distance);
        }

        void Update(Coord tileA, Coord tileB, int distance)
        {
            this.tileA = tileA;
            this.tileB = tileB;
            this.distance = distance;
        }
    } 
}