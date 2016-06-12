using UnityEngine;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Storage class to hold generated meshes.
    /// </summary>
    public class MapMeshes
    {
        public Mesh wallMesh { get; private set; }
        public Mesh ceilingMesh { get; private set; }

        private MapMeshes() { }

        public MapMeshes(Mesh ceilingMesh = null, Mesh wallMesh = null)
        {
            this.ceilingMesh = ceilingMesh;
            this.wallMesh = wallMesh;
        }
    } 
}