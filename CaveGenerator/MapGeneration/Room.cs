using System;
using System.Linq;
using System.Collections.Generic;

namespace CaveGeneration.MapGeneration
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
        public int Count { get { return allTiles.Count; } }

        public Room(TileRegion region, Map map)
        {
            allTiles = region;
            edgeTiles = GetEdgeTiles(map);
        }

        /// <summary>
        /// Gets the floor tiles that are on the boundary between floors and walls. The floor tiles should be returned
        /// in roughly 'sorted' order: iterating through the tile region should trace out a roughly continuous path 
        /// around the room.
        /// </summary>
        TileRegion GetEdgeTiles(Map map)
        {
            Dictionary<Coord, bool> visited = new Dictionary<Coord, bool>();
            for (int i = 0; i < allTiles.Count; i++)
            {
                visited[allTiles[i]] = false;
            }
            TileRegion edgeTiles = new TileRegion();
            Stack<Coord> stack = new Stack<Coord>();

            Coord firstTile = allTiles[0];
            stack.Push(firstTile);
            edgeTiles.Add(firstTile);
            visited[firstTile] = true;

            while (stack.Count > 0)
            {
                Coord tile = stack.Pop();
                foreach (Coord adjacentTile in GetAdjacentCoords(tile))
                {
                    if (visited.ContainsKey(adjacentTile) 
                        && map.IsAdjacentToWallFast(adjacentTile) 
                        && !visited[adjacentTile])
                    {
                        visited[adjacentTile] = true;
                        stack.Push(adjacentTile);
                        edgeTiles.Add(adjacentTile);
                    }
                }
            }
            return edgeTiles;
        }

        IEnumerable<Coord> GetAdjacentCoords(Coord tile)
        {
            Coord left = tile.left;
            Coord right = tile.right;

            yield return tile.up;
            yield return tile.down;
            yield return right;
            yield return left;
            yield return left.up;
            yield return left.down;
            yield return right.up;
            yield return right.down;
        }

        public int CompareTo(Room otherRoom)
        {
            return Count.CompareTo(otherRoom.Count);
        }

        public override string ToString()
        {
            return string.Format("Room with {0} tiles, {1} on the edge.", allTiles.Count, edgeTiles.Count);
        }
    }
}