using System.Collections.Generic;
using System.Linq;
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
        /// Generate the data necessary to produce mesh for isometric type cave. Generates ceiling, wall and floor meshes.
        /// </summary>
        public void GenerateIsometric(Map map, int wallHeight, IHeightMap floorHeightMap, IHeightMap ceilingHeightMap)
        {
            mapIndex = map.index;
            GenerateCeiling(map, wallHeight, ceilingHeightMap);
            ComputeMeshOutlines(ceilingMesh);
            GenerateWallsFromCeiling(wallHeight);
            GenerateFloor(map, floorHeightMap);
        }

        /// <summary>
        /// Generate the data necessary to produce meshes for enclosed cave. Generates floor, wall and enclosure meshes.
        /// </summary>
        public void GenerateEnclosed(Map map, int wallHeight, IHeightMap floorHeightMap, IHeightMap enclosureHeightMap)
        {
            mapIndex = map.index;
            GenerateFloor(map, floorHeightMap);
            ComputeMeshOutlines(floorMesh);
            ReverseOutlines();
            GenerateEnclosure(wallHeight, enclosureHeightMap);
            GenerateWallsFromEnclosure(wallHeight);
        }

        /// <summary>
        /// Generate the data necessary to produce meshes for 2D cave. Generates floor and ceiling meshes.
        /// </summary>
        public void Generate2D(Map map)
        {
            mapIndex = map.index;
            GenerateCeiling(map, 0, null);
            GenerateFloor(map, null);
            ComputeMeshOutlines(ceilingMesh);
        }

        /// <summary>
        /// Get the mesh for the ceiling component. Check the docstring for the type of cave you generated to ensure
        /// it produces this type of mesh.
        /// </summary>
        public Mesh GetCeilingMesh()
        {
            return CreateMesh(ceilingMesh);
        }

        /// <summary>
        /// Get the mesh for the wall component. Check the docstring for the type of cave you generated to ensure
        /// it produces this type of mesh.
        /// </summary>
        public Mesh GetWallMesh()
        {
            return CreateMesh(wallMesh);
        }

        /// <summary>
        /// Get the mesh for the floor component. Check the docstring for the type of cave you generated to ensure
        /// it produces this type of mesh.
        /// </summary>
        public Mesh GetFloorMesh()
        {
            return CreateMesh(floorMesh);
        }

        /// <summary>
        /// Get the mesh for the enclosure component. Check the docstring for the type of cave you generated to ensure
        /// it produces this type of mesh.
        /// </summary>
        public Mesh GetEnclosureMesh()
        {
            return CreateMesh(enclosureMesh);
        }

        /// <summary>
        /// Generates a list of arrays of 2D points corresponding to the vertices of the outlines of the walls.
        /// Use after generating meshes.
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

        void GenerateCeiling(Map map, int wallHeight, IHeightMap ceilingHeightMap)
        {
            IMeshBuilder ceilingBuilder = new CeilingBuilder(map, wallHeight, ceilingHeightMap);
            ceilingMesh = ceilingBuilder.Build();
        }

        void GenerateWallsFromCeiling(int wallHeight)
        {
            IMeshBuilder wallBuilder = new WallBuilder(ceilingMesh.vertices, outlines);
            wallMesh = wallBuilder.Build();
        }

        void GenerateWallsFromEnclosure(int wallHeight)
        {
            IMeshBuilder wallBuilder = new WallBuilder(enclosureMesh.vertices, outlines);
            wallMesh = wallBuilder.Build();
        }

        void GenerateFloor(Map map, IHeightMap heightMap)
        {
            IMeshBuilder floorBuilder = new FloorBuilder(map, heightMap);
            floorMesh = floorBuilder.Build();
        }

        void GenerateEnclosure(int wallHeight, IHeightMap heightMap)
        {
            IMeshBuilder enclosureBuilder = new EnclosureBuilder(floorMesh, wallHeight, heightMap);
            enclosureMesh = enclosureBuilder.Build();
        }

        void ComputeMeshOutlines(MeshData mesh)
        {
            OutlineGenerator outlineGenerator = new OutlineGenerator(mesh.vertices, mesh.triangles);
            outlines = outlineGenerator.GenerateOutlines();
        }

        void ReverseOutlines()
        {
            foreach (Outline outline in outlines)
            {
                outline.Reverse();
            }
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