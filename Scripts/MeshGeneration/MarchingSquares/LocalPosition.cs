/* This is a compact representation of a point (x,y) where x,y are floating point numbers
 between 0 and 255 inclusive such that non-integer values can only be multiples of 0.5. e.g. (3.5,9) is possible
 but (3.3,9) is not. 
 
  To store such a point, x and y are both multiplied by 2 to get integers between 0 and 511.
 Since they can each be stored in 9 bits, x gets shifted over 9 bits to the left, and XORed with y to produce
 an integer: this is the representation. To recover the integers, a bitmask can be used to recover y with an AND 
 operation, while x is recovered by shifting 9 bits to the right. In both cases it's necessary to then divide by 2
 to recover the original float values.
 
  This is used in the context of marching squares triangulations, where we have integral x,y corresponding to
 a square in a grid, and a number 0-7 representing the position on the square. The representation was chosen to be fast
 to convert to and from, and also to allow for extremely efficient hashcode and equality checks without storing
 unnecessary data. By storing a single unique int to represent the struct, the hashcode and equality checks 
 literally cannot be faster. 

  An important fact about this representation is that different arguments can lead to the same LocalPosition, 
 corresponding to the fact that, for example, the top left corner of a given square in the grid is also the 
 bottom right corner of another square. 
 
  The end result is that compared to the obvious alternative (using Vector2 to hold the values) we use 4 bytes
 instead of 8 for each struct, we get dramatically faster hash code and equality checks, and we avoid 
 floating point arithmetic until we need to retrieve Vector3s for the mesh data.
 
  It is an interesting side note that C# dictionaries and hash sets both use extra memory for each entry
 to store hashcodes to avoid computing them on each query. For this and other reasons, specialized implementations
 of these collections could outperform the built-in implementations by quite a bit, though as of this writing,
 the performance gains would not be sufficient to warrant the added complexity.*/

using UnityEngine;
using System;

namespace AKSaigyouji.MeshGeneration
{
    struct LocalPosition: IEquatable<LocalPosition>
    {
        readonly int id;

        readonly static byte[] xOffsets = new byte[] { 0, 1, 2, 2, 2, 1, 0, 0 };
        readonly static byte[] yOffsets = new byte[] { 2, 2, 2, 1, 0, 0, 0, 1 };

        /// <summary>
        /// Represent a position on a square grid of at most 255 by 255.
        /// </summary>
        /// <param name="x">Position along first dimension, must be between 0 and 255 inclusive.</param>
        /// <param name="y">Position along second dimension, must be between 0 and 255 inclusive.</param>
        /// <param name="squarePoint">Position on square as defined by the Map Triangulator, from 0 to 7 inclusive.</param>
        public LocalPosition(int x, int y, int squarePoint)
        { 
            id = ((2 * x + xOffsets[squarePoint]) << 9) ^ (2 * y + yOffsets[squarePoint]);
        }

        /// <summary>
        /// Retrieve position as a Vector3.
        /// </summary>
        public Vector3 ToGlobalPosition(int scale, Vector3 basePosition)
        {
            int rawY = id & 511;
            int rawX = id >> 9;
            return new Vector3(basePosition.x + scale * rawX * 0.5f, 0f, basePosition.z + scale * rawY * 0.5f);
        }

        public bool Equals(LocalPosition other)
        {
            return id == other.id;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is LocalPosition))
            {
                return false;
            }
            else
            {
                return Equals((LocalPosition)obj);
            }
        }

        public override int GetHashCode()
        {
            return id;
        }

        public static bool operator ==(LocalPosition a, LocalPosition b)
        {
            return a.id == b.id;
        }

        public static bool operator !=(LocalPosition a, LocalPosition b)
        {
            return a.id != b.id;
        }
    } 
}