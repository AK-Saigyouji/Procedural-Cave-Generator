/* MapBuilder offers a library of methods for map generation. The intention is to write light-weight, higher-level 
 map generator modules that can mix and match various methods from this class.*/

using AKSaigyouji.Maps;
using AKSaigyouji.GraphAlgorithms;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AKSaigyouji.MapGeneration
{
    /// <summary>
    /// Offers a variety of methods for generating Map objects.
    /// </summary>
    public static class MapBuilder
    {
        const int SMOOTHING_ITERATIONS = 5;
        const int SMOOTHING_THRESHOLD = 4;
        const int MAX_SMOOTHING_ITERATIONS = 10;
        const int TUNNELING_SWITCH_THRESHOLD = 5;

        /// <summary>
        /// The map is filled with wall tiles randomly based on the map density: e.g. if the map density is 0.45 
        /// then roughly 45% will be filled with wall tiles and the rest with floor tiles. 
        /// </summary>
        public static Map InitializeRandomMap(int length, int width, float mapDensity, int seed)
        {
            Map map = new Map(length, width);
            var random = new System.Random(seed);
            map.Transform((x, y) => random.NextDouble() < mapDensity ? Tile.Wall : Tile.Floor);
            return map;
        }

        /// <summary>
        /// Smooth out the grid by making each point more like its neighbours. i.e. floors surrounded
        /// by walls become walls, and walls surrounded by floors become floors. Caution: may affect connectivity.
        /// </summary>
        /// <param name="iterations">The number of smoothing passes to perform. The default is sufficient to
        /// turn completely random noise into smooth caverns. Can set to a lower number if map is already 
        /// well-structured but a bit jagged. Higher than 10 will be clamped to 10.</param>
        public static void Smooth(Map inputMap, int iterations = SMOOTHING_ITERATIONS)
        {
            if (inputMap == null)
                throw new ArgumentNullException("inputMap");

            iterations = Mathf.Min(MAX_SMOOTHING_ITERATIONS, iterations);
            Map currentMap = inputMap.Clone();
            Map smoothedMap = inputMap.Clone();
            for (int i = 0; i < iterations; i++)
            {
                smoothedMap.TransformBoundary((x, y) => GetSmoothedBoundaryTile(currentMap, x, y));
                smoothedMap.TransformInterior((x, y) => GetSmoothedTile(currentMap, x, y));
                Swap(ref currentMap, ref smoothedMap);
            }
            inputMap.Copy(currentMap);
        }

        /// <summary>
        /// Similar to Smooth, but only affects walls. Useful for smoothing out rough edges without affecting
        /// connectivity. 
        /// </summary>
        /// <param name="iterations">The number of smoothing passes to perform. Higher than 10 will be clamped to 10.</param>
        public static void SmoothOnlyWalls(Map inputMap, int iterations = 1)
        {
            if (inputMap == null)
                throw new ArgumentNullException("inputMap");

            iterations = Mathf.Min(MAX_SMOOTHING_ITERATIONS, iterations);
            Map currentMap = inputMap.Clone();
            Map smoothedMap = inputMap.Clone();
            for (int i = 0; i < iterations; i++)
            {
                smoothedMap.TransformBoundary((x, y) => GetSmoothedBoundaryTile(currentMap, x, y), currentMap.IsWall);
                smoothedMap.TransformInterior((x, y) => GetSmoothedTile(currentMap, x, y), currentMap.IsWall);
                Swap(ref currentMap, ref smoothedMap);
            }
            inputMap.Copy(currentMap);
        }

        /// <summary>
        /// Remove small regions of walls. Walls are considered to be in the same region if they are connected by a 
        /// sequence of vertical and horizontal steps through walls. 
        /// </summary>
        /// <param name="threshold">Number of tiles a region must have to not be removed.</param>
        public static void RemoveSmallWallRegions(Map inputMap, int threshold)
        {
            RemoveSmallRegions(inputMap, threshold, Tile.Wall);
        }

        /// <summary>
        /// Remove small regions of floor tiles. Floor tiles are considered to be in the same region if they are connected 
        /// by a sequence of vertical and horizontal steps through floor tiles. 
        /// </summary>
        /// <param name="threshold">Number of tiles a region must have to not be removed.</param>
        public static void RemoveSmallFloorRegions(Map inputMap, int threshold)
        {
            RemoveSmallRegions(inputMap, threshold, Tile.Floor);
        }

        /// <summary>
        /// Ensure connectivity between all regions of floors in the map by carving paths between them. 
        /// Run time is polynomial in the number of floor regions: if generating a large map with many small rooms 
        /// and this method is taking too long, consider removing rooms below a certain threshold in size.
        /// </summary>
        /// <param name="tunnelRadius">How wide the carved paths should be. Must be nonnegative. If 0, method does nothing.</param>
        /// <param name="seed">Fixes the randomness used in carving out the paths.</param>
        public static void ConnectFloors(Map inputMap, int seed = 0, int tunnelRadius = 1)
        {
            if (inputMap == null)
                throw new ArgumentNullException("inputMap");

            if (tunnelRadius == 0)
                return;

            if (tunnelRadius < 0)
                throw new ArgumentOutOfRangeException("tunnelRadius");

            Boundary boundary = new Boundary(inputMap.Length, inputMap.Width);
            ITunneler tunneler = MapTunnelers.GetRandomDirectedTunneler(boundary, seed, MapTunnelers.Variance.Low);

            ConnectFloors(inputMap, tunneler, tunnelRadius);
        }

        /// <summary>
        /// Ensure connectivity between all regions of floors in the map, using a custom tunneling algorithm. 
        /// </summary>
        /// <param name="mapTunneler">Determines the strategy used in carving out paths.</param>
        public static void ConnectFloors(Map inputMap, ITunneler mapTunneler, int tunnelRadius = 1)
        {
            if (inputMap == null)
                throw new ArgumentNullException("inputMap");

            if (mapTunneler == null)
                throw new ArgumentNullException("mapTunneler");

            Map map = inputMap.Clone();
            List<TileRegion> floors = BFS.GetConnectedRegions(map, Tile.Floor);
            ConnectionInfo[] finalConnections = MapConnector.GetConnections(map, floors);
            foreach (var connection in finalConnections)
            {
                Coord start = connection.tileA;
                Coord end = connection.tileB;
                foreach (Coord tile in mapTunneler.GetPath(start, end))
                {
                    ClearNeighbours(map, tile, tunnelRadius);
                }
            }
            inputMap.Copy(map);
        }

        /// <summary>
        /// Enlarges tunnels so that they can be traversed by a slightly larger object. In particular, enlarges passages
        /// that are of width 1, to be of width at least 2. 
        /// </summary>
        public static void WidenTunnels(Map inputMap)
        {
            if (inputMap == null)
                throw new ArgumentNullException("inputMap");

            var floorEnlarger = new PassageEnlarger(inputMap.Length, inputMap.Width);
            floorEnlarger.ExpandFloors(inputMap);
        }

        /// <summary>
        /// Add walls around the map of given thickness. Note that a border of thickness n will result in 2n being added to both
        /// width and length of the resulting map.
        /// </summary>
        /// <param name="borderSize">How thick the border should be on each side.</param>
        public static Map ApplyBorder(Map inputMap, int borderSize)
        {
            if (borderSize < 0)
                throw new ArgumentException("Cannot add a border of negative size.", "borderSize");

            if (borderSize == 0)
                return inputMap.Clone();

            var boundary = new Boundary(inputMap.Length, inputMap.Width);
            Map borderedMap = new Map(inputMap.Length + borderSize * 2, inputMap.Width + borderSize * 2);
            borderedMap.Transform((x, y) =>
            {
                int yShifted = y - borderSize;
                int xShifted = x - borderSize;
                return boundary.IsInBounds(xShifted, yShifted) ? inputMap[xShifted, yShifted] : Tile.Wall;
            });
            return borderedMap;
        }

        /// <summary>
        /// Is every floor tile on the map reachable from every other floor tile on the map? Note that diagonals don't 
        /// count, so that there must be a path between floor tiles consisting of horizontal and vertical steps.
        /// </summary>
        public static bool IsConnected(Map map)
        {
            if (map == null)
                throw new ArgumentNullException("map");

            return BFS.GetConnectedRegions(map, Tile.Floor).Count < 2;
        }

        static void RemoveSmallRegions(Map inputMap, int threshold, Tile tileType)
        {
            if (inputMap == null)
                throw new ArgumentNullException("inputMap");

            if (threshold < 0)
                throw new ArgumentException("Removal threshold cannot be negative.", "threshold");

            if (threshold == 0)
                return;

            BFS.RemoveSmallRegions(inputMap, tileType, threshold);
        }

        /// <summary>
        /// Retrieve the majority tile type of the neighbours of the coord passed in, unless it's a draw (4 walls,
        /// 4 floors) in which case it'll return the value of the map at that point.
        /// </summary>
        static Tile GetSmoothedTile(Map map, int x, int y)
        {
            return map.GetSurroundingWallCount(x, y) > SMOOTHING_THRESHOLD ? Tile.Wall : Tile.Floor;
        }

        static Tile GetSmoothedBoundaryTile(Map map, int centerX, int centerY)
        {
            int left  = Mathf.Max(centerX - 1, 0);
            int bot   = Mathf.Max(centerY - 1, 0);
            int right = Mathf.Min(centerX + 1, map.Length - 1);
            int top   = Mathf.Min(centerY + 1, map.Width - 1);
            int numWalls = 0;
            int numTiles = 0;
            for (int y = bot; y <= top; y++)
            {
                for (int x = left; x <= right; x++)
                {
                    numWalls += (int)map[x, y];
                    numTiles++;
                }
            }
            return 2 * numWalls >= numTiles ? Tile.Wall : Tile.Floor;
        }

        /// <summary>
        /// Replace nearby tiles with floors.
        /// </summary>
        static void ClearNeighbours(Map map, Coord center, int radius)
        {
            Boundary boundary = new Boundary(1, map.Length - 2, 1, map.Width - 2);
            int xMin = center.x - radius;
            int yMin = center.y - radius;
            int xMax = center.x + radius;
            int yMax = center.y + radius;
            // Look at each x,y in a square surrounding the center, but only remove those that fall within
            // the circle of given radius. 
            int squaredRadius = radius * radius;
            for (int y = yMin; y <= yMax; y++)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    if (boundary.IsInBounds(x, y) && IsInCircle(new Coord(x, y), center, squaredRadius))
                    {
                        map[x, y] = Tile.Floor;
                    }
                }
            }
        }

        static bool IsInCircle(Coord testCoord, Coord center, int squaredRadius)
        {
            return center.SquaredDistance(testCoord) <= squaredRadius;
        }

        static void Swap(ref Map a, ref Map b)
        {
            Map temp = a;
            a = b;
            b = temp;
        }
    }
}