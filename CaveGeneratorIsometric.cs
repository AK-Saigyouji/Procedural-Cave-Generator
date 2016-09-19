using UnityEngine;
using System.Collections.Generic;
using CaveGeneration.MeshGeneration;
using System.Linq;
using System.Collections;

namespace CaveGeneration
{
    /// <summary>
    /// A 3D cave generator with an isometric camera in mind. Generates mesh colliders for the walkable floors as well as
    /// the walls around them, but not the 'ceilings'. 
    /// </summary>
    public class CaveGeneratorIsometric : CaveGenerator
    {
        public Material ceilingMaterial;
        public Material wallMaterial;
        public Material floorMaterial;

        override protected void PrepareMeshGenerator(MeshGenerator meshGenerator, Map map)
        {
            meshGenerator.GenerateIsometric(map, floorHeightMap, mainHeightMap);
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

            Mesh ceilingMesh = CreateComponent(meshGenerator.GetCeilingMesh(), sector, ceilingMaterial, "Ceiling", index, false);
            yield return null;

            GeneratedMeshes.Add(new MapMeshes(ceilingMesh, wallMesh, floorMesh));
        }
    } 
}