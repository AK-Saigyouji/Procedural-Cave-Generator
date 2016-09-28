using UnityEngine;
using CaveGeneration.MeshGeneration;
using System.Collections;

using Map = CaveGeneration.MapGeneration.Map;

namespace CaveGeneration
{
    /// <summary>
    /// A 3D cave generator with a 1st person camera in mind. Generates fully enclosed caves, with separate meshes
    /// for the floors, walls and enclosure/ceiling. Generates mesh colliders for the walkable areas and walls, but 
    /// not the enclosure/ceiling above them. 
    /// </summary>
    public sealed class CaveGeneratorEnclosed : CaveGenerator
    {
        [SerializeField] Material enclosureMaterial;
        [SerializeField] Material wallMaterial;
        [SerializeField] Material floorMaterial;

        protected override MeshGenerator PrepareMeshGenerator(Map map)
        {
            MeshGenerator meshGenerator = new MeshGenerator(Map.maxSubmapSize, map.Index.ToString());
            meshGenerator.GenerateEnclosed(MapConverter.ToWallGrid(map), floorHeightMap, mainHeightMap);
            return meshGenerator;
        }

        protected override CaveMeshes CreateMapMeshes(MeshGenerator meshGenerator)
        {
            string index = meshGenerator.Index;
            Transform sector = ObjectFactory.CreateSector(index, Cave.transform).transform;

            Mesh wallMesh = ObjectFactory.CreateComponent(meshGenerator.GetWallMesh(), sector, wallMaterial, "Wall", true);
            Mesh floorMesh = ObjectFactory.CreateComponent(meshGenerator.GetFloorMesh(), sector, floorMaterial, "Floor", true);
            Mesh enclosureMesh = ObjectFactory.CreateComponent(meshGenerator.GetEnclosureMesh(), sector, enclosureMaterial, "Enclosure", false);

            return new CaveMeshes(wallMesh, floorMesh, enclosureMesh);
        }
    }
}
