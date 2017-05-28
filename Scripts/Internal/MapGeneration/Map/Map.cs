/* This class represents a 2D grid of Tiles, and offers low level functionality for this purpose.*/

using System;
using UnityEngine;

namespace CaveGeneration.MapGeneration
{
    public enum Tile : byte
    {
        Floor = 0,
        Wall = 1
    }

    [Serializable]
    /// <summary>
    /// A 2D grid-based Map. Points in the map are given by integer pairs like a 2d array. Each point is either 
    /// a floor or wall tile. Offers a variety of methods tailored to map construction.
    /// </summary>
    public sealed class Map
    {
        public int Length { get { return length; } }
        public int Width { get { return width; } }

        readonly Tile[,] grid;

        readonly int length;
        readonly int width;
        
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Map(int length, int width)
        {
            if (length < 0) throw new ArgumentOutOfRangeException("length");
            if (width < 0) throw new ArgumentOutOfRangeException("width");

            grid = new Tile[length, width];
            this.length = length;
            this.width = width;
        }

        /// <exception cref="IndexOutOfRangeException"></exception>
        public Tile this[int x, int y]
        {
            get { return grid[x, y]; }
            set { grid[x, y] = value; }
        }

        /// <exception cref="IndexOutOfRangeException"></exception>
        public Tile this[Coord tile]
        {
            get { return grid[tile.x, tile.y]; }
            set { grid[tile.x, tile.y] = value; }
        }

        /// <summary>
        /// Copies the tiles from the given map. Maps must have the same dimensions (length and width).
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public void Copy(Map other)
        {
            if (other == null)
                throw new ArgumentNullException();

            if (length != other.length || width != other.width)
                throw new ArgumentException("Cannot copy map with different dimensions!");

            Copy(other.grid);
        }

        /// <summary>
        /// Copy the values from the other map into this one, with the given offset. i.e. for each (x, y) in 
        /// other's bounds, we take this[x + offset.x, y + offset.y] = other[x, y].
        /// </summary>
        public void CopyRegion(Map other, Coord offset)
        {
            if (other == null)
                throw new ArgumentNullException();

            if (length < other.Length + offset.x || width < other.Width + offset.y)
                throw new ArgumentOutOfRangeException("Other does not fit into this map with this offset.");

            if (offset.x < 0 || offset.y < 0)
                throw new ArgumentOutOfRangeException("Cannot have negative offset.");

            int xOffset = offset.x;
            int yOffset = offset.y;
            for (int y = 0; y < other.Width; y++)
            {
                for (int x = 0; x < other.Length; x++)
                {
                    grid[x + xOffset, y + yOffset] = other[x, y];
                }
            }
        }

        void Copy(Tile[,] other)
        {
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    grid[x, y] = other[x, y];
                }
            }
        }

        /// <summary>
        /// Fill the entire map with the given type of Tile. 
        /// </summary>
        public void Fill(Tile tileType)
        {
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    grid[x, y] = tileType;
                }
            }
        }

        /// <summary>
        /// Set the 9 points around the given center (inclusive) to floors. Works on edge tiles.
        /// </summary>
        public void CarveSquare(int xCenter, int yCenter)
        {
            int xMin = Mathf.Max(0, xCenter - 1);
            int yMin = Mathf.Max(0, yCenter - 1);
            int xMax = Mathf.Min(length - 1, xCenter + 1);
            int yMax = Mathf.Min(width - 1, yCenter + 1);
            for (int y = yMin; y <= yMax; y++)
            {
                for (int x = xMin; x <= xMax; x++)
                {
                    grid[x, y] = Tile.Floor;
                }
            }
        }

        /// <summary>
        /// Set the 9 points around the given center (inclusive) to floors. Works on edge tiles.
        /// </summary>
        public void CarveSquare(Coord center)
        {
            CarveSquare(center.x, center.y);
        }

        /// <summary>
        /// Create a new map with the same values, leaving the original unaltered.
        /// </summary>
        public Map Clone()
        {
            Map clone = new Map(length, width);
            clone.Copy(this);
            return clone;
        }

        /// <summary>
        /// Is the tile adjacent to a wall tile?
        /// </summary>
        public bool IsAdjacentToWall(int x, int y)
        {
            return (x > 0          && grid[x - 1, y] == Tile.Wall)
                || (y > 0          && grid[x, y - 1] == Tile.Wall)
                || (x + 1 < length && grid[x + 1, y] == Tile.Wall)
                || (y + 1 < width  && grid[x, y + 1] == Tile.Wall);
        }

        /// <summary>
        /// The number of walls adjacent to the given point, including the point itself. 
        /// Both horizontal/vertical and diagonal tiles count.
        /// Note that the input coordinates must be contained in the interior of the map (not on the boundary);
        /// </summary>
        /// <returns>Number of walls surrounding the given tile, between 0 and 9 inclusive.</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public int GetSurroundingWallCount(int x, int y)
        {
            // Unrolling the double loop like this makes a big performance difference with the current compiler,
            // though with a good optimizer it shouldn't matter.
            return (int)grid[x - 1, y + 1] + (int)grid[x, y + 1] + (int)grid[x + 1, y + 1]
                 + (int)grid[x - 1, y    ] + (int)grid[x, y    ] + (int)grid[x + 1, y    ]
                 + (int)grid[x - 1, y - 1] + (int)grid[x, y - 1] + (int)grid[x + 1, y - 1];
        }

        /// <summary>
        /// Get a 2D byte array of 0s and 1s corresponding to floors and walls respectively. 
        /// </summary>
        public byte[,] ToByteArray()
        {
            return (byte[,])grid.Clone();
        }

        /// <summary>
        /// Get a 2D bool array indicating the location of tiles of the given type. e.g. ToBoolArray(Tile.Floor) will
        /// return a 2D bool array, with dimensions (Length, Width) with true at (x, y) if and only if the map
        /// has a floor tile at (x, y).
        /// </summary>
        public bool[,] ToBoolArray(Tile tileType)
        {
            bool[,] bools = new bool[length, width];
            ToBoolArray(tileType, bools);
            return bools;
        }

        /// <summary>
        /// Instead of returning a new array, reads values into the given boolean grid. Length and width of boolean
        /// grid must match the map.
        /// </summary>
        public void ToBoolArray(Tile tileType, bool[,] boolGrid)
        {
            if (boolGrid == null)
                throw new ArgumentNullException("boolGrid");

            if (length != boolGrid.GetLength(0) || width != boolGrid.GetLength(1))
                throw new ArgumentException("Boolean grid's size does not match map.");

            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    boolGrid[x, y] = grid[x, y] == tileType;
                }
            }
        }

        /// <exception cref="IndexOutOfRangeException"></exception>
        public bool IsFloor(int x, int y)
        {
            return grid[x, y] == Tile.Floor;
        }

        /// <exception cref="IndexOutOfRangeException"></exception>
        public bool IsFloor(Coord tile)
        {
            return grid[tile.x, tile.y] == Tile.Floor;
        }

        /// <exception cref="IndexOutOfRangeException"></exception>
        public bool IsWall(int x, int y)
        {
            return grid[x, y] == Tile.Wall;
        }

        /// <exception cref="IndexOutOfRangeException"></exception>
        public bool IsWall(Coord tile)
        {
            return grid[tile.x, tile.y] == Tile.Wall;
        }

        /// <summary>
        /// Returns true if the coordinates correspond to anything other than a valid floor tile. Differs from
        /// IsWall by returning true for out of range coordinates.
        /// </summary>
        public bool IsWallOrVoid(int x, int y)
        {
            return 0 > x || x > length - 1 || 0 > y || y > width - 1 || grid[x, y] == Tile.Wall;
        }

        public bool IsWallOrVoid(Coord coord)
        {
            return IsWallOrVoid(coord.x, coord.y);
        }

        #region FunctionalExtensions

        /// <summary>
        /// Perform an action for each tile.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public void ForEach(Action<int, int> action)
        {
            ThrowIfNull(action);

            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    action(x, y);
                }
            }
        }

        /// <summary>
        /// Perform an action for each boundary tile.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public void ForEachBoundary(Action<int, int> action)
        {
            ThrowIfNull(action);

            for (int x = 0; x < length; x++)
            {
                action(x, 0);
                action(x, width - 1);
            }
            for (int y = 1; y < width - 1; y++)
            {
                action(0, y);
                action(length - 1, y);
            }
        }

        /// <summary>
        /// Perform an action for each interior tile.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public void ForEachInterior(Action<int, int> action)
        {
            ThrowIfNull(action);

            for (int y = 1; y < width - 1; y++)
            {
                for (int x = 1; x < length - 1; x++)
                {
                    action(x, y);
                }
            }
        }

        /// <summary>
        /// Reassigns every tile in the map using the provided function.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public void Transform(Func<int, int, Tile> transformation)
        {
            ThrowIfNull(transformation);

            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    grid[x, y] = transformation(x, y);
                }
            }
        }

        /// <summary>
        /// Reassigns every boundary tile in the map using the provided function.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public void TransformBoundary(Func<int, int, Tile> transformation)
        {
            ThrowIfNull(transformation);

            for (int x = 0; x < length; x++)
            {
                grid[x, 0] = transformation(x, 0);
                grid[x, width - 1] = transformation(x, width - 1);
            }
            for (int y = 1; y < width - 1; y++)
            {
                grid[0, y] = transformation(0, y);
                grid[length - 1, y] = transformation(length - 1, y);
            }
        }

        /// <summary>
        /// Reassigns every interior tile in the map using the provided function.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public void TransformInterior(Func<int, int, Tile> transformation)
        {
            ThrowIfNull(transformation);

            for (int y = 1; y < width - 1; y++)
            {
                for (int x = 1; x < length - 1; x++)
                {
                    grid[x, y] = transformation(x, y);
                }
            }
        }

        /// <summary>
        /// Reassigns every tile that satisfies the provided predicate, using the provided transformation.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public void Transform(Func<int, int, Tile> transformation, Func<int, int, bool> predicate)
        {
            ThrowIfNull(transformation);
            ThrowIfNull(predicate);

            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    if (predicate(x, y))
                    {
                        grid[x, y] = transformation(x, y);
                    }
                }
            }
        }

        /// <summary>
        /// Reassigns every boundary tile that satisfies the provided predicate, using the provided transformation.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public void TransformBoundary(Func<int, int, Tile> transformation, Func<int, int, bool> predicate)
        {
            ThrowIfNull(transformation);
            ThrowIfNull(predicate);

            for (int x = 0; x < length; x++)
            {
                if (predicate(x, 0))          grid[x, 0] = transformation(x, 0);
                if (predicate(x, width - 1))  grid[x, width - 1] = transformation(x, width - 1);
            }
            for (int y = 1; y < width - 1; y++) // adjust boundaries so we don't double-visit 0,0 and length-1,width-1
            {
                if (predicate(0, y))          grid[0, y] = transformation(0, y);
                if (predicate(length - 1, y)) grid[length - 1, y] = transformation(length - 1, y);
            }
        }

        /// <summary>
        /// Reassigns every interior tile satisfying the provided predicate, using the provided transformation.
        /// </summary
        /// <exception cref="ArgumentNullException"></exception>
        public void TransformInterior(Func<int, int, Tile> transformation, Func<int, int, bool> predicate)
        {
            ThrowIfNull(transformation);
            ThrowIfNull(predicate);

            for (int y = 1; y < width - 1; y++)
            {
                for (int x = 1; x < length - 1; x++)
                {
                    if (predicate(x, y))
                    {
                        grid[x, y] = transformation(x, y);
                    }
                }
            }
        }

        #endregion

        #region ExceptionHelpers

        void ThrowIfNull(Func<int, int, Tile> transformation)
        {
            if (transformation == null)
                throw new ArgumentNullException("transformation");
        }

        void ThrowIfNull(Func<int, int, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException("predicate");
        }

        void ThrowIfNull(Action<int, int> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");
        }

        #endregion
    }
}