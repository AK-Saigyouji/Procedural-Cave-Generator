using UnityEngine;
using System.Collections.Generic;
using CaveGeneration.MeshGeneration;
using System.Linq;

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
        public Material floorMaterial;

        IHeightMap ceilingHeightMap;
        IHeightMap floorHeightMap;

        protected override void PrepareHeightMaps()
        {
            floorHeightMap = GetFloorHeightMap();
            ceilingHeightMap = GetMainHeightMap();
        }

        override protected void PrepareMeshGenerator(MeshGenerator meshGenerator, Map map)
        {
            meshGenerator.GenerateCeiling(map);
            meshGenerator.GenerateWalls(wallHeight, ceilingHeightMap);
            meshGenerator.GenerateFloor(map, floorHeightMap);
        }

        protected override MapMeshes CreateMeshes(MeshGenerator meshGenerator, int index)
        {
            GameObject sector = CreateSector(index);
            Mesh ceilingMesh = CreateCeiling(meshGenerator, sector);
            Mesh wallMesh = CreateWall(meshGenerator, sector);
            Mesh floorMesh = CreateFloor(meshGenerator, sector);
            return new MapMeshes(ceilingMesh, wallMesh, floorMesh);
        }

        Mesh CreateCeiling(MeshGenerator meshGenerator, GameObject sector)
        {
            Mesh ceilingMesh = meshGenerator.GetCeilingMesh();
            CreateGameObjectFromMesh(ceilingMesh, "Ceiling", sector, ceilingMaterial, false);
            return ceilingMesh;
        }

        Mesh CreateWall(MeshGenerator meshGenerator, GameObject sector)
        {
            Mesh wallMesh = meshGenerator.GetWallMesh();
            GameObject wall = CreateGameObjectFromMesh(wallMesh, "Walls", sector, wallMaterial, true);
            AddMeshCollider(wall, wallMesh);
            return wallMesh;
        }

        Mesh CreateFloor(MeshGenerator meshGenerator, GameObject sector)
        {
            Mesh floorMesh = meshGenerator.GetFloorMesh();
            GameObject floor = CreateGameObjectFromMesh(floorMesh, "Floor", sector, floorMaterial, false);
            AddMeshCollider(floor, floorMesh);
            return floorMesh;
        }

        void AddMeshCollider(GameObject gameObject, Mesh mesh)
        {
            MeshCollider collider = gameObject.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
        }

        IHeightMap GetFloorHeightMap()
        {
            HeightMapFloor heightMap = GetComponent<HeightMapFloor>();
            if (heightMap != null)
            {
                heightMap.Create(seed.GetHashCode());
            }
            return heightMap;
        }

        IHeightMap GetMainHeightMap()
        {
            HeightMapMain heightMap = GetComponent<HeightMapMain>();
            if (heightMap != null)
            {
                heightMap.Create(seed.GetHashCode());
            }
            return heightMap;
        }
    } 
}