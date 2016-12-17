/* The primary purpose of this class is to provide a container for all the meshes for an appropriate editor script
 * to convert into mesh assets. */

using UnityEngine;

namespace CaveGeneration.MeshGeneration
{
    /// <summary>
    /// Readonly storage class to hold generated meshes.
    /// </summary>
    public sealed class CaveMeshes
    {
        public Mesh Floor { get; private set; }
        public Mesh Walls { get; private set; }
        public Mesh Ceiling { get; private set; }
        public string Index { get; private set; }

        public CaveMeshes(Mesh floor, Mesh walls, Mesh ceiling, string index)
        {
            Floor = floor;
            Walls = walls;
            Ceiling = ceiling;
            Index = index;
        }
    } 
}