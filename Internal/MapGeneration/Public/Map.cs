/* This is the class that gets built up by a MapBuilder to represent a 2D grid consisting of walls and floors. 
 * 
 * A major design decision was the use of array type for the underlying grid. The three obvious choices
 * are a 2D array, a jagged array, and a flat array. Normally, jagged and flat arrays are noticeably faster than 2D
 * arrays. But given the way data is accessed, the cost associated with translating between 2d coordinates and position
 * in a flat array proved to strip flat arrays of any speedup they otherwise would have. As for jagged arrays, they would be
 * faster here, but they come with two penalties. The first is readability, as they would be accessed grid[y][x] (reversed
 * coordinates). The second is the extra memory consumption of jagged arrays associated with the overhead for each 
 * array, which adds up as the grid may have to be copied several times. Thus ultimately the 2D array was chosen.*/

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using Array = System.Array;

namespace CaveGeneration.MapGeneration
{
    public enum Tile : byte
    {
        Floor = 0,
        Wall = 1
    }

    /// <summary>
    /// The 2D grid-based Map. Points in the map are given by integer pairs like a 2d array. Each point is either 
    /// a floor or wall tile. Offers a variety of methods tailored to map construction.
    /// </summary>
    public sealed class Map
    {
        public int SquareSize { get; private set; }
        public int Length { get { return length; } }
        public int Width { get { return width; } }
        public Vector3 Position { get; private set; } 
        public Coord Index { get; private set; }

        public static readonly int maxSubmapSize = 150; // Chunk size when the map is broken up.

        Tile[,] grid;
        int length;
        int width;

        public Map(int length, int width, int squareSize)
        {
            grid = new Tile[length, width];
            this.length = length;
            this.width = width;
            SquareSize = squareSize;
            Position = Vector3.zero;
        }

        public Map(Map map) : this(map.grid, map.SquareSize)
        {
            Index = map.Index;
            Position = map.Position;
        }

        public Map(Tile[,] tiles, int squareSize) : this(tiles.GetLength(0), tiles.GetLength(1), squareSize)
        {
            Array.Copy(tiles, grid, grid.Length);
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
        /// Copies the values from the other map. Maps must have the same dimensions (length and width).
        /// </summary>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public void Copy(Map other)
        {
            if (other == null) throw new System.ArgumentNullException();
            if (length != other.length || width != other.width)
                throw new System.ArgumentException("Cannot copy map with different dimensions!");

            Array.Copy(other.grid, this.grid, grid.Length);
        }

        /// <summary>
        /// Divide the map into smaller Map chunks.
        /// </summary>
        /// <returns>Returns a readonly list of smaller Map objects.</returns>
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
                    subMap.Index = new Coord(x, y);
                    maps.Add(subMap);
                }
            }
            return maps.AsReadOnly();
        }

        Map GenerateSubMap(int xStart, int yStart, int submapSize)
        {
            int xEnd = ComputeSubMapLengthEndPoint(xStart, submapSize);
            int yEnd = ComputeSubMapWidthEndPoint(yStart, submapSize);
            Map subMap = new Map(xEnd - xStart, yEnd - yStart, SquareSize);
            for (int x = xStart; x < xEnd; x++)
            {
                for (int y = yStart; y < yEnd; y++)
                {
                    subMap[x - xStart, y - yStart] = grid[x, y];
                }
            }
            subMap.Position = new Vector3(xStart * SquareSize, 0f, yStart * SquareSize);
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
        /// <exception cref="System.IndexOutOfRangeException"></exception>
        public bool IsAdjacentToWall(int x, int y)
        {
            return GetAdjacentTiles(x, y).Any(adjTile => grid[x, y] == Tile.Wall);
        }

        /// <summary>
        /// Is the tile adjacent to a wall tile? Assumes tile is a valid map tile (use Contains method to check if 
        /// not sure).
        /// </summary>
        /// <exception cref="System.IndexOutOfRangeException"></exception>
        public bool IsAdjacentToWall(Coord tile)
        {
            return IsAdjacentToWall(tile.x, tile.y);
        }

        /// <summary>
        /// Is the tile adjacent to a wall tile? Assumes the tile is not along the boundary (throws exception otherwise)
        /// so use only if this tile is in the interior of the map.
        /// </summary>
        /// <exception cref="System.IndexOutOfRangeException"></exception>
        public bool IsAdjacentToWallFast(int x, int y)
        {
            Tile[,] grid = this.grid;
            return grid[x - 1, y] == Tile.Wall || grid[x + 1, y] == Tile.Wall 
                || grid[x, y + 1] == Tile.Wall || grid[x, y - 1] == Tile.Wall;
        }

        /// <summary>
        /// Is the tile adjacent to a wall tile? Assumes tile is a valid map tile (use Contains method to check if 
        /// not sure).
        /// </summary>
        /// <exception cref="System.IndexOutOfRangeException"></exception>
        public bool IsAdjacentToFloor(int x, int y)
        {
            return GetAdjacentTiles(x, y).Any(adjTile => grid[x, y] == Tile.Floor);
        }

        /// <summary>
        /// Is the tile adjacent to a wall tile? Assumes tile is a valid map tile (use Contains method to check if 
        /// not sure).
        /// </summary>
        /// <exception cref="System.IndexOutOfRangeException"></exception>
        public bool IsAdjacentToFloor(Coord tile)
        {
            return IsAdjacentToFloor(tile.x, tile.y);
        }

        /// <summary>
        /// Is the tile adjacent to a wall tile? Assumes the tile is not along the boundary (throws exception otherwise)
        /// so use only if this tile is in the interior of the map.
        /// </summary>
        /// <exception cref="System.IndexOutOfRangeException"></exception>
        public bool IsAdjacentToFloorFast(int x, int y)
        {
            Tile[,] grid = this.grid;
            return grid[x - 1, y] == Tile.Floor || grid[x + 1, y] == Tile.Floor
                || grid[x, y + 1] == Tile.Floor || grid[x, y - 1] == Tile.Floor;
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
        /// The number of walls adjacent to the given point. Both horizontal and diagonal tiles count, but the tile itself 
        /// does not. Note that the input coordinates must be contained in the interior of the map (not on the boundary);
        /// </summary>
        /// <returns>Number of walls surrounding the given tile, between 0 and 8 inclusive.</returns>
        /// <exception cref="System.IndexOutOfRangeException"></exception>
        public int GetSurroundingWallCount(int x, int y)
        {
            Tile[,] grid = this.grid;
            return (int)grid[x - 1, y + 1] + (int)grid[x, y + 1] + (int)grid[x + 1, y + 1]  // top-left, top, top-right
                 + (int)grid[x - 1, y    ] +                       (int)grid[x + 1, y    ]  // middle-left, middle-right
                 + (int)grid[x - 1, y - 1] + (int)grid[x, y - 1] + (int)grid[x + 1, y - 1]; // bottom-left, bottom, bottom-right
        }

        /// <summary>
        /// Do the coordinates correspond to the boundary of the map? The boundary is defined as valid points in the map
        /// such that at least one of left, right, up or down step off the map.
        /// </summary>
        public bool IsBoundaryTile(int x, int y)
        {
            return (x == 0 || x == length - 1) && (0 <= y && y <= width - 1) // vertical boundary
                || (y == 0 || y == width - 1) && (0 <= x && x <= length - 1); // horizontal boundary
        }

        /// <summary>
        /// Do the coordinates correspond to the boundary of the map? The boundary is defined as valid points in the map
        /// such that at least one of left, right, up or down step off the map.
        /// </summary>
        public bool IsBoundaryTile(Coord coord)
        {
            return IsBoundaryTile(coord.x, coord.y);
        }

        /// <summary>
        /// Are the coordinates in the interior of the map? The interior is defined as valid points in the map not lying
        /// on the boundary. In particular, all eight neighbouring points of an interior point are valid map coordinates.
        /// </summary>
        public bool IsInteriorTile(int x, int y)
        {
            return (0 < x && x < length - 1) && (0 < y && y < width - 1);
        }

        /// <summary>
        /// Are the coordinates in the interior of the map? The interior is defined as valid points in the map not lying
        /// on the boundary. In particular, all eight neighbouring points of an interior point are valid map coordinates.
        /// </summary>
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
            return GetAdjacentTiles(new Coord(x, y));
        }

        /// <summary>
        /// Gets the horizontally adjacent tiles. Examples:
        /// (1,3) -> (0,3), (2,3), (1,4), (1,2) (assumes map is at least 3 by 5).
        /// (0,0) -> (0,1), (1,0) (assumes map is at least 2 by 2).
        /// (0,1) -> (1,1), (0,0), (0,2) (assumes map is at least (2 by 3).
        /// </summary>
        public IEnumerable<Coord> GetAdjacentTiles(Coord tile)
        {
            if (tile.x > 0)
                yield return tile.Left;
            if (tile.x + 1 < length)
                yield return tile.Right;
            if (tile.y > 0)
                yield return tile.Down;
            if (tile.y + 1 < width)
                yield return tile.Up;
        }

        /// <summary>
        /// Get a 2D byte array of 0s and 1s corresponding to floors and walls respectively. 
        /// </summary>
        public byte[,] ToByteArray()
        {
            byte[,] byteArray = new byte[length, width];
            Array.Copy(grid, byteArray, grid.Length);
            return byteArray;
        }

        /// <exception cref="System.IndexOutOfRangeException"></exception>
        public bool IsFloor(int x, int y)
        {
            return grid[x, y] == Tile.Floor;
        }

        /// <exception cref="System.IndexOutOfRangeException"></exception>
        public bool IsFloor(Coord tile)
        {
            return grid[tile.x, tile.y] == Tile.Floor;
        }

        /// <exception cref="System.IndexOutOfRangeException"></exception>
        public bool IsWall(int x, int y)
        {
            return grid[x, y] == Tile.Wall;
        }

        /// <exception cref="System.IndexOutOfRangeException"></exception>
        public bool IsWall(Coord tile)
        {
            return grid[tile.x,tile.y] == Tile.Wall;
        }
    }
}