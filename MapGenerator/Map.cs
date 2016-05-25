using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using MapHelpers;

public enum Tile : byte
{
    Floor = 0,
    Wall = 1
}

/// <summary>
/// The 2D grid-based Map. Points in the map are given x and y integer pairs like a 2d array. 
/// Values of 1 correspond to walls, 0 to open space.
/// </summary>
public class Map
{
    public Tile[,] grid { get; private set; }
    public int squareSize { get; private set; }
    public int length { get { return grid.GetLength(0); } }
    public int width { get { return grid.GetLength(1); } }
    public Vector2 position { get; private set; }
    public int index { get; private set; }

    public Map(int length, int width, int squareSize)
    {
        grid = new Tile[length, width];
        this.squareSize = squareSize;
        position = new Vector2(0f, 0f);
    }

    public Map(Map map)
    {
        grid = map.grid;
        squareSize = map.squareSize;
        position = map.position;
    }

    public Tile this[int x, int y]
    {
        get { return grid[x, y]; }
        set { grid[x, y] = value; }
    }

    public Tile this[Coord tile]
    {
        get { return grid[tile.x, tile.y]; }
        set { grid[tile.x, tile.y] = value; }
    }

    /// <summary>
    /// Cut up the Map into smaller Map chunks.
    /// </summary>
    /// <returns>Returns a list of smaller Map objects.</returns>
    public IList<Map> SubdivideMap(int submapSize = 100)
    {
        IList<Map> maps = new List<Map>();
        int xNumComponents = Mathf.CeilToInt(length / (float)submapSize);
        int yNumComponents = Mathf.CeilToInt(width / (float)submapSize);
        for (int x = 0; x < xNumComponents; x++)
        {
            for (int y = 0; y < yNumComponents; y++)
            {
                Map subMap = GenerateSubMap(x * submapSize, y * submapSize, submapSize);
                subMap.index = x * yNumComponents + y;
                maps.Add(subMap);
            }
        }
        return maps;
    }

    Map GenerateSubMap(int xStart, int yStart, int submapSize)
    {
        int xEnd = ComputeSubMapLengthEndPoint(xStart, submapSize);
        int yEnd = ComputeSubMapWidthEndPoint(yStart, submapSize);
        Map subMap = new Map(xEnd - xStart, yEnd - yStart, squareSize);
        for (int x = xStart; x < xEnd; x++)
        {
            for (int y = yStart; y < yEnd; y++)
            {
                subMap[x - xStart, y - yStart] = this[x, y];
            }
        }
        subMap.position = new Vector2(xStart * squareSize, yStart * squareSize);
        return subMap;
    }

    int ComputeSubMapLengthEndPoint(int xStart, int submapSize)
    {
        return (xStart + submapSize >= length) ? length : xStart + submapSize + 1;
    }

    int ComputeSubMapWidthEndPoint(int yStart, int submapSize)
    {
        return (yStart + submapSize >= width) ? width : yStart + submapSize + 1;
    }

    public bool IsEdgeTile(int x, int y)
    {
        return GetAdjacentTiles(x, y).Any(adjTile => this[x, y] == Tile.Wall);
    }

    public bool IsEdgeTile(Coord tile)
    {
        return GetAdjacentTiles(tile).Any(adjTile => this[adjTile] == Tile.Wall);
    }

    public bool IsInMap(int x, int y)
    {
        return 0 <= x && x < length && 0 <= y && y < width;
    }

    public bool IsInMap(Coord coord)
    {
        return IsInMap(coord.x, coord.y);
    }

    public IEnumerable<Coord> GetAdjacentTiles(int x, int y)
    {
        if (x > 0)
            yield return new Coord(x - 1, y);
        if (x + 1 < length)
            yield return new Coord(x + 1, y);
        if (y > 0)
            yield return new Coord(x, y - 1);
        if (y + 1 < width)
            yield return new Coord(x, y + 1);
    }

    public IEnumerable<Coord> GetAdjacentTiles(Coord tile)
    {
        return GetAdjacentTiles(tile.x, tile.y);
    }
}
