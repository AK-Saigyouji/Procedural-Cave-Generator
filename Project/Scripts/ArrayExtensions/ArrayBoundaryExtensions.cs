/* This class provides some helpers for boundary-checking on grids (2D arrays). Having to write out four explicit checks
 * every time we wish to check if a coordinate is valid becomes tedious and error prone:
 * 0 <= x && x < arr.GetLength(0) && 0 <= y && y < arr.GetLength(1). Furthermore, when working with grids it's often
 * important to know whether a point is in the interior of the grid. */

namespace AKSaigyouji.ArrayExtensions
{
    public static class ArrayBoundaryExtensions
    {
        /// <summary>
        /// Are these coordinates in-bounds for this grid?
        /// </summary>
        public static bool AreValidCoords<T>(this T[,] arr, int x, int y)
        {
            return 0 <= x && x < arr.GetLength(0) && 0 <= y && y < arr.GetLength(1);
        }

        /// <summary>
        /// Are these coordinates contained in the interior of this grid?
        /// </summary>
        public static bool AreValidInteriorCoords<T>(this T[,] arr, int x, int y)
        {
            return 0 < x && 0 < arr.GetLength(0) - 1 && 0 < y && y < arr.GetLength(1);
        }
    }
}