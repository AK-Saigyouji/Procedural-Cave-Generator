/* This class takes a region of tiles and outputs its edge tiles in more or less sorted order. Sorted in this case
 * just means they carve out a more or less continuous path. 
 * 
 * It was decided to make this class dependent upon a given map (as opposed to being a static class) so that a single
 * visited array could be re-used for every region in the map. The alternatives would be to either create a new giant
 * 2d bool array hundreds or even thousands of times, or to use hash tables which are much slower and have a 
 * much larger memory footprint. */

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CaveGeneration.MapGeneration.Connectivity
{
    sealed class EdgeExtractor
    {
        Coord[] adjacentTiles;
        Map map;
        bool[,] visited;

        public EdgeExtractor(Map map)
        {
            this.map = map;
            adjacentTiles = new Coord[8];
            visited = new bool[map.Length, map.Width];
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
         * The final complexity is the possibility of the path jumping, leading to irregularities in the edges.
         * Example:
         * 
         * x x o o o 8 x
         * x x 4 5 6 7 x
         * 1 2 3 x x o x
         * o o o x x o x
         * 
         * Ultimately this level of error is accepted as is. */

        public TileRegion Extract(TileRegion region)
        {
            UnityEngine.Assertions.Assert.AreNotEqual(region.Count, 0, "Room is empty!");
            List<Coord> edgeTiles = new List<Coord>(region.Count);
            Stack<Coord> stack = new Stack<Coord>();
            Coord firstTile = GetStartingEdgeTile(map, region);
            stack.Push(firstTile);
            edgeTiles.Add(firstTile);
            visited[firstTile.x, firstTile.y] = true;

            while (stack.Count > 0)
            {
                Coord tile = stack.Pop();
                foreach (Coord adj in GetAdjacentCoords(tile, adjacentTiles))
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

        static Coord GetStartingEdgeTile(Map map, TileRegion region)
        {
            // Note that in practice, this should return the very first item in alltiles, but that may change in the future.
            return region.First(tile => map.IsAdjacentToWallFast(tile.x, tile.y));
        }

        /// <summary>
        /// Have we found a valid edge tile for this room?
        /// </summary>
        /// <param name="source">The tile from which the new tile was discovered. Used to ensure they belong to the 
        /// same room.</param>
        /// <param name="target">The new tile.</param>
        static bool FoundEdgeTile(Coord source, Coord target, Map map)
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
        static bool IsValidJump(Coord source, Coord destination, Map map)
        {
            int x = destination.x - source.x;
            int y = destination.y - source.y;
            return map.IsFloor(source.x + x, source.y) || map.IsFloor(source.x, source.y + y);
        }

        // Coords are being put into outputCoords rather than assigning a new array each time. This is because 
        // we're always getting exactly 8 coords and calling this method many times, so we don't need 
        // to create a new array every time.
        static Coord[] GetAdjacentCoords(Coord tile, Coord[] outputCoords)
        {
            int x = tile.x, y = tile.y;
            int left = x - 1, right = x + 1, up = y + 1, down = y - 1;

            outputCoords[0] = new Coord(x, up);
            outputCoords[1] = new Coord(x, down);
            outputCoords[2] = new Coord(right, y);
            outputCoords[3] = new Coord(left, y);
            outputCoords[4] = new Coord(left, up);
            outputCoords[5] = new Coord(left, down);
            outputCoords[6] = new Coord(right, up);
            outputCoords[7] = new Coord(right, down);
            return outputCoords;
        }
    } 
}
