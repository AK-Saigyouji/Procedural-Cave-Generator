/* A 2d integer vector. Shorts were chosen as the backing integer type as a trade off between performance
 * and flexibility. Coords often have to be used in large collections, where data compression can have a substantial
 * effect on performance, so we want to use the smallest type appropriate for our needs.
 * On the other hand, choosing a byte or sbyte would severely restrict the range of applications for this type.
 * A short is large enough to handle practical map sizes: Unity transforms issue a warning when working with 
 * values much larger than the range of a short.
 *
 * The other non-trivial design decision was to make Coord immutable, in contrast to Vector2. This is in keeping
 * with best practices when dealing with C# structs. The one annoying consequence of this decision is that Unity
 * does not serialize readonly fields, so that has to be worked around when serializing data involving coords.
 * This could be avoided by avoiding the use of read-only and instead offering a get-only property, but coord needs
 * to be used in hot loops where the overhead of the method call would be significant.*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AKSaigyouji
{
    /// <summary>
    /// Similar to Vector2 but for integers, Coord is designed with coordinate grids (2d arrays) in mind. 
    /// Handles values between -32768 and 32767. Unlike Vector2, Coord is immutable.
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
        public Coord LeftShift        { get { return new Coord(x - 1, y); } }

        /// <summary>
        /// Get the coord one unit to the right of this one. (x,y) -> (x+1,y).
        /// </summary>
        public Coord RightShift       { get { return new Coord(x + 1, y); } }

        /// <summary>
        /// Get the coord one unit above this one. (x,y) -> (x,y+1).
        /// </summary>
        public Coord UpShift          { get { return new Coord(x, y + 1); } }

        /// <summary>
        /// Get the coord one unit below this one. (x,y) -> (x,y-1).
        /// </summary>
        public Coord DownShift        { get { return new Coord(x, y - 1); } }

        /// <summary>
        /// Get the coord one unit to the top left. (x,y) -> (x-1,y+1).
        /// </summary>
        public Coord TopLeftShift     { get { return new Coord(x - 1, y + 1); } }

        /// <summary>
        /// Get the coord one unit to the top right. (x,y) -> (x+1,y+1).
        /// </summary>
        public Coord TopRightShift    { get { return new Coord(x + 1, y + 1); } }

        /// <summary>
        /// Get the coord one unit to the bottom right. (x,y) -> (x+1,y-1).
        /// </summary>
        public Coord BottomRightShift { get { return new Coord(x + 1, y - 1); } }

        /// <summary>
        /// Get the coord one unit to the bottom left. (x,y) -> (x-1,y-1).
        /// </summary>
        public Coord BottomLeftShift  { get { return new Coord(x - 1, y - 1); } }

        /// <summary>
        /// (0,0)
        /// </summary>
        public static Coord zero = new Coord(0, 0);

        /// <summary>
        /// (1,1)
        /// </summary>
        public static Coord one = new Coord(1, 1);

        /// <summary>
        /// (-1,0)
        /// </summary>
        public static Coord left = new Coord(-1, 0);

        /// <summary>
        /// (1,0)
        /// </summary>
        public static Coord right = new Coord(1, 0);

        /// <summary>
        /// (0,1)
        /// </summary>
        public static Coord up = new Coord(0, 1);

        /// <summary>
        /// (0,-1)
        /// </summary>
        public static Coord down = new Coord(0, -1);

        /// <summary>
        /// (1,1)
        /// </summary>
        public static Coord topRight = new Coord(1, 1);

        /// <summary>
        /// (1,-1)
        /// </summary>
        public static Coord bottomRight = new Coord(1, -1);

        /// <summary>
        /// (-1,1)
        /// </summary>
        public static Coord topLeft = new Coord(-1, 1);

        /// <summary>
        /// (-1,-1)
        /// </summary>
        public static Coord bottomLeft = new Coord(-1, -1);

        /// <summary>
        /// Returns coordinates corresponding to absolute values of these coordinates. e.g. (1,-3) -> (1,3).
        /// </summary>
        public Coord AbsoluteValues()
        {
            return new Coord(Math.Abs(x), Math.Abs(y));
        }

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
        /// <para>(2,3).SupNormDistance(5,105) -> 102, since 105 - 3 = 102.</para>
        /// <para>(2,2).SupNormDistance(1,1) -> 1, since 2 - 1 = 1.</para>
        /// </summary>
        /// <returns>Returns the maximum of the absolute difference between individual dimensions.</returns>
        public int SupNormDistance(Coord otherTile)
        {
            return Mathf.Max(Mathf.Abs(x - otherTile.x), Mathf.Abs(y - otherTile.y));
        }

        // The next three methods all return a custom struct-based enumerable which can be consumed with a foreach.
        // The custom struct implementations avoid allocating when enumerating and are also significantly faster
        // than the implementations automatically generated when using the yield return syntax.

        /// <summary>
        /// Gets the four horizontal/vertical neighbours around this coordinate (left, right, above, below).
        /// </summary>
        public NeighbourEnumerable GetFourNeighbours()
        {
            return new NeighbourEnumerable(this, 4);
        }

        /// <summary>
        /// Gets all eight neighbours around this coordinate.
        /// </summary>
        public NeighbourEnumerable GetEightNeighbours()
        {
            return new NeighbourEnumerable(this, 8);
        }

        /// <summary>
        /// Enumerates the coordinates in a straight line between this coord (inclusive) and the other coord (inclusive).
        /// </summary>
        public LineEnumerable GetLineTo(Coord other)
        {
            return new LineEnumerable(this, other);
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

        public static implicit operator Vector3(Coord tile)
        {
            // This conforms to how Vector2 implicitly casts to a Vector3
            return new Vector3(tile.x, tile.y, 0f);
        }

        // Going from vector to coord requires an explicit cast, as loss of data is possible.
        public static explicit operator Coord(Vector2 vector)
        {
            return new Coord((int)vector.x, (int)vector.y);
        }

        public static explicit operator Coord(Vector3 vector)
        {
            return new Coord((int)vector.x, (int)vector.y);
        }

        public override string ToString()
        {
            return string.Format("({0},{1})", x, y);
        }

        /// <summary>
        /// Allows for enumeration over the neighbours of a coord.
        /// </summary>
        public struct NeighbourEnumerable : IEnumerable<Coord>
        {
            readonly Coord center;
            readonly int numNeighbours;

            public NeighbourEnumerable(Coord center, int numNeighbours)
            {
                this.center = center;
                this.numNeighbours = numNeighbours;
            }

            public NeighbourEnumerator GetEnumerator()
            {
                return new NeighbourEnumerator(center, numNeighbours);
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

        public struct NeighbourEnumerator : IEnumerator<Coord>
        {
            int index;
            readonly int maxIndex;
            short xCenter;
            short yCenter;

            // These encode the 8 neighbours of (0, 0). First the horizontal/vertical, then the diagonals.
            // The reason for this seemingly convoluted approach is to avoid using a heap-allocated collection
            // and also to avoid branching to determine which neighbour to retrieve based on the index variable.
            const long ENCODED_X = 2 << 12 ^ 2 << 10 ^ 1 << 6 ^ 2 << 4 ^ 1 << 2;
            const long ENCODED_Y = 2 << 10 ^ 2 << 8 ^ 1 << 4 ^ 2 << 2 ^ 1;

            public NeighbourEnumerator(Coord center, int numNeighbours)
            {
                index = -2;
                maxIndex = numNeighbours * 2;
                xCenter = center.x;
                yCenter = center.y;
            }

            public Coord Current
            {
                get
                {
                    if (index < 0)
                        throw new InvalidOperationException();

                    return new Coord(xCenter + (int)((ENCODED_X >> index) & 3) - 1, yCenter + (int)((ENCODED_Y >> index) & 3) - 1);
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
                index += 2;
                return index < maxIndex;
            }

            public void Reset()
            {
                index = -2;
            }
        }

        /// <summary>
        /// Enumerates a line of coordinates between two points.
        /// </summary>
        public struct LineEnumerable : IEnumerable<Coord>
        {
            readonly Coord start, end;

            public LineEnumerable(Coord start, Coord end)
            {
                this.start = start;
                this.end = end;
            }

            public LineEnumerator GetEnumerator()
            {
                return new LineEnumerator(start, end);
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

        public struct LineEnumerator : IEnumerator<Coord>
        {
            readonly int maxIndex;
            readonly float xIncrementor, yIncrementor;
            readonly short xStart, yStart;

            int index;

            public LineEnumerator(Coord start, Coord end)
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