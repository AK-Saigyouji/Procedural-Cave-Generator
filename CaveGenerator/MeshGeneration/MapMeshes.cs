using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Readonly storage class to hold generated meshes.
    /// </summary>
    public sealed class MapMeshes
    {
        readonly System.Collections.ObjectModel.ReadOnlyCollection<Mesh> meshes;

        public MapMeshes(params Mesh[] meshes)
        {
            this.meshes = meshes.ToList().AsReadOnly();
        }

        public IEnumerable<Mesh> Meshes { get { return meshes; } }
    } 
}