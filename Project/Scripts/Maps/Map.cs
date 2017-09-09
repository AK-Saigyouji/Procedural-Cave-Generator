/* This class represents a 2D grid of Tiles, and offers low level functionality for this purpose. 
 
 Textures are serializable Unity objects, and are very convenient vehicles for serialized maps for several reasons.
 One is that visualization of textures is provided natively by Unity. Another is that it means maps can be edited by
 any paint program. A third is that PNG files use a compression algorithm well-suited to compressing a 2D map. 
 Even if we pack each tile into a single bit and serialize the bits, we'd end up with a file 10x the size of the
 corresponding PNG. */

using AKSaigyouji.ArrayExtensions;
using System;
using UnityEngine;

namespace AKSaigyouji.Maps
{
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
            other.ForEach((x, y) => grid[x + xOffset, yOffset] = other[x, y]);
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
            grid.SetAll(tileType);
        }

        /// <summary>
        /// Deep clone.
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
            return 0 > x || x >= length || 0 > y || y >= width || grid[x, y] == Tile.Wall;
        }

        /// <summary>
        /// Returns true if the coordinates correspond to anything other than a valid floor tile. Differs from
        /// IsWall by returning true for out of range coordinates.
        /// </summary>
        public bool IsWallOrVoid(Coord coord)
        {
            return IsWallOrVoid(coord.x, coord.y);
        }

        /// <summary>
        /// Converts the map to a texture. Walls are converted to black, floors to white.
        /// </summary>
        public Texture2D ToTexture()
        {
            var texture = new Texture2D(length, width, TextureFormat.RGB24, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Point;
            var colors = new Color32[length * width];
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    colors[y * length + x] = IsWall(x, y) ? Color.black : Color.white;
                }
            }
            texture.SetPixels32(colors);
            texture.Apply(updateMipmaps: false, makeNoLongerReadable: false);
            return texture;
        }

        /// <summary>
        /// Generate a Map from the pixels in the texture. Black will be converted to walls, everything else
        /// will be converted to floors.
        /// </summary>
        public static Map FromTexture(Texture2D texture)
        {
            if (texture == null)
                throw new ArgumentNullException("texture");

            int length = texture.width;
            int width = texture.height;
            Map map = new Map(length, width);
            Tile[,] grid = map.grid;
            Color[] pixels = texture.GetPixels();
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    grid[x, y] = (pixels[y * length + x] == Color.black ? Tile.Wall : Tile.Floor);
                }
            }
            return map;
        }

        #region FunctionalExtensions

        /// <exception cref="ArgumentNullException"></exception>
        public void ForEach(Action<int, int> action)
        {
            grid.ForEach(action);
        }

        /// <exception cref="ArgumentNullException"></exception>
        public void ForEachBoundary(Action<int, int> action)
        {
            grid.ForEachBoundary(action);
        }

        /// <exception cref="ArgumentNullException"></exception>
        public void ForEachInterior(Action<int, int> action)
        {
            grid.ForEachInterior(action);
        }

        /// <exception cref="ArgumentNullException"></exception>
        public void ForEach(Action<int, int> action, Func<int, int, bool> predicate)
        {
            grid.ForEach(action, predicate);
        }

        /// <exception cref="ArgumentNullException"></exception>
        public void ForEachBoundary(Action<int, int> action, Func<int, int, bool> predicate)
        {
            grid.ForEachBoundary(action, predicate);
        }

        /// <exception cref="ArgumentNullException"></exception>
        public void ForEachInterior(Action<int, int> action, Func<int, int, bool> predicate)
        {
            grid.ForEachInterior(action, predicate);
        }

        /// <exception cref="ArgumentNullException"></exception>
        public void Transform(Func<int, int, Tile> transformation)
        {
            grid.Transform(transformation);
        }

        /// <exception cref="ArgumentNullException"></exception>
        public void TransformBoundary(Func<int, int, Tile> transformation)
        {
            grid.TransformBoundary(transformation);
        }

        /// <exception cref="ArgumentNullException"></exception>
        public void TransformInterior(Func<int, int, Tile> transformation)
        {
            grid.TransformInterior(transformation);
        }

        /// <exception cref="ArgumentNullException"></exception>
        public void Transform(Func<int, int, Tile> transformation, Func<int, int, bool> predicate)
        {
            grid.Transform(transformation, predicate);
        }

        /// <exception cref="ArgumentNullException"></exception>
        public void TransformBoundary(Func<int, int, Tile> transformation, Func<int, int, bool> predicate)
        {
            grid.TransformBoundary(transformation, predicate);
        }

        /// <exception cref="ArgumentNullException"></exception>
        public void TransformInterior(Func<int, int, Tile> transformation, Func<int, int, bool> predicate)
        {
            grid.TransformInterior(transformation, predicate);
        }

        #endregion
    }
}