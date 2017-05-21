using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace CaveGeneration.MapGeneration
{
    /// <summary>
    /// Represents a 2D boundary with integral coordinates. 
    /// </summary>
    public struct Boundary
    {
        public readonly int xMin, yMin, xMax, yMax;

        /// <summary>
        /// Create a boundary for a 2d array with the corresponding length and width.
        /// </summary>
        public Boundary(int length, int width) : this(0, length - 1, 0, width - 1)
        {

        }

        /// <summary>
        /// Create new boundary: all coordinates are inclusive. 
        /// </summary>
        public Boundary(int xMin, int xMax, int yMin, int yMax)
        {
            if (xMin > xMax) throw new ArgumentException("Minimum boundary cannot exceed maximum.");
            if (yMin > yMax) throw new ArgumentException("Minimum boundary cannot exceed maximum.");

            this.xMin = xMin;
            this.xMax = xMax;
            this.yMin = yMin;
            this.yMax = yMax;
        }

        public bool IsInBounds(Coord coord)
        {
            return xMin <= coord.x && coord.x <= xMax && yMin <= coord.y && coord.y <= yMax;
        }

        public bool IsInBounds(int x, int y)
        {
            return xMin <= x && x <= xMax && yMin <= y && y <= yMax;
        }
    } 
}