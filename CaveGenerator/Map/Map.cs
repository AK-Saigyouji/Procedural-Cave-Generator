using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CaveGeneration
{
    public enum Tile : byte
    {
        Floor = 0,
        Wall = 1
    }

    /// <summary>
    /// The 2D grid-based Map. Points in the map are given by integer pairs like a 2d array. Each point is either 
    /// a floor or wall tile. Exposed methods are intended for use internally for the purposes of generating the cave - 
    /// for a read-only grid intended for external use, see the Grid class. 
    /// </summary>
    public class Map
    {
        Tile[,] grid;
        public int squareSize { get; private set; }
        public int length { get { return grid.GetLength(0); } }
        public int width { get { return grid.GetLength(1); } }
        public Vector3 position { get; private set; }
        public int index { get; private set; }

        static public readonly int maxSubmapSize = 75;

        public Map(int length, int width, int squareSize)
        {
            grid = new Tile[length, width];
            this.squareSize = squareSize;
            position = Vector3.zero;
        }

        public Map(Map map) : this(map.grid, map.squareSize)
        {
            index = map.index;
            position = map.position;
        }

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
        public IList<Map> Subdivide()
        {
            List<Map> maps = new List<Map>();
            int xNumComponents = Mathf.CeilToInt(length / (float)maxSubmapSize);
            int yNumComponents = Mathf.CeilToInt(width / (float)maxSubmapSize);
            for (int x = 0; x < xNumComponents; x++)
            {
                for (int y = 0; y < yNumComponents; y++)
                {
                    Map subMap = GenerateSubMap(x * maxSubmapSize, y * maxSubmapSize, maxSubmapSize);
                    subMap.index = x * yNumComponents + y;
                    maps.Add(subMap);
                }
            }
            return maps.AsReadOnly();
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
        /// Is the tile adjacent to a wall tile? Assumes tile is a valid map tile (use Contains method to check if 
        /// not sure).
        /// </summary>
        public bool IsAdjacentToWall(int x, int y)
        {
            return GetAdjacentTiles(x, y).Any(adjTile => this[x, y] == Tile.Wall);
        }

        /// <summary>
        /// Is the tile adjacent to a wall tile? Assumes tile is a valid map tile (use Contains method to check if 
        /// not sure).
        /// </summary>
        public bool IsAdjacentToWall(Coord tile)
        {
            return GetAdjacentTiles(tile).Any(adjTile => this[adjTile] == Tile.Wall);
        }

        /// <summary>
        /// Is the tile adjacent to a wall tile? Assumes the tile is not along the boundary (throws exception otherwise)
        /// so use only if this tile is in the interior of the map.
        /// </summary>
        public bool IsAdjacentToWallFast(int x, int y)
        {
            return this[x - 1, y] == Tile.Wall || this[x + 1, y] == Tile.Wall 
                || this[x, y + 1] == Tile.Wall || this[x, y - 1] == Tile.Wall;
        }

        /// <summary>
        /// Is the tile adjacent to a wall tile? Assumes the tile is not along the boundary (throws exception otherwise)
        /// so use only if this tile is in the interior of the map.
        /// </summary>
        public bool IsAdjacentToWallFast(Coord tile)
        {
            return IsAdjacentToWallFast(tile.x, tile.y);
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
        /// The number of walls beside the given point. Both horizontal and diagonal tiles count, but the tile itself does not.
        /// Note that the coordinates must be contained in the interior of the map (not on the boundary);
        /// </summary>
        /// <returns>Number of walls surrounding the given tile, between 0 and 8 inclusive.</returns>
        public int GetSurroundingWallCount(int x, int y)
        {
            return (int)grid[x - 1, y + 1] + (int)grid[x, y + 1] + (int)grid[x + 1, y + 1]
                + (int)grid[x - 1, y] + (int)grid[x + 1, y]
                + (int)grid[x - 1, y - 1] + (int)grid[x, y - 1] + (int)grid[x + 1, y - 1];
        }

        public bool IsBoundaryTile(int x, int y)
        {
            return (x == 0 || x == length - 1) && (0 <= y && y <= width - 1) // vertical boundary
                || (y == 0 || y == width - 1) && (0 <= x && x <= length - 1); // horizontal boundary
        }

        public bool IsBoundaryTile(Coord coord)
        {
            return IsBoundaryTile(coord.x, coord.y);
        }

        public bool IsInteriorTile(int x, int y)
        {
            return (0 < x && x < length - 1) && (0 < y && y < width - 1);
        }

        public bool IsInteriorTile(Coord tile)
        {
            return IsInteriorTile(tile.x, tile.y);
        }

        /// <summary>
        /// Gets the horizontally adjacent tiles. Examples:
        /// (1,3) -> (0,3), (2,3), (1,4), (1,2) (assumes map is at least 3 by 5).
        /// (0,0) -> (0,1), (1,0) (assumes map is at least 2 by 2).
        /// (0,1) -> (1,1), (0,0), (0,2) (assumes map is at least (2 by 3).
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
        /// (1,3) -> (0,3), (2,3), (1,4), (1,2) (assumes map is at least 3 by 5).
        /// (0,0) -> (0,1), (1,0) (assumes map is at least 2 by 2).
        /// (0,1) -> (1,1), (0,0), (0,2) (assumes map is at least (2 by 3).
        /// </summary>
        public IEnumerable<Coord> GetAdjacentTiles(Coord tile)
        {
            return GetAdjacentTiles(tile.x, tile.y);
        }

        /// <summary>
        /// Swaps floor tiles and wall tiles.
        /// </summary>
        public void Invert()
        {
            for (int x = 0; x < length; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    grid[x, y] = (grid[x, y] == Tile.Wall) ? Tile.Floor : Tile.Wall;
                }
            }
        }

        public Grid ToGrid()
        {
            return new Grid(grid);
        }
    }
}