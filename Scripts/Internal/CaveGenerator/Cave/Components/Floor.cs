using UnityEngine;

namespace CaveGeneration
{
    sealed class Floor : CaveComponent
    {
        internal Floor(Mesh mesh, string name) : base(mesh, name)
        {
            AddMeshCollider();
        }
    } 
}
