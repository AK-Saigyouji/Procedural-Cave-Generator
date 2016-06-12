using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Produces meshes and colliders for Map objects using the marching squares algorithm. 
    /// Break large maps (max of 100 by 100 recommended - beyond 200 by 200 likely to produde exceptions) into 
    /// smaller maps before generating meshes. 
    /// </summary>
    public class MeshGenerator
    {
        // Ceiling Mesh Data:
        Vector3[] ceilingVertices;
        int[] ceilingTriangles;
        Vector2[] ceilingUV;

        // Wall mesh data:
        Vector3[] wallVertices;
        int[] wallTriangles;
        Vector2[] wallUV;

        // Outline data: 
        IDictionary<int, List<Triangle>> vertexIndexToContainingTriangles;
        List<Outline> outlines;

        Map map;

        /// <summary>
        /// Generate the data necessary to produce the ceiling mesh. Safe to run on background threads.
        /// </summary>
        public void GenerateCeiling(Map map, Vector2 ceilingTextureDimensions)
        {
            this.map = map;
            TriangulateMap();
            CalculateMeshOutlines();
            ComputeCeilingUVArray(ceilingTextureDimensions);
        }

        /// <summary>
        /// Create and return the ceiling mesh. Must first run GenerateCeiling to populate the data.
        /// </summary>
        /// <returns></returns>
        public Mesh GetCeilingMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = ceilingVertices;
            mesh.triangles = ceilingTriangles;
            mesh.RecalculateNormals();
            mesh.uv = ceilingUV;
            mesh.name = "Ceiling Mesh" + map.index;
            return mesh;
        }

        /// <summary>
        /// Create and return the wall 3D wall mesh. Must first run GenerateWalls.
        /// </summary>
        public Mesh GetWallMesh()
        {
            Mesh wallMesh = new Mesh();
            wallMesh.vertices = wallVertices;
            wallMesh.triangles = wallTriangles;
            wallMesh.RecalculateNormals();
            wallMesh.uv = wallUV;
            wallMesh.name = "Wall Mesh" + map.index;
            return wallMesh;
        }

        /// <summary>
        /// Generates a list of 2D points for the creation of edge colliders along 2D boundaries in the cave.
        /// </summary>
        /// <returns>Returns a list of Vector2 points indicating where edge colliders should be placed.</returns>
        public List<Vector2[]> GenerateColliderEdges()
        {
            List<Vector2[]> edgePointLists = new List<Vector2[]>();
            foreach (Outline outline in outlines)
            {
                Vector2[] edgePoints = new Vector2[outline.Size];
                for (int i = 0; i < outline.Size; i++)
                {
                    edgePoints[i] = new Vector2(ceilingVertices[outline[i]].x, ceilingVertices[outline[i]].z);
                }
                edgePointLists.Add(edgePoints);
            }
            return edgePointLists;
        }

        /// <summary>
        /// Generate the data necessary to produce the wall mesh. Must first run GenerateCeiling. 
        /// </summary>
        public void GenerateWalls(int height, int wallsPerTextureTile)
        {
            int outlineParameter = outlines.Select(x => x.Size - 1).Sum();
            Vector3[] wallVertices = new Vector3[4 * outlineParameter];
            Vector2[] wallUV = new Vector2[4 * outlineParameter];
            int[] wallTriangles = new int[6 * outlineParameter];

            int vertexCount = 0;
            int triangleCount = 0;
            // Run along each outline, and create a quad between each pair of points in the outline.
            foreach (Outline outline in outlines)
            {
                for (int i = 0; i < outline.Size - 1; i++)
                {
                    wallVertices[vertexCount] = ceilingVertices[outline[i]];
                    wallVertices[vertexCount + 1] = ceilingVertices[outline[i + 1]];
                    wallVertices[vertexCount + 2] = ceilingVertices[outline[i]] - Vector3.up * height;
                    wallVertices[vertexCount + 3] = ceilingVertices[outline[i + 1]] - Vector3.up * height;

                    // This uv configuration ensures that the texture gets tiled once every wallsPerTextureTile quads in the 
                    // horizontal direction.
                    float uLeft = i / (float)wallsPerTextureTile;
                    float uRight = (i + 1) / (float)wallsPerTextureTile;
                    wallUV[vertexCount] = new Vector2(uLeft, 1f);
                    wallUV[vertexCount + 1] = new Vector2(uRight, 1f);
                    wallUV[vertexCount + 2] = new Vector2(uLeft, 0f);
                    wallUV[vertexCount + 3] = new Vector2(uRight, 0f);

                    wallTriangles[triangleCount] = vertexCount;
                    wallTriangles[triangleCount + 1] = vertexCount + 2;
                    wallTriangles[triangleCount + 2] = vertexCount + 3;

                    wallTriangles[triangleCount + 3] = vertexCount + 3;
                    wallTriangles[triangleCount + 4] = vertexCount + 1;
                    wallTriangles[triangleCount + 5] = vertexCount;
                    vertexCount += 4;
                    triangleCount += 6;
                }
            }
            this.wallVertices = wallVertices;
            this.wallTriangles = wallTriangles;
            this.wallUV = wallUV;
        }

        /// <summary>
        /// Triangulates the squares according to the marching squares algorithm.
        /// In the process, this method populates the baseVertices, triangleMap and and meshTriangles collections.
        /// </summary>
        void TriangulateMap()
        {
            MapTriangulator mapTriangulator = new MapTriangulator();
            mapTriangulator.Triangulate(map);

            ceilingVertices = mapTriangulator.meshVertices;
            ceilingTriangles = mapTriangulator.meshTriangles;
            vertexIndexToContainingTriangles = mapTriangulator.vertexIndexToTriangles;
        }

        void ComputeCeilingUVArray(Vector2 textureDimensions)
        {
            Vector2[] uv = new Vector2[ceilingVertices.Length];
            float xMax = textureDimensions.x;
            float yMax = textureDimensions.y;
            for (int i = 0; i < ceilingVertices.Length; i++)
            {
                float percentX = ceilingVertices[i].x / xMax;
                float percentY = ceilingVertices[i].z / yMax;
                uv[i] = new Vector2(percentX, percentY);
            }
            ceilingUV = uv;
        }

        void CalculateMeshOutlines()
        {
            OutlineGenerator outlineGenerator = new OutlineGenerator(ceilingVertices, vertexIndexToContainingTriangles);
            outlines = outlineGenerator.GenerateOutlines();
        }
    } 
}