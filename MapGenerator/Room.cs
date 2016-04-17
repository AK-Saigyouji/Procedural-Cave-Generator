using System;
using System.Linq;

namespace MapHelpers
{
    class Room : IComparable<Room>
    {
        internal TileRegion innerTiles { get; private set; }
        internal TileRegion edgeTiles { get; private set; }
        internal int Size { get; private set; }

        internal Room(TileRegion region, Map map)
        {
            this.innerTiles = region;
            Size = region.Size();
            DetermineEdgeTiles(map);
        }

        void DetermineEdgeTiles(Map map)
        {
            edgeTiles = new TileRegion();
            foreach (Coord tile in innerTiles)
            {
                if (map.IsEdgeTile(tile))
                {
                    edgeTiles.Add(tile);
                }
            }
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