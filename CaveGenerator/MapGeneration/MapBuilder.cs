/* MapBuilder is a low-level class that offers a library of methods for map generation. The intention is to write
 * light-weight, higher-level map generator classes that can easily be customized by choosing which of the methods
 * in this class should be used and in what order.*/

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CaveGeneration.MapGeneration
{
    /// <summary>
    /// Offers a variety of methods for configuring and generating a randomized Map object. Start with an initialization
    /// method, and end with the build method to receive the map.
    /// </summary>
    public class MapBuilder
    {
        Map map;

        // Once computed, regions are cached until they're invalidated (nullified).
        List<TileRegion> floorRegions;
        List<TileRegion> wallRegions;

        // Reusable list of tiles with max capacity. Used to minimize allocations.
        List<Coord> tiles;

        const int SMOOTHING_ITERATIONS = 5;
        const int CELLULAR_THRESHOLD = 4;

        /// <summary>
        /// Begin building a new map by specifying its dimensions.
        /// </summary>
        public MapBuilder(int length, int width, int squareSize)
        {
            map = new Map(length, width, squareSize);
            tiles = new List<Coord>(length * width);
        }

        /// <summary>
        /// Fills the map as follows: the outer most boundary is filled with wall tiles. The rest of the map is filled with
        /// map tiles randomly based on the map density: e.g. if the map density is 0.45 then roughly 45% will be filled
        /// with map tiles (excluding boundary) and the rest with floor tiles. 
        /// </summary>
        public void InitializeRandomFill(float mapDensity, int seed)
        {
            System.Random random = new System.Random(seed);
            for (int x = 0; x < map.length; x++)
            {
                for (int y = 0; y < map.width; y++)
                {
                    map[x, y] = GetRandomTile(mapDensity, x, y, random);
                }
            }
            ResetRegions();
        }

        /// <summary>
        /// Uses synchronous cellular automata to smooth out the map. Each cell becomes more like its neighbors,
        /// turning noise into a smoother map consisting of regions.
        /// </summary>
        public void Smooth()
        {
            int interiorLength = map.length - 1;
            int interiorWidth = map.width - 1;
            Map oldMap = map;
            Map newMap = new Map(oldMap);
            for (int i = 0; i < SMOOTHING_ITERATIONS; i++)
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
            ResetRegions();
        }

        /// <summary>
        /// Expand each floor region by a number of tiles in each direction based on the provided argument.
        /// </summary>
        public void ExpandRegions(int radius)
        {
            Map expandedMap = new Map(map);
            for (int x = 2; x < map.length - 2; x++)
            {
                for (int y = 2; y < map.width - 2; y++)
                {
                    if (map[x,y] == Tile.Floor)
                    {
                        ClearNeighbors(expandedMap, new Coord(x, y), radius); 
                    }
                }
            }
            map = expandedMap;
            ResetRegions();
        }

        /// <summary>
        /// Remove small regions of walls. Walls are considered to be in the same region if they are connected by a 
        /// sequence of vertical and horizontal steps through walls. 
        /// </summary>
        /// <param name="threshold">Number of tiles a region must have to not be removed.</param>
        public void RemoveSmallWallRegions(int threshold)
        {
            wallRegions = GetRegions(Tile.Wall);
            wallRegions = RemoveSmallRegions(threshold, Tile.Wall, wallRegions);
        }

        /// <summary>
        /// Remove small regions of floor tiles. Floor tiles are considered to be in the same region if they are connected 
        /// by a sequence of vertical and horizontal steps through floor tiles. 
        /// </summary>
        /// <param name="threshold">Number of tiles a region must have to not be removed.</param>
        public void RemoveSmallFloorRegions(int threshold)
        {
            floorRegions = GetRegions(Tile.Floor);
            floorRegions = RemoveSmallRegions(threshold, Tile.Floor, floorRegions);
        }

        /// <summary>
        /// Ensure connectivity between all regions of floors in the map. It is recommended that you first prune
        /// small floor regions in order to avoid creating tunnels to tiny regions.
        /// </summary>
        public void ConnectFloors(int tunnelRadius)
        {
            List<TileRegion> floors = floorRegions ?? GetRegions(Tile.Floor);
            List<Room> rooms = floors.Select(region => new Room(region, map)).ToList();
            List<RoomConnection> allRoomConnections = ComputeRoomConnections(rooms);
            List<RoomConnection> finalConnections = MinimumSpanningTree.GetMinimalConnectionsDiscrete(allRoomConnections, rooms.Count);
            foreach (RoomConnection connection in finalConnections)
            {
                CreatePassage(connection, tunnelRadius);
            }
            ResetRegions();
        }

        /// <summary>
        /// Add walls around the map of given thickness. Note that a border of thickness n will result in 2n being added to both
        /// width and length.
        /// </summary>
        /// <param name="borderSize">How thick the border should be on each side.</param>
        public void ApplyBorder(int borderSize)
        {
            Map borderedMap = new Map(map.length + borderSize * 2, map.width + borderSize * 2, map.squareSize);
            for (int x = 0; x < borderedMap.length; x++)
            {
                int xShifted = x - borderSize;
                for (int y = 0; y < borderedMap.width; y++)
                {
                    int yShifted = y - borderSize;
                    bool isInsideBorder = (0 <= xShifted && xShifted < map.length) && (0 <= yShifted && yShifted < map.width);
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

        List<TileRegion> RemoveSmallRegions(int threshold, Tile tileType, List<TileRegion> regions)
        {
            List<TileRegion> remainingRegions = new List<TileRegion>();
            Tile otherType = tileType == Tile.Wall ? Tile.Floor : Tile.Wall;
            foreach (TileRegion region in regions)
            {
                if (region.Count < threshold)
                {
                    FillRegion(region, otherType);
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
            bool[,] visited = new bool[map.length, map.width];

            if (tileType == Tile.Wall)
                VisitBoundaryRegion(visited);

            for (int x = 0; x < map.length; x++)
            {
                for (int y = 0; y < map.width; y++)
                {
                    if (IsNewTileOfType(visited, new Coord(x, y), tileType))
                    {
                        regions.Add(GetRegion(x, y, visited));
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
            VisitColumns(visited, 0, 1, map.length - 2, map.length - 1);
            VisitRows(visited, 0, 1, map.width - 2, map.width - 1);
            Queue<Coord> queue = InitializeBoundaryQueue();
            GetTilesReachableFromQueue(queue, visited);
        }

        void VisitColumns(bool[,] visited, params int[] columns)
        {
            foreach (int columnNumber in columns)
            {
                for (int y = 0; y < map.width; y++)
                {
                    visited[columnNumber, y] = true;
                }
            }
        }

        void VisitRows(bool[,] visited, params int[] rows)
        {
            foreach (int rowNumber in rows)
            {
                for (int x = 0; x < map.length; x++)
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
            for (int x = 1; x < map.length - 1; x++)
            {
                if (map[x, 1] == Tile.Wall) // left
                    queue.Enqueue(new Coord(x, 1));

                if (map[x, map.width - 2] == Tile.Wall) // right
                    queue.Enqueue(new Coord(x, map.width - 2));
            }
            for (int y = 1; y < map.width - 1; y++)
            {
                if (map[1, y] == Tile.Wall) // bottom
                    queue.Enqueue(new Coord(1, y));

                if (map[map.length - 2, y] == Tile.Wall) // top
                    queue.Enqueue(new Coord(map.length - 2, y));
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
            tiles.Clear();
            if (queue.Count == 0)
            {
                return new TileRegion(tiles);
            }
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
            return new TileRegion(tiles);
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
            foreach (Coord coord in line)
            {
                ClearNeighbors(map, coord, tunnelingRadius);
            }
        }

        /// <summary>
        /// Replace nearby tiles with floors. Does not affect boundary tiles.
        /// </summary>
        /// <param name="neighborReach">The radius of replacement: e.g. if 1, will replace the 8 adjacent tiles.</param>
        void ClearNeighbors(Map map, Coord coord, int neighborReach)
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

        void Swap(ref Map a, ref Map b)
        {
            Map temp = a;
            a = b;
            b = temp;
        }

        /// <summary>
        /// Used when a method renders existing region information invalid. 
        /// </summary>
        void ResetRegions()
        {
            floorRegions = null;
            wallRegions = null;
        }
    }

}