using UnityEngine;

namespace CaveGeneration
{
    sealed class Walls : CaveComponent
    {
        internal Walls(Mesh mesh, string name) : base(mesh, name)
        {
            AddMeshCollider();
        }
    }  
}