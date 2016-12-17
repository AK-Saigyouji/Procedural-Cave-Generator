using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Floor : CaveComponent
{
    internal Floor(Mesh mesh, string name) : base(mesh, name)
    {
        AddMeshCollider();
    }
}
