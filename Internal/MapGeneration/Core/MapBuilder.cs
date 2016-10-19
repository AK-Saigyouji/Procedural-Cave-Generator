/* MapBuilder is a low-level class that offers a library of methods for map generation. The intention is to write
 * light-weight, higher-level map generator classes that can easily be customized by choosing which of the methods
 * in this class should be used and in what order. See the default map generator for an example.*/

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CaveGeneration.MapGeneration
{
    using MapConnector = Connectivity.MapConnector;
    using ConnectionInfo = Connectivity.ConnectionInfo;

    /// <summary>
    /// Offers a variety of methods for configuring a randomized Map object. Start with an initialization
    /// method, and end with the build method to receive the map.
    /// </summary>
    sealed class MapBuilder
    {
        Map map;

        int length;
        int width;

        const int SMOOTHING_ITERATIONS = 5;
        const int SMOOTHING_THRESHOLD = 4;

        /// <summary>
        /// Begin building a new map by specifying its dimensions.
        /// </summary>
        public MapBuilder(int length, int width, int squareSize)
        {
            map = new Map(length, width, squareSize);
            this.length = length;
            this.width = width;
        }

        /// <summary>
        /// Fills the map as follows: the outer most boundary is filled with wall tiles. The rest of the map is filled with
        /// wall tiles randomly based on the map density: e.g. if the map density is 0.45 then roughly 45% will be filled
        /// with wall tiles (excluding boundary) and the rest with floor tiles. 
        /// </summary>
        public void InitializeRandomFill(float mapDensity, string seed)
        {
            // Unity's Random seed cannot be set in a secondary thread, so System.Random is used instead.
            var random = new System.Random(seed.GetHashCode());
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    map[x, y] = GetRandomTile(mapDensity, x, y, random);
                }
            }
        }

        /// <summary>
        /// Uses synchronous cellular automata to smooth out the map. Each cell becomes more like its neighbors,
        /// turning noise into a smoother map filled with more consistent regions.
        /// </summary>
        public void Smooth()
        {
            int interiorLength = length - 1;
            int interiorWidth = width - 1;
            Map currentMap = map;
            Map tempMap = map.Clone();
            for (int i = 0; i < SMOOTHING_ITERATIONS; i++)
            {
                for (int y = 1; y < interiorWidth; y++)
                {
                    for (int x = 1; x < interiorLength; x++)
                    {
                        tempMap[x, y] = GetSmoothedTile(currentMap, x, y);
                    }
                }
                Swap(ref currentMap, ref tempMap);
            }
        }

        /// <summary>
        /// Expand each floor region by a number of tiles in each direction based on the provided argument. Use cautiously,
        /// as this method will dramatically reduce the proportion of walls in the map even for a small radius.
        /// </summary>
        public void ExpandRegions(int radius)
        {
            if (radius <= 0) return;
            radius = Mathf.Min(radius, Mathf.Max(length, width));
            Map currentMap = map;
            Map tempMap = map.Clone();
            for (int iteration = 0; iteration < radius; iteration++)
            {
                for (int y = 1; y < width - 1; y++)
                {
                    for (int x = 1; x < length - 1; x++)
                    {
                        if (currentMap.IsAdjacentToFloorFast(x, y))
                        {
                            tempMap[x, y] = Tile.Floor;
                        }
                    }
                }
                Swap(ref currentMap, ref tempMap);
            }
        }

        /// <summary>
        /// Remove small regions of walls. Walls are considered to be in the same region if they are connected by a 
        /// sequence of vertical and horizontal steps through walls. 
        /// </summary>
        /// <param name="threshold">Number of tiles a region must have to not be removed.</param>
        public void RemoveSmallWallRegions(int threshold)
        {
            if (threshold <= 0) return;

            bool[,] visited = map.ToBoolArray(Tile.Floor); 
            VisitBoundaryRegion(visited);

            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    if (!visited[x,y])
                    {
                        List<Coord> region = GetRegion(x, y, visited);
                        if (region.Count < threshold)
                        {
                            FillRegion(region, Tile.Floor);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Remove small regions of floor tiles. Floor tiles are considered to be in the same region if they are connected 
        /// by a sequence of vertical and horizontal steps through floor tiles. 
        /// </summary>
        /// <param name="threshold">Number of tiles a region must have to not be removed.</param>
        public void RemoveSmallFloorRegions(int threshold)
        {
            if (threshold <= 0) return;
            GetFloorRegions(threshold);
        }

        /// <summary>
        /// Ensure connectivity between all regions of floors in the map. It is recommended that you first prune
        /// small floor regions in order to avoid creating tunnels to tiny regions.
        /// </summary>
        public void ConnectFloors(int tunnelRadius)
        {
            List<TileRegion> floors = GetFloorRegions();
            ConnectionInfo[] finalConnections = MapConnector.GetConnections(map, floors);
            System.Array.ForEach(finalConnections, connection => CreatePassage(connection, tunnelRadius));
        }

        /// <summary>
        /// Add walls around the map of given thickness. Note that a border of thickness n will result in 2n being added to both
        /// width and length.
        /// </summary>
        /// <param name="borderSize">How thick the border should be on each side.</param>
        public void ApplyBorder(int borderSize)
        {
            if (borderSize <= 0) return;
            Map borderedMap = new Map(length + borderSize * 2, width + borderSize * 2, map.SquareSize);
            for (int y = 0; y < borderedMap.Width; y++)
            {
                int yShifted = y - borderSize;
                for (int x = 0; x < borderedMap.Length; x++)
                {
                    int xShifted = x - borderSize;
                    bool isInsideBorder = (0 <= xShifted && xShifted < length) && (0 <= yShifted && yShifted < width);
                    borderedMap[x, y] = isInsideBorder ? map[xShifted, yShifted] : Tile.Wall;
                }
            }
            map = borderedMap;
        }

        /// <summary>
        /// Build the map and return it.
        /// </summary>
        public Map ToMap()
        {
            return map;
        }

        /// <summary>
        /// Retrieve the majority tile type of the neighbours of the coord passed in, unless it's a draw (4 walls,
        /// 4 floors) in which case it'll return the value of the map at that point.
        /// </summary>
        Tile GetSmoothedTile(Map map, int x, int y)
        {
            int neighbourCount = map.GetSurroundingWallCount(x, y);
            if (neighbourCount > SMOOTHING_THRESHOLD)
            {
                return Tile.Wall;
            }
            else if (neighbourCount < SMOOTHING_THRESHOLD)
            {
                return Tile.Floor;
            }
            else
            {
                return map[x, y];
            }
        }

        Tile GetRandomTile(float mapDensity, int x, int y, System.Random random)
        {
            if (random.NextDouble() < mapDensity || map.IsBoundaryTile(x, y))
            {
                return Tile.Wall;
            }
            else
            {
                return Tile.Floor;
            }
        }

        /// <summary>
        /// Get all the floor regions consisting of a number of tiles greater than the specified threshold, filling the 
        /// rest in (i.e. turning them into walls).
        /// </summary>
        List<TileRegion> GetFloorRegions(int threshold = 0)
        {
            List<TileRegion> regions = new List<TileRegion>();
            bool[,] visited = map.ToBoolArray(Tile.Wall);

            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    if (!visited[x, y])
                    {
                        List<Coord> region = GetRegion(x, y, visited);
                        if (region.Count < threshold)
                        {
                            FillRegion(region, Tile.Wall);
                        }
                        else
                        {
                            regions.Add(new TileRegion(region));
                        }
                    }
                }
            }
            return regions;
        }

        /// <summary>
        /// Visits all tiles corresponding to walls connected to the outermost boundary of the map. Doing this ensures
        /// that every remaining point in the map has four neighbors, removing the need for four boundary checks every time a 
        /// new tile is visited.
        /// </summary>
        void VisitBoundaryRegion(bool[,] visited)
        {
            VisitColumns(visited, 0, 1, length - 2, length - 1);
            VisitRows(visited, 0, 1, width - 2, width - 1);
            Queue<Coord> queue = InitializeBoundaryQueue();
            GetTilesReachableFromQueue(queue, visited);
        }

        void VisitColumns(bool[,] visited, params int[] columns)
        {
            foreach (int columnNumber in columns)
            {
                for (int y = 0; y < width; y++)
                {
                    visited[columnNumber, y] = true;
                }
            }
        }

        void VisitRows(bool[,] visited, params int[] rows)
        {
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
        Queue<Coord> InitializeBoundaryQueue()
        {
            Queue<Coord> queue = new Queue<Coord>();
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
        List<Coord> GetRegion(int xStart, int yStart, bool[,] visited)
        {
            Queue<Coord> queue = new Queue<Coord>();
            queue.Enqueue(new Coord(xStart, yStart));
            visited[xStart, yStart] = true;
            return GetTilesReachableFromQueue(queue, visited);
        }

        List<Coord> GetRegionFast(int xStart, int yStart, bool[,] visited)
        {
            Queue<Coord> queue = new Queue<Coord>();
            queue.Enqueue(new Coord(xStart, yStart));
            visited[xStart, yStart] = true;
            return GetTilesReachableFromQueue(queue, visited);
        }

        // There are several assumptions built into this method that will cause problems if not met. 
        // Most substantially, it is assumed that the elements of queue all correspond to a single tile type,
        // and that the visited array has already set to true every element of the opposite type. e.g.
        // if queue has a single Coord (2,3) and map[2,3] = Tile.Wall, then for each (x,y) such that map[x,y] = Tile.Floor,
        // visited[x,y] = true. 
        List<Coord> GetTilesReachableFromQueue(Queue<Coord> queue, bool[,] visited)
        {
            // This list ends up consuming a lot of memory from resizing (~ 10 times the entire map) but maintaining a 
            // cached list and clearing it each time increased the run-time of this method by a factor of 4. 
            List<Coord> tiles = new List<Coord>();
            while (queue.Count > 0)
            {
                Coord currentTile = queue.Dequeue();
                tiles.Add(currentTile);

                // Packing the following into a foreach loop would be cleaner, but results in a noticeable performance hit
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
        /// Fill each tile in the region with the given type.
        /// </summary>
        void FillRegion(List<Coord> region, Tile tileType)
        {
            for (int i = 0; i < region.Count; i++)
            {
                map[region[i]] = tileType;
            }
        }

        void CreatePassage(ConnectionInfo connection, int tunnelingRadius)
        {
            tunnelingRadius = Mathf.Max(tunnelingRadius, 1);
            List<Coord> line = connection.tileA.CreateLineTo(connection.tileB);
            foreach (Coord tile in line)
            {
                ClearNeighbors(map, tile.x, tile.y, tunnelingRadius);
            }
        }

        /// <summary>
        /// Replace nearby tiles with floors. Does not affect boundary tiles.
        /// </summary>
        /// <param name="radius">The radius of replacement: e.g. if 1, will replace the 8 adjacent tiles. If 2,
        /// will replace those 8 and their 16 immediate neighbours, etc.</param>
        void ClearNeighbors(Map map, int xCenter, int yCenter, int radius)
        {
            // These computations ensure that only interior (non-boundary) tiles are affected.
            int xMin = Mathf.Max(1, xCenter - radius);
            int yMin = Mathf.Max(1, yCenter - radius);
            int xMax = Mathf.Min(length - 2, xCenter + radius);
            int yMax = Mathf.Min(width - 2, yCenter + radius);
            for (int x = xMin; x <= xMax; x++)
            {
                for (int y = yMin; y <= yMax; y++)
                {
                    map[x, y] = Tile.Floor;
                }
            }
        }

        void Swap(ref Map a, ref Map b)
        {
            Map temp = a;
            a = b;
            b = temp;
        }
    }
}