﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CaveGeneration.MapGeneration
{
    public enum Tile : byte
    {
        Floor = 0,
        Wall = 1
    }

    /// <summary>
    /// The 2D grid-based Map. Points in the map are given by integer pairs like a 2d array. Each point is either 
    /// a floor or wall tile. 
    /// </summary>
    public sealed class Map
    {
        public int SquareSize { get; private set; }
        public int Length { get { return length; } }
        public int Width { get { return width; } }
        public Vector3 Position { get; private set; } 
        public Coord Index { get; private set; }

        public const int maxSubmapSize = 150; // Chunk size when the map is broken up.

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
            Tile[,] grid = this.grid;
            for (int x = 0; x < length; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    grid[x, y] = tiles[x, y];
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
        public bool IsAdjacentToWall(int x, int y)
        {
            return GetAdjacentTiles(x, y).Any(adjTile => grid[x, y] == Tile.Wall);
        }

        /// <summary>
        /// Is the tile adjacent to a wall tile? Assumes tile is a valid map tile (use Contains method to check if 
        /// not sure).
        /// </summary>
        public bool IsAdjacentToWall(Coord tile)
        {
            return IsAdjacentToWall(tile.x, tile.y);
        }

        /// <summary>
        /// Is the tile adjacent to a wall tile? Assumes the tile is not along the boundary (throws exception otherwise)
        /// so use only if this tile is in the interior of the map.
        /// </summary>
        public bool IsAdjacentToWallFast(int x, int y)
        {
            Tile[,] grid = this.grid;
            return grid[x - 1, y] == Tile.Wall || grid[x + 1, y] == Tile.Wall 
                || grid[x, y + 1] == Tile.Wall || grid[x, y - 1] == Tile.Wall;
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
        /// The number of walls adjacent to the given point. Both horizontal and diagonal tiles count, but the tile itself 
        /// does not. Note that the input coordinates must be contained in the interior of the map (not on the boundary);
        /// </summary>
        /// <returns>Number of walls surrounding the given tile, between 0 and 8 inclusive.</returns>
        public int GetSurroundingWallCount(int x, int y)
        {
            Tile[,] grid = this.grid;
            return (int)grid[x - 1, y + 1] + (int)grid[x, y + 1] + (int)grid[x + 1, y + 1] // top-left, top, top-right
                + (int)grid[x - 1, y] + (int)grid[x + 1, y]                                // middle-left, middle-right
                + (int)grid[x - 1, y - 1] + (int)grid[x, y - 1] + (int)grid[x + 1, y - 1]; // bottom-left, bottom, bottom-right
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
                yield return tile.left;
            if (tile.x + 1 < length)
                yield return tile.right;
            if (tile.y > 0)
                yield return tile.down;
            if (tile.y + 1 < width)
                yield return tile.up;
        }

        /// <summary>
        /// Extract a read-only Grid object from the map to represent the locations of walls and floors.
        /// </summary>
        public Grid ToGrid()
        {
            return new Grid(grid);
        }

        /// <summary>
        /// Get a 2D byte array of 0s and 1s corresponding to floors and walls respectively. 
        /// </summary>
        public byte[,] ToByteArray()
        {
            byte[,] byteArray = new byte[length, width];
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    byteArray[x, y] = (byte)grid[x, y];
                }
            }
            return byteArray;
        }
    }
}