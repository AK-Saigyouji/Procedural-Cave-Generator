/* This class represents a 2D grid of Tiles, and has functionality focused on facilitating the generation
of such a grid. 
 
 A major design decision was the use of array type for the underlying grid. The three obvious choices
are a 2D array, a jagged array, and a flat array. Initially 2D arrays were chosen for maximal readability. 
The other two options were tested, but found to offer no significant performance improvement. Thus 2D arrays remain,
despite normally offering significantly worse performance.
 */

using System;

namespace CaveGeneration.MapGeneration
{
    public enum Tile : byte
    {
        Floor = 0,
        Wall = 1
    }

    [Serializable]
    /// <summary>
    /// The 2D grid-based Map. Points in the map are given by integer pairs like a 2d array. Each point is either 
    /// a floor or wall tile. Offers a variety of methods tailored to map construction.
    /// </summary>
    public sealed class Map
    {
        public int Length { get { return length; } }
        public int Width { get { return width; } }
        public Coord Index { get { return index; } private set { index = value; } }

        Tile[,] grid;

        int length;
        int width;
        Coord index;

        public Map(int length, int width)
        {
            grid = new Tile[length, width];
            this.length = length;
            this.width = width;
        }

        public Map(int length, int width, Coord index) : this(length, width)
        {
            Index = index;
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
        /// Copies the tiles from the other map. Maps must have the same dimensions (length and width).
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public void Copy(Map other)
        {
            if (other == null) throw new ArgumentNullException();
            if (length != other.length || width != other.width)
                throw new ArgumentException("Cannot copy map with different dimensions!");

            Copy(other.grid);
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
        /// Create a new map with the same values, leaving the original unaltered.
        /// </summary>
        public Map Clone()
        {
            Map clone = new Map(length, width);
            clone.Copy(this);
            return clone;
        }

        /// <summary>
        /// Is the tile adjacent to a wall tile? Assumes the tile is not along the boundary (throws exception otherwise)
        /// so use only if this tile is in the interior of the map.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public bool IsAdjacentToWallFast(int x, int y)
        {
            return grid[x - 1, y] == Tile.Wall || grid[x + 1, y] == Tile.Wall 
                || grid[x, y + 1] == Tile.Wall || grid[x, y - 1] == Tile.Wall;
        }

        /// <summary>
        /// Is the tile adjacent to a wall tile? Assumes the tile is not along the boundary (throws exception otherwise)
        /// so use only if this tile is in the interior of the map.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public bool IsAdjacentToFloorFast(int x, int y)
        {
            return grid[x - 1, y] == Tile.Floor || grid[x + 1, y] == Tile.Floor
                || grid[x, y + 1] == Tile.Floor || grid[x, y - 1] == Tile.Floor;
        }

        /// <summary>
        /// Does the map contain these coordinates? Equivalently, will map[x,y] return a tile?
        /// </summary>
        public bool Contains(int x, int y)
        {
            return 0 <= x && x < length && 0 <= y && y < width;
        }

        /// <summary>
        /// Does the map contain these coordinates? Equivalently, will map[coord] return a tile?
        /// </summary>
        public bool Contains(Coord coord)
        {
            return Contains(coord.x, coord.y);
        }

        /// <summary>
        /// The number of walls adjacent to the given point. Both horizontal/vertical and diagonal tiles count.
        /// Note that the input coordinates must be contained in the interior of the map (not on the boundary);
        /// </summary>
        /// <returns>Number of walls surrounding the given tile, between 0 and 8 inclusive.</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public int GetSurroundingWallCount(int x, int y)
        {
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
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    bools[x, y] = grid[x, y] == tileType;
                }
            }
            return bools;
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
            return grid[tile.x,tile.y] == Tile.Wall;
        }

        /// <summary>
        /// Perform an action for each tile.
        /// </summary>
        public void ForEach(Action<int, int> action)
        {
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
        public void ForEachBoundary(Action<int, int> action)
        {
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
        public void ForEachInterior(Action<int, int> action)
        {
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
        public void Transform(Func<int, int, Tile> transformation)
        {
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
        public void TransformBoundary(Func<int, int, Tile> transformation)
        {
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
        public void TransformInterior(Func<int, int, Tile> transformation)
        {
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
        public void Transform(Func<int, int, Tile> transformation, Func<int, int, bool> predicate)
        {
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
        public void TransformBoundary(Func<int, int, Tile> transformation, Func<int, int, bool> predicate)
        {
            for (int x = 0; x < length; x++)
            {
                if (predicate(x, 0))          grid[x, 0] = transformation(x, 0);
                if (predicate(x, width - 1))  grid[x, width - 1] = transformation(x, width - 1);
            }
            for (int y = 1; y < width - 1; y++)
            {
                if (predicate(0, y))          grid[0, y] = transformation(0, y);
                if (predicate(length - 1, y)) grid[length - 1, y] = transformation(length - 1, y);
            }
        }

        /// <summary>
        /// Reassigns every interior tile satisfying the provided predicate, using the provided transformation.
        /// </summary>
        public void TransformInterior(Func<int, int, Tile> transformation, Func<int, int, bool> predicate)
        {
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
    }
}