using UnityEngine;
using System.Collections.Generic;
using MapHelpers;

public class MapGenerator : MonoBehaviour {
    [SerializeField]
    int length;
    [SerializeField]
    int width;
    [SerializeField]
    [Range(0,1)]
    float randomFillPercent;
    [SerializeField]
    string seed;
    [SerializeField]
    bool useRandomSeed;
    [SerializeField]
    int borderSize;
    [SerializeField]
    int squareSize;

    Map map;

    int SMOOTHING_ITERATIONS = 5;
    int CELLULAR_THRESHOLD = 4;
    int MINIMUM_WALL_REGION_SIZE = 50;
    int MINIMUM_OPEN_REGION_SIZE = 50;
    int TUNNELING_RADIUS = 1;

    void Start()
    {
        GenerateMap();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GenerateMap();
        }
    }

    void GenerateMap()
    {
        map = new Map(length, width, squareSize);
        RandomFillMap();
        SmoothMap(SMOOTHING_ITERATIONS);
        ProcessMap();
        map.ApplyBorder(borderSize);
        GetComponent<MeshGenerator>().GenerateMesh(map);
    }

    void RandomFillMap()
    {
        Random.seed = useRandomSeed ? System.Environment.TickCount : seed.GetHashCode();

        for (int x = 0; x < length; x++)
        {
            for (int y = 0; y < width; y++)
            {
                bool isEdge = (x == 0 || x == length - 1 || y == 0 || y == width - 1);
                map[x, y] = (isEdge || Random.value < randomFillPercent) ? 1 : 0;
            }
        }
    }

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

    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int x = gridX - 1; x <= gridX + 1; x++)
            for (int y = gridY - 1; y <= gridY + 1; y++)
                if (x != gridX || y != gridY)
                    wallCount += map[x, y];
        return wallCount;
    }

    void ProcessMap()
    {
        List<Room> rooms = RemoveSmallRegions(MINIMUM_OPEN_REGION_SIZE, 0);
        RemoveSmallRegions(MINIMUM_WALL_REGION_SIZE, 1);
        ConnectRooms(rooms);
    }

    List<Room> RemoveSmallRegions(int removalThreshold, int tileType)
    {
        int otherType = (tileType == 1) ? 0 : 1;
        List<Region> regions = GetRegions(tileType);
        List<Room> remainingRegions = new List<Room>();
        foreach (Region region in regions)
        {
            if (region.Size() < removalThreshold)
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

    void FillRegion(Region region, int value)
    {
        foreach (Coord tile in region)
        {
            map[tile.x, tile.y] = value;
        }
    }

    void ConnectRooms(List<Room> rooms)
    {
        List<RoomConnection> roomConnections = ComputeRoomConnections(rooms);
        List<RoomConnection> finalConnections = Kruskal.GetPrunedConnections(roomConnections, rooms.Count);
        foreach (RoomConnection connection in finalConnections)
        {
            CreatePassage(connection);
        }
    }

    List<RoomConnection> ComputeRoomConnections(List<Room> rooms)
    {
        List<RoomConnection> connections = new List<RoomConnection>();
        for (int i = 0; i < rooms.Count; i++)
        {
            Room roomA = rooms[i];
            for (int k = i + 1; k < rooms.Count; k++)
            {
                Room roomB = rooms[k];
                connections.Add(new RoomConnection(roomA, roomB, i, k));
            }
        }
        return connections;
    }

    void CreatePassage(RoomConnection connection)
    {
        List<Coord> line = CreateLineBetween(connection.tileA, connection.tileB);
        CreatePassage(line);
    }

    void CreatePassage(List<Coord> line)
    {
        foreach (Coord coord in line)
        {
            ClearNeighbors(coord, TUNNELING_RADIUS);
        }
    }

    void ClearNeighbors(Coord coord, int neighborReach)
    {
        for (int x = coord.x - neighborReach; x <= coord.x + neighborReach; x++)
            for (int y = coord.y - neighborReach; y <= coord.y + neighborReach; y++)
                if (map.IsInMap(x, y))
                    map[x, y] = 0;
    }

    List<Coord> CreateLineBetween(Coord start, Coord end)
    {
        Vector2 incrementor;
        Vector2 startVector = new Vector2(start.x, start.y);
        List<Coord> line = new List<Coord>();

        int xDelta = end.x - start.x;
        int yDelta = end.y - start.y;
        int numIterations = Mathf.Max(System.Math.Abs(xDelta), System.Math.Abs(yDelta));
        incrementor = new Vector2(xDelta, yDelta) / numIterations;

        for (int i = 0; i <= numIterations; i++)
        {
            Vector2 nextVector = startVector + i * incrementor;
            line.Add(new Coord((int)nextVector.x, (int)nextVector.y));
        }

        return line;
    }

    List<Region> GetRegions(int tileType)
    {
        List<Region> regions = new List<Region>();
        int[,] visited = new int[length, width];

        for (int x = 0; x < length; x++)
        {
            for (int y = 0; y < width; y++)
            {
                if (IsValidTile(x, y, visited, tileType))
                {
                    Region newRegion = GetRegion(x, y, visited);
                    regions.Add(newRegion);
                }
            }
        }
        return regions;
    }

    Region GetRegion(int xStart, int yStart, int[,] visited)
    {
        Region tiles = new Region();
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
                if (IsValidTile(newTile.x, newTile.y, visited, tileType))
                {
                    visited[newTile.x, newTile.y] = 1;
                    queue.Enqueue(newTile);
                }
            }
        }
        return tiles;
    }

    bool IsValidTile(int x, int y, int[,] visited, int tileType)
    {
        return (visited[x, y] == 0) && (map[x, y] == tileType);
    }

}


