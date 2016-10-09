/* MapBuilder is a low-level class that offers a library of methods for map generation. The intention is to write
 * light-weight, higher-level map generator classes that can easily be customized by choosing which of the methods
 * in this class should be used and in what order. See the default map generator for an example.*/

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CaveGeneration.MapGeneration
{
    /// <summary>
    /// Offers a variety of methods for configuring a randomized Map object. Start with an initialization
    /// method, and end with the build method to receive the map.
    /// </summary>
    sealed class MapBuilder
    {
        Map map;

        int length;
        int width;

        Map spareMap; // Some methods require a second temporary map.
        List<TileRegion> floorRegions; // Once computed, regions are cached until the map changes.
        List<Coord> tiles; // Reusable list of tiles with capacity equal to size of map.
        bool[,] visited; // Reusable 2d boolean array for each tile in map

        const int SMOOTHING_ITERATIONS = 5;
        const int SMOOTHING_THRESHOLD = 4;

        /// <summary>
        /// Begin building a new map by specifying its dimensions.
        /// </summary>
        public MapBuilder(int length, int width, int squareSize)
        {
            map = new Map(length, width, squareSize);
            spareMap = new Map(length, width, squareSize);
            tiles = new List<Coord>(length * width);
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
            ResetRegions();
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
            Map tempMap = GetMapCopy(map);
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
            RestoreMapReferences(currentMap, tempMap);
            ResetRegions();
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
            Map tempMap = GetMapCopy(map);
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
            RestoreMapReferences(currentMap, tempMap);
            ResetRegions();
        }

        /// <summary>
        /// Remove small regions of walls. Walls are considered to be in the same region if they are connected by a 
        /// sequence of vertical and horizontal steps through walls. 
        /// </summary>
        /// <param name="threshold">Number of tiles a region must have to not be removed.</param>
        public void RemoveSmallWallRegions(int threshold)
        {
            if (threshold <= 0) return;

            bool[,] visited = GetVisitedArray();
            VisitBoundaryRegion(visited);

            for (int x = 0; x < length; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    if (IsNewTileOfType(visited, x, y, Tile.Wall))
                    {
                        List<Coord> region = GetRegion(x, y, visited);
                        if (region.Count < threshold)
                        {
                            FillRegion(region, Tile.Floor);
                        }
                    }
                }
            }
            ResetRegions();
        }

        /// <summary>
        /// Remove small regions of floor tiles. Floor tiles are considered to be in the same region if they are connected 
        /// by a sequence of vertical and horizontal steps through floor tiles. 
        /// </summary>
        /// <param name="threshold">Number of tiles a region must have to not be removed.</param>
        public void RemoveSmallFloorRegions(int threshold)
        {
            if (threshold <= 0) return;
            floorRegions = GetFloorRegions(threshold);
        }

        /// <summary>
        /// Ensure connectivity between all regions of floors in the map. It is recommended that you first prune
        /// small floor regions in order to avoid creating tunnels to tiny regions.
        /// </summary>
        public void ConnectFloors(int tunnelRadius)
        {
            List<TileRegion> floors = floorRegions ?? GetFloorRegions();
            List<Room> rooms = FloorRegionsToRooms(floors);
            List<RoomConnection> allRoomConnections = ComputeRoomConnections(rooms);
            List<RoomConnection> finalConnections = MinimumSpanningTree.GetMinimalConnectionsDiscrete(allRoomConnections, rooms.Count);
            finalConnections.ForEach(connection => CreatePassage(connection, tunnelRadius));
            ResetRegions();
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
            for (int x = 0; x < borderedMap.Length; x++)
            {
                int xShifted = x - borderSize;
                for (int y = 0; y < borderedMap.Width; y++)
                {
                    int yShifted = y - borderSize;
                    bool isInsideBorder = (0 <= xShifted && xShifted < length) && (0 <= yShifted && yShifted < width);
                    borderedMap[x, y] = isInsideBorder ? map[xShifted, yShifted] : Tile.Wall;
                }
            }
            map = borderedMap;
            ResetRegions();
        }

        /// <summary>
        /// Build the map and return it.
        /// </summary>
        public Map ToMap()
        {
            return map;
        }

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
            bool[,] visited = GetVisitedArray();

            for (int x = 0; x < length; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    if (IsNewTileOfType(visited, x, y, Tile.Floor))
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
            ResetRegions();
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

        /// <summary>
        /// Builds rooms out of floor regions.
        /// </summary>
        List<Room> FloorRegionsToRooms(List<TileRegion> floors)
        {
            bool[,] visited = GetVisitedArray();
            return floors.Select(region => new Room(region, map, visited)).ToList();
        }

        List<Coord> GetTilesReachableFromQueue(Queue<Coord> queue, bool[,] visited)
        {
            tiles.Clear();
            if (queue.Count == 0)
            {
                return tiles;
            }
            Tile tileType = map[queue.Peek()];
            while (queue.Count > 0)
            {
                Coord currentTile = queue.Dequeue();
                tiles.Add(currentTile);

                // Packing the following into a foreach loop would be cleaner, but results in a noticeable performance hit
                int x = currentTile.x, y = currentTile.y;
                int left = x - 1, right = x + 1, up = y + 1, down = y - 1;

                if (IsNewTileOfType(visited, left, y, tileType))
                    queue.Enqueue(new Coord(left, y));
                if (IsNewTileOfType(visited, right, y, tileType))
                    queue.Enqueue(new Coord(right, y));
                if (IsNewTileOfType(visited, x, up, tileType))
                    queue.Enqueue(new Coord(x, up));
                if (IsNewTileOfType(visited, x, down, tileType))
                    queue.Enqueue(new Coord(x, down));
            }
            return tiles;
        }

        bool IsNewTileOfType(bool[,] visited, int x, int y, Tile tileType)
        {
            if (visited[x, y])
                return false;

            visited[x, y] = true;
            return map[x, y] == tileType;
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

        /// <summary>
        /// Generate a RoomConnection object between every two rooms in the map, where the connection stores information
        /// about the shortest distance between the two rooms and the tiles corresponding to this distance.
        /// </summary>
        /// <param name="rooms">All rooms in the Map.</param>
        /// <returns>A list of every connection between rooms in the map.</returns>
        List<RoomConnection> ComputeRoomConnections(List<Room> rooms)
        {
            List<RoomConnection> connections = new List<RoomConnection>();
            for (int i = 0; i < rooms.Count; i++)
            {
                for (int j = i + 1; j < rooms.Count; j++)
                {
                    RoomConnection connection = new RoomConnection(rooms[i], rooms[j], i, j);
                    connections.Add(connection);
                    connection.FindShortConnection();
                }
            }
            return connections;
        }

        void CreatePassage(RoomConnection connection, int tunnelingRadius)
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
        /// <param name="radius">The radius of replacement: e.g. if 1, will replace the 8 adjacent tiles.</param>
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

        bool[,] GetVisitedArray()
        {
            if (visited != null)
            {
                System.Array.Clear(visited, 0, visited.Length);
            }
            else
            {
                visited = new bool[length, width];
            }
            return visited;
        }

        /// <summary>
        /// Copies the passed map to the spare map and returns it. Can be used to get a map copy without allocations.
        /// </summary>
        Map GetMapCopy(Map original)
        {
            spareMap.Copy(original);
            return spareMap;
        }

        /// <summary>
        /// Ensures the instance-level map references are referring to the correct maps at the end of a method
        /// involving reference-swapping with a temporary map obtained with GetMapCopy.
        /// </summary>
        void RestoreMapReferences(Map main, Map spare)
        {
            map = main;
            spareMap = spare;
        }

        /// <summary>
        /// Used when a method renders existing region information invalid. 
        /// </summary>
        void ResetRegions()
        {
            floorRegions = null;
        }
    }
}