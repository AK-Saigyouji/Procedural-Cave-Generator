﻿using System;

namespace MapHelpers
{
    /// <summary>
    /// Coord is an integer equivalent of Vector2. 
    /// </summary>
    public struct Coord
    {
        public int x;
        public int y;

        public Coord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Get the squared distance between this coordinate and the given one. Faster than computing the actual distance.
        /// </summary>
        /// <returns>Returns the squared distance between Coords.</returns>
        public int SquaredDistance(Coord otherTile)
        {
            int xDelta = x - otherTile.x;
            int yDelta = y - otherTile.y;
            return xDelta * xDelta + yDelta * yDelta;
        }


        /// <summary>
        /// Get the distance between this coordinate and given one as given by the supremum norm. Examples:
        /// (2,3).SupNormDistance(5,105) -> 102
        /// (2,2).SupNormDistance(1,1) -> 1
        /// </summary>
        /// <returns>Returns the maximum of the absolute difference between individual dimensions.</returns>
        public int SupNormDistance(Coord otherTile)
        {
            return Math.Max(Math.Abs(x - otherTile.x), Math.Abs(y - otherTile.y));
        }

        public static bool operator ==(Coord tileA, Coord tileB)
        {
            return tileA.x == tileB.x && tileA.y == tileB.y;
        }

        public static bool operator !=(Coord tileA, Coord tileB)
        {
            return !(tileA == tileB);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Coord))
            {
                return false;
            }
            Coord p = (Coord)obj;
            return (x == p.x) && (y == p.y);
        }

        public bool Equals(Coord p)
        {
            return (x == p.x) && (y == p.y);
        }

        public override int GetHashCode()
        {
            return x ^ y;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", x, y);
        }
    }
}