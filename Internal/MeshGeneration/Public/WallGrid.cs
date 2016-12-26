using UnityEngine;
using System.Collections;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Readonly representation of data associated with the map from which meshes should be built, 
    /// including position, scale and location of walls. Floors are 0s, walls are 1s.
    /// </summary>
    public sealed class WallGrid
    {
        public int     Scale    { get; private set; }
        public int     Length   { get; private set; }
        public int     Width    { get; private set; }
        public Vector3 Position { get; private set; }

        readonly byte[,] walls;

        /// <summary>
        /// Create a new wall grid out of a 2d array of 0s and 1s. 
        /// </summary>
        /// <param name="walls">Must contain 0s corresponding to floors and 1s corresponding to walls.</param>
        /// <param name="position">Location in world coordinates of coordinate (0,0) in the walls array. Mainly used to ensure
        /// chunks line up if breaking a grid into multiple pieces.</param>
        public WallGrid(byte[,] walls, Vector3 position, int scale = 1)
        {
            ValidateWalls(walls);

            Position = position;
            Scale = scale;
            Length = walls.GetLength(0);
            Width = walls.GetLength(1);
            this.walls = (byte[,])walls.Clone();
        }

        public byte this[int x, int y]
        {
            get { return walls[x, y]; }
        }

        /// <summary>
        /// Create a copy grid whose values are all inverted, i.e. 1s and 0s are flipped.
        /// </summary>
        public WallGrid Invert()
        {
            byte[,] newWalls = new byte[Length, Width];
            for (int x = 0; x < Length; x++)
            {
                for (int y = 0; y < Width; y++)
                {
                    newWalls[x, y] = (byte)((walls[x, y] + 1) & 1); // flips 0 and 1. About three times as fast as using conditionals.
                }
            }
            return new WallGrid(newWalls, Position, Scale);
        }

        void ValidateWalls(byte[,] walls)
        {
            if (walls == null)
                throw new System.ArgumentNullException("walls");

            int length = walls.GetLength(0);
            int width = walls.GetLength(1);
            int accumulator = 0;
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    accumulator |= walls[x, y]; // Accumulator will be > 1 if wall[x,y] is > 1 for any x, y.
                }
            }
            bool wallsContainsOnlyZerosAndOnes = accumulator < 2;
            if (!wallsContainsOnlyZerosAndOnes)
            {
                throw new System.ArgumentException("Must contain only 0s and 1s.", "walls");
            }
        }
    } 
}
