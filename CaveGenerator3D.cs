using UnityEngine;
using System.Collections.Generic;
using CaveGeneration.MeshGeneration;

namespace CaveGeneration
{
    /// <summary>
    /// A 3D map generator. Generates flat cavernous regions and perpendicular walls along the outlines of those regions.
    /// The walls receive a mesh collider for collision detection.
    /// </summary>
    public class CaveGenerator3D : CaveGenerator
    {
        public int wallHeight = 3;
        public Material ceilingMaterial;
        public Material wallMaterial;
        public Vector2 ceilingTextureDimensions = new Vector2(100f, 100f);
        public int wallsPerTextureTile = 5;

        protected override GameObject GenerateCaveFromMap(Map map)
        {
            IList<Map> submaps = map.Subdivide();
            MeshGenerator[] meshGenerators = PrepareMeshGenerators(submaps);
            cave = CreateChild("Cave3D", transform);
            List<MapMeshes> meshes = new List<MapMeshes>();
            for (int i = 0; i < submaps.Count; i++)
            {
                GameObject sector = CreateSector(submaps[i].index);
                Mesh ceilingMesh = CreateCeiling(meshGenerators[i], sector);
                Mesh wallMesh = CreateWall(meshGenerators[i], sector);
                meshes.Add(new MapMeshes(ceilingMesh, wallMesh));
            }
            generatedMeshes = meshes;
            return cave;
        }

        override protected void PrepareMeshGenerator(MeshGenerator meshGenerator, Map map)
        {
            meshGenerator.GenerateCeiling(map, ceilingTextureDimensions);
            meshGenerator.GenerateWalls(wallsPerTextureTile, wallHeight);
        }

        Mesh CreateCeiling(MeshGenerator meshGenerator, GameObject sector)
        {
            Mesh ceilingMesh = meshGenerator.GetCeilingMesh();
            CreateObjectFromMesh(ceilingMesh, "Ceiling", sector, ceilingMaterial);
            return ceilingMesh;
        }

        Mesh CreateWall(MeshGenerator meshGenerator, GameObject sector)
        {
            Mesh wallMesh = meshGenerator.GetWallMesh();
            GameObject wall = CreateObjectFromMesh(wallMesh, "Walls", sector, wallMaterial);
            AddWallCollider(wall, wallMesh);
            return wallMesh;
        }

        void AddWallCollider(GameObject walls, Mesh wallMesh)
        {
            MeshCollider wallCollider = walls.AddComponent<MeshCollider>();
            wallCollider.sharedMesh = wallMesh;
        }
    } 
}