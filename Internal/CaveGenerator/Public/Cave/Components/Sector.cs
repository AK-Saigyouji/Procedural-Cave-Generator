using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CaveGeneration;
using CaveGeneration.MeshGeneration;

public sealed class Sector
{
    public GameObject GameObject { get; private set; }

    public CaveComponent Ceiling { get; private set; }
    public CaveComponent Walls { get; private set; }
    public CaveComponent Floor { get; private set; }

    internal Sector(CaveMeshes caveMeshes, string index)
    {
        GameObject = new GameObject(AppendIndex("Sector", index));
        GameObject.SetActive(false);

        Ceiling = new Ceiling(caveMeshes.Ceiling, AppendIndex("Ceiling", index));
        Walls   = new Walls(caveMeshes.Walls, AppendIndex("Walls", index));
        Floor   = new Floor(caveMeshes.Floor, AppendIndex("Floor", index));

        AddChild(Ceiling);
        AddChild(Walls);
        AddChild(Floor);
    }

    void AddChild(CaveComponent component)
    {
        component.GameObject.transform.parent = GameObject.transform;
    }

    string AppendIndex(string name, string index)
    {
        return string.Format("{0} {1}", name, index);
    }
}
