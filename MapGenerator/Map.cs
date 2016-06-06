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
    public Vector3 position { get; private set; }
    public int index { get; private set; }

    public Map(int length, int width, int squareSize)
    {
        grid = new Tile[length, width];
        this.squareSize = squareSize;
        position = Vector3.zero;
    }

    public Map(Map map) : this(map.grid, map.squareSize){}

    public Map(Tile[,] grid, int squareSize) : this(grid.GetLength(0), grid.GetLength(1), squareSize)
    {
        int length = grid.GetLength(0);
        int width = grid.GetLength(1);
        for (int x = 0; x < length; x++)
        {
            for (int y = 0; y < width; y++)
            {
                this.grid[x, y] = grid[x, y];
            }
        }
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
    /// <param name="submapSize">Maximum length and width for each submap.</param>
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
        subMap.position = new Vector3(xStart * squareSize, 0f, yStart * squareSize);
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

    /// <summary>
    /// Is the tile adjacent to a wall tile?
    /// </summary>
    public bool IsEdgeTile(int x, int y)
    {
        return GetAdjacentTiles(x, y).Any(adjTile => this[x, y] == Tile.Wall);
    }

    /// <summary>
    /// Is the tile adjacent to a wall tile?
    /// </summary>
    public bool IsEdgeTile(Coord tile)
    {
        return GetAdjacentTiles(tile).Any(adjTile => this[adjTile] == Tile.Wall);
    }

    /// <summary>
    /// Are the coordinates valid for this map?
    /// </summary>
    public bool Contains(int x, int y)
    {
        return 0 <= x && x < length && 0 <= y && y < width;
    }

    /// <summary>
    /// Is the Coord valid for this map?
    /// </summary>
    public bool Contains(Coord coord)
    {
        return Contains(coord.x, coord.y);
    }

    /// <summary>
    /// Gets the horizontally adjacent tiles. Examples:
    /// (1,3) -> (0,3), (2,3), (1,4), (1,2) (assumes map is at least 3 by 5)
    /// (0,0) -> (0,1), (1,0) (assumes map is at least 2 by 2)
    /// (0,1) -> (1,1), (0,0), (0,2) (assumes map is at least (2 by 3)
    /// </summary>
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

    /// <summary>
    /// Gets the horizontally adjacent tiles. Examples:
    /// (1,3) -> (0,3), (2,3), (1,4), (1,2) (assumes map is at least 3 by 5)
    /// (0,0) -> (0,1), (1,0) (assumes map is at least 2 by 2)
    /// (0,1) -> (1,1), (0,0), (0,2) (assumes map is at least (2 by 3)
    /// </summary>
    public IEnumerable<Coord> GetAdjacentTiles(Coord tile)
    {
        return GetAdjacentTiles(tile.x, tile.y);
    }
}
