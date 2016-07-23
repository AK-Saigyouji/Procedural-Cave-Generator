using UnityEngine;
using CaveGeneration.MeshGeneration;

namespace CaveGeneration
{
    public class CaveGeneratorEnclosed : CaveGenerator
    {
        public int wallHeight = 3;
        public Material enclosureMaterial;
        public Material wallMaterial;
        public Material floorMaterial;

        IHeightMap floorHeightMap;
        IHeightMap enclosureHeightMap;

        protected override void PrepareHeightMaps()
        {
            floorHeightMap = GetFloorHeightMap();
            enclosureHeightMap = GetMainHeightMap();
        }

        protected override void PrepareMeshGenerator(MeshGenerator meshGenerator, Map map)
        {
            meshGenerator.GenerateCeiling(map);
            meshGenerator.GenerateWalls(wallHeight);
            meshGenerator.GenerateFloor(map, floorHeightMap);
            meshGenerator.GenerateEnclosure(wallHeight, enclosureHeightMap);
        }

        protected override MapMeshes CreateMeshes(MeshGenerator meshGenerator, int index)
        {
            GameObject sector = CreateSector(index);
            Mesh wallMesh = CreateWall(meshGenerator, sector);
            Mesh floorMesh = CreateFloor(meshGenerator, sector);
            Mesh enclosureMesh = CreateEnclosure(meshGenerator, sector);
            return new MapMeshes(wallMesh, floorMesh, enclosureMesh);
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

        Mesh CreateEnclosure(MeshGenerator meshGenerator, GameObject sector)
        {
            Mesh enclosureMesh = meshGenerator.GetEnclosureMesh();
            CreateGameObjectFromMesh(enclosureMesh, "Enclosure", sector, enclosureMaterial, false);
            return enclosureMesh;
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
                heightMap.Create(mapParameters.Seed.GetHashCode());
            }
            return heightMap;
        }

        IHeightMap GetMainHeightMap()
        {
            HeightMapMain heightMap = GetComponent<HeightMapMain>();
            if (heightMap != null)
            {
                heightMap.Create(mapParameters.Seed.GetHashCode());
            }
            return heightMap;
        }
    }
}
