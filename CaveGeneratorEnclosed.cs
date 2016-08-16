using UnityEngine;
using CaveGeneration.MeshGeneration;
using System.Collections;

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

        protected override IEnumerator CreateMapMeshes(MeshGenerator meshGenerator)
        {
            GameObject sector = CreateSector(meshGenerator.index);
            yield return null;

            Mesh wallMesh = CreateWall(meshGenerator, sector, wallMaterial);
            yield return null;

            Mesh floorMesh = CreateFloor(meshGenerator, sector, floorMaterial);
            yield return null;

            Mesh enclosureMesh = CreateEnclosure(meshGenerator, sector, enclosureMaterial);
            GeneratedMeshes.Add(new MapMeshes(wallMesh, floorMesh, enclosureMesh));
        }
    }
}
