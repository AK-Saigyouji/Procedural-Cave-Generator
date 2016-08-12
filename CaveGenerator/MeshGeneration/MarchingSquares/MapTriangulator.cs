/* This class turns a Map, which is a grid of 0s and 1s, into a mesh (specifically, into vertices and triangles).
 * It does this using the Marching Squares algorithm. A square in this context refers to four points in the map
 * in the arrangement (x,y),(x+1,y),(x,y+1),(x+1,y+1). The algorithm iterates over all such squares, and builds triangles
 * based on which of the four corners are walls (giving rise to 16 configurations). The corners of the triangles are taken
 * from the four corners of the square plus the four midpoints of the square. 
 */

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
        VertexLookup vertexCache;

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
            vertexCache = new VertexLookup(map.length);
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
                    TriangulateSquare(x, y);
                }
                vertexCache.RowComplete();
            }
        }

        void TriangulateSquare(int x, int y)
        {
            int configuration = ComputeConfiguration(map, x, y);
            if (configuration != 0)
            {
                int[] points = configurationTable[configuration];
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
                Vector2 localPosition = GetLocalPosition(point, x, y);
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

        int ComputeConfiguration(Map map, int x, int y)
        {
            int bottomLeft = (int)map[x, y];
            int bottomRight = (int)map[x + 1, y];
            int topRight = (int)map[x + 1, y + 1];
            int topLeft = (int)map[x, y + 1];
            return bottomLeft + bottomRight * 2 + topRight * 4 + topLeft * 8;
        }
    }
}
