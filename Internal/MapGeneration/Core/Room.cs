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
            edgeTiles = ComputeOrderedEdgeTiles(map, visited);
            adjacentCoords = null;
        }

        /* The following method has some subtleties to it that need to be explained. The purpose of it is to trace out the
         * floor tiles in the room that are adjacent to a wall tile. The tricky part is ensuring that this is done
         * in an orderly fashion, i.e. that it traces out a more or less continuous path (some error is fine, but a random
         * ordering is not, as the ordering of the edge tiles is needed for an enormous optimization in the connectivity
         * algorithm used elsewhere). 
         * 
         * The basic idea is to start on an edge tile, and then perform a depth-first search along edge tiles. The difficulty
         * comes from two situations. First, consider this example:
         * 
         * x x o
         * x x o
         * 1 2 o
         * o o o
         * 
         * x represents wall, o represents floor, numbers represent the path travelled (and the order). If we only 
         * look at horizontal neighbors, the search will terminate at 2, because no adjacent floor is adjacent to a wall.
         * So we need to look at diagonal neighbors. That leads to the following problem:
         * 
         * x x x x
         * x x o x
         * x 3 x x
         * 1 2 x x
         * 
         * The remaining o is not connected to the path so far, but it's diagonally adjacent to the 3. This is handled
         * by explicitly checking for this situation: in order to take a diagonal jump, one of the 
         * two adjacent tiles must be a floor. i.e. one of these situations:
         * 
         * x x x x     x x x x      x x x x
         * x x o x     x o o x      x o o x
         * x 3 o x     x 3 x x      x 3 o x
         * 1 2 x x     1 2 x x      1 2 x x
         *
         * The final complexity is the possibility of the path jumping, leading to irregularities in the edges. Example:
         * 
         * x x o o o 8 x
         * x x 4 5 6 7 x
         * 1 2 3 x x o x
         * o o o x x o x
         * 
         * Ultimately this level of error is accepted as is. */

        /// <summary>
        /// Determines the floor tiles that are on the boundary between floors and walls. The floor tiles are returned
        /// in roughly 'sorted' order: iterating through the tile region should trace out a roughly continuous path 
        /// around the room.
        /// </summary>
        TileRegion ComputeOrderedEdgeTiles(Map map, bool[,] visited)
        {
            UnityEngine.Assertions.Assert.AreNotEqual(allTiles.Count, 0, "Room is empty!");
            List<Coord> edgeTiles = new List<Coord>(allTiles.Count);
            Stack<Coord> stack = new Stack<Coord>();
            Coord firstTile = GetEdgeTile(map);
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

        Coord GetEdgeTile(Map map)
        {
            // Note that in practice, this should return the very first item in alltiles, but that may change in the future.
            return allTiles.First(tile => map.IsAdjacentToWallFast(tile.x, tile.y));
        }

        /// <summary>
        /// Have we found a valid edge tile for this room?
        /// </summary>
        /// <param name="source">The tile from which the new tile was discovered. Used to ensure they belong to the 
        /// same room.</param>
        /// <param name="target">The new tile.</param>
        bool FoundEdgeTile(Coord source, Coord target, Map map)
        {
            int x = target.x, y = target.y;
            return map[x, y] == Tile.Floor 
                && map.IsAdjacentToWallFast(x, y) 
                && IsValidJump(source, target, map);
        }

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
            // Re-use the same array defined at the instance level to avoid allocating a new array each time
            // The entire array gets populated each call, so we don't need to clear it.
            Coord[] adjacentCoords = this.adjacentCoords;

            int x = tile.x, y = tile.y;
            int left = x - 1, right = x + 1, up = y + 1, down = y - 1;

            adjacentCoords[0] = new Coord(x, up);
            adjacentCoords[1] = new Coord(x, down);
            adjacentCoords[2] = new Coord(right, y);
            adjacentCoords[3] = new Coord(left, y);
            adjacentCoords[4] = new Coord(left, up);
            adjacentCoords[5] = new Coord(left, down);
            adjacentCoords[6] = new Coord(right, up);
            adjacentCoords[7] = new Coord(right, down);
            return adjacentCoords;
        }

        public override string ToString()
        {
            return string.Format("Room with {0} tiles, {1} on the edge.", allTiles.Count, edgeTiles.Count);
        }
    }
}
 
 
 
 
 
 
 
 