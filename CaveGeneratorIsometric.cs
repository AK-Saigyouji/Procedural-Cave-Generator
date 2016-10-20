using UnityEngine;
using CaveGeneration.MeshGeneration;

using Map = CaveGeneration.MapGeneration.Map;

namespace CaveGeneration
{
    /// <summary>
    /// A 3D cave generator with an isometric camera in mind. Generates mesh colliders for the walkable floors as well as
    /// the walls around them, but not the ceilings. 
    /// </summary>
    public sealed class CaveGeneratorIsometric : CaveGenerator
    {
        [SerializeField] Material ceilingMaterial;
        [SerializeField] Material wallMaterial;
        [SerializeField] Material floorMaterial;

        public Material CeilingMaterial { get { return ceilingMaterial; } set { ceilingMaterial = value; } }
        public Material WallMaterial    { get { return wallMaterial; }    set { wallMaterial = value; } }
        public Material FloorMaterial   { get { return floorMaterial; }   set { floorMaterial = value; } }

        protected override MeshGenerator PrepareMeshGenerator(MeshGenerator meshGenerator, Map map)
        {
            WallGrid wallGrid = MapConverter.ToWallGrid(map);
            meshGenerator.GenerateIsometric(wallGrid, FloorHeightMap, MainHeightMap);
            return meshGenerator;
        }

        protected override CaveMeshes CreateMapMeshes(MeshGenerator meshGenerator, Transform sector)
        {
            Mesh wallMesh    = ObjectFactory.CreateComponent(meshGenerator.GetWallMesh(), sector, wallMaterial, "Wall", true);
            Mesh floorMesh   = ObjectFactory.CreateComponent(meshGenerator.GetFloorMesh(), sector, floorMaterial, "Floor", true);
            Mesh ceilingMesh = ObjectFactory.CreateComponent(meshGenerator.GetCeilingMesh(), sector, ceilingMaterial, "Ceiling", false);

            return new CaveMeshes(ceilingMesh, wallMesh, floorMesh);
        }
    } 
}