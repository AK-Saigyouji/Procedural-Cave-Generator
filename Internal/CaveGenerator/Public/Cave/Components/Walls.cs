using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Walls : CaveComponent
{
    internal Walls(Mesh mesh, string name) : base(mesh, name)
    {
        AddMeshCollider();
    }
}
