/* When converting maps with 1-unit passages into 3d geometry, it was found that they could snag player colliders
 that were exactly 1 unit in radius, and in some cases flat out block the player. This could prove gamebreaking, and
 forcing users of this system to make their agents at least a little smaller than 1 unit was a bit onerous. 
 
  Attempts at trying to create a algorithm to expand tunnels by n for arbitrary integers n proved problematic. 
 They would either be too aggressive, or leave certain cases unaddressed, and generally scaled poorly. Ultimately I 
 decided on a simple implementation which can only expand to a width of 2, but which handles that case very well (no
 unaddressed edge cases, good performance). 
 
  If one requires larger passages then either they can raise the scale, or else use a different algorithm.*/

using AKSaigyouji.ArrayExtensions;
using AKSaigyouji.Maps;
using System;

namespace AKSaigyouji.GraphAlgorithms
{
    /// <summary>
    /// Class dedicated to enlarging floors within a map so that larger objects can navigate the map.
    /// </summary>
    public sealed class PassageEnlarger
    {
        readonly bool[,] cachedBoolArray;

        readonly int length;
        readonly int width;

        // Clockwise loop around the origin, starting at the top left.
        readonly Coord[] loop = new[]
        {
            Coord.zero.TopLeftShift, Coord.zero.UpShift, Coord.zero.TopRightShift, Coord.zero.RightShift,
            Coord.zero.BottomLeftShift, Coord.zero.DownShift, Coord.zero.BottomLeftShift, Coord.zero.LeftShift
        };

        public PassageEnlarger(int length, int width)
        {
            this.length = length;
            this.width = width;
            cachedBoolArray = new bool[length, width];
        }

        public void ExpandFloors(Map map)
        {
            if (map.Length != length || map.Width != width)
                throw new ArgumentException("Map dimensions must match dimensions passed to constructor.");

            bool[,] toExpand = cachedBoolArray;
            toExpand.TransformInterior((x, y) => IsPassage(map, x, y));
            toExpand.ForEach(
                action:    (x, y) => ClearMap(map, new Coord(x, y)), 
                predicate: (x, y) => toExpand[x, y]
            );
        }

        /// <summary>
        /// Does this square look like a passage? Must only be applied to interior points.
        /// </summary>
        bool IsPassage(Map map, int x, int y)
        {
            if (map[x, y] == Tile.Wall)
            {
                return false;
            }
            Coord center = new Coord(x, y);
            // This is the number of times we change between floor and wall in a circular loop around the point.
            int numChanges = 0;
            for (int i = 1; i < loop.Length; i++)
            {
                if (map[center + loop[i - 1]] != map[center + loop[i]])
                {
                    numChanges++;
                }
            }
            return numChanges > 2;
        }

        void ClearMap(Map map, Coord center)
        {
            Boundary bdry = new Boundary(1, map.Length - 2, 1, map.Width - 2);
            foreach (Coord offset in loop)
            {
                Coord coord = center + offset;
                if (bdry.IsInBounds(coord))
                {
                    map[coord] = Tile.Floor;
                }
            }
        }
    } 
}