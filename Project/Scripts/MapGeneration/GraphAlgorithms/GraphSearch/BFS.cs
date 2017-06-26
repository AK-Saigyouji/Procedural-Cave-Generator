/* This class supports functionality used to find and remove regions in the map.*/

using AKSaigyouji.Maps;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace AKSaigyouji.GraphAlgorithms
{
    static class BFS
    {
        /// <summary>
        /// Get a list of all the connected regions in the map of the given type. Two tiles are part of the same
        /// region if one can be reached from the other by a sequence of horizontal and vertical steps through
        /// the same tile type.
        /// </summary>
        public static List<TileRegion> GetConnectedRegions(Map map, Tile tileType)
        {
            Assert.IsNotNull(map);
            var regions = new List<TileRegion>();
            bool[,] visited = map.ToBoolArray(GetOpposite(tileType));
            int regionsFound = 0;
            map.ForEach((x, y) =>
            {
                if (!visited[x, y])
                {
                    List<Coord> region = GetConnectedRegion(x, y, visited);
                    regions.Add(new TileRegion(region, regionsFound));
                    regionsFound++;
                }
            });
            return regions;
        }

        /// <summary>
        /// Remove regions of the given type in the map whose tile count falls below the indicated threshold.
        /// e.g. if threshold = 50 and tileType = Tile.Wall, every region of walls with fewer than 50 tiles will be 
        /// replaced with floor tiles. A region is defined as a collection of tiles of a given type such that every
        /// tile is reachable from every other by a series of horizontal and vertical steps.
        /// </summary>
        public static void RemoveSmallRegions(Map map, Tile tileType, int threshold)
        {
            Assert.IsNotNull(map);
            Tile oppositeTileType = GetOpposite(tileType);
            bool[,] visited = map.ToBoolArray(oppositeTileType);
            map.ForEach((x, y) =>
            {
                if (!visited[x, y])
                {
                    List<Coord> region = GetConnectedRegion(x, y, visited);
                    if (region.Count < threshold)
                    {
                        FillRegion(map, region, oppositeTileType);
                    }
                }
            });
        }

        // By using an appropriately initialized visited array, this BFS does not need a reference to the map at all:
        // All the tiles of the wrong type have already been marked as visited, so by discovering all unvisited 
        // regions, we're getting exactly the regions of the desired type.

        /// <summary>
        /// Get the connected region of unvisited tiles starting at this point.
        /// </summary>
        public static List<Coord> GetConnectedRegion(int xStart, int yStart, bool[,] visited)
        {
            Assert.IsNotNull(visited);
            int xMax = visited.GetLength(0);
            int yMax = visited.GetLength(1);
            Assert.IsTrue(0 <= xStart && xStart < xMax && 0 <= yStart && yStart < yMax);
            var tiles = new List<Coord>();

            var queue = new Queue<Coord>();
            queue.Enqueue(new Coord(xStart, yStart));
            visited[xStart, yStart] = true;
            while (queue.Count > 0)
            {
                Coord currentTile = queue.Dequeue();
                tiles.Add(currentTile);

                // Unpacking the foreach loop for the following four checks offers a dramatic speedup, and this
                // is a performance hot spot in the generator as a whole.
                int x = currentTile.x, y = currentTile.y;
                int left = x - 1, right = x + 1, up = y + 1, down = y - 1;

                if (left >= 0 && !visited[left, y])
                {
                    queue.Enqueue(new Coord(left, y));
                    visited[left, y] = true;
                }
                if (right < xMax && !visited[right, y])
                {
                    queue.Enqueue(new Coord(right, y));
                    visited[right, y] = true;
                }
                if (up < yMax && !visited[x, up])
                {
                    queue.Enqueue(new Coord(x, up));
                    visited[x, up] = true;
                }
                if (down >= 0 && !visited[x, down])
                {
                    queue.Enqueue(new Coord(x, down));
                    visited[x, down] = true;
                }
            }
            return tiles;
        }

        /// <summary>
        /// Fill each tile in the region with tiles of the specified type.
        /// </summary>
        static void FillRegion(Map map, List<Coord> region, Tile tileType)
        {
            for (int i = 0; i < region.Count; i++)
            {
                map[region[i]] = tileType;
            }
        }

        static Tile GetOpposite(Tile tile)
        {
            return tile == Tile.Wall ? Tile.Floor : Tile.Wall;
        }
    }
}