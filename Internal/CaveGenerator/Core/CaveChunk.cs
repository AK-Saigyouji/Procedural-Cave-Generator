using CaveGeneration.MapGeneration;
using CaveGeneration.MeshGeneration;

namespace CaveGeneration
{
    /// <summary>
    /// Simple wrapper for CaveMeshes, giving it an index representing its location in a grid of CaveMeshes objects.
    /// </summary>
    sealed class CaveMeshChunk
    {
        public CaveMeshes CaveMeshes { get; private set; }
        public Coord Index { get; private set; }

        public CaveMeshChunk(CaveMeshes caveMeshes, Coord index)
        {
            CaveMeshes = caveMeshes;
            Index = index;
        }
    } 
}