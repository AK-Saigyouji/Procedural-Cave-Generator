using System;
using System.Linq;

namespace MapHelpers
{
    class Room : IComparable<Room>
    {
        internal Region innerTiles { get; private set; }
        internal Region edgeTiles { get; private set; }
        internal int Size { get; private set; }

        Map map;

        internal Room(Region region, Map map)
        {
            this.map = map;
            this.innerTiles = region;
            Size = region.Size();
            DetermineEdgeTiles();
        }

        void DetermineEdgeTiles()
        {
            edgeTiles = new Region();
            foreach (Coord tile in innerTiles)
            {
                if (IsEdgeTile(tile))
                {
                    edgeTiles.Add(tile);
                }
            }
        }

        bool IsEdgeTile(Coord tile)
        {
            return map.GetAdjacentTiles(tile).Any(adjTile => map[adjTile.x, adjTile.y] == 1);
        }

        public int CompareTo(Room otherRoom)
        {
            return Size.CompareTo(otherRoom.Size);
        }

        public override string ToString()
        {
            return String.Format("Room with {0} tiles, {1} on the edge.", innerTiles.Size(), edgeTiles.Size());
        }
    } 
}