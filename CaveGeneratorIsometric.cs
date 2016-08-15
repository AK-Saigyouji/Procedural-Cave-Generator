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

        protected override MapMeshes CreateMapMeshes(MeshGenerator meshGenerator, Coord index)
        {
            GameObject sector = CreateSector(index);
            Mesh ceilingMesh = CreateCeiling(meshGenerator, sector, index);
            Mesh wallMesh = CreateWall(meshGenerator, sector, index);
            Mesh floorMesh = CreateFloor(meshGenerator, sector, index);
            return new MapMeshes(ceilingMesh, wallMesh, floorMesh);
        }

        Mesh CreateCeiling(MeshGenerator meshGenerator, GameObject sector, Coord index)
        {
            string name = "Ceiling " + index;
            Mesh ceilingMesh = meshGenerator.GetCeilingMesh();
            ceilingMesh.name = name;
            CreateGameObjectFromMesh(ceilingMesh, name, sector, ceilingMaterial);
            return ceilingMesh;
        }

        Mesh CreateWall(MeshGenerator meshGenerator, GameObject sector, Coord index)
        {
            string name = "Wall " + index;
            Mesh wallMesh = meshGenerator.GetWallMesh();
            wallMesh.name = name;
            GameObject wall = CreateGameObjectFromMesh(wallMesh, name, sector, wallMaterial);
            AddMeshCollider(wall, wallMesh);
            return wallMesh;
        }

        Mesh CreateFloor(MeshGenerator meshGenerator, GameObject sector, Coord index)
        {
            string name = "Floor " + index;
            Mesh floorMesh = meshGenerator.GetFloorMesh();
            floorMesh.name = name;
            GameObject floor = CreateGameObjectFromMesh(floorMesh, name, sector, floorMaterial);
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