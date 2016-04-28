using System;

namespace MapHelpers
{
    /// <summary>
    /// This class represents a connected, open space in the map. It keeps track of the tiles on its fringe (tiles adjacent
    /// to walls or exits from the room). 
    /// </summary>
    class Room : IComparable<Room>
    {
        public TileRegion allTiles { get; private set; }
        public TileRegion edgeTiles { get; private set; }
        public int Size { get; private set; }

        public Room(TileRegion region, Map map)
        {
            this.allTiles = region;
            Size = region.Size;
            DetermineEdgeTiles(map);
        }

        void DetermineEdgeTiles(Map map)
        {
            edgeTiles = new TileRegion();
            foreach (Coord tile in allTiles)
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
            return String.Format("Room with {0} tiles, {1} on the edge.", allTiles.Size, edgeTiles.Size);
        }
    } 
}