using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using MapHelpers;

internal class Map
{
    internal int[,] grid { get; private set; }
    internal int squareSize { get; private set; }
    internal int length { get { return grid.GetLength(0); } }
    internal int width { get { return grid.GetLength(1); } }
    internal int scaledTotalLength { get { return parentLength * squareSize; } }
    internal int scaledTotalWidth { get { return parentWidth * squareSize; } }
    internal Vector2 position = new Vector2(0f, 0f);
    internal int index { get; private set; }
    static int SUBMAP_SIZE = 100;
    int parentLength;
    int parentWidth;

    internal Map(int length, int width, int squareSize, Map parent = null)
    {
        grid = new int[length, width];
        this.squareSize = squareSize;
        if (parent == null)
        {
            parentLength = length;
            parentWidth = width;
        }
        else
        {
            parentLength = parent.length;
            parentWidth = parent.width;
        }
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

    internal int this[Coord tile]
    {
        get { return grid[tile.x, tile.y]; }
        set { grid[tile.x, tile.y] = value; }
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
        int xNumComponents = Mathf.CeilToInt(length / (float)SUBMAP_SIZE);
        int yNumComponents = Mathf.CeilToInt(width / (float)SUBMAP_SIZE);
        for (int x = 0; x < xNumComponents; x++)
        {
            for (int y = 0; y < yNumComponents; y++)
            {
                Map subMap = GenerateSubMap(x * SUBMAP_SIZE, y * SUBMAP_SIZE);
                subMap.index = x * yNumComponents + y;
                maps.Add(subMap);
            }
        }
        return maps;
    }

    Map GenerateSubMap(int xStart, int yStart)
    {
        int xEnd = (xStart + SUBMAP_SIZE >= length) ? length : xStart + SUBMAP_SIZE + 1;
        int yEnd = (yStart + SUBMAP_SIZE >= width) ? width : yStart + SUBMAP_SIZE + 1;
        Map subMap = new Map(xEnd - xStart, yEnd - yStart, squareSize, this);
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

    internal bool IsEdgeTile(int x, int y)
    {
        return GetAdjacentTiles(x, y).Any(adjTile => this[x, y] == 1);
    }

    internal bool IsEdgeTile(Coord tile)
    {
        return GetAdjacentTiles(tile).Any(adjTile => this[adjTile] == 1);
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
