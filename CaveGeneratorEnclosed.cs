using UnityEngine;
using CaveGeneration.MeshGeneration;
using System.Collections;

namespace CaveGeneration
{
    public class CaveGeneratorEnclosed : CaveGenerator
    {
        public int wallHeight = 3;
        public Material enclosureMaterial;
        public Material wallMaterial;
        public Material floorMaterial;
        public int wallsPerTextureTile = 5;

        protected override void PrepareMeshGenerator(MeshGenerator meshGenerator, Map map)
        {
            meshGenerator.GenerateCeiling(map);
            meshGenerator.GenerateWalls(wallsPerTextureTile, wallHeight);
            meshGenerator.GenerateFloor(map);
            meshGenerator.GenerateEnclosure(wallHeight);
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

        Mesh CreateEnclosure(MeshGenerator meshGenerator, GameObject sector)
        {
            Mesh enclosureMesh = meshGenerator.GetEnclosureMesh();
            GameObject enclosure = CreateGameObjectFromMesh(enclosureMesh, "Enclosure", sector, enclosureMaterial);
            AddMeshCollider(enclosure, enclosureMesh);
            return enclosureMesh;
        }

        void AddMeshCollider(GameObject gameObject, Mesh mesh)
        {
            MeshCollider collider = gameObject.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
        }
    }
}
