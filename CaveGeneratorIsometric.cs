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
        public int wallHeight = 3;
        public Material ceilingMaterial;
        public Material wallMaterial;
        public Material floorMaterial;

        IHeightMap ceilingHeightMap;
        IHeightMap floorHeightMap;

        protected override void PrepareHeightMaps()
        {
            floorHeightMap = GetHeightMap<HeightMapFloor>(0);
            ceilingHeightMap = GetHeightMap<HeightMapMain>(wallHeight);
        }

        override protected void PrepareMeshGenerator(MeshGenerator meshGenerator, Map map)
        {
            meshGenerator.GenerateIsometric(map, floorHeightMap, ceilingHeightMap);
        }

        protected override IEnumerator CreateMapMeshes(MeshGenerator meshGenerator)
        {
            GameObject sector = CreateSector(meshGenerator.index);

            Mesh ceilingMesh = CreateCeiling(meshGenerator, sector, ceilingMaterial);
            yield return null;

            Mesh wallMesh = CreateWall(meshGenerator, sector, ceilingMaterial);
            yield return null;

            Mesh floorMesh = CreateFloor(meshGenerator, sector, ceilingMaterial);
            GeneratedMeshes.Add(new MapMeshes(ceilingMesh, wallMesh, floorMesh));
        }
    } 
}