using UnityEngine;
using System.Collections.Generic;

namespace MeshHelpers
{
    class MapTriangulator
    {
        // The lookup table for the marching squares algorithm. The eight points in the square are enumerated from 0 to 7, 
        // starting in the top left corner and going clockwise (see visual below). Based on the sixteen possible configurations 
        // for the corners of a square, this table returns the points needed to triangulate that square.
        // 0 1 2
        // 7 - 3
        // 6 5 4
        static readonly int[][] configurationTable = new int[][]
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
        static readonly Vector3[] positionOffsets = new Vector3[]
        {
            new Vector3(0f, 0f, 1f),
            new Vector3(0.5f, 0f, 1f),
            new Vector3(1f, 0f, 1f),
            new Vector3(1f, 0f, 0.5f),
            new Vector3(1f, 0f, 0f),
            new Vector3(0.5f, 0f, 0f),
            new Vector3(0f, 0f, 0f),
            new Vector3(0f, 0f, 0.5f)
        };

        Map map;
        Dictionary<Vector3, int> positionToVertexIndex;

        List<Vector3> vertices;
        List<int> triangles;

        public IDictionary<int, List<Triangle>> vertexIndexToTriangles { get; private set; }

        public Vector3[] meshVertices
        {
            get{ return vertices.ToArray(); }
        }

        public int[] meshTriangles
        {
            get{ return triangles.ToArray(); }
        }

        public void Triangulate(Map map)
        {
            Initialize(map);
            int numSquaresAcross = map.length - 1;
            int numSquaresDeep = map.width - 1;
            for (int x = 0; x < numSquaresAcross; x++)
            {
                for (int y = 0; y < numSquaresDeep; y++)
                {
                    TriangulateSquare(x, y);
                }
            }
        }

        void Initialize(Map map)
        {
            this.map = map;
            vertices = new List<Vector3>();
            triangles = new List<int>();
            vertexIndexToTriangles = new Dictionary<int, List<Triangle>>();
            positionToVertexIndex = new Dictionary<Vector3, int>();
        }

        void TriangulateSquare(int x, int y)
        {
            int configuration = ComputeConfiguration(map[x, y + 1], map[x + 1, y + 1], map[x + 1, y], map[x, y]);
            int[] points = configurationTable[configuration];
            int[] vertexIndices = AddVertices(points, x, y);
            CreateTriangles(vertexIndices);
        }

        int[] AddVertices(int[] points, int x, int y)
        {
            int[] indices = new int[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                int index;
                Vector3 position = GetPosition(points[i], x, y);
                bool newPosition = !positionToVertexIndex.TryGetValue(position, out index);
                if (newPosition)
                {
                    index = vertices.Count;
                    vertices.Add(position);
                    positionToVertexIndex[position] = index;
                }
                indices[i] = index;
            }
            return indices;
        }

        void CreateTriangles(int[] indices)
        {
            int numTrianglesToCreate = indices.Length - 2;
            for (int i = 0; i < numTrianglesToCreate; i++)
            {
                CreateTriangle(indices[0], indices[i + 1], indices[i + 2]);
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
            if (vertexIndexToTriangles.ContainsKey(index))
            {
                vertexIndexToTriangles[index].Add(triangle);
            }
            else
            {
                vertexIndexToTriangles[index] = new List<Triangle> { triangle };
            }
        }

        Vector3 GetPosition(int squarePoint, int x, int y)
        {
            return map.position + (positionOffsets[squarePoint] + new Vector3(x, 0f, y)) * map.squareSize;
        }

        int ComputeConfiguration(Tile topLeft, Tile topRight, Tile bottomRight, Tile bottomLeft)
        {
            return 8 * (int)topLeft + 4 * (int)topRight + 2 * (int)bottomRight + (int)bottomLeft;
        }
    }
}
