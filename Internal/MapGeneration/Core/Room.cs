using System;
using System.Linq;
using System.Collections.Generic;

namespace CaveGeneration.MapGeneration
{
    /// <summary>
    /// A connected, open space in the map, keeping track of which of its tiles are adjacent to walls.
    /// </summary>
    sealed class Room
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

        public Room(TileRegion region, Map map, bool[,] visited)
        {
            adjacentCoords = new Coord[8];
            allTiles = region;
            edgeTiles = GetEdgeTiles(map, visited);
            adjacentCoords = null;
        }

        /// <summary>
        /// Determines the floor tiles that are on the boundary between floors and walls. The floor tiles are returned
        /// in roughly 'sorted' order: iterating through the tile region should trace out a roughly continuous path 
        /// around the room.
        /// </summary>
        TileRegion GetEdgeTiles(Map map, bool[,] visited)
        {
            List<Coord> edgeTiles = new List<Coord>(allTiles.Count);
            Stack<Coord> stack = new Stack<Coord>();

            if (allTiles.Count == 0) return new TileRegion(new List<Coord>());

            Coord firstTile = allTiles[0];
            stack.Push(firstTile);
            edgeTiles.Add(firstTile);
            visited[firstTile.x, firstTile.y] = true;

            while (stack.Count > 0)
            {
                Coord tile = stack.Pop();
                foreach (Coord adj in GetAdjacentCoords(tile)) 
                {
                    if (!visited[adj.x, adj.y] && FoundEdgeTile(tile, adj, map))
                    {
                        visited[adj.x, adj.y] = true;
                        stack.Push(adj);
                        edgeTiles.Add(adj);
                    }
                }
            }
            return new TileRegion(edgeTiles);
        }

        /// <summary>
        /// Have we found a valid edge tile for this room?
        /// </summary>
        /// <param name="source">The tile from which the new tile was discovered. Used to ensure they belong to the 
        /// same room.</param>
        /// <param name="target">The new tile.</param>
        bool FoundEdgeTile(Coord source, Coord target, Map map)
        {
            return map[target.x, target.y] == Tile.Floor 
                && map.IsAdjacentToWallFast(target) 
                && IsValidJump(source, target, map);
        }

        // An example of an invalid jump according to the following method is the following:
        // 1 0
        // 0 1
        // If we jump from one zero to the other, then it's possible we changed rooms because rooms are defined by horizontal
        // reachability only. 

        /// <summary>
        /// Is the adjacent tile in question immediately reachable? In particular, is the other tile a diagonal jump
        /// such that the shared adjacent tiles are not both walls? If they're both walls, then the destination
        /// is potentially a tile belonging to another room and must be forbidden.
        /// </summary>
        bool IsValidJump(Coord source, Coord destination, Map map)
        {
            int x = destination.x - source.x;
            int y = destination.y - source.y;
            return map[source.x + x, source.y] == Tile.Floor || map[source.x, source.y + y] == Tile.Floor;
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