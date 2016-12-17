using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaveGeneration.MapGeneration
{
    /// <summary>
    /// Similar to Vector2 but for integers, Coord is designed with coordinate grids (2d arrays) in mind. 
    /// Handles coordinates between -32768 and 32767.
    /// </summary>
    public struct Coord : IEquatable<Coord>
    {
        public readonly short x;
        public readonly short y;

        public Coord(int x, int y)
        {
            this.x = (short)x;
            this.y = (short)y;
        }

        /// <summary>
        /// Get the coord one unit to the left of this one. (x,y) -> (x-1,y).
        /// </summary>
        public Coord Left        { get { return new Coord(x - 1, y    ); } }

        /// <summary>
        /// Get the coord one unit to the right of this one. (x,y) -> (x+1,y).
        /// </summary>
        public Coord Right       { get { return new Coord(x + 1, y    ); } }

        /// <summary>
        /// Get the coord one unit above this one. (x,y) -> (x,y+1).
        /// </summary>
        public Coord Up          { get { return new Coord(x    , y + 1); } }

        /// <summary>
        /// Get the coord one unit below this one. (x,y) -> (x,y-1).
        /// </summary>
        public Coord Down        { get { return new Coord(x    , y - 1); } }

        /// <summary>
        /// Get the coord one unit to the top left. (x,y) -> (x-1,y+1).
        /// </summary>
        public Coord TopLeft     { get { return new Coord(x - 1, y + 1); } }

        /// <summary>
        /// Get the coord one unit to the top right. (x,y) -> (x+1,y+1).
        /// </summary>
        public Coord TopRight    { get { return new Coord(x + 1, y + 1); } }

        /// <summary>
        /// Get the coord one unit to the bottom right. (x,y) -> (x+1,y-1).
        /// </summary>
        public Coord BottomRight { get { return new Coord(x + 1, y - 1); } }

        /// <summary>
        /// Get the coord one unit to the bottom left. (x,y) -> (x-1,y-1).
        /// </summary>
        public Coord BottomLeft  { get { return new Coord(x - 1, y - 1); } }

        /// <summary>
        /// (0,0).
        /// </summary>
        public static readonly Coord zero = new Coord(0, 0);

        /// <summary>
        /// (1,1).
        /// </summary>
        public static readonly Coord one  = new Coord(1, 1);

        /// <summary>
        /// Get the Euclidean distance between this coordinate and the given one.
        /// </summary>
        public float Distance(Coord otherTile)
        {
            return Mathf.Sqrt(SquaredDistance(otherTile));
        }

        /// <summary>
        /// Get the squared Euclidean distance between this coordinate and the given one. Preserves ordering by distance, 
        /// but is faster to compute than actual Euclidean distance.
        /// </summary>
        public int SquaredDistance(Coord otherTile)
        {
            int xDelta = x - otherTile.x;
            int yDelta = y - otherTile.y;
            return xDelta * xDelta + yDelta * yDelta;
        }

        /// <summary>
        /// Get the distance between this coordinate and given one as given by the supremum norm. Examples:
        /// (2,3).SupNormDistance(5,105) -> 102, since 105 - 3 = 102.
        /// (2,2).SupNormDistance(1,1) -> 1, since 2 - 1 = 1.
        /// </summary>
        /// <returns>Returns the maximum of the absolute difference between individual dimensions.</returns>
        public int SupNormDistance(Coord otherTile)
        {
            return Mathf.Max(Mathf.Abs(x - otherTile.x), Mathf.Abs(y - otherTile.y));
        }

        /// <summary>
        /// Generate a list of coordinates representing a path beween the given coordinates (inclusive). 
        /// </summary>
        /// <returns>List of Coords between this and the other coord (inclusive).</returns>
        public List<Coord> GetLineTo(Coord other)
        {
            Vector2 startVector = new Vector2(x, y);
            List<Coord> line = new List<Coord>();

            int xDelta = other.x - x;
            int yDelta = other.y - y;
            int numIterations = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(yDelta));
            Vector2 incrementor = new Vector2(xDelta, yDelta) / numIterations;

            for (int i = 0; i <= numIterations; i++)
            {
                Vector2 nextVector = startVector + i * incrementor;
                line.Add((Coord)nextVector);
            }

            return line;
        }

        public static bool operator ==(Coord tileA, Coord tileB)
        {
            return tileA.x == tileB.x && tileA.y == tileB.y;
        }

        public static bool operator !=(Coord tileA, Coord tileB)
        {
            return tileA.x != tileB.x || tileA.y != tileB.y;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Coord))
            {
                return false;
            }
            Coord p = (Coord)obj;
            return Equals(p);
        }

        public bool Equals(Coord p)
        {
            return (x == p.x) && (y == p.y);
        }

        public override int GetHashCode()
        {
            // Since Coord is a pair of 16 bit objects, 32 bits is exactly enough for a bijective mapping between 
            // Coords and hash codes.
            return (x << 16) ^ y;
        }

        public static implicit operator Vector2(Coord tile)
        {
            return new Vector2(tile.x, tile.y);
        }

        // Going from vector to coord requires an explicit cast, as loss of data is possible.
        public static explicit operator Coord(Vector2 vector)
        {
            return new Coord((int)vector.x, (int)vector.y);
        }

        public override string ToString()
        {
            return string.Format("({0},{1})", x, y);
        }
    }
}