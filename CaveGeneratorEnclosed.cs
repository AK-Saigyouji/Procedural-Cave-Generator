using UnityEngine;
using CaveGeneration.MeshGeneration;

namespace CaveGeneration
{
    /// <summary>
    /// A 3D cave generator with a 1st person camera in mind. Generates fully enclosed caves. Generates mesh colliders 
    /// for the walkable areas and walls, but not the enclosure/ceiling above them. 
    /// </summary>
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
            floorHeightMap = GetHeightMap<HeightMapFloor>(0);
            enclosureHeightMap = GetHeightMap<HeightMapMain>(wallHeight);
        }

        protected override void PrepareMeshGenerator(MeshGenerator meshGenerator, Map map)
        {
            meshGenerator.GenerateEnclosed(map, floorHeightMap, enclosureHeightMap);
        }

        protected override MapMeshes CreateMapMeshes(MeshGenerator meshGenerator, int index)
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
            CreateGameObjectFromMesh(enclosureMesh, "Enclosure", sector, enclosureMaterial);
            return enclosureMesh;
        }

        void AddMeshCollider(GameObject gameObject, Mesh mesh)
        {
            MeshCollider collider = gameObject.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
        }
    }
}
