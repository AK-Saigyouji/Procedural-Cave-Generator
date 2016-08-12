using UnityEngine;
using System.Collections.Generic;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Triangulates a Map according to the Marching Squares algorithm, yielding vertices and triangles ready to be
    /// used in a mesh.
    /// </summary>
    class MapTriangulator
    {
        // The lookup table for the marching squares algorithm. The eight points in the square are enumerated from 0 to 7, 
        // starting in the top left corner and going clockwise (see visual below). Based on the sixteen possible configurations 
        // for the corners of a square, this table returns the points needed to triangulate that square.
        // 0 1 2
        // 7 - 3
        // 6 5 4
        static int[][] configurationTable = new int[][]
        {
            new int[] { },
            new int[] {5, 6, 7 },
            new int[] {3, 4, 5 },
            new int[] {3, 4, 6, 7 },
            new int[] {1, 2, 3 },
            new int[] {1, 2, 3, 5, 6, 7 },
            new int[] {1, 2, 4, 5 },
            new int[] {1, 2, 4, 6, 7 },
            new int[] {0, 1, 7 },
            new int[] {0, 1, 5, 6 },
            new int[] {0, 1, 3, 4, 5, 7 },
            new int[] {0, 1, 3, 4, 6 },
            new int[] {0, 2, 3, 7 },
            new int[] {0, 2, 3, 5, 6 },
            new int[] {0, 2, 4, 5, 7 },
            new int[] {0, 2, 4, 6}
        };

        // Lookup table for determining the position of the 8 points in the square relative to the bottom-left corner,
        // not taking into account scaling associated with the map's square size. 
        static Vector2[] positionOffsets = new Vector2[]
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

        VertexIndex[] vertexIndices; // Reusable array to hold the vertices in each square as they're processed.
        Map map;

        // These are used to cache vertices as we triangulate the map row by row. 
        VertexIndex[] currentRow;
        VertexIndex[] previousRow;

        // These hold all vertices and triangles as they're computed.
        List<Vector2> localVertices;
        List<VertexIndex> triangles;

        const int MAX_VERTICES_IN_TRIANGULATION = 6;

        public MapTriangulator(Map map)
        {
            this.map = map;
            vertexIndices = new VertexIndex[MAX_VERTICES_IN_TRIANGULATION];
            int maxPossibleVertices = map.length * map.width;
            localVertices = new List<Vector2>(maxPossibleVertices);
            triangles = new List<VertexIndex>(maxPossibleVertices * 6);
            InitializeRows();
        }

        /// <summary>
        /// Compute a triangulation of the map passed into the constructor.
        /// </summary>
        /// <returns>MeshData object containing vertices and triangles.</returns>
        public MeshData Triangulate()
        {
            TriangulateAllSquares();
            return BuildMeshData();
        }

        MeshData BuildMeshData()
        {
            MeshData mesh = new MeshData();
            mesh.vertices = LocalToGlobalPositions(localVertices);
            mesh.triangles = ExtractTriangleArray();
            return mesh;
        }

        int[] ExtractTriangleArray()
        {
            int[] triangleArray = new int[triangles.Count];
            for (int i = 0; i < triangleArray.Length; i++)
            {
                triangleArray[i] = triangles[i];
            }
            return triangleArray;
        }

        void TriangulateAllSquares()
        {
            int numSquaresAcross = map.length - 1;
            int numSquaresDeep = map.width - 1;
            for (int y = 0; y < numSquaresDeep; y++)
            {
                for (int x = 0; x < numSquaresAcross; x++)
                {
                    int configuration = ComputeConfiguration(map[x, y + 1], map[x + 1, y + 1], map[x + 1, y], map[x, y]);
                    if (configuration != 0)
                    {
                        TriangulateSquare(configuration, x, y);
                    }
                }
                SwapRows();
            }
        }

        void InitializeRows()
        {
            // Each row contains all the squares for that row of the map. Each square holds 8 vertices.
            VertexIndex[] currentRow = new VertexIndex[map.length * 8];
            VertexIndex[] previousRow = new VertexIndex[map.length * 8];
            for (int i = 0; i < currentRow.Length; i++)
            {
                currentRow[i] = VertexIndex.VoidValue;
                previousRow[i] = VertexIndex.VoidValue;
            }
            this.currentRow = currentRow;
            this.previousRow = previousRow;
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

        void TriangulateSquare(int configuration, int x, int y)
        {
            int[] points = configurationTable[configuration];
            SetVertexIndices(points, x, y);
            AddTriangles(points.Length);
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
            if (!TryGetCachedVertex(point, x, out vertexIndex))
            {
                vertexIndex = localVertices.Count;
                Vector2 localPosition = GetLocalPosition(point, x, y);
                localVertices.Add(localPosition);
            }
            CacheVertex(vertexIndex, point, x);
            return vertexIndex;
        }

        bool TryGetCachedVertex(int point, int x, out VertexIndex vertexIndex)
        {
            if (IsPointOnBottomOfSquare(point))
            {
                vertexIndex = GetVertexFromBelow(point, x);
            }
            else if (IsPointOnLeftOfSquare(point) && x > 0)
            {
                vertexIndex = GetVertexFromLeft(point, x);
            }
            else
            {
                vertexIndex = VertexIndex.VoidValue;
            }
            return vertexIndex != VertexIndex.VoidValue;
        }

        void CacheVertex(VertexIndex vertexIndex, int point, int x)
        {
            int positionInRow = 8 * x + point;
            currentRow[positionInRow] = vertexIndex;
        }

        bool IsPointOnBottomOfSquare(int point)
        {
            return point == 6 || point == 5 || point == 4;
        }

        // Note: we exclude the case where point is on the bottom left corner, which is handled by the method for bottom.
        bool IsPointOnLeftOfSquare(int point)
        {
            return point == 0 || point == 7;
        }

        VertexIndex GetVertexFromBelow(int point, int x)
        {
            int positionInPreviousRow = 8 * x - point + 6;
            return previousRow[positionInPreviousRow];
        }

        // Assumes point is either 0 or 7 but not 6.
        VertexIndex GetVertexFromLeft(int point, int x)
        {
            int positionInPreviousSquare = 8 * (x - 1) + 2 + point / 7;
            return currentRow[positionInPreviousSquare];
        }

        void AddTriangles(int numVertices)
        {
            int numTriangles = numVertices - 2;
            for (int i = 0; i < numTriangles; i++)
            {
                AddTriangle(vertexIndices[0], vertexIndices[i + 1], vertexIndices[i + 2]);
            }
        }

        void AddTriangle(VertexIndex a, VertexIndex b, VertexIndex c)
        {
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
        }

        Vector3[] LocalToGlobalPositions(List<Vector2> localPositions)
        {
            int scale = map.squareSize;
            Vector3 basePosition = map.position;

            Vector3[] globalPositions = new Vector3[localPositions.Count];
            for (int i = 0; i < globalPositions.Length; i++)
            {
                globalPositions[i] = basePosition + new Vector3(localPositions[i].x, 0f, localPositions[i].y) * scale;
            }
            return globalPositions;
        }

        Vector2 GetLocalPosition(int squarePoint, int x, int y)
        {
            Vector2 offset = positionOffsets[squarePoint];
            return new Vector2(x + offset.x, y + offset.y);
        }

        int ComputePositionId(Vector2 position)
        {
            return (int)(position.x * 10000 + position.y * 10);
        }

        int ComputeConfiguration(Tile topLeft, Tile topRight, Tile bottomRight, Tile bottomLeft)
        {
            return (int)bottomLeft + ((int)bottomRight * 2) + ((int)topRight * 4) + ((int)topLeft * 8);
        }
    }
}
