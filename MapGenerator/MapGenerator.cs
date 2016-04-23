using MapHelpers;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public abstract class MapGenerator : MonoBehaviour {
    [SerializeField]
    int length = 50;
    [SerializeField]
    int width = 50;
    [SerializeField]
    [Range(0, 1)]
    float randomFillPercent = 0.5f;
    [SerializeField]
    string seed;
    [SerializeField]
    bool useRandomSeed = true;
    [SerializeField]
    int borderSize = 0;
    [SerializeField]
    int squareSize = 1;

    protected Map map { get; private set; }
    protected internal GameObject cave { get; set; }
    protected internal List<MapMeshes> generatedMeshes { get; set; }

    int SMOOTHING_ITERATIONS = 5;
    int CELLULAR_THRESHOLD = 4;
    int MINIMUM_WALL_REGION_SIZE = 50;
    int MINIMUM_OPEN_REGION_SIZE = 50;
    int TUNNELING_RADIUS = 1;

    public void GenerateNewMapWithMesh()
    {
        DestroyChildren();
        GenerateMap();
        GenerateMeshFromMap(map);
    }

    public Map GenerateMap()
    {
        map = new Map(length, width, squareSize);
        RandomFillMap();
        SmoothMap(SMOOTHING_ITERATIONS);
        ProcessMap();
        map.ApplyBorder(borderSize);
        return map;
    }

    abstract protected void GenerateMeshFromMap(Map map);

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

    void ProcessMap()
    {
        List<Room> rooms = RemoveSmallRegions(MINIMUM_OPEN_REGION_SIZE, 0);
        RemoveSmallRegions(MINIMUM_WALL_REGION_SIZE, 1);
        ConnectRooms(rooms);
    }

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

    List<Room> RemoveSmallRegions(int removalThreshold, int tileType)
    {
        int otherType = (tileType == 1) ? 0 : 1;
        List<TileRegion> regions = GetRegions(tileType);
        List<Room> remainingRegions = new List<Room>();
        foreach (TileRegion region in regions)
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

    void FillRegion(TileRegion region, int value)
    {
        foreach (Coord tile in region)
        {
            map[tile] = value;
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
        foreach (Coord coord in line)
        {
            ClearNeighbors(coord, TUNNELING_RADIUS);
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

    protected GameObject CreateObjectFromMesh(Mesh mesh, string name, GameObject parent, Material material)
    {
        GameObject newObject = new GameObject(name, typeof(MeshRenderer), typeof(MeshFilter));
        newObject.transform.parent = parent.transform;
        newObject.GetComponent<MeshFilter>().mesh = mesh;
        newObject.GetComponent<MeshRenderer>().material = material;
        return newObject;
    }

    bool IsNewTileOfGivenType(int x, int y, int[,] visited, int tileType)
    {
        return (visited[x, y] == 0) && (map[x, y] == tileType);
    }

    void DestroyChildren()
    {
        List<Transform> children = new List<Transform>();
        foreach (Transform child in transform)
        {
            children.Add(child);
        }
        foreach (Transform child in children)
        {
            child.parent = null;
            Destroy(child.gameObject);
        }
    }
}

public class MapMeshes
{
    public Mesh wallMesh { get; private set; }
    public Mesh ceilingMesh { get; private set; }

    private MapMeshes() { }

    internal MapMeshes(Mesh ceilingMesh, Mesh wallMesh = null)
    {
        this.ceilingMesh = ceilingMesh;
        this.wallMesh = wallMesh;
    }
}


