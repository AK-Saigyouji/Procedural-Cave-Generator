/* During map triangulation, we process many triangles that have overlapping vertices. In order to avoid seams, 
 * it's necessary to ensure each triangle with a common vertex points to the same vertex in the vertices array.
 * Keeping a dictionary for every vertex would work, but consumes O(map length * map width) memory. To improve performance
 * we can exploit the fact that when we determine vertices for a single row, those vertices can only be shared by the
 * vertices in the row directly above it. Thus we need only keep track of two rows at a time, not the entire map.
 * This brings memory down to O(map length).
 */

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Specialized lookup table for the map triangulator using O(map length) memory. Can store and lookup
    /// visited vertex indices row by row. Note that it requires squares to be triangulated row-wise, in increasing
    /// order of x.
    /// </summary>
    class VertexLookup
    {
        VertexIndex[] currentRow;
        VertexIndex[] previousRow;

        const int perSquareCacheSize = 5;

        public VertexLookup(int rowLength)
        {
            VertexIndex[] currentRow = new VertexIndex[rowLength * perSquareCacheSize];
            VertexIndex[] previousRow = new VertexIndex[rowLength * perSquareCacheSize];
            for (int i = 0; i < currentRow.Length; i++)
            {
                currentRow[i] = VertexIndex.VoidValue;
                previousRow[i] = VertexIndex.VoidValue;
            }
            this.currentRow = currentRow;
            this.previousRow = previousRow;
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
            vertexIndex = VertexIndex.VoidValue;
            if (IsPointOnBottomOfSquare(point))
            {
                vertexIndex = GetVertexFromBelow(point, x);
            }
            if (IsVertexIndexVoid(vertexIndex) && IsPointOnLeftOfSquare(point) && x > 0)
            {
                vertexIndex = GetVertexFromLeft(point, x);
            }
            return !IsVertexIndexVoid(vertexIndex);
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
                int positionInRow = perSquareCacheSize * x + point;
                currentRow[positionInRow] = vertexIndex;
            }
        }

        /// <summary>
        /// Call this every time a row is complete, i.e. when iterating the width/height of the map.
        /// </summary>
        public void RowComplete()
        {
            SwapRows();
        }

        bool IsVertexIndexVoid(VertexIndex index)
        {
            return index == VertexIndex.VoidValue;
        }

        bool IsPointOnBottomOfSquare(int point)
        {
            return point == 6 || point == 5 || point == 4;
        }

        bool IsPointOnLeftOfSquare(int point)
        {
            return point == 0 || point == 6 || point == 7;
        }

        VertexIndex GetVertexFromBelow(int point, int x)
        {
            int offset = -point + 6; // 6 -> 0, 5 -> 1, 4 -> 2
            int positionInPreviousRow = perSquareCacheSize * x + offset;
            return previousRow[positionInPreviousRow];
        }

        VertexIndex GetVertexFromLeft(int point, int x)
        {
            int offset = point == 6 ? 4 : 2 + point / 7; // 6 -> 4, 7 -> 3, 0 -> 2
            int positionInPreviousSquare = perSquareCacheSize * (x - 1) + offset;
            return currentRow[positionInPreviousSquare];
        }

        void SwapRows()
        {
            VertexIndex[] temp = currentRow;
            currentRow = previousRow;
            previousRow = temp;
            for (int i = 0; i < currentRow.Length; i++)
            {
                currentRow[i] = VertexIndex.VoidValue;
            }
        }
    } 
}
