using UnityEngine;
using System.Collections.Generic;
using MapHelpers;

internal class Map
{
    internal int[,] grid { get; private set; }
    public int squareSize { get; private set; }
    public int length { get { return grid.GetLength(0); } }
    public int width { get { return grid.GetLength(1); } }
    internal int scaledLength { get { return length * squareSize; } }
    internal int scaledWidth { get { return width * squareSize; } }
    internal Vector2 position = new Vector2(0f, 0f);
    static int MAX_MAP_SIZE = 100;

    public Map(int length, int width, int squareSize)
    {
        grid = new int[length, width];
        this.squareSize = squareSize;
    }

    internal Map(Map map)
    {
        grid = map.grid;
        squareSize = map.squareSize;
    }

    public int this[int x, int y]
    {
        get { return grid[x, y]; }
        set { grid[x, y] = value; }
    }

    internal void ApplyBorder(int borderSize)
    {
        int[,] borderedMap = new int[length + borderSize * 2, width + borderSize * 2];
        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            int xShifted = x - borderSize;
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                int yShifted = y - borderSize;
                bool isInsideBorder = (0 <= xShifted && xShifted < length) && (0 <= yShifted && yShifted < width);
                borderedMap[x, y] = isInsideBorder ? grid[xShifted, yShifted] : 1;
            }
        }
        grid = borderedMap;
    }

    internal IList<Map> SubdivideMap()
    {
        IList<Map> maps = new List<Map>();
        int xNumComponents = Mathf.CeilToInt(length / (float)MAX_MAP_SIZE);
        int yNumComponents = Mathf.CeilToInt(width / (float)MAX_MAP_SIZE);
        for (int x = 0; x < xNumComponents; x++)
        {
            for (int y = 0; y < yNumComponents; y++)
            {
                Map subMap = GenerateSubMap(x * MAX_MAP_SIZE, y * MAX_MAP_SIZE);
                maps.Add(subMap);
            }
        }
        return maps;
    }

    Map GenerateSubMap(int xStart, int yStart)
    {
        int xEnd = (xStart + MAX_MAP_SIZE >= length) ? length : xStart + MAX_MAP_SIZE + 1;
        int yEnd = (yStart + MAX_MAP_SIZE >= width) ? width : yStart + MAX_MAP_SIZE + 1;
        Map subMap = new Map(xEnd - xStart, yEnd - yStart, 1);
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

    internal bool IsInMap(int x, int y)
    {
        return 0 <= x && x <= length && 0 <= y && y <= width;
    }

    internal bool IsInMap(Coord coord)
    {
        return IsInMap(coord.x, coord.y);
    }

    internal IEnumerable<Coord> GetAdjacentTiles(int x, int y)
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

    internal IEnumerable<Coord> GetAdjacentTiles(Coord tile)
    {
        return GetAdjacentTiles(tile.x, tile.y);
    }
}
