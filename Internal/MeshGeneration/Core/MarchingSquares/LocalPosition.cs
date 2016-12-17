/* This struct is used exclusively by the MapTriangulator as a compact representation of a relative (local) position in the
 * grid of squares as they're being triangulated. Since the Mesh Generator requires grids to be 255 by 255 or less,
 * we can store arbitrary positions in the grid of squares using just three bytes: one for the x coordinate, one for the
 * y coordinate, and one to track which of the 8 positions in the square (corners and midpoints) the point occupies. 
 * Previously such positions were represented with Vector2s, which took 8 bytes instead of 3. */

using UnityEngine;

namespace CaveGeneration.MeshGeneration
{
    struct LocalPosition
    {
        // These correspond to the 8 positions on the unit square, with 0 being topleft and going clockwise.
        static readonly Vector2[] positionOffsets = new Vector2[]
        {
            new Vector2(0f, 1f),
            new Vector2(0.5f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 0.5f),
            new Vector2(1f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0.5f)
        };

        readonly byte x;
        readonly byte y;
        readonly byte squarePoint;

        /// <summary>
        /// Represent a new position on a square grid of at most 255 by 255.
        /// </summary>
        /// <param name="x">Position along first dimension, must be between 0 and 255 inclusive.</param>
        /// <param name="y">Position along second dimension, must be between 0 and 255 inclusive.</param>
        /// <param name="squarePoint">Position on square as defined by the Map Triangulator, from 0 to 7 inclusive.</param>
        public LocalPosition(int x, int y, int squarePoint)
        {
            this.x = (byte)x;
            this.y = (byte)y;
            this.squarePoint = (byte)squarePoint;
        }

        /// <summary>
        /// Retrieve position as a Vector3.
        /// </summary>
        public Vector3 ToVector3()
        {
            Vector2 offset = positionOffsets[squarePoint];
            return new Vector3(x + offset.x, 0f, y + offset.y);
        }
    } 
}