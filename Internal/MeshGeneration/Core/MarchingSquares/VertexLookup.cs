/* During map triangulation, we process many triangles that have overlapping vertices. In order to avoid seams, 
 * it's necessary to ensure each triangle with a common vertex points to the same vertex in the vertices array.
 * Keeping a dictionary for every vertex would work, but consumes O(map length * map width) memory. To improve performance
 * we can exploit the fact that when we determine vertices for a single row, those vertices can only be shared by the
 * vertices in the row directly above it or to the right of it. Thus we need only keep track of two rows at a time, 
 * not the entire map. This brings memory down to O(map length).
 */

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Specialized lookup table for the map triangulator using O(map length) memory. Can store and lookup
    /// visited vertex indices row by row. Note that it requires squares to be triangulated row-wise, in increasing
    /// order of x, and each time a row is finished, RowComplete must be called.
    /// </summary>
    sealed class VertexLookup
    {
        VertexIndex?[,] currentRow;
        VertexIndex?[,] previousRow;

        // left side of a square corresponds to 0, 6, 7
        readonly bool[] isOnLeft = new[]
        {
            true,   // topleft
            false,  
            false,
            false,  
            false,  
            false,  
            true,   // bottomleft
            true    // left
        };

        // bottom of a square corresponds to 4, 5, 6
        readonly bool[] isOnBottom = new[]
        {
            false,
            false,
            false,
            false,
            true,   // bottomright
            true,   // bottom
            true,   // bottomleft
            false
        };

        // these two arrays convert a point on a square to the corresponding point to the left or below.

        // 0, 7, 6 to 2, 3, 4 respectively (topleft, left, bottomleft to topright, right, bottomright)
        readonly int[] leftOffset = new[] { 2, -1, -1, -1, -1, -1, 4, 3 };
        // 4, 5, 6 to 2, 1, 0 respectively (bottomright, bottom, bottomleft to topright, top, topleft)
        readonly int[] bottomOffset = new[] { -1, -1, -1, -1, 2, 1, 0, -1 };

        const int perSquareCacheSize = 5;

        public VertexLookup(int rowLength)
        {
            currentRow = new VertexIndex?[perSquareCacheSize, rowLength];
            previousRow = new VertexIndex?[perSquareCacheSize, rowLength];
        }

        /// <summary>
        /// Retrieve the vertex from the cache, if it exists.
        /// </summary>
        /// <param name="vertexIndex">The index associated with this vertex.</param>
        /// <param name="point">The location of the point on the square: an int from 0 to 7.</param>
        /// <param name="x">Location in the current row.</param>
        /// <returns>Bool representing whether the vertex was found.</returns>
        public bool TryGetCachedVertex(int point, int x, out VertexIndex vertexIndex)
        {
            VertexIndex? index = null;
            if (isOnBottom[point])
            {
                index = previousRow[bottomOffset[point], x]; 
            }
            if (!index.HasValue && isOnLeft[point] && x > 0)
            {
                index = currentRow[leftOffset[point], x - 1];
            }
            vertexIndex = index.HasValue ? index.Value : (VertexIndex)0;
            return index.HasValue;
        }

        /// <summary>
        /// Store the vertex into the cache for later retrieval.
        /// </summary>
        /// <param name="vertexIndex">The index associated with this vertex.</param>
        /// <param name="point">The location of the point on the square: an int from 0 to 7.</param>
        /// <param name="x">Location in the current row of squares.</param>
        public void CacheVertex(VertexIndex vertexIndex, int point, int x)
        {
            if (point < perSquareCacheSize)
            {
                currentRow[point, x] = vertexIndex;
            }
        }

        /// <summary>
        /// Call this every time a row is complete, i.e. when iterating the width of the map.
        /// </summary>
        public void FinalizeRow()
        {
            SwapRows();
            ResetBottomRow();
        }

        void SwapRows()
        {
            var temp = currentRow;
            currentRow = previousRow;
            previousRow = temp;
        }

        void ResetBottomRow()
        {
            System.Array.Clear(currentRow, 0, currentRow.Length);
        }
    } 
}
