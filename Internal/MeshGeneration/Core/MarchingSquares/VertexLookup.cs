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
    /// order of x.
    /// </summary>
    sealed class VertexLookup
    {
        VertexIndex?[,] currentRow;
        VertexIndex?[,] previousRow;

        int rowLength;
        const int perSquareCacheSize = 5;

        public VertexLookup(int rowLength)
        {
            this.rowLength = rowLength;
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
            if (IsPointOnBottomOfSquare(point))
            {
                index = GetVertexFromBelow(point, x);
            }
            if (!index.HasValue && IsPointOnLeftOfSquare(point) && x > 0)
            {
                index = GetVertexFromLeft(point, x);
            }
            vertexIndex = index.HasValue ? index.Value : (VertexIndex)0;
            return index.HasValue;
        }

        /// <summary>
        /// Store the vertex into the cache for later retrieval.
        /// </summary>
        /// <param name="vertexIndex">The index associated with this vertex.</param>
        /// <param name="point">The location of the point on the square: an int from 0 to 7.</param>
        /// <param name="x">Location in the current row.</param>
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
        public void RowComplete()
        {
            SwapRows();
        }

        bool IsPointOnBottomOfSquare(int point)
        {
            return point == 4 || point == 5 || point == 6;
        }

        bool IsPointOnLeftOfSquare(int point)
        {
            return point == 0 || point == 6 || point == 7;
        }

        VertexIndex? GetVertexFromBelow(int point, int x)
        {
            int offset = -point + 6; // 6 -> 0, 5 -> 1, 4 -> 2
            return previousRow[offset, x];
        }

        VertexIndex? GetVertexFromLeft(int point, int x)
        {
            int offset = point == 6 ? 4 : 2 + point / 7; // 6 -> 4, 7 -> 3, 0 -> 2
            return currentRow[offset, x - 1];
        }

        void SwapRows()
        {
            var temp = currentRow;
            currentRow = previousRow;
            previousRow = temp;
            for (int y = 0; y < perSquareCacheSize; y++)
            {
                for (int x = 0; x < rowLength; x++)
                {
                    currentRow[y, x] = null;
                }
            }
        }
    } 
}
