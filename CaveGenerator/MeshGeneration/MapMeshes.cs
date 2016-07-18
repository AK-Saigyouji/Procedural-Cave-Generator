using UnityEngine;
using System.Collections.Generic;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Storage class to hold generated meshes.
    /// </summary>
    public class MapMeshes
    {
        public Mesh wallMesh { get; private set; }
        public Mesh ceilingMesh { get; private set; }
        public Mesh floorMesh { get; private set; }

        private MapMeshes() { }

        public MapMeshes(Mesh ceilingMesh = null, Mesh wallMesh = null, Mesh floorMesh = null)
        {
            this.ceilingMesh = ceilingMesh;
            this.wallMesh = wallMesh;
            this.floorMesh = floorMesh;
        }

        /// <summary>
        /// Get an iterable object containing the meshes that are not null.
        /// </summary>
        public IEnumerable<Mesh> ExtractMeshes()
        {
            if (floorMesh != null)
            {
                yield return floorMesh;
            }
            if (wallMesh != null)
            {
                yield return wallMesh;
            }
            if (ceilingMesh != null)
            {
                yield return ceilingMesh;
            }
        }
    } 
}