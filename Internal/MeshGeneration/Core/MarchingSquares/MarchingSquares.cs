/* This class is used to store the core implementation of the marching squares algorithm. Specifically, how to
 * triangulate a square based on which of its corners are walls, and an efficient function for querying whether
 * a point intersects a wall in a given configuration.
 * This class could be served well by making use of polymorphism, treating each configuration as a subclass of a 
 * configuration base class: although this would make it much easier to work with, the performance penalty for
 * the indirection would be fairly severe in this case. 
 */

using UnityEngine;
using System;

namespace CaveGeneration.MeshGeneration
{
    static class MarchingSquares
    {
        public const int MAX_VERTICES_IN_TRIANGULATION = 6;

        /// <summary>
        /// Does the point intersect a triangle in the given configuration for a unit square? i.e. if we 
        /// triangulate a square of the given configuration type, will the point intersect a triangle in 
        /// that triangulation? 
        /// </summary>
        /// <param name="point">A point in the unit square from (0,0) to (1,1). Unpredictable result if point is 
        /// outside this range.</param>
        /// <param name="configuration">A square configuration (int from 0 to 15).</param>
        public static bool IntersectsTriangle(Vector2 point, int configuration)
        {
            float x = point.x;
            float y = point.y;
            // This is likely to be compiler-optimized with a jump table, but just in case, the two most common cases 
            // (empty and full) are handled first).
            switch (configuration)
            {
                case 0:
                    return false;
                case 15:
                    return true;
                case 1:
                    return x + y <= 0.5f;
                case 2:
                    return x >= y + 0.5f;
                case 3:
                    return y <= 0.5f;
                case 4:
                    return x + y >= 1.5f;
                case 5:
                    return x >= y + 0.5f || y >= 0.5f + x;
                case 6:
                    return x >= 0.5f;
                case 7:
                    return y <= 0.5f + x;
                case 8:
                    return y >= 0.5f + x;
                case 9:
                    return x <= 0.5f;
                case 10:
                    return x + y <= 0.5f || x + y >= 1.5f;
                case 11:
                    return x + y <= 1.5f;
                case 12:
                    return y >= 0.5f;
                case 13:
                    return x <= y + 0.5f;
                case 14:
                    return x + y >= 0.5f;
                default:
                    throw new ArgumentException("Configuration must be between 0 and 15 inclusive.", "configuration");
            }
        }

        /// <summary>
        /// Compute the marching squares configuration for the given square. Arguments with values other than
        /// 0 or 1 will have unpredictable results.
        /// </summary>
        public static int ComputeConfiguration(byte botLeft, byte botRight, byte topRight, byte topLeft)
        {
            return botLeft + 2 * botRight + 4 * topRight + 8 * topLeft;
        }

        /// <summary>
        /// Returns a jagged array where the first variable corresponds to configuration (0 to 15 inclusive) and 
        /// the second the points in the triangulation for that square, from 0 (top left) to 7 (top mid) inclusive.
        /// </summary>
        public static byte[][] BuildConfigurationTable()
        {
            return new byte[][]
            {
                new byte[] { },                 //  0: empty
                new byte[] {5, 6, 7 },          //  1: bottom-left triangle
                new byte[] {3, 4, 5 },          //  2: bottom-right triangle
                new byte[] {3, 4, 6, 7 },       //  3: bottom half
                new byte[] {1, 2, 3 },          //  4: top-right triangle
                new byte[] {1, 2, 3, 5, 6, 7 }, //  5: all but top-left and bottom-right triangles
                new byte[] {1, 2, 4, 5 },       //  6: right half
                new byte[] {1, 2, 4, 6, 7 },    //  7: all but top-left triangle
                new byte[] {0, 1, 7 },          //  8: top-left triangle
                new byte[] {0, 1, 5, 6 },       //  9: left half
                new byte[] {0, 1, 3, 4, 5, 7 }, // 10: all but bottom-left and top-right
                new byte[] {0, 1, 3, 4, 6 },    // 11: all but top-right
                new byte[] {0, 2, 3, 7 },       // 12: top half
                new byte[] {0, 2, 3, 5, 6 },    // 13: all but bottom-right
                new byte[] {0, 2, 4, 5, 7 },    // 14: all but bottom-left
                new byte[] {0, 2, 4, 6}         // 15: full square
            };
        }

        public static byte[,] ComputeConfigurations(WallGrid wallGrid)
        {
            int length = wallGrid.Length - 1;
            int width = wallGrid.Width - 1;
            byte[,] configurations = new byte[length, width];
            byte[,] grid = wallGrid.ToByteArray();
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    configurations[x, y] = (byte)GetConfiguration(grid, x, y);
                }
            }
            return configurations;
        }

        static int GetConfiguration(byte[,] grid, int x, int y)
        {
            byte botLeft = grid[x, y];
            byte botRight = grid[x + 1, y];
            byte topRight = grid[x + 1, y + 1];
            byte topLeft = grid[x, y + 1];
            return ComputeConfiguration(botLeft, botRight, topRight, topLeft);
        }
    } 
}