/* This class is used to store the core implementation of the marching squares algorithm. Specifically, how to
 * triangulate a square based on which of its corners are walls, and an efficient function for querying whether
 * a point intersects a wall in a given configuration.
 * This class could be served well by making use of polymorphism, treating each configuration as a subclass of a 
 * configuration base class: although this would make it much easier to work with, the performance penalty for
 * the indirection would be fairly severe in this case. 
 */

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AKSaigyouji.MeshGeneration
{
    [Flags]
    enum Square
    {
        None = 0,
        TopLeft = 1,
        Top = 2,
        TopRight = 4,
        Right = 8,
        BotRight = 16,
        Bot = 32,
        BotLeft = 64,
        Left = 128,
    }

    static class MarchingSquares
    {
        public const int MAX_VERTICES_IN_TRIANGULATION = 6;

        const Square AllEightPoints = (Square)0xFF;

        const Square BottomLeftTriangle  = Square.Bot | Square.BotLeft  | Square.Left;
        const Square BottomRightTriangle = Square.Bot | Square.BotRight | Square.Right;
        const Square TopRightTriangle    = Square.Top | Square.TopRight | Square.Right;
        const Square TopLeftTriangle     = Square.Top | Square.TopLeft  | Square.Left;

        const Square BottomHalf = Square.BotRight | Square.Left | Square.Right | Square.BotLeft;
        const Square RightHalf  = Square.BotRight | Square.Top  | Square.Bot   | Square.TopRight;
        const Square TopHalf    = Square.TopRight | Square.Left | Square.Right | Square.TopLeft;
        const Square LeftHalf   = Square.TopLeft  | Square.Bot  | Square.Top   | Square.BotLeft;

        const Square AllButTopLeft  = Square.Top | Square.TopRight | Square.BotRight | Square.BotLeft | Square.Left;
        const Square AllButTopRight = Square.Top | Square.BotRight | Square.BotLeft  | Square.TopLeft | Square.Right;
        const Square AllButBotRight = Square.Bot | Square.TopRight | Square.BotLeft  | Square.TopLeft | Square.Right;
        const Square AllButBotLeft  = Square.Bot | Square.TopRight | Square.BotRight | Square.TopLeft | Square.Left;

        const Square TopRightAndBotLeft = TopRightTriangle | BottomLeftTriangle;
        const Square TopLeftAndBotRight = TopLeftTriangle  | BottomRightTriangle;

        const Square FullSquare = Square.TopLeft | Square.TopRight | Square.BotRight | Square.BotLeft;

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
            // (empty and full) are handled first.
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
                    throw new ArgumentOutOfRangeException("Configuration must be between 0 and 15 inclusive.", "configuration");
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

        public static byte[][] BuildConfigurationTable()
        {
            // This may seem like a peculiar way to define this method, but is done to maximize readability without
            // relying on comments and to avoid storing data in static fields. 
            return new Square[]
            {
                Square.None,              //  0
                BottomLeftTriangle,       //  1
                BottomRightTriangle,      //  2
                BottomHalf,               //  3
                TopRightTriangle,         //  4
                TopRightAndBotLeft,       //  5
                RightHalf,                //  6
                AllButTopLeft,            //  7
                TopLeftTriangle,          //  8 
                LeftHalf,                 //  9
                TopLeftAndBotRight,       // 10
                AllButTopRight,           // 11
                TopHalf,                  // 12
                AllButBotRight,           // 13
                AllButBotLeft,            // 14
                FullSquare                // 15
            }.Select(UnpackSquare).ToArray();
        }

        static byte[] UnpackSquare(Square square)
        {
            var unpackedSquare = new List<byte>();
            byte squareValue = 1;
            for (byte i = 0; i < 8; i++, squareValue *= 2)
            {
                if ((square & (Square)squareValue) > 0)
                {
                    unpackedSquare.Add(i);
                }
            }
            return unpackedSquare.ToArray();
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