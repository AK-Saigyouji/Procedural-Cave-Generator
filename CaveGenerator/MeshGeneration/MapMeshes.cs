using UnityEngine;
using System.Collections.Generic;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Storage class to hold generated meshes.
    /// </summary>
    public class MapMeshes
    {
        Mesh[] meshes;

        private MapMeshes() { }

        public MapMeshes(params Mesh[] meshes)
        {
            this.meshes = meshes;
        }

        public IEnumerable<Mesh> Meshes { get { return meshes; } }
    } 
}