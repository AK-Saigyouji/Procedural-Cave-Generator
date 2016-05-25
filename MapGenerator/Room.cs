using System;

namespace MapHelpers
{
    /// <summary>
    /// A connected, open space in the map, keeping track of which of its tiles are adjacent to walls.
    /// </summary>
    class Room : IComparable<Room>
    {
        public TileRegion allTiles { get; private set; }
        /// <summary>
        /// All floor tiles in this room that have an adjacent wall tile. 
        /// </summary>
        public TileRegion edgeTiles { get; private set; }
        public int Count { get; private set; }

        public Room(TileRegion region, Map map)
        {
            allTiles = region;
            Count = region.Count;
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
            return Count.CompareTo(otherRoom.Count);
        }

        public override string ToString()
        {
            return String.Format("Room with {0} tiles, {1} on the edge.", allTiles.Count, edgeTiles.Count);
        }
    } 
}