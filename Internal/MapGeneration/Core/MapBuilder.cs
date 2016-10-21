/* MapBuilder is a low-level class that offers a library of methods for map generation. The intention is to write
 * light-weight, higher-level map generator classes that can easily be customized by choosing which of the methods
 * in this class should be used and in what order. See the default map generator for an example.*/

using UnityEngine;
using System.Collections.Generic;

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
            map.TransformBoundary((x, y) => Tile.Wall);
            map.TransformInterior((x, y) => random.NextDouble() < mapDensity ? Tile.Wall : Tile.Floor);
        }

        /// <summary>
        /// Uses synchronous cellular automata to smooth out the map. Each cell becomes more like its neighbors,
        /// turning noise into a smoother map filled with more consistent regions.
        /// </summary>
        public void Smooth()
        {
            Map currentMap = map;
            Map tempMap = map.Clone();
            for (int i = 0; i < SMOOTHING_ITERATIONS; i++)
            {
                tempMap.TransformInterior((x, y) => GetSmoothedTile(currentMap, x, y));
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
            radius = Mathf.Min(radius, Mathf.Max(length, width)); // Reduce work done for unreasonable radius input
            Map currentMap = map;
            Map tempMap = map.Clone();
            for (int iteration = 0; iteration < radius; iteration++)
            {
                tempMap.TransformInterior((x, y) => currentMap.IsAdjacentToFloorFast(x, y) ? Tile.Floor : Tile.Wall);
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
            BFS.RemoveSmallRegions(map, Tile.Wall, threshold);
        }

        /// <summary>
        /// Remove small regions of floor tiles. Floor tiles are considered to be in the same region if they are connected 
        /// by a sequence of vertical and horizontal steps through floor tiles. 
        /// </summary>
        /// <param name="threshold">Number of tiles a region must have to not be removed.</param>
        public void RemoveSmallFloorRegions(int threshold)
        {
            if (threshold <= 0) return;
            BFS.RemoveSmallRegions(map, Tile.Floor, threshold);
        }

        /// <summary>
        /// Ensure connectivity between all regions of floors in the map. It is recommended that you first prune
        /// small floor regions in order to avoid creating tunnels to tiny regions.
        /// </summary>
        public void ConnectFloors(int tunnelRadius)
        {
            List<TileRegion> floors = BFS.GetRegions(map, Tile.Floor);
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
            borderedMap.Transform((x, y) =>
            {
                int yShifted = y - borderSize;
                int xShifted = x - borderSize;
                return map.Contains(xShifted, yShifted) ? map[xShifted, yShifted] : Tile.Wall;
            });
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

        void CreatePassage(ConnectionInfo connection, int tunnelingRadius)
        {
            tunnelingRadius = Mathf.Max(tunnelingRadius, 1);
            List<Coord> line = connection.tileA.CreateLineTo(connection.tileB);
            line.ForEach(tile => ClearNeighbors(map, tile, tunnelingRadius));

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

        void ClearNeighbors(Map map, Coord center, int radius)
        {
            ClearNeighbors(map, center.x, center.y, radius);
        }

        void Swap(ref Map a, ref Map b)
        {
            Map temp = a;
            a = b;
            b = temp;
        }
    }
}