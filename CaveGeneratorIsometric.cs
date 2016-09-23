using UnityEngine;
using CaveGeneration.MeshGeneration;

namespace CaveGeneration
{
    /// <summary>
    /// A 3D cave generator with an isometric camera in mind. Generates mesh colliders for the walkable floors as well as
    /// the walls around them, but not the 'ceilings'. 
    /// </summary>
    public sealed class CaveGeneratorIsometric : CaveGenerator
    {
        [SerializeField] Material ceilingMaterial;
        [SerializeField] Material wallMaterial;
        [SerializeField] Material floorMaterial;

        override protected void PrepareMeshGenerator(MeshGenerator meshGenerator, Map map)
        {
            meshGenerator.GenerateIsometric(map, floorHeightMap, mainHeightMap);
        }

        protected override MapMeshes CreateMapMeshes(MeshGenerator meshGenerator)
        {
            Coord index = meshGenerator.index;
            Transform sector = ObjectFactory.CreateSector(index, Cave.transform).transform;
            
            Mesh wallMesh = ObjectFactory.CreateComponent(meshGenerator.GetWallMesh(), sector, wallMaterial, "Wall", index, true);
            Mesh floorMesh = ObjectFactory.CreateComponent(meshGenerator.GetFloorMesh(), sector, floorMaterial, "Floor", index, true);
            Mesh ceilingMesh = ObjectFactory.CreateComponent(meshGenerator.GetCeilingMesh(), sector, ceilingMaterial, "Ceiling", index, false);

            return new MapMeshes(ceilingMesh, wallMesh, floorMesh);
        }
    } 
}