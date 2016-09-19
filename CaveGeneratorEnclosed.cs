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
        public Material enclosureMaterial;
        public Material wallMaterial;
        public Material floorMaterial;

        protected override void PrepareMeshGenerator(MeshGenerator meshGenerator, Map map)
        {
            meshGenerator.GenerateEnclosed(map, floorHeightMap, mainHeightMap);
        }

        protected override IEnumerator CreateMapMeshes(MeshGenerator meshGenerator)
        {
            Coord index = meshGenerator.index;
            Transform sector = CreateSector(index).transform;
            yield return null;

            Mesh wallMesh = CreateComponent(meshGenerator.GetWallMesh(), sector, wallMaterial, "Wall", index, true);
            yield return null;

            Mesh floorMesh = CreateComponent(meshGenerator.GetFloorMesh(), sector, floorMaterial, "Floor", index, true);
            yield return null;

            Mesh enclosureMesh = CreateComponent(meshGenerator.GetEnclosureMesh(), sector, enclosureMaterial, "Enclosure", index, false);
            yield return null;

            GeneratedMeshes.Add(new MapMeshes(wallMesh, floorMesh, enclosureMesh));
        }
    }
}
