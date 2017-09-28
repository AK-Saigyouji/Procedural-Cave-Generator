/* This class turns a WallGrid, which is a grid of 0s and 1s, into vertices and triangles for a Mesh.
 * It does this using the Marching Squares algorithm. A square in this context refers to four points in the grid 
 * in the arrangement (x,y),(x+1,y),(x,y+1),(x+1,y+1). The algorithm iterates over all such squares, and builds triangles
 * based on which of the four corners are walls (giving rise to 16 configurations). The vertices of the triangles are taken
 * from the four corners of the square plus the four midpoints of the square. 
 */

using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System.Linq;

namespace AKSaigyouji.MeshGeneration
{
    /// <summary>
    /// Triangulates a Map according to the Marching Squares algorithm, yielding vertices and triangles ready to be
    /// used in a mesh.
    /// </summary>
    public sealed class MapTriangulator
    {
        const int PER_SQUARE_CACHE_SIZE = 5;

        readonly bool[] isOnLeftSide = new bool[] { true, false, false, false, false, false, true, true };
        readonly bool[] isOnBottom = new bool[] { false, false, false, false, true, true, true, false };

        readonly sbyte[] bottomOffset = new sbyte[] { -1, -1, -1, -1, 2, 1, 0, -1 };
        readonly sbyte[] leftOffset = new sbyte[] { 2, -1, -1, -1, -1, -1, 4, 3 };

        public MeshData Triangulate(WallGrid wallGrid)
        {
            byte[,] configurations = MarchingSquares.ComputeConfigurations(wallGrid);
            byte[][] configurationTable = MarchingSquares.BuildConfigurationTable();

            var localVertices = new List<LocalPosition>();
            var triangles = new List<int>();

            // Stores vertex indices for a single square at a time.
            var vertexIndices = new ushort[MarchingSquares.MAX_VERTICES_IN_TRIANGULATION];

            var currentRow  = new ushort[PER_SQUARE_CACHE_SIZE, wallGrid.Length];
            var previousRow = new ushort[PER_SQUARE_CACHE_SIZE, wallGrid.Length];

            int width = configurations.GetLength(1);
            int length = configurations.GetLength(0);
            for (byte y = 0; y < width; y++)
            {
                for (byte x = 0; x < length; x++)
                {
                    int config = configurations[x, y];
                    byte[] points = configurationTable[config];
                    for (int i = 0; i < points.Length; i++)
                    {
                        byte point = points[i];
                        ushort vertexIndex;
                        if (isOnBottom[point] && y > 0) // is vertex cached below
                        {
                            vertexIndex = previousRow[bottomOffset[point], x];
                        }
                        else if (isOnLeftSide[point] && x > 0) // is vertex cached to the left
                        {
                            vertexIndex = currentRow[leftOffset[point], x - 1];
                        }
                        else // new vertex
                        {
                            vertexIndex = (ushort)localVertices.Count;
                            localVertices.Add(new LocalPosition(x, y, point));
                        }
                        if (point < PER_SQUARE_CACHE_SIZE) // cache vertex if top left, top, top right, right or bot right
                        {
                            currentRow[point, x] = vertexIndex;
                        }
                        vertexIndices[i] = vertexIndex;
                    }
                    int numTrianglesToBuild = points.Length - 2;
                    for (int i = 0; i < numTrianglesToBuild; i++)
                    {
                        triangles.Add(vertexIndices[0]);
                        triangles.Add(vertexIndices[i + 1]);
                        triangles.Add(vertexIndices[i + 2]);
                    }
                }
                SwapRows(ref currentRow, ref previousRow);
            }
            MeshData mesh = new MeshData();
            mesh.vertices = localVertices.Select(v => v.ToGlobalPosition(wallGrid.Scale, wallGrid.Position)).ToArray();
            mesh.triangles = triangles.ToArray();
            return mesh;
        }

        static void SwapRows(ref ushort[,] currentRow, ref ushort[,] previousRow)
        {
            var temp = currentRow;
            currentRow = previousRow;
            previousRow = temp;
        }
    }
}
