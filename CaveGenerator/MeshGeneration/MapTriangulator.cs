using UnityEngine;
using System.Collections.Generic;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Triangulates a Map according to the Marching Squares algorithm, yielding core mesh data (triangles and vertices)
    /// along with a lookup table of the triangles containing each vertex.
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

        int[] vertexIndices;

        Map map;
        Dictionary<int, int> positionToVertexIndex;

        List<Vector2> localVertices;
        List<int> triangles;

        const int MAX_VERTICES_IN_TRIANGULATION = 6;

        public IDictionary<int, List<Triangle>> vertexIndexToTriangles { get; private set; }
        public Vector3[] meshVertices { get { return LocalToGlobalPositions(localVertices); } }
        public int[] meshTriangles { get { return triangles.ToArray(); } }

        public void Triangulate(Map map)
        {
            Initialize(map);
            int numSquaresAcross = map.length - 1;
            int numSquaresDeep = map.width - 1;
            for (int x = 0; x < numSquaresAcross; x++)
            {
                for (int y = 0; y < numSquaresDeep; y++)
                {
                    int configuration = ComputeConfiguration(map[x, y + 1], map[x + 1, y + 1], map[x + 1, y], map[x, y]);
                    if (configuration != 0)
                    {
                        TriangulateSquare(configuration, x, y);
                    }
                }
            }
        }

        void Initialize(Map map)
        {
            this.map = map;
            vertexIndices = new int[MAX_VERTICES_IN_TRIANGULATION];
            localVertices = new List<Vector2>();
            triangles = new List<int>();
            vertexIndexToTriangles = new Dictionary<int, List<Triangle>>();
            positionToVertexIndex = new Dictionary<int, int>();
        }

        void TriangulateSquare(int configuration, int x, int y)
        {
            int[] points = configurationTable[configuration];
            SetVertexIndices(points, x, y);
            CreateTriangles(points.Length);
        }

        void SetVertexIndices(int[] points, int x, int y)
        {
            for (int i = 0; i < points.Length; i++)
            {
                vertexIndices[i] = GetVertexIndex(points[i], x, y);
            }
        }

        int GetVertexIndex(int point, int x, int y)
        {
            Vector2 localPosition = GetLocalPosition(point, x, y);
            return PositionToIndex(localPosition);
        }

        int PositionToIndex(Vector2 position)
        {
            int vertexIndex;
            int positionId = ComputePositionId(position);
            bool isNewPosition = !positionToVertexIndex.TryGetValue(positionId, out vertexIndex);
            if (isNewPosition)
            {
                vertexIndex = localVertices.Count;
                localVertices.Add(position);
                positionToVertexIndex[positionId] = vertexIndex;
            }
            return vertexIndex;
        }

        void CreateTriangles(int numVertices)
        {
            int numTriangles = numVertices - 2;
            for (int i = 0; i < numTriangles; i++)
            {
                CreateTriangle(vertexIndices[0], vertexIndices[i + 1], vertexIndices[i + 2]);
            }
        }

        void CreateTriangle(int a, int b, int c)
        {
            Triangle triangle = new Triangle(a, b, c);

            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);

            AddTriangleToTable(a, triangle);
            AddTriangleToTable(b, triangle);
            AddTriangleToTable(c, triangle);
        }

        void AddTriangleToTable(int index, Triangle triangle)
        {
            List<Triangle> triangles;
            if (vertexIndexToTriangles.TryGetValue(index, out triangles))
            {
                triangles.Add(triangle);
            }
            else
            {
                triangles = new List<Triangle> { triangle };
                vertexIndexToTriangles[index] = triangles;
            }
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
