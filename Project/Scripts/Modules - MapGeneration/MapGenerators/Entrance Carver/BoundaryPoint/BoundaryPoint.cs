/* This class represents a point on the boundary of a map. While this could have been modelled by a simple Coord, 
 that would require specifying an arbitrary pair of values, making it easy to miss the boundary by one. It's much 
 easier to specify which side of the boundary the point is on (left, right, top or bottom) and then specificy how 
 far along the boundary the point should be.
 
  This implementation is actually Map-agnostic in the sense of that can represents a point on the boundary of any
 map big enough to accommodate the magnitude value. It can be translated to a concrete Coord value by passing
 it a Boundary object.*/

using System;
using UnityEngine;
using UnityEngine.Assertions;
using AKSaigyouji.Maps;

namespace AKSaigyouji.Modules.MapGeneration
{
    [Serializable]
    public struct BoundaryPoint
    {
        /// <summary>
        /// On which side of the map is this boundary point on?
        /// </summary>
        public enum Side : byte
        {
            Top, Right, Bottom, Left
        };

        public Side BoundarySide { get { return side; } }

        /// <summary>
        /// How many units along the side this point lies. Always goes from left to right, from bottom to top. 
        /// </summary>
        public int Magnitude { get { return magnitude; } }

        [SerializeField] Side side;
        [SerializeField] int magnitude;

        public BoundaryPoint(Side side, int magnitude)
        {
            if (magnitude < 0)
                throw new ArgumentOutOfRangeException("magnitude");

            if (!IsValidSide(side))
                throw new System.ComponentModel.InvalidEnumArgumentException();

            this.side = side;
            this.magnitude = magnitude;
        }

        /// <summary>
        /// Translate to the corresponding coordinates for the given boundary. 
        /// </summary>
        /// <param name="boundary">Must be large enough to accommodate the boundary point. e.g. if magnitude is 
        /// 60 and side is left, then the boundary's yMax must be at least 60.</param>
        public Coord ToCoord(Boundary boundary)
        {
            if (boundary.xMax < magnitude && (BoundarySide == Side.Bottom || BoundarySide == Side.Top))
                throw new ArgumentException("Boundary length too small for this boundary point.");

            if (boundary.yMax < magnitude && (BoundarySide == Side.Left || BoundarySide == Side.Right))
                throw new ArgumentException("Boundary width too small for this boundary point.");

            switch (BoundarySide)
            {
                case Side.Top:
                    return new Coord(magnitude, boundary.yMax);
                case Side.Right:
                    return new Coord(boundary.xMax, magnitude);
                case Side.Bottom:
                    return new Coord(magnitude, boundary.yMin);
                case Side.Left:
                    return new Coord(boundary.xMin, magnitude);
                default:
                    throw new ArgumentException("Boundary point has an unrecognized 'side'.");
            }
        }

        /// <summary>
        /// Point lies on bottom or top. Is true if and only if IsVertical is false.
        /// </summary>
        public bool IsHorizontal()
        {
            bool result = side == Side.Bottom || side == Side.Top;
            return result;
        }

        /// <summary>
        /// Point lies on right or left. Is true if and only if IsHorizontal is false;
        /// </summary>
        public bool IsVertical()
        {
            bool result = side == Side.Left || side == Side.Right;
            return result;
        }

        public override string ToString()
        {
            return string.Format("{0} units along the boundary on the {1}", magnitude, side);
        }

        /// <summary>
        /// Equivalent to Enum.IsDefined, but doesn't use reflection.
        /// </summary>
        static bool IsValidSide(Side side)
        {
            return side == Side.Bottom || side == Side.Top || side == Side.Left || side == Side.Right;
        }
    } 
}