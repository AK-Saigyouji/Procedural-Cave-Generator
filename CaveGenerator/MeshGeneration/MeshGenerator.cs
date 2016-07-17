using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Produces meshes and colliders for Map objects using the marching squares algorithm. 
    /// Break large maps (max of 100 by 100 recommended - beyond 200 by 200 is likely to produde exceptions) into 
    /// smaller maps before generating meshes. 
    /// </summary>
    public class MeshGenerator
    {
        MeshData ceilingMesh;
        const string ceilingName = "Ceiling Mesh";

        MeshData wallMesh;
        const string wallName = "Wall Mesh";

        List<Outline> outlines;

        int mapIndex;

        /// <summary>
        /// Generate the data necessary to produce the ceiling mesh. Safe to run on background threads.
        /// </summary>
        public void GenerateCeiling(Map map, Vector2 ceilingTextureDimensions)
        {
            mapIndex = map.index;

            var ceilingBuilder = new CeilingBuilder(map, ceilingTextureDimensions);
            ceilingBuilder.Build();
            ceilingMesh = ceilingBuilder.mesh;
            outlines = ceilingBuilder.outlines;
        }

        /// <summary>
        /// Generate the data necessary to produce the wall mesh. Must first run GenerateCeiling. Note that this will
        /// raise the ceiling to accommodate the walls. 
        /// </summary>
        public void GenerateWalls(int wallsPerTextureTile, int wallHeight)
        {
            var wallBuilder = new WallBuilder(ceilingMesh.vertices, outlines, wallsPerTextureTile, wallHeight);
            wallBuilder.Build();
            wallMesh = wallBuilder.mesh;
        }

        /// <summary>
        /// Get the mesh for the ceiling/base component. Must first run GenerateCeiling to populate the data. If you plan
        /// to generate walls, do so before calling this method, as generating walls will raise the ceiling mesh. 
        /// </summary>
        public Mesh GetCeilingMesh()
        {
            return BuildMesh(ceilingMesh, ceilingName);
        }

        /// <summary>
        /// Create and return the wall 3D wall mesh. Must first run GenerateWalls.
        /// </summary>
        public Mesh GetWallMesh()
        {
            return BuildMesh(wallMesh, wallName);
        }

        /// <summary>
        /// Generates a list of arrays of 2D points corresponding to the vertices of the outlines of the walls.
        /// </summary>
        public List<Vector2[]> GetOutlines()
        {
            List<Vector2[]> outlines2D = new List<Vector2[]>();
            foreach (Outline outline in outlines)
            {
                Vector2[] edgePoints = new Vector2[outline.Count];
                for (int i = 0; i < outline.Count; i++)
                {
                    Vector3 vertex = ceilingMesh.vertices[outline[i]];
                    edgePoints[i] = new Vector2(vertex.x, vertex.z);
                }
                outlines2D.Add(edgePoints);
            }
            return outlines2D;
        }

        Mesh BuildMesh(MeshData meshData, string name)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = meshData.vertices;
            mesh.triangles = meshData.triangles;
            mesh.RecalculateNormals();
            mesh.uv = meshData.uv;
            mesh.name = name + " " + mapIndex;
            return mesh;
        }
    } 
}