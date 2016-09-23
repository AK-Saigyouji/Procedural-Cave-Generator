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
    public sealed class CaveGeneratorEnclosed : CaveGenerator
    {
        [SerializeField] Material enclosureMaterial;
        [SerializeField] Material wallMaterial;
        [SerializeField] Material floorMaterial;

        protected override void PrepareMeshGenerator(MeshGenerator meshGenerator, Map map)
        {
            meshGenerator.GenerateEnclosed(map, floorHeightMap, mainHeightMap);
        }

        protected override MapMeshes CreateMapMeshes(MeshGenerator meshGenerator)
        {
            Coord index = meshGenerator.index;
            Transform sector = ObjectFactory.CreateSector(index, Cave.transform).transform;

            Mesh wallMesh = ObjectFactory.CreateComponent(meshGenerator.GetWallMesh(), sector, wallMaterial, "Wall", index, true);
            Mesh floorMesh = ObjectFactory.CreateComponent(meshGenerator.GetFloorMesh(), sector, floorMaterial, "Floor", index, true);
            Mesh enclosureMesh = ObjectFactory.CreateComponent(meshGenerator.GetEnclosureMesh(), sector, enclosureMaterial, "Enclosure", index, false);

            return new MapMeshes(wallMesh, floorMesh, enclosureMesh);
        }
    }
}
