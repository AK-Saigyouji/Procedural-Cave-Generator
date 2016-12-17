/* This class turns a WallGrid, which is a grid of 0s and 1s, into vertices and triangles for a Mesh.
 * It does this using the Marching Squares algorithm. A square in this context refers to four points in the grid 
 * in the arrangement (x,y),(x+1,y),(x,y+1),(x+1,y+1). The algorithm iterates over all such squares, and builds triangles
 * based on which of the four corners are walls (giving rise to 16 configurations). The corners of the triangles are taken
 * from the four corners of the square plus the four midpoints of the square. 
 * 
 * A specialized data structure is used to cache vertices during triangulation to avoid doubling up on vertices. */

using UnityEngine;
using System.Collections.Generic;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Triangulates a Map according to the Marching Squares algorithm, yielding vertices and triangles ready to be
    /// used in a mesh.
    /// </summary>
    sealed class MapTriangulator
    {
        WallGrid grid;

        VertexIndex[] vertexIndices; // Reusable array to hold one square's vertices at a time.
        VertexLookup vertexCache; 

        // These temporary lists hold all vertices and triangles as they're computed.
        List<LocalPosition> localVertices;
        List<VertexIndex> triangles;

        public static MeshData Triangulate(WallGrid grid)
        {
            var triangulator = new MapTriangulator(grid);
            return triangulator.Triangulate();
        }

        MapTriangulator(WallGrid grid)
        {
            this.grid = grid;
            int maxPossibleVertices = grid.Length * grid.Width;
            vertexIndices = new VertexIndex[MarchingSquares.MAX_VERTICES_IN_TRIANGULATION];
            localVertices = new List<LocalPosition>(maxPossibleVertices);
            triangles = new List<VertexIndex>(maxPossibleVertices * 6);
            vertexCache = new VertexLookup(grid.Length);
        }

        /// <summary>
        /// Compute a triangulation of the map passed into the constructor.
        /// </summary>
        /// <returns>MeshData object containing vertices and triangles.</returns>
        MeshData Triangulate()
        {
            TriangulateAllSquares();
            MeshData mesh = BuildMeshData();
            return mesh;
        }

        MeshData BuildMeshData()
        {
            MeshData mesh = new MeshData();
            mesh.vertices = LocalToGlobalPositions(localVertices);
            mesh.triangles = ExtractTriangleArray();
            return mesh;
        }

        void TriangulateAllSquares()
        {
            int numSquaresAcross = grid.Length - 1;
            int numSquaresDeep = grid.Width - 1;
            for (int y = 0; y < numSquaresDeep; y++)
            {
                for (int x = 0; x < numSquaresAcross; x++)
                {
                    TriangulateSquare(x, y);
                }
                vertexCache.FinalizeRow();
            }
        }

        void TriangulateSquare(int x, int y)
        {
            int configuration = ComputeConfiguration(grid, x, y);
            if (configuration != 0)
            {
                int[] points = MarchingSquares.GetPoints(configuration);
                SetVertexIndices(points, x, y);
                AddTriangles(points.Length); 
            }
        }

        void SetVertexIndices(int[] points, int x, int y)
        {
            for (int i = 0; i < points.Length; i++)
            {
                vertexIndices[i] = GetVertexIndex(points[i], x, y);
            }
        }

        VertexIndex GetVertexIndex(int point, int x, int y)
        {
            VertexIndex vertexIndex;
            if (!vertexCache.TryGetCachedVertex(point, x, out vertexIndex))
            {
                vertexIndex = localVertices.Count;
                LocalPosition localPosition = new LocalPosition(x, y, point);
                localVertices.Add(localPosition);
            }
            vertexCache.CacheVertex(vertexIndex, point, x);
            return vertexIndex;
        }

        void AddTriangles(int numVertices)
        {
            int numTriangles = numVertices - 2;
            for (int i = 0; i < numTriangles; i++)
            {
                triangles.Add(vertexIndices[0]);
                triangles.Add(vertexIndices[i + 1]);
                triangles.Add(vertexIndices[i + 2]);
            }
        }

        Vector3[] LocalToGlobalPositions(List<LocalPosition> localPositions)
        {
            Vector3 basePosition = grid.Position;
            int scale = grid.Scale;

            var globalPositions = new Vector3[localPositions.Count];
            for (int i = 0; i < globalPositions.Length; i++)
            {
                globalPositions[i] = basePosition + localPositions[i].ToVector3() * scale;
            }
            return globalPositions;
        }

        int[] ExtractTriangleArray()
        {
            var triangleArray = new int[triangles.Count];
            for (int i = 0; i < triangleArray.Length; i++)
            {
                triangleArray[i] = triangles[i];
            }
            return triangleArray;
        }

        static int ComputeConfiguration(WallGrid grid, int x, int y)
        {
            byte botLeft = grid[x, y];
            byte botRight = grid[x + 1, y];
            byte topRight = grid[x + 1, y + 1];
            byte topLeft = grid[x, y + 1];
            return MarchingSquares.ComputeConfiguration(botLeft, botRight, topRight, topLeft);
        }
    }
}
