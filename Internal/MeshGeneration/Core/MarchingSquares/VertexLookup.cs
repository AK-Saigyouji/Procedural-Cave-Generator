/* During map triangulation, we process many triangles that have overlapping vertices. In order to avoid seams, 
 * it's necessary to ensure each triangle with a common vertex points to the same vertex in the vertices array.
 * Keeping a generic dictionary for every vertex would work, but that would be both slow and memory-intensive. 
 * To achieve better performance, we can exploit the following observation: a given square can only share its vertices
 * with its immediate neighbours. If we handle squares left to right, bottom to top, this means once we process a 
 * square at x, y, then the only future squares that share its vertices are the squares at (x + 1, y) and (x, y + 1).
 * What this means is that we only need to keep track of two rows at a time: the current one, and the previous one. 
 * 
 * Furthermore, we can perform extremely fast checks by using the fact that we know exactly where to look: e.g.
 * the left-mid point on a square can only be shared by the right-mid point on the previous square on the current row.
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
        // The use of unsigned shorts reflects the fact that there can only be 2^16 vertices in a mesh. 
        // Using 2 bytes instead of 4 like this improves cache performance quite a bit, and resulted in about
        // a 15% performance improvement.
        ushort[,] currentRow;
        ushort[,] previousRow;

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
        // -1 represents an invalid entry

        // 0, 7, 6 to 2, 3, 4 respectively (topleft, left, bottomleft to topright, right, bottomright)
        readonly sbyte[] leftOffset = new sbyte[] { 2, -1, -1, -1, -1, -1, 4, 3 };

        // 4, 5, 6 to 2, 1, 0 respectively (bottomright, bottom, bottomleft to topright, top, topleft)
        readonly sbyte[] bottomOffset = new sbyte[] { -1, -1, -1, -1, 2, 1, 0, -1 };

        // how many points in each square we cache (just the first five).
        const int perSquareCacheSize = 5;

        public VertexLookup(int rowLength)
        {
            currentRow = new ushort[perSquareCacheSize, rowLength];
            previousRow = new ushort[perSquareCacheSize, rowLength];
        }

        public bool TryGetCachedVertex(byte x, byte y, byte squarePoint, out ushort vertexIndex)
        {
            if (isOnBottom[squarePoint] && y > 0)
            {
                vertexIndex = previousRow[bottomOffset[squarePoint], x];
                return true;
            }
            if (isOnLeft[squarePoint] && x > 0)
            {
                vertexIndex = currentRow[leftOffset[squarePoint], x - 1];
                return true;
            }
            vertexIndex = 0;
            return false;
        }

        public void CacheVertex(byte x, byte point, ushort vertexIndex)
        {
            if (point < perSquareCacheSize) // Only the first five points (0,1,2,3,4) need to be stored
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
        }

        void SwapRows()
        {
            var temp = currentRow;
            currentRow = previousRow;
            previousRow = temp;
        }
    } 
}
