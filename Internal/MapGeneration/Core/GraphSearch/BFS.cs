/* This class supports functionality used to find and remove regions in the map.*/

using System.Collections.Generic;
using UnityEngine.Assertions;

namespace CaveGeneration.MapGeneration
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
            bool[,] visited = InitializeVisitedArray(map, tileType);
            map.ForEach((x, y) =>
            {
                if (!visited[x, y])
                {
                    List<Coord> region = GetRegion(x, y, visited);
                    regions.Add(new TileRegion(region));
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
            bool[,] visited = InitializeVisitedArray(map, tileType);
            map.ForEach((x, y) =>
            {
                if (!visited[x, y])
                {
                    List<Coord> region = GetRegion(x, y, visited);
                    if (region.Count < threshold)
                    {
                        FillRegion(map, region, GetOpposite(tileType));
                    }
                }
            });
        }

        static bool[,] InitializeVisitedArray(Map map, Tile tileType)
        {
            bool[,] visited = map.ToBoolArray(GetOpposite(tileType));
            return visited;
        }

        /// <summary>
        /// Get the region containing the start point. Two tiles are considered to be in the same region if there is a sequence
        /// of horizontal steps from one to the other (inclusive) passing through only the same tile type.
        /// </summary>
        /// <returns>The region of tiles containing the start point.</returns>
        static List<Coord> GetRegion(int xStart, int yStart, bool[,] visited)
        {
            var queue = new Queue<Coord>();
            queue.Enqueue(new Coord(xStart, yStart));
            visited[xStart, yStart] = true;
            return GetTilesReachableFromQueue(queue, visited);
        }

        static List<Coord> GetTilesReachableFromQueue(Queue<Coord> queue, bool[,] visited)
        {
            int xMax = visited.GetLength(0);
            int yMax = visited.GetLength(1);
            var tiles = new List<Coord>();
            while (queue.Count > 0)
            {
                Coord currentTile = queue.Dequeue();
                tiles.Add(currentTile);

                // Unpacking the foreach loop for the following four checks offers a dramatic speedup.
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
        /// Fill each tile in the region with tiles of the opposite type.
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