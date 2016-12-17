/*The purpose of this data structure is to efficiently compute the number of walls in a rectangular subgrid.
It does so in constant time by exploiting a well known dynamic programming algorithm for computing
submatrix sums. Note that construction takes time and space linear in the size of the original grid. */

using UnityEngine.Assertions;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Given a grid of walls, handles queries about the number of walls in a given subgrid in constant time.
    /// </summary>
    sealed class WallCache
    {
        // wallSum[x,y] corresponds to the number of walls on lattice points between (0,0) and (x,y) inclusive.
        // e.g. wallSum[5,17] is the sum of all grid[x,y] for 0 <= x <= 5 and 0 <= y <= 17. 
        readonly int[,] wallSums;
        readonly int length;
        readonly int width;

        public WallCache(WallGrid grid)
        {
            Assert.IsNotNull(grid);
            length = grid.Length;
            width = grid.Width;
            wallSums = new int[length, width];
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    wallSums[x, y] = grid[x, y] + GetWallSum(x - 1, y) + GetWallSum(x, y - 1) - GetWallSum(x - 1, y - 1);
                }
            }
        }

        /// <summary>
        /// Count the number of walls in the box given by integrals boundaries. Boundaries are inclusive. e.g.
        /// CountWallsInBox(3,5,0,1) will return the number of walls among the points (0,3), (0,4), (0,5), (1,3),
        /// (1,4), (1,5). 
        /// </summary>
        public int CountWallsInBox(int bottom, int top, int left, int right)
        {
            Assert.IsTrue(bottom >= 0 && left >= 0 && top < width && right < length);

            return GetWallSum(right, top) 
                - GetWallSum(left - 1, top) 
                - GetWallSum(right, bottom - 1) 
                + GetWallSum(left - 1, bottom - 1);
        }

        /// <summary>
        /// Does the given box contain a wall? Boundaries are inclusive. e.g. CountWallsInBox(3,5,0,1) will return 
        /// the number of walls among the points (0,3), (0,4), (0,5), (1,3), (1,4), (1,5). 
        /// </summary>
        public bool BoxContainsWall(int bottom, int top, int left, int right)
        {
            return CountWallsInBox(bottom, top, left, right) > 0;
        }

        /// <summary>
        /// Get the wall sum at the coordinates in question, or else return 0 if a coordinate is negative.
        /// Simplifies the formula to compute a subrectangle sum. 
        /// </summary>
        int GetWallSum(int x, int y)
        {
            return x >= 0 && y >= 0 ? wallSums[x, y] : 0;
        }
    } 
}