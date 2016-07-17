using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CaveGeneration.MapGeneration
{
    /// <summary>
    /// Generates a randomized cave-like Map object with the property that every floor tile is reachable from every other
    /// floor tile. The outermost boundary of the map consists of wall tiles.
    /// </summary>
    public class MapGenerator : IMapGenerator
    {
        int length;
        int width;
        float mapDensity;
        string seed;
        bool useRandomSeed;
        int borderSize;
        int squareSize;
        int minimumWallRegionSize;
        int MinimumFloorRegionSize;

        public Map map { get; private set; }

        const int SMOOTHING_ITERATIONS = 5;
        const int CELLULAR_THRESHOLD = 4;
        const int TUNNELING_RADIUS = 1;

        public MapGenerator(MapParameters parameters)
        {
            UpdateParameters(parameters);
        }

        public void UpdateParameters(MapParameters parameters)
        {
            parameters.Validate();

            length = parameters.length;
            width = parameters.length;
            mapDensity = parameters.mapDensity;
            seed = parameters.seed;
            useRandomSeed = parameters.useRandomSeed;
            borderSize = parameters.borderSize;
            squareSize = parameters.squareSize;
            minimumWallRegionSize = parameters.minWallSize;
            MinimumFloorRegionSize = parameters.minFloorSize;
        }

        /// <summary>
        /// Generates a randomized Map object based on the map generator's properties. May take a significant amount of time
        /// for large maps (particularly for width * length > 10e6). 
        /// </summary>
        /// <returns>Returns the generated Map object</returns>
        public Map GenerateMap()
        {
            map = new Map(length, width, squareSize);
            RandomFillMap(mapDensity);

            SmoothMap(SMOOTHING_ITERATIONS);

            List<TileRegion> floorRegions = GetRegions(Tile.Floor);
            List<TileRegion> remainingFloorRegions = RemoveSmallRegions(floorRegions, MinimumFloorRegionSize);

            List<TileRegion> wallRegions = GetRegions(Tile.Wall);
            RemoveSmallRegions(wallRegions, minimumWallRegionSize);

            ConnectRegions(remainingFloorRegions);

            ApplyBorder(borderSize);
            return map;
        }

        /// <summary>
        /// Fills the map as follows: the outer most boundary is filled with wall tiles. The rest of the map is filled with
        /// map tiles randomly based on the map density: e.g. if the map density is 0.45 then roughly 45% will be filled
        /// with map tiles (excluding boundary) and the rest with floor tiles. 
        /// </summary>
        void RandomFillMap(float mapDensity)
        {
            Random.seed = GetSeed();

            for (int x = 0; x < length; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    map[x, y] = GetRandomTile(mapDensity, x, y);
                }
            }
        }

        /// <summary>
        /// Get the seed, which determines which map gets generated. Same seed will generate the same map. Different seeds
        /// will generate unpredictably different maps.
        /// </summary>
        int GetSeed()
        {
            if (useRandomSeed)
            {
                return System.Environment.TickCount;
            }
            else
            {
                return seed.GetHashCode();
            }
        }

        Tile GetRandomTile(float mapDensity, int x, int y)
        {
            if (Random.value < mapDensity || map.IsBoundaryTile(x, y))
            {
                return Tile.Wall;
            }
            else
            {
                return Tile.Floor;
            }
        }

        /// <summary>
        /// Uses synchronous cellular automata to smooth out the map. Each cell becomes more like its neighbors,
        /// turning noise into a smoother map consisting of regions.
        /// </summary>
        /// <param name="iterations">The number of smoothing passes.</param>
        void SmoothMap(int iterations)
        {
            int interiorLength = length - 1;
            int interiorWidth = width - 1;
            Map oldMap = map;
            Map newMap = new Map(oldMap);
            for (int i = 0; i < iterations; i++)
            {
                for (int x = 1; x < interiorLength; x++)
                {
                    for (int y = 1; y < interiorWidth; y++)
                    {
                        newMap[x, y] = GetNewTileBasedOnNeighbors(x, y);
                    }
                }
                map = newMap;
                // This method requires copying the values at each step into a new map. By reusing the old one
                // instead of creating a new one each time, we save (iteration - 1) * length * width bytes worth
                // of memory from going to the garbage collector. 
                Swap(ref oldMap, ref newMap);
            }
        }

        /// <summary>
        /// Get a tile that sides with the majority of its neigbors. Specifically, count the number of walls in its
        /// 8 neighbors. If the majority are walls, get a wall. If the majority are floors, get a floor. If 
        /// it's a tie, get the existing tile at that spot.
        /// </summary>
        Tile GetNewTileBasedOnNeighbors(int x, int y)
        {
            int neighborCount = map.GetSurroundingWallCount(x, y);
            if (neighborCount > CELLULAR_THRESHOLD)
            {
                return Tile.Wall;
            }
            else if (neighborCount < CELLULAR_THRESHOLD)
            {
                return Tile.Floor;
            }
            else
            {
                return map[x, y];
            }
        }

        /// <summary>
        /// Remove small regions of the given type and return the rest.
        /// </summary>
        /// <param name="removalThreshold">Number of tiles a region must have to not be removed.</param>
        /// <param name="tileType">Tile corresponding to which type of regions should be removed.</param>
        /// <returns>List of Regions at least as large as the removal threshold.</returns>
        List<TileRegion> RemoveSmallRegions(List<TileRegion> regions, int removalThreshold)
        {
            List<TileRegion> remainingRegions = new List<TileRegion>();
            if (regions.Count == 0) // Degenerate map has nothing but walls
                return remainingRegions; 

            Tile tileType = map[regions[0][0]];
            Tile otherTileType = (tileType == Tile.Wall) ? Tile.Floor : Tile.Wall;
            foreach (TileRegion region in regions)
            {
                if (region.Count < removalThreshold)
                {
                    FillRegion(region, otherTileType);
                }
                else
                {
                    remainingRegions.Add(region);
                }
            }
            return remainingRegions;
        }

        /// <summary>
        /// Gets all the regions of the corresponding tile type, with the exception of the outer region containing the map's
        /// boundary.
        /// </summary>
        List<TileRegion> GetRegions(Tile tileType)
        {
            List<TileRegion> regions = new List<TileRegion>();
            bool[,] visited = new bool[length, width];

            if (tileType == Tile.Wall)
                VisitBoundaryRegion(visited);

            for (int x = 0; x < length; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    if (!visited[x, y] && map[x, y] == tileType)
                    {
                        TileRegion newRegion = GetRegion(x, y, visited);
                        regions.Add(newRegion);
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
                if (map[x, 1] == Tile.Wall)
                    queue.Enqueue(new Coord(x, 1));

                if (map[x, width - 2] == Tile.Wall)
                    queue.Enqueue(new Coord(x, width - 2));
            }
            for (int y = 1; y < width - 1; y++)
            {
                if (map[1, y] == Tile.Wall)
                    queue.Enqueue(new Coord(1, y));

                if (map[length - 2, y] == Tile.Wall)
                    queue.Enqueue(new Coord(length - 2, y));
            }
            return queue;
        }

        /// <summary>
        /// Get the region containing the start point. Two tiles are considered to be in the same region if there is a sequence
        /// of horizontal steps from one to the other (inclusive) passing through only the same tile type.
        /// </summary>
        /// <returns>The region of tiles containing the start point.</returns>
        TileRegion GetRegion(int xStart, int yStart, bool[,] visited)
        {
            Queue<Coord> queue = new Queue<Coord>();
            queue.Enqueue(new Coord(xStart, yStart));
            visited[xStart, yStart] = true;
            return GetTilesReachableFromQueue(queue, visited);
        }

        TileRegion GetTilesReachableFromQueue(Queue<Coord> queue, bool[,] visited)
        {
            TileRegion tiles = new TileRegion();
            Tile tileType = map[queue.Peek()];
            while (queue.Count > 0)
            {
                Coord currentTile = queue.Dequeue();
                tiles.Add(currentTile);

                // Packing the following into a foreach loop would be cleaner, but results in a noticeable performance hit
                Coord left = currentTile.left;
                Coord right = currentTile.right;
                Coord up = currentTile.up;
                Coord down = currentTile.down;

                if (IsNewTileOfType(visited, left, tileType))
                    queue.Enqueue(left);
                if (IsNewTileOfType(visited, right, tileType))
                    queue.Enqueue(right);
                if (IsNewTileOfType(visited, up, tileType))
                    queue.Enqueue(up);
                if (IsNewTileOfType(visited, down, tileType))
                    queue.Enqueue(down);
            }
            return tiles;
        }

        bool IsNewTileOfType(bool[,] visited, Coord tile, Tile tileType)
        {
            if (visited[tile.x, tile.y])
                return false;

            visited[tile.x, tile.y] = true;
            return map[tile] == tileType;
        }

        void FillRegion(TileRegion region, Tile tileType)
        {
            for (int i = 0; i < region.Count; i++)
            {
                map[region[i]] = tileType;
            }
        }

        void ConnectRegions(List<TileRegion> regions)
        {
            List<Room> rooms = regions.Select(region => new Room(region, map)).ToList();
            List<RoomConnection> allRoomConnections = ComputeRoomConnections(rooms);
            List<RoomConnection> finalConnections = MinimumSpanningTree.GetMinimalConnectionsDiscrete(allRoomConnections, rooms.Count);
            foreach (RoomConnection connection in finalConnections)
            {
                CreatePassage(connection, TUNNELING_RADIUS);
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
            List<Coord> line = connection.tileA.CreateLineTo(connection.tileB);
            foreach (Coord coord in line)
            {
                ClearNeighbors(coord, tunnelingRadius);
            }
        }

        /// <summary>
        /// Debug method for visualizing the created connections. Use in place of CreatePassage if you want to see in the editor
        /// exactly what connections are being made and where. Do not use in actual build, as Debug functionality is slow and will
        /// run during gameplay. 
        /// </summary>
        void CreatePassageDebug(RoomConnection connection, int tunnelingRadius)
        {
            Coord A = connection.tileA;
            Coord B = connection.tileB;
            Vector3 start = new Vector3(A.x, 0f, A.y);
            Vector3 end = new Vector3(B.x, 0f, B.y);
            Debug.DrawLine(start, end, Color.cyan, 10000);
            List<Coord> line = connection.tileA.CreateLineTo(connection.tileB);
            foreach (Coord coord in line)
            {
                ClearNeighbors(coord, tunnelingRadius);
            }
        }

        /// <summary>
        /// Replace nearby tiles with floors. Does not affect boundary tiles.
        /// </summary>
        /// <param name="neighborReach">The radius of replacement: e.g. if 1, will replace the 8 adjacent tiles.</param>
        void ClearNeighbors(Coord coord, int neighborReach)
        {
            for (int x = coord.x - neighborReach; x <= coord.x + neighborReach; x++)
            {
                for (int y = coord.y - neighborReach; y <= coord.y + neighborReach; y++)
                {
                    if (map.IsInteriorTile(x, y))
                    {
                        map[x, y] = Tile.Floor;
                    }
                }
            }
        }

        /// <summary>
        /// Add walls around the map of given thickness. Note that a border of thickness n will result in 2n being added to both
        /// width and length.
        /// </summary>
        /// <param name="borderSize">How thick the border should be on each side.</param>
        void ApplyBorder(int borderSize)
        {
            Map borderedMap = new Map(map.length + borderSize * 2, map.width + borderSize * 2, map.squareSize);
            for (int x = 0; x < borderedMap.length; x++)
            {
                int xShifted = x - borderSize;
                for (int y = 0; y < borderedMap.width; y++)
                {
                    int yShifted = y - borderSize;
                    bool isInsideBorder = (0 <= xShifted && xShifted < length) && (0 <= yShifted && yShifted < width);
                    borderedMap[x, y] = isInsideBorder ? map[xShifted, yShifted] : Tile.Wall;
                }
            }
            map = borderedMap;
        }

        void Swap(ref Map a, ref Map b)
        {
            Map temp = a;
            a = b;
            b = temp;
        }
    } 
}