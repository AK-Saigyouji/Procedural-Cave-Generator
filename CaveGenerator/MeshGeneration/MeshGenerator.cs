using System.Collections.Generic;
using UnityEngine;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Produces meshes and colliders for Map objects. Break large maps into smaller maps before generating meshes.
    /// Maps should not be larger than 200 by 200.
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
        }

        /// <summary>
        /// Generate the data necessary to produce the wall mesh. Must first generate ceiling. Note that this will
        /// raise the ceiling to accommodate the walls, if one was generated.
        /// </summary>
        public void GenerateWalls(int wallHeight, IHeightMap heightMap = null)
        {
            ComputeMeshOutlines();

            IMeshBuilder wallBuilder = new WallBuilder(ceilingMesh.vertices, outlines, wallHeight, heightMap);
            wallMesh = wallBuilder.Build();
        }

        /// <summary>
        /// Generate the data necessary to produce the floor mesh. Must first generate ceiling. 
        /// </summary>
        public void GenerateFloor(Map map, IHeightMap heightMap = null)
        {
            mapIndex = map.index;

            IMeshBuilder floorBuilder = new FloorBuilder(map, heightMap);
            floorMesh = floorBuilder.Build();
        }

        /// <summary>
        /// Generate the data necessary to produce the enclosure mesh. The enclosure refers to the part that hangs
        /// over the walkable areas of the map, enclosing the cave. Must first generate floor.
        /// </summary>
        public void GenerateEnclosure(int wallHeight, IHeightMap heightMap = null)
        {
            IMeshBuilder enclosureBuilder = new EnclosureBuilder(floorMesh, wallHeight, heightMap);
            enclosureMesh = enclosureBuilder.Build();
        }

        /// <summary>
        /// Get the mesh for the ceiling/base component. Must first generate ceiling to populate the data. If you plan
        /// to generate walls, do so before calling this method, as generating walls will raise the ceiling mesh. 
        /// </summary>
        public Mesh GetCeilingMesh()
        {
            return CreateMesh(ceilingMesh);
        }

        /// <summary>
        /// Create and return the wall 3D wall mesh. Must first generate walls.
        /// </summary>
        public Mesh GetWallMesh()
        {
            return CreateMesh(wallMesh);
        }

        /// <summary>
        /// Create and return the floor mesh. Must first generate floors.
        /// </summary>
        public Mesh GetFloorMesh()
        {
            return CreateMesh(floorMesh);
        }

        /// <summary>
        /// Create a return the enclosure mesh. Must first generate enclosure.
        /// </summary>
        public Mesh GetEnclosureMesh()
        {
            return CreateMesh(enclosureMesh);
        }

        void ComputeMeshOutlines()
        {
            OutlineGenerator outlineGenerator = new OutlineGenerator(ceilingMesh.vertices, ceilingMesh.triangles);
            outlines = outlineGenerator.GenerateOutlines();
        }

        /// <summary>
        /// Generates a list of arrays of 2D points corresponding to the vertices of the outlines of the walls.
        /// Use after generating meshes.
        /// </summary>
        public List<Vector2[]> GetOutlines()
        {
            ComputeMeshOutlines();
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

        Mesh CreateMesh(MeshData meshData)
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