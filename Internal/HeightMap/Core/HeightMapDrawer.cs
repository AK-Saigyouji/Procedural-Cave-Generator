/* This is a class used solely in the editor to create a height map visualization that responds immediately to
 * changes in the parameters of a heightmap, making it easy to fine-tune the parameters without having to generate caves.*/

using UnityEngine;

using IHeightMap = CaveGeneration.MeshGeneration.IHeightMap;

namespace CaveGeneration.HeightMaps
{
    sealed class HeightMapDrawer
    {
        const int WIDTH = 100;
        const int LENGTH = 100;

        Vector3[] vertices = new Vector3[(LENGTH - 1) * (WIDTH - 1) * 4];
        int[] triangles = new int[(LENGTH - 1) * (WIDTH - 1) * 6];
        IHeightMap heightMap;

        public Mesh mesh { get; private set; }

        /// <summary>
        /// Used to draw height maps in the editor as parameters change.
        /// </summary>
        public HeightMapDrawer()
        {
            mesh = new Mesh();
            CreateTriangles();
        }

        /// <summary>
        /// Build the mesh to be visualized based on the given height map.
        /// </summary>
        public void BuildMesh(IHeightMap heightMap)
        {
            this.heightMap = heightMap;
            UpdateVertices();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
        }

        void UpdateVertices()
        {
            int vertexIndex = 0;
            for (int x = 0; x < LENGTH - 1; x++)
            {
                for (int y = 0; y < WIDTH - 1; y++)
                {
                    vertices[vertexIndex] = GetVertex(x, y);
                    vertices[vertexIndex + 1] = GetVertex(x, y + 1);
                    vertices[vertexIndex + 2] = GetVertex(x + 1, y + 1);
                    vertices[vertexIndex + 3] = GetVertex(x + 1, y);
                    vertexIndex += 4;
                }
            }
        }

        Vector3 GetVertex(int x, int y)
        {
            return new Vector3(x, heightMap.GetHeight(x, y), y);
        }

        void CreateTriangles()
        {
            int triangleIndex = 0;
            int vertexIndex = 0;
            for (int x = 0; x < LENGTH - 1; x++)
            {
                for (int y = 0; y < WIDTH - 1; y++)
                {
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex + 1] = vertexIndex + 1;
                    triangles[triangleIndex + 2] = vertexIndex + 2;

                    triangles[triangleIndex + 3] = vertexIndex;
                    triangles[triangleIndex + 4] = vertexIndex + 2;
                    triangles[triangleIndex + 5] = vertexIndex + 3;
                    triangleIndex += 6;
                    vertexIndex += 4;
                }
            }
        }
    } 
}
