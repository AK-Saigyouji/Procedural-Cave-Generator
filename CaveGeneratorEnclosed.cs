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

        public Material EnclosureMaterial { get { return enclosureMaterial; } set { enclosureMaterial = value; } }
        public Material WallMaterial      { get { return wallMaterial; }      set { wallMaterial = value; } }
        public Material FloorMaterial     { get { return floorMaterial; }     set { floorMaterial = value; } }

        protected override MeshGenerator PrepareMeshGenerator(MeshGenerator meshGenerator, Map map)
        {
            WallGrid wallGrid = MapConverter.ToWallGrid(map);
            meshGenerator.GenerateEnclosed(wallGrid, FloorHeightMap, MainHeightMap);
            return meshGenerator;
        }

        protected override CaveMeshes CreateMapMeshes(MeshGenerator meshGenerator, Transform sector)
        {
            Mesh wallMesh      = ObjectFactory.CreateComponent(meshGenerator.GetWallMesh(), sector, wallMaterial, "Wall", true);
            Mesh floorMesh     = ObjectFactory.CreateComponent(meshGenerator.GetFloorMesh(), sector, floorMaterial, "Floor", true);
            Mesh enclosureMesh = ObjectFactory.CreateComponent(meshGenerator.GetEnclosureMesh(), sector, enclosureMaterial, "Enclosure", false);

            return new CaveMeshes(wallMesh, floorMesh, enclosureMesh);
        }
    }
}
