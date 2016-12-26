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
            if (tileType == Tile.Wall) VisitBoundaryRegion(visited, map);
            return visited;
        }

        /// <summary>
        /// Visits all tiles corresponding to walls connected to the outermost boundary of the map. Doing this ensures
        /// that every remaining point in the map has four neighbors, removing the need for four boundary checks every time a 
        /// new tile is visited.
        /// </summary>
        static void VisitBoundaryRegion(bool[,] visited, Map map)
        {
            Assert.IsTrue(map.Length > 1 && map.Width > 1, "Map is too small.");
            VisitColumns(visited, 0, 1, map.Length - 2, map.Length - 1);
            VisitRows(visited, 0, 1, map.Width - 2, map.Width - 1);
            Queue<Coord> queue = InitializeBoundaryQueue(map);
            GetTilesReachableFromQueue(queue, visited);
        }

        static void VisitColumns(bool[,] visited, params int[] columns)
        {
            int width = visited.GetLength(1);
            foreach (int columnNumber in columns)
            {
                for (int y = 0; y < width; y++)
                {
                    visited[columnNumber, y] = true;
                }
            }
        }

        static void VisitRows(bool[,] visited, params int[] rows)
        {
            int length = visited.GetLength(0);
            foreach (int rowNumber in rows)
            {
                for (int x = 0; x < length; x++)
                {
                    visited[x, rowNumber] = true;
                }
            }
        }

        /// <summary>
        /// Prepares a queue of coordinates corresponding to all the wall tiles exactly 1 unit away from the boundary. A BFS
        /// from this queue will find all the wall tiles connected to the boundary. 
        /// </summary>
        static Queue<Coord> InitializeBoundaryQueue(Map map)
        {
            int length = map.Length, width = map.Width;
            var queue = new Queue<Coord>();
            for (int x = 1; x < length - 1; x++)
            {
                if (map.IsWall(x, 1)) // left
                    queue.Enqueue(new Coord(x, 1));

                if (map.IsWall(x, width - 2)) // right
                    queue.Enqueue(new Coord(x, width - 2));
            }
            for (int y = 1; y < width - 1; y++)
            {
                if (map.IsWall(1, y)) // bottom
                    queue.Enqueue(new Coord(1, y));

                if (map.IsWall(length - 2, y)) // top
                    queue.Enqueue(new Coord(length - 2, y));
            }
            return queue;
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

        // There are several assumptions built into this method that will cause problems if not met. 
        // Most substantially, it is assumed that the elements of queue all correspond to a single tile type,
        // and that the visited array has already set to true every element of the opposite type. e.g.
        // if queue has a single Coord (2,3) and map[2,3] = Tile.Wall, then for each (x,y) such that map[x,y] = Tile.Floor,
        // visited[x,y] = true. Checking these explicitly with assertions would degrade performance significantly,
        // as this is a computationally intense routine.
        static List<Coord> GetTilesReachableFromQueue(Queue<Coord> queue, bool[,] visited)
        {
            // This list ends up consuming a lot of memory from resizing but maintaining a 
            // cached list and clearing it each time increased the run-time of this method by a factor of 4. 
            var tiles = new List<Coord>();
            while (queue.Count > 0)
            {
                Coord currentTile = queue.Dequeue();
                tiles.Add(currentTile);

                // Packing the following into a foreach loop would be cleaner, but results in a big performance hit
                int x = currentTile.x, y = currentTile.y;
                int left = x - 1, right = x + 1, up = y + 1, down = y - 1;

                if (!visited[left, y])
                {
                    queue.Enqueue(new Coord(left, y));
                    visited[left, y] = true;
                }
                if (!visited[right, y])
                {
                    queue.Enqueue(new Coord(right, y));
                    visited[right, y] = true;
                }
                if (!visited[x, up])
                {
                    queue.Enqueue(new Coord(x, up));
                    visited[x, up] = true;
                }
                if (!visited[x, down])
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