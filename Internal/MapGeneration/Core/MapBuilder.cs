/* MapBuilder is a low-level class that offers a library of extension methods for map generation. 
The intention is to write light-weight, higher-level map generator classes that can easily be customized 
by choosing which of the methods in this class should be used and in what order. See the default map generator 
for an example. 
 
  Each public method is an extension method for the Map class, and returns the resulting Map object, allowing
the methods to be chained together. These methods are not necessarily side-effect free, however - in many cases
the original Map object is returned.*/

using UnityEngine;
using System.Collections.Generic;
using CaveGeneration.MapGeneration.Connectivity;
using System;

namespace CaveGeneration.MapGeneration
{
    /// <summary>
    /// Offers a variety of extension methods for generating Map objects. 
    /// </summary>
    static class MapBuilder
    {
        const int SMOOTHING_ITERATIONS = 5;
        const int SMOOTHING_THRESHOLD = 4;

        /// <summary>
        /// Fills the map as follows: the outer most boundary is filled with wall tiles. The rest of the map is filled with
        /// wall tiles randomly based on the map density: e.g. if the map density is 0.45 then roughly 45% will be filled
        /// with wall tiles (excluding boundary) and the rest with floor tiles. 
        /// </summary>
        public static Map InitializeRandomMap(int length, int width, float mapDensity, int seed)
        {
            Map map = new Map(length, width);
            // Unity's Random seed cannot be set in a secondary thread, so System.Random is used instead.
            var random = new System.Random(seed);
            map.TransformBoundary((x, y) => Tile.Wall);
            map.TransformInterior((x, y) => random.NextDouble() < mapDensity ? Tile.Wall : Tile.Floor);
            return map;
        }

        /// <summary>
        /// Uses synchronous cellular automata to smooth out the map. Each cell becomes more like its neighbors,
        /// turning noise into regions and smoothing out sharp edges.
        /// </summary>
        /// <param name="iterations">The number of smoothing passes to perform. The default is sufficient to
        /// turn completely random noise into smooth caverns. Can set to a lower number if map is already 
        /// well-structured but a bit jagged. Setting higher not recommended.</param>
        public static Map Smooth(this Map inputMap, int iterations = SMOOTHING_ITERATIONS)
        {
            Map map = inputMap.Clone();

            Map currentMap = map;
            Map tempMap = map.Clone();
            for (int i = 0; i < iterations; i++)
            {
                tempMap.TransformInterior((x, y) => GetSmoothedTile(currentMap, x, y));
                Swap(ref currentMap, ref tempMap);
            }
            map = currentMap;
            return map;
        }

        /// <summary>
        /// Expand each floor region by a number of tiles in each direction based on the provided argument. Use cautiously,
        /// as this method will dramatically reduce the proportion of walls in the map even for a small radius.
        /// </summary>
        public static Map ExpandRegions(this Map inputMap, int radius)
        {
            if (radius < 0) throw new ArgumentException("Cannot expand regions by a negative number.", "radius");
            if (radius == 0) return inputMap.Clone();

            Map map = inputMap.Clone();
            radius = Mathf.Min(radius, Mathf.Max(inputMap.Length, inputMap.Width));
            inputMap.ForEachInterior((x, y) => 
            {
                if (inputMap.IsFloor(x, y)) ClearNeighbours(map, x, y, radius);
            });
            return map;
        }

        /// <summary>
        /// Remove small regions of walls. Walls are considered to be in the same region if they are connected by a 
        /// sequence of vertical and horizontal steps through walls. 
        /// </summary>
        /// <param name="threshold">Number of tiles a region must have to not be removed.</param>
        public static Map RemoveSmallWallRegions(this Map inputMap, int threshold)
        {
            return RemoveSmallRegions(inputMap, threshold, Tile.Wall);
        }

        /// <summary>
        /// Remove small regions of floor tiles. Floor tiles are considered to be in the same region if they are connected 
        /// by a sequence of vertical and horizontal steps through floor tiles. 
        /// </summary>
        /// <param name="threshold">Number of tiles a region must have to not be removed.</param>
        public static Map RemoveSmallFloorRegions(this Map inputMap, int threshold)
        {
            return RemoveSmallRegions(inputMap, threshold, Tile.Floor);
        }

        /// <summary>
        /// Ensure connectivity between all regions of floors in the map. It is recommended that you first prune
        /// small floor regions in order to avoid creating tunnels to tiny regions.
        /// </summary>
        public static Map ConnectFloors(this Map inputMap, int tunnelRadius)
        {
            if (tunnelRadius < 0) throw new ArgumentException("Cannot tunnel a negative radius", "tunnelRadius");
            if (tunnelRadius == 0) return inputMap.Clone();

            Map map = inputMap.Clone();
            List<TileRegion> floors = BFS.GetRegions(map, Tile.Floor);
            ConnectionInfo[] finalConnections = MapConnector.GetConnections(map, floors);
            Array.ForEach(finalConnections, connection => CreatePassage(map, connection, tunnelRadius));
            return map;
        }

        /// <summary>
        /// Add walls around the map of given thickness. Note that a border of thickness n will result in 2n being added to both
        /// width and length.
        /// </summary>
        /// <param name="borderSize">How thick the border should be on each side.</param>
        public static Map ApplyBorder(this Map inputMap, int borderSize)
        {
            if (borderSize < 0) throw new ArgumentException("Cannot add a border of negative size.", "borderSize");
            if (borderSize == 0) return inputMap.Clone();
            Map borderedMap = new Map(inputMap.Length + borderSize * 2, inputMap.Width + borderSize * 2);
            borderedMap.Transform((x, y) =>
            {
                int yShifted = y - borderSize;
                int xShifted = x - borderSize;
                return inputMap.Contains(xShifted, yShifted) ? inputMap[xShifted, yShifted] : Tile.Wall;
            });
            return borderedMap;
        }

        static Map RemoveSmallRegions(Map inputMap, int threshold, Tile tileType)
        {
            if (threshold < 0) throw new ArgumentException("Removal threshold cannot be negative.", "threshold");
            if (threshold == 0) return inputMap.Clone();

            Map map = inputMap.Clone();
            BFS.RemoveSmallRegions(map, tileType, threshold);
            return map;
        }

        /// <summary>
        /// Retrieve the majority tile type of the neighbours of the coord passed in, unless it's a draw (4 walls,
        /// 4 floors) in which case it'll return the value of the map at that point.
        /// </summary>
        static Tile GetSmoothedTile(Map map, int x, int y)
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

        static void CreatePassage(Map map, ConnectionInfo connection, int tunnelingRadius)
        {
            tunnelingRadius = Mathf.Max(tunnelingRadius, 1);
            foreach (Coord tile in GetPath(connection))
            {
                ClearNeighbours(map, tile, tunnelingRadius);
            }
        }

        /// <summary>
        /// Replace nearby tiles with floors. Does not affect boundary tiles.
        /// </summary>
        static void ClearNeighbours(Map map, Coord center, int radius)
        {
            // These computations ensure that only interior (non-boundary) tiles are affected.
            int xMin = Mathf.Max(1, center.x - radius);
            int yMin = Mathf.Max(1, center.y - radius);
            int xMax = Mathf.Min(map.Length - 2, center.x + radius);
            int yMax = Mathf.Min(map.Width - 2, center.y + radius);
            // Look at each x,y in a square surrounding the center, but only remove those that fall within
            // the circle of given radius. 
            int squaredRadius = radius * radius;
            for (int x = xMin; x <= xMax; x++)
            {
                for (int y = yMin; y <= yMax; y++)
                {
                    Coord testCoord = new Coord(x, y);
                    if (IsInCircle(testCoord, center, squaredRadius)) map[x, y] = Tile.Floor;
                }
            }
        }

        static void ClearNeighbours(Map map, int xCenter, int yCenter, int radius)
        {
            ClearNeighbours(map, new Coord(xCenter, yCenter), radius);
        }

        static bool IsInCircle(Coord testCoord, Coord center, int squaredRadius)
        {
            return center.SquaredDistance(testCoord) <= squaredRadius;
        }

        static List<Coord> GetPath(ConnectionInfo connection)
        {
            return connection.tileA.GetLineTo(connection.tileB);
        }

        static void Swap(ref Map a, ref Map b)
        {
            Map temp = a;
            a = b;
            b = temp;
        }
    }
}