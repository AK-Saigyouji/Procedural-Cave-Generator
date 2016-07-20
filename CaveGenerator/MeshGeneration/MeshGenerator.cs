using System.Collections.Generic;
using UnityEngine;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Produces meshes and colliders for Map objects. Break large maps into smaller maps before generating meshes.
    /// Maps should not be larger than 100 by 100.
    /// </summary>
    public class MeshGenerator
    {
        MeshData ceilingMesh;
        MeshData wallMesh;
        MeshData floorMesh;
        MeshData enclosureMesh;

        List<Outline> outlines;

        int mapIndex;

        /// <summary>
        /// Generate the data necessary to produce the ceiling mesh.
        /// </summary>
        public void GenerateCeiling(Map map)
        {
            mapIndex = map.index;

            IMeshBuilder ceilingBuilder = new CeilingBuilder(map);
            ceilingMesh = ceilingBuilder.Build();

            ComputeMeshOutlines();
        }

        /// <summary>
        /// Generate the data necessary to produce the wall mesh. Must first run GenerateCeiling. Note that this will
        /// raise the ceiling to accommodate the walls. 
        /// </summary>
        public void GenerateWalls(int wallHeight)
        {
            IMeshBuilder wallBuilder = new WallBuilder(ceilingMesh.vertices, outlines, wallHeight);
            wallMesh = wallBuilder.Build();
        }

        /// <summary>
        /// Generate the data necessary to produce the floor mesh. Must first run GenerateCeiling. 
        /// </summary>
        public void GenerateFloor(Map map)
        {
            IMeshBuilder floorBuilder = new FloorBuilder(map);
            floorMesh = floorBuilder.Build();
        }

        /// <summary>
        /// Generate the data necessary to produce the enclosure mesh. The enclosure refers to the part that hangs
        /// over the walkable areas of the map, effectively enclosing the cave. Must first run GenerateFloor.
        /// </summary>
        public void GenerateEnclosure(int wallHeight)
        {
            IMeshBuilder enclosureBuilder = new EnclosureBuilder(floorMesh, wallHeight);
            enclosureMesh = enclosureBuilder.Build();
        }

        /// <summary>
        /// Get the mesh for the ceiling/base component. Must first run GenerateCeiling to populate the data. If you plan
        /// to generate walls, do so before calling this method, as generating walls will raise the ceiling mesh. 
        /// </summary>
        public Mesh GetCeilingMesh()
        {
            return BuildMesh(ceilingMesh);
        }

        /// <summary>
        /// Create and return the wall 3D wall mesh. Must first run GenerateWalls.
        /// </summary>
        public Mesh GetWallMesh()
        {
            return BuildMesh(wallMesh);
        }

        /// <summary>
        /// Create and return the floor mesh. Must first run GenerateEnclosure.
        /// </summary>
        /// <returns></returns>
        public Mesh GetFloorMesh()
        {
            return BuildMesh(floorMesh);
        }

        /// <summary>
        /// Create a return the enclosure mesh. Must first run GenerateFloors.
        /// </summary>
        /// <returns></returns>
        public Mesh GetEnclosureMesh()
        {
            return BuildMesh(enclosureMesh);
        }

        void ComputeMeshOutlines()
        {
            OutlineGenerator outlineGenerator = new OutlineGenerator(ceilingMesh.vertices, ceilingMesh.triangles);
            outlines = outlineGenerator.GenerateOutlines();
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

        Mesh BuildMesh(MeshData meshData)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = meshData.vertices;
            mesh.triangles = meshData.triangles;
            mesh.RecalculateNormals();
            mesh.uv = meshData.uv;
            mesh.name = meshData.name + " " + mapIndex;
            return mesh;
        }
    } 
}