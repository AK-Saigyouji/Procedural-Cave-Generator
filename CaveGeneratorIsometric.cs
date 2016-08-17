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
            GameObject sector = CreateSector(meshGenerator.index);
            yield return null;

            Mesh ceilingMesh = CreateCeiling(meshGenerator, sector, ceilingMaterial);
            yield return null;

            Mesh wallMesh = CreateWall(meshGenerator, sector, wallMaterial);
            yield return null;

            Mesh floorMesh = CreateFloor(meshGenerator, sector, floorMaterial);
            GeneratedMeshes.Add(new MapMeshes(ceilingMesh, wallMesh, floorMesh));
        }
    } 
}