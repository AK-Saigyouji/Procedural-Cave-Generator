using UnityEngine;
using CaveGeneration.MeshGeneration;

namespace CaveGeneration
{
    /// <summary>
    /// A 3D cave generator with a 1st person camera in mind. Generates fully enclosed caves, with separate meshes
    /// for the floors, walls and enclosure/ceiling. Generates mesh colliders for the walkable areas and walls, but 
    /// not the enclosure/ceiling above them. 
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

        protected override MapMeshes CreateMapMeshes(MeshGenerator meshGenerator, Coord index)
        {
            GameObject sector = CreateSector(index);
            Mesh wallMesh = CreateWall(meshGenerator, sector, index);
            Mesh floorMesh = CreateFloor(meshGenerator, sector, index);
            Mesh enclosureMesh = CreateEnclosure(meshGenerator, sector, index);
            return new MapMeshes(wallMesh, floorMesh, enclosureMesh);
        }

        Mesh CreateWall(MeshGenerator meshGenerator, GameObject sector, Coord index)
        {
            string name = "Walls " + index;
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

        Mesh CreateEnclosure(MeshGenerator meshGenerator, GameObject sector, Coord index)
        {
            string name = "Enclosure " + index;
            Mesh enclosureMesh = meshGenerator.GetEnclosureMesh();
            enclosureMesh.name = name;
            CreateGameObjectFromMesh(enclosureMesh, name, sector, enclosureMaterial);
            return enclosureMesh;
        }

        void AddMeshCollider(GameObject gameObject, Mesh mesh)
        {
            MeshCollider collider = gameObject.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
        }
    }
}
