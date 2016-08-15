using System;
using System.Linq;
using System.Collections.Generic;

namespace CaveGeneration.MapGeneration
{
    /// <summary>
    /// A connected, open space in the map, keeping track of which of its tiles are adjacent to walls.
    /// </summary>
    class Room
    {
        /// <summary>
        /// All tiles in the room, including edge tiles.
        /// </summary>
        public TileRegion allTiles { get; private set; }

        /// <summary>
        /// All floor tiles in this room that have an adjacent wall tile. 
        /// </summary>
        public TileRegion edgeTiles { get; private set; }

        Coord[] adjacentCoords; // See GetAdjacentCoords method for explanation

        public Room(TileRegion region, Map map)
        {
            adjacentCoords = new Coord[8];
            allTiles = region;
            edgeTiles = GetEdgeTiles(map);
            adjacentCoords = null;
        }

        /// <summary>
        /// Determines the floor tiles that are on the boundary between floors and walls. The floor tiles are returned
        /// in roughly 'sorted' order: iterating through the tile region should trace out a roughly continuous path 
        /// around the room.
        /// </summary>
        TileRegion GetEdgeTiles(Map map)
        {
            // We'll use both whether or not a key is in the dictionary, as well as its true/false value.
            // This is why a dictionary is used instead of a hashset.
            Dictionary<Coord, bool> visited = new Dictionary<Coord, bool>(allTiles.Count);
            for (int i = 0; i < allTiles.Count; i++)
            {
                visited[allTiles[i]] = false;
            }
            List<Coord> edgeTiles = new List<Coord>(allTiles.Count);
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
                    if (visited.ContainsKey(adjacentTile) // ensure we haven't jumped to a different room
                        && map.IsAdjacentToWallFast(adjacentTile) 
                        && !visited[adjacentTile])
                    {
                        visited[adjacentTile] = true;
                        stack.Push(adjacentTile);
                        edgeTiles.Add(adjacentTile);
                    }
                }
            }
            return new TileRegion(edgeTiles);
        }

        Coord[] GetAdjacentCoords(Coord tile)
        {
            // re-use the same array defined at the instance level to avoid a huge number of allocations
            Coord[] adjacentCoords = this.adjacentCoords;

            Coord left = tile.left;
            Coord right = tile.right;

            adjacentCoords[0] = tile.up;
            adjacentCoords[1] = tile.down;
            adjacentCoords[2] = right;
            adjacentCoords[3] = left;
            adjacentCoords[4] = left.up;
            adjacentCoords[5] = left.down;
            adjacentCoords[6] = right.up;
            adjacentCoords[7] = right.down;
            return adjacentCoords;
        }

        public override string ToString()
        {
            return string.Format("Room with {0} tiles, {1} on the edge.", allTiles.Count, edgeTiles.Count);
        }
    }
}