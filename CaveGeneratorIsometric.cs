using UnityEngine;
using System.Collections.Generic;
using CaveGeneration.MeshGeneration;
using System.Linq;

namespace CaveGeneration
{
    /// <summary>
    /// A 3D cave generator with an isometric camera in mind. Generates mesh colliders for the walkable floors as well as
    /// the walls around them, but not the 'ceilings'. 
    /// </summary>
    public class CaveGeneratorIsometric : CaveGenerator
    {
        public int wallHeight = 3;
        public Material ceilingMaterial;
        public Material wallMaterial;
        public Material floorMaterial;

        IHeightMap ceilingHeightMap;
        IHeightMap floorHeightMap;

        protected override void PrepareHeightMaps()
        {
            floorHeightMap = GetHeightMap<HeightMapFloor>(0);
            ceilingHeightMap = GetHeightMap<HeightMapMain>(wallHeight);
        }

        override protected void PrepareMeshGenerator(MeshGenerator meshGenerator, Map map)
        {
            meshGenerator.GenerateIsometric(map, floorHeightMap, ceilingHeightMap);
        }

        protected override MapMeshes CreateMapMeshes(MeshGenerator meshGenerator, int index)
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
            CreateGameObjectFromMesh(ceilingMesh, "Ceiling", sector, ceilingMaterial);
            return ceilingMesh;
        }

        Mesh CreateWall(MeshGenerator meshGenerator, GameObject sector)
        {
            Mesh wallMesh = meshGenerator.GetWallMesh();
            GameObject wall = CreateGameObjectFromMesh(wallMesh, "Walls", sector, wallMaterial);
            AddMeshCollider(wall, wallMesh);
            return wallMesh;
        }

        Mesh CreateFloor(MeshGenerator meshGenerator, GameObject sector)
        {
            Mesh floorMesh = meshGenerator.GetFloorMesh();
            GameObject floor = CreateGameObjectFromMesh(floorMesh, "Floor", sector, floorMaterial);
            AddMeshCollider(floor, floorMesh);
            return floorMesh;
        }

        void AddMeshCollider(GameObject gameObject, Mesh mesh)
        {
            MeshCollider collider = gameObject.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
        }
    } 
}