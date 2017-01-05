/* This class is used to store the core implementation of the marching squares algorithm. Specifically, how to
 * triangulate a square based on which of its corners are walls, and an efficient function for querying whether
 * a point intersects a wall in a given configuration. */

using UnityEngine;

namespace CaveGeneration.MeshGeneration
{
    static class MarchingSquares
    {
        // The lookup table for the marching squares algorithm. The eight points in the square are enumerated from 0 to 7, 
        // starting in the top left corner and going clockwise (see visual below). Based on the sixteen possible configurations 
        // for the corners of a square, this table returns the points needed to triangulate that square.
        // 0 1 2
        // 7 - 3
        // 6 5 4
        static byte[][] configurationTable = new byte[][]
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

        // wallTests[config](x, y) tells whether the point (x, y) intersects a triangle in the given configuration
        static System.Func<float, float, bool>[] wallTests = new System.Func<float, float, bool>[]
        {
            (x, y) => false,
            (x, y) => x + y <= 0.5f,
            (x, y) => x     >= y + 0.5f,
            (x, y) => y     <= 0.5f,
            (x, y) => x + y >= 1.5f,
            (x, y) =>  wallTests[2](x, y) || wallTests[8](x, y),
            (x, y) => x     >= 0.5f,
            (x, y) => !wallTests[8](x, y),
            (x, y) => y     >= 0.5f + x,
            (x, y) => x     <= 0.5f,
            (x, y) =>  wallTests[1](x, y) || wallTests[4](x, y),
            (x, y) => !wallTests[4](x, y),
            (x, y) => y     >= 0.5f,
            (x, y) => !wallTests[2](x, y),
            (x, y) => !wallTests[1](x, y),
            (x, y) => true
        };

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
            return wallTests[configuration](point.x, point.y);
        }

        /// <summary>
        /// Compute the marching squares configuration for the given square.
        /// </summary>
        public static int ComputeConfiguration(byte botLeft, byte botRight, byte topRight, byte topLeft)
        {
            return botLeft + 2 * botRight + 4 * topRight + 8 * topLeft;
        }

        /// <summary>
        /// Get a compact array of integers indicating how to triangulate the square. 
        /// </summary>
        public static byte[] GetPoints(int configuration)
        {
            return configurationTable[configuration];
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