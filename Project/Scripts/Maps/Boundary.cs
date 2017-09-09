using System;
using System.Collections.Generic;

namespace AKSaigyouji.Maps
{
    /// <summary>
    /// Represents a 2D boundary with integral coordinates. 
    /// </summary>
    public struct Boundary
    {
        public Coord BotLeft { get { return new Coord(xMin, yMin); } }
        public Coord TopLeft { get { return new Coord(xMin, yMax); } }
        public Coord BotRight { get { return new Coord(xMax, yMin); } }
        public Coord TopRight { get { return new Coord(xMax, yMax); } }

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
            if (xMin > xMax || yMin > yMax)
                throw new ArgumentException("Minimum boundary cannot exceed maximum.");

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

        /// <summary>
        /// Gets all the coords that run along the boundary. 
        /// </summary>
        public IEnumerable<Coord> GetAllCoords()
        {
            for (int x = xMin; x <= xMax; x++)
            {
                yield return new Coord(x, 0);
                yield return new Coord(x, yMax);
            }
            for (int y = yMin + 1; y < yMax; y++)
            {
                yield return new Coord(0, y);
                yield return new Coord(xMax, y);
            }
        }

        public override string ToString()
        {
            return string.Format("Boundary between {0} and {1} (inclusive).", BotLeft, TopRight);
        }
    } 
}