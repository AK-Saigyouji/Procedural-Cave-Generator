using System.Collections.Generic;
using UnityEngine;
using MapHelpers;
using System.Linq;

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

    readonly int SMOOTHING_ITERATIONS = 5;
    readonly int CELLULAR_THRESHOLD = 4;
    readonly int MINIMUM_WALL_REGION_SIZE = 50;
    readonly int MINIMUM_OPEN_REGION_SIZE = 50;
    readonly int TUNNELING_RADIUS = 1;

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
    /// Generates a randomized Map object based on the map generator's properties. May take prohibitively long for very 
    /// large maps (width * length > 10^6).
    /// </summary>
    /// <returns>Returns the generated Map object</returns>
    public Map GenerateMap()
    {
        map = new Map(length, width, squareSize);
        RandomFillMap(mapDensity);
        SmoothMap(SMOOTHING_ITERATIONS);
        List<Room> rooms = RemoveSmallRegions(MINIMUM_WALL_REGION_SIZE, MINIMUM_OPEN_REGION_SIZE);
        ConnectRooms(rooms);
        ApplyBorder(borderSize);
        return map;
    }

    /// <summary>
    /// Randomly fills the map with a proportion of wall tiles based on map density. The rest is filled with floor tiles.
    /// </summary>
    void RandomFillMap(float mapDensity)
    {
        Random.seed = GetSeed();

        for (int x = 0; x < length; x++)
        {
            for (int y = 0; y < width; y++)
            {
                bool isOnMapEdge = (x == 0 || x == length - 1 || y == 0 || y == width - 1);
                map[x, y] = (isOnMapEdge || Random.value < mapDensity) ? Tile.Wall : Tile.Floor;
            }
        }
    }

    int GetSeed()
    {
        return useRandomSeed ? System.Environment.TickCount : seed.GetHashCode();
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
                    newMap[x, y] = GetNewTileBasedOnNeighbors(x, y);
                }
            }

            map = newMap;
        }
    }

    Tile GetNewTileBasedOnNeighbors(int x, int y)
    {
        int neighborCount = GetSurroundingWallCount(x, y);
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
    /// Removes contiguous sections of the map that are too small. 
    /// </summary>
    /// <returns>List of Room objects corresponding to open regions that were not removed.</returns>
    List<Room> RemoveSmallRegions(int minWallRegionSize, int minOpenRegionSize)
    {
        RemoveSmallRegionsOfType(minWallRegionSize, Tile.Wall);
        List<Room> rooms = RemoveSmallRegionsOfType(minOpenRegionSize, Tile.Floor);
        return rooms;
    }

    /// <summary>
    /// The number of walls beside the given point. Both horizontal and diagonal tiles count, but the tile itself does not.
    /// Note that the tile passed in must not be on the edge of the Map.
    /// </summary>
    /// <returns>Number of walls surrounding the given tile, between 0 and 8 inclusive.</returns>
    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int x = gridX - 1; x <= gridX + 1; x++)
        {
            for (int y = gridY - 1; y <= gridY + 1; y++)
            {
                if (x != gridX || y != gridY)
                {
                    wallCount += (int)map[x, y];
                }
            }
        }
        return wallCount;
    }

    /// <summary>
    /// Remove small regions of the given type and return the rest.
    /// </summary>
    /// <param name="removalThreshold">Number of tiles a region must have to not be removed.</param>
    /// <param name="tileType">Tile corresponding to which type of regions should be removed.</param>
    /// <returns>List of Rooms at least as large as the removal threshold.</returns>
    List<Room> RemoveSmallRegionsOfType(int removalThreshold, Tile tileType)
    {
        Tile otherType = (tileType == Tile.Wall) ? Tile.Floor : Tile.Wall;
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

    List<TileRegion> GetRegions(Tile tileType)
    {
        List<TileRegion> regions = new List<TileRegion>();
        bool[,] visited = new bool[length, width];

        for (int x = 0; x < length; x++)
        {
            for (int y = 0; y < width; y++)
            {
                if (!visited[x,y] && map[x,y] == tileType)
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
    TileRegion GetRegion(int xStart, int yStart, bool[,] visited)
    {
        TileRegion tiles = new TileRegion();
        Tile tileType = map[xStart, yStart];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(xStart, yStart));
        visited[xStart, yStart] = true;
        while (queue.Count > 0)
        {
            Coord currentTile = queue.Dequeue();
            tiles.Add(currentTile);

            foreach (Coord newTile in map.GetAdjacentTiles(currentTile.x, currentTile.y))
            {
                int x = newTile.x;
                int y = newTile.y;
                if (!visited[x, y] && map[x, y] == tileType)
                {
                    visited[x, y] = true;
                    queue.Enqueue(newTile);
                }
            }
        }
        return tiles;
    }

    void FillRegion(TileRegion region, Tile tileType)
    {
        foreach (Coord coord in region)
        {
            map[coord] = tileType;
        }
    }

    /// <summary>
    /// Ensure that there is a path between every two floor tiles in the map, creating passages where necessary.
    /// </summary>
    /// <param name="rooms">A list of all the rooms in the Map.</param>
    void ConnectRooms(List<Room> rooms)
    {
        List<RoomConnection> allRoomConnections = ComputeRoomConnections(rooms);
        List<RoomConnection> finalConnections = Kruskal.GetMinimalConnections(allRoomConnections, rooms.Count);
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
        RoomConnection[] connections = new RoomConnection[rooms.Count * rooms.Count];
        var actions = new List<System.Action>();
        for (int j = 0; j < rooms.Count; j ++)
        {
            for (int k = j + 1; k < rooms.Count; k++)
            {
                int jCopy = j;
                int kCopy = k;
                actions.Add(() => 
                    connections[jCopy * rooms.Count + kCopy] = new RoomConnection(rooms[jCopy], rooms[kCopy], jCopy, kCopy)
                );
            }
        }
        Utility.Threading.ParallelExecute(actions.ToArray());
        return connections.Where(x => x != null).ToList();
    }

    /// <summary>
    /// Create a passage along the path represented by the given connection.
    /// </summary>
    /// <param name="connection">RoomConnection object giving the two tiles that should be connected.</param>
    /// <param name="tunnelingRadius">Width of the created passage.</param>
    void CreatePassage(RoomConnection connection, int tunnelingRadius)
    {
        List<Coord> line = connection.tileA.CreateLineTo(connection.tileB);
        foreach (Coord coord in line)
        {
            ClearNeighbors(coord, tunnelingRadius);
        }
    }

    /// <summary>
    /// Debug method for visualizing the created connections. Use in place of CreatePassage if you want to see
    /// exactly what connections are being made. 
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
    /// Replace nearby tiles with floors.
    /// </summary>
    /// <param name="neighborReach">The radius of replacement: e.g. if 1, will replace the 8 adjacent tiles.</param>
    void ClearNeighbors(Coord coord, int neighborReach)
    {
        for (int x = coord.x - neighborReach; x <= coord.x + neighborReach; x++)
        {
            for (int y = coord.y - neighborReach; y <= coord.y + neighborReach; y++)
            {
                if (map.Contains(x, y))
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
    /// <param name="borderSize">How thick the border should be.</param>
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
}