using UnityEngine;
using System;

namespace CaveGeneration.MeshGeneration
{
    struct LocalPosition: IEquatable<LocalPosition>
    {
        readonly int id;

        readonly static byte[] xOffsets = new byte[] { 0, 1, 2, 2, 2, 1, 0, 0 };
        readonly static byte[] yOffsets = new byte[] { 2, 2, 2, 1, 0, 0, 0, 1 };

        /// <summary>
        /// Represent a new position on a square grid of at most 255 by 255.
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
            int rawY = id & 511; // takes first 9 bits
            int rawX = id >> 9; // drops last 9 bits
            return new Vector3(scale * (basePosition.x + rawX * 0.5f), 0f, scale * (basePosition.z + rawY * 0.5f));
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