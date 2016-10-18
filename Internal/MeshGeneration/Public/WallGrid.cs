using UnityEngine;
using System.Collections;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Represents data associated with the map from which meshes should be built, including position, scale and location of
    /// walls.
    /// </summary>
    public sealed class WallGrid
    {
        public int Scale { get; private set; }
        public Vector3 Position { get; private set; }
        public int Length { get; private set; }
        public int Width { get; private set; }

        readonly byte[,] walls;

        /// <summary>
        /// Create a new wall grid out of a 2d array of 0s and 1s.
        /// </summary>
        /// <param name="walls">Must contains 0s corresponding to floors and 1s corresponding to walls.</param>
        /// <param name="position">Location in world coordinates of coordinate (0,0) in the walls array. Mainly used to ensure
        /// chunks line up if breaking a grid into multiple pieces.</param>
        public WallGrid(byte[,] walls, Vector3 position, int scale = 1)
        {
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
        /// Create a copy grid whose values are all inverted.
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
    } 
}
