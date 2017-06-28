/* A 2d integer vector. Shorts were chosen as the backing integer type as a trade off between performance
 and flexibility. Coords often have to be used in large collections, where data compression can have a substantial
 affect on performance, so choosing the smallest appropriate integer type can affect performance significantly.
 On the other hand, choosing a byte or sbyte would severely restrict the applicability of this type.
 A short is large enough to handle practical map sizes: Unity transforms issue a warning when working with 
 values much larger than the range of a short.
 
  The other non-trivial design decision was to make Coord immutable, in contrast to Vector2. This is in keeping
 with best practices when dealing with C# structs. The one annoying consequence of this decision is that Unity
 does not serialize readonly fields, so that has to be worked around when serializing data involving coords.*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AKSaigyouji.Maps
{
    /// <summary>
    /// Similar to Vector2 but for integers, Coord is designed with coordinate grids (2d arrays) in mind. 
    /// Handles coordinates between -32768 and 32767. Unlike Vector2, Coord is immutable.
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
        public Coord Left        { get { return new Coord(x - 1, y); } }

        /// <summary>
        /// Get the coord one unit to the right of this one. (x,y) -> (x+1,y).
        /// </summary>
        public Coord Right       { get { return new Coord(x + 1, y); } }

        /// <summary>
        /// Get the coord one unit above this one. (x,y) -> (x,y+1).
        /// </summary>
        public Coord Up          { get { return new Coord(x, y + 1); } }

        /// <summary>
        /// Get the coord one unit below this one. (x,y) -> (x,y-1).
        /// </summary>
        public Coord Down        { get { return new Coord(x, y - 1); } }

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
        public static Coord Zero { get { return new Coord(0, 0); } }

        /// <summary>
        /// (1,1).
        /// </summary>
        public static Coord One { get { return new Coord(1, 1); } }

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

        public Enumerable GetLineTo(Coord other)
        {
            // Uses a custom struct-based enumerable meant to be consumed with a foreach.
            return new Enumerable(this, other);
        }

        public static Coord operator *(int c, Coord tile)
        {
            return new Coord(c * tile.x, c * tile.y);
        }

        public static Coord operator +(Coord tileA, Coord tileB)
        {
            return new Coord(tileA.x + tileB.x, tileA.y + tileB.y);
        }

        public static Coord operator -(Coord tileA, Coord tileB)
        {
            return new Coord(tileA.x - tileB.x, tileA.y - tileB.y);
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

        /// <summary>
        /// Enumerates a line of coordinates between two points.
        /// </summary>
        public struct Enumerable : IEnumerable<Coord>
        {
            readonly Coord start, end;

            public Enumerable(Coord start, Coord end)
            {
                this.start = start;
                this.end = end;
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(start, end);
            }

            IEnumerator<Coord> IEnumerable<Coord>.GetEnumerator()
            {
                return GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public struct Enumerator : IEnumerator<Coord>
        {
            readonly int maxIndex;
            readonly float xIncrementor, yIncrementor;
            readonly short xStart, yStart;

            int index;

            public Enumerator(Coord start, Coord end)
            {
                xStart = start.x;
                yStart = start.y;
                int xDelta = end.x - start.x;
                int yDelta = end.y - start.y;
                maxIndex = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(yDelta));
                Vector2 incrementor = new Vector2(xDelta, yDelta) / maxIndex;
                xIncrementor = incrementor.x;
                yIncrementor = incrementor.y;
                index = -1;
            }

            public Coord Current
            {
                get
                {
                    if (index < 0)
                        throw new InvalidOperationException();

                    int xOffset = (int)(index * xIncrementor);
                    int yOffset = (int)(index * yIncrementor);
                    return new Coord(xStart + xOffset, yStart + yOffset);
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public void Dispose()
            {

            }

            public bool MoveNext()
            {
                index++;
                return index <= maxIndex;
            }

            public void Reset()
            {
                index = -1;
            }
        }
    }
}