using System.Collections.Generic;
using UnityEngine;
using MapHelpers;
using System.Linq;
using System.Threading;

/// <summary>
/// Generates a randomized cave-like Map object. Ensures that every point is reachable from every other point. Boundary of the
/// map is guaranteed to consist of walls. 
/// </summary>
public class MapGenerator : IMapGenerator
{
    public int length { get; set; }
    public int width { get; set; }
    public float mapDensity { get; set; }
    public string seed { get; set; }
    public bool useRandomSeed { get; set; }
    public int borderSize { get; set; }
    public int squareSize { get; set; }

    public Map map { get; private set; }

    int SMOOTHING_ITERATIONS = 5;
    int CELLULAR_THRESHOLD = 4;
    int MINIMUM_WALL_REGION_SIZE = 50;
    int MINIMUM_OPEN_REGION_SIZE = 50;
    int TUNNELING_RADIUS = 1;

    public MapGenerator(int length, int width, float mapDensity = 0.5f, string seed = "", bool useRandomSeed = true, 
        int borderSize = 0, int squareSize = 1)
    {
        this.length = length;
        this.width = width;
        this.mapDensity = mapDensity;
        this.seed = seed;
        this.useRandomSeed = useRandomSeed;
        this.borderSize = borderSize;
        this.squareSize = squareSize;
    }

    /// <summary>
    /// Generates a randomized Map object based on the map generator's properties. May take a long time for large maps 
    /// (length * width > 10^7).
    /// </summary>
    /// <returns>Returns the generated Map object</returns>
    public Map GenerateMap()
    {
        map = new Map(length, width, squareSize);
        RandomFillMap();
        SmoothMap(SMOOTHING_ITERATIONS);
        List<Room> rooms = RemoveSmallRegions(MINIMUM_WALL_REGION_SIZE, MINIMUM_OPEN_REGION_SIZE);
        ConnectRooms(rooms);
        ApplyBorder(borderSize);
        return map;
    }

    /// <summary>
    /// Fills the map with 1s and 0s uniformly at random, based on the map density.
    /// </summary>
    void RandomFillMap()
    {
        Random.seed = useRandomSeed ? GetRandomSeed() : seed.GetHashCode();

        for (int x = 0; x < length; x++)
        {
            for (int y = 0; y < width; y++)
            {
                bool isEdge = (x == 0 || x == length - 1 || y == 0 || y == width - 1);
                map[x, y] = (isEdge || Random.value < mapDensity) ? 1 : 0;
            }
        }
    }

    int GetRandomSeed()
    {
        return System.Environment.TickCount;
    }

    /// <summary>
    /// Uses synchronous cellular automata to smooth out the map. Each cell becomes more like its neighbors,
    /// resulting in a more regular map.
    /// </summary>
    /// <param name="iterations">The number of smoothing passes.</param>
    void SmoothMap(int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            Map newMap = new Map(map);

            for (int x = 1; x < length - 1; x++)
            {
                for (int y = 1; y < width - 1; y++)
                {
                    int neighborCount = GetSurroundingWallCount(x, y);
                    if (neighborCount != CELLULAR_THRESHOLD)
                    {
                        newMap[x, y] = (neighborCount > CELLULAR_THRESHOLD) ? 1 : 0;
                    }
                }
            }
            map = newMap;
        }
    }

    /// <summary>
    /// Removes contiguous sections of the map that are too small. 
    /// </summary>
    /// <returns>List of Room objects corresponding to open regions that were not removed.</returns>
    List<Room> RemoveSmallRegions(int minWallRegionSize, int minOpenRegionSize)
    {
        RemoveSmallRegionsOfType(minWallRegionSize, 1);
        List<Room> rooms = RemoveSmallRegionsOfType(minOpenRegionSize, 0);
        return rooms;
    }

    /// <summary>
    /// The number of walls beside the given point. Both horizontal and diagonal tiles count, but the tile itself does not.
    /// Note that the tile passed in must not be on the edge of the Map.
    /// </summary>
    /// <returns>Number of walls surrounding the given tile.</returns>
    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int x = gridX - 1; x <= gridX + 1; x++)
        {
            for (int y = gridY - 1; y <= gridY + 1; y++)
            {
                if (x != gridX || y != gridY)
                {
                    wallCount += map[x, y];
                }
            }
        }
        return wallCount;
    }

    /// <summary>
    /// Remove small regions of the given type and return the rest.
    /// </summary>
    /// <param name="removalThreshold">Number of tiles a region must have to not be removed.</param>
    /// <param name="tileType">1 or 0, corresponding to which type of regions should be removed.</param>
    /// <returns>List of Rooms at least as large as the removal threshold.</returns>
    List<Room> RemoveSmallRegionsOfType(int removalThreshold, int tileType)
    {
        int otherType = (tileType == 1) ? 0 : 1;
        List<TileRegion> regions = GetRegions(tileType);
        List<Room> remainingRegions = new List<Room>();
        foreach (TileRegion region in regions)
        {
            if (region.Count < removalThreshold)
            {
                FillRegion(region, otherType);
            }
            else
            {
                remainingRegions.Add(new Room(region, map));
            }
        }
        return remainingRegions;
    }

    List<TileRegion> GetRegions(int tileType)
    {
        List<TileRegion> regions = new List<TileRegion>();
        int[,] visited = new int[length, width];

        for (int x = 0; x < length; x++)
        {
            for (int y = 0; y < width; y++)
            {
                if (IsNewTileOfGivenType(x, y, visited, tileType))
                {
                    TileRegion newRegion = GetRegion(x, y, visited);
                    regions.Add(newRegion);
                }
            }
        }
        return regions;
    }

    /// <summary>
    /// Get the region containing the start point. Two tiles are considered to be in the same region if there is a sequence
    /// of horizontal steps from one to the other (inclusive) passing through only the same tile type.
    /// </summary>
    /// <param name="visited">A 2D int array tracking visited nodes to reduce work.</param>
    /// <returns>The region of tiles containing the start point.</returns>
    TileRegion GetRegion(int xStart, int yStart, int[,] visited)
    {
        TileRegion tiles = new TileRegion();
        int tileType = map[xStart, yStart];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(xStart, yStart));
        visited[xStart, yStart] = 1;
        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            foreach (Coord newTile in map.GetAdjacentTiles(tile.x, tile.y))
            {
                if (IsNewTileOfGivenType(newTile.x, newTile.y, visited, tileType))
                {
                    visited[newTile.x, newTile.y] = 1;
                    queue.Enqueue(newTile);
                }
            }
        }
        return tiles;
    }

    void FillRegion(TileRegion region, int value)
    {
        foreach (Coord tile in region)
        {
            map[tile] = value;
        }
    }

    /// <summary>
    /// Ensure that there is a path between every two open spots in the map (i.e. every pair of 0s), creating passages
    /// if necessary.
    /// </summary>
    /// <param name="rooms">A list of all the rooms in the Map.</param>
    void ConnectRooms(List<Room> rooms)
    {
        List<RoomConnection> roomConnections = ComputeRoomConnections(rooms);
        List<RoomConnection> finalConnections = Kruskal.GetPrunedConnections(roomConnections, rooms.Count);
        foreach (RoomConnection connection in finalConnections)
        {
            CreatePassage(connection, TUNNELING_RADIUS);
        }
    }

    //Old, singlethreaded method for computing room connections. The new method does the same thing,
    //but spread across multiple threads.

    //List<RoomConnection> ComputeRoomConnections(List<Room> rooms)
    //{
    //    List<RoomConnection> connections = new List<RoomConnection>();
    //    for (int i = 0; i < rooms.Count; i++)
    //    {
    //        for (int k = i + 1; k < rooms.Count; k++)
    //        {
    //            connections.Add(new RoomConnection(rooms[i], rooms[k], i, k));
    //        }
    //    }
    //    return connections;
    //}

    /// <summary>
    /// Generate a RoomConnection object between every two rooms in the map, where the connection stores information
    /// about the shortest distance between the two rooms and the tiles corresponding to this distance.
    /// </summary>
    /// <param name="rooms">All rooms in the Map.</param>
    /// <returns>A list of every connection between rooms in the map.</returns>
    List<RoomConnection> ComputeRoomConnections(List<Room> rooms)
    {
        int connectionCount = rooms.Count * (rooms.Count - 1) / 2;
        int workItemCount = 8;
        RoomConnection[] connections = new RoomConnection[rooms.Count * rooms.Count];
        ManualResetEvent[] resetEvents = new ManualResetEvent[workItemCount];
        for (int i = 0; i < workItemCount; i++)
        {
            resetEvents[i] = new ManualResetEvent(false);
            ThreadPool.QueueUserWorkItem(new WaitCallback((object index) => 
            {
                int workItemIndex = (int)index;
                for (int j = workItemIndex; j < rooms.Count; j += workItemCount)
                {
                    for (int k = j + 1; k < rooms.Count; k++)
                    {
                        connections[j * rooms.Count + k] = new RoomConnection(rooms[j], rooms[k], j, k);
                    }
                }
                resetEvents[workItemIndex].Set();
            }), i);
        }
        WaitHandle.WaitAll(resetEvents);
        return connections.Where(x => x != null).ToList();
    }

    /// <summary>
    /// Create a passage along the path represented by the given connection.
    /// </summary>
    /// <param name="connection">RoomConnection object giving the two tiles that should be connected.</param>
    /// <param name="tunnelingRadius">Width of the created passage.</param>
    void CreatePassage(RoomConnection connection, int tunnelingRadius)
    {
        List<Coord> line = CreateLineBetween(connection.tileA, connection.tileB);
        foreach (Coord coord in line)
        {
            ClearNeighbors(coord, tunnelingRadius);
        }
    }

    void ClearNeighbors(Coord coord, int neighborReach)
    {
        for (int x = coord.x - neighborReach; x <= coord.x + neighborReach; x++)
        {
            for (int y = coord.y - neighborReach; y <= coord.y + neighborReach; y++)
            {
                if (map.IsInMap(x, y))
                {
                    map[x, y] = 0;
                }
            }
        }
    }

    /// <summary>
    /// Generate a list of coordinates representing a thin path beween the given coordinates (inclusive).
    /// </summary>
    /// <returns>List of Coords between start and end (inclusive).</returns>
    List<Coord> CreateLineBetween(Coord start, Coord end)
    {
        Vector2 startVector = new Vector2(start.x, start.y);
        List<Coord> line = new List<Coord>();

        int xDelta = end.x - start.x;
        int yDelta = end.y - start.y;
        int numIterations = Mathf.Max(System.Math.Abs(xDelta), System.Math.Abs(yDelta));
        Vector2 incrementor = new Vector2(xDelta, yDelta) / numIterations;

        for (int i = 0; i <= numIterations; i++)
        {
            Vector2 nextVector = startVector + i * incrementor;
            line.Add(new Coord((int)nextVector.x, (int)nextVector.y));
        }

        return line;
    }

    /// <summary>
    /// Add walls around the map of given thickness. Note that a border of thickness n will result in 2n being added to both
    /// width and length.
    /// </summary>
    /// <param name="borderSize">How thick the border should be.</param>
    void ApplyBorder(int borderSize)
    {
        Map borderedMap = new Map(length + borderSize * 2, width + borderSize * 2, map.squareSize);
        for (int x = 0; x < borderedMap.length; x++)
        {
            int xShifted = x - borderSize;
            for (int y = 0; y < borderedMap.width; y++)
            {
                int yShifted = y - borderSize;
                bool isInsideBorder = (0 <= xShifted && xShifted < length) && (0 <= yShifted && yShifted < width);
                borderedMap[x, y] = isInsideBorder ? map[xShifted, yShifted] : 1;
            }
        }
        map = borderedMap;
    }

    bool IsNewTileOfGivenType(int x, int y, int[,] visited, int tileType)
    {
        return (visited[x, y] == 0) && (map[x, y] == tileType);
    }
}