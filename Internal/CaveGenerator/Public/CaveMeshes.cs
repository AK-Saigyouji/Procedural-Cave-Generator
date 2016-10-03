/* The primary purpose of this class is to provide a container for all the meshes for the an appropriate editor script
 * to convert into mesh assets. */

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Readonly storage class to hold generated meshes.
    /// </summary>
    public sealed class CaveMeshes
    {
        readonly IList<Mesh> meshes;

        public CaveMeshes(params Mesh[] meshes)
        {
            this.meshes = meshes.ToList().AsReadOnly();
        }

        public IEnumerable<Mesh> Meshes { get { return meshes; } }
    } 
}