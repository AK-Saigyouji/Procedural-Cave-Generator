using UnityEngine;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// A 3D map generator. Generates flat cavernous regions and perpendicular walls along the outlines of those regions.
/// The walls receive a mesh collider for collision detection.
/// </summary>
public class CaveGenerator3D : CaveGenerator
{
    [SerializeField]
    int wallHeight = 3;
    [SerializeField]
    Material ceilingMaterial;
    [SerializeField]
    Material wallMaterial;

    public CaveGenerator3D(int length, int width, int wallHeight, Material ceilingMaterial, Material wallMaterial, 
        float mapDensity = 0.5f, string seed = "", bool useRandomSeed = true, int borderSize = 0, int squareSize = 1) 
        : base(length, width, mapDensity, seed, useRandomSeed, borderSize, squareSize)
    {
        this.wallHeight = wallHeight;
        this.ceilingMaterial = ceilingMaterial;
        this.wallMaterial = wallMaterial;
    }

    protected override void GenerateMeshFromMap(Map map)
    {
        IList<Map> submaps = map.SubdivideMap();
        MeshGenerator[] meshGenerators = GetMeshGenerators(submaps);
        cave = CreateChild("Cave3D", transform);
        for (int i = 0; i < submaps.Count; i++)
        {
            GameObject sector = CreateSector(submaps[i].index);
            CreateCeiling(meshGenerators[i], sector);
            CreateWall(meshGenerators[i], sector);
        }
    }

    void CreateCeiling(MeshGenerator meshGenerator, GameObject sector)
    {
        Mesh ceilingMesh = meshGenerator.CreateCeilingMesh();
        CreateObjectFromMesh(ceilingMesh, "Ceiling", sector, ceilingMaterial);
    }

    void CreateWall(MeshGenerator meshGenerator, GameObject sector)
    {
        Mesh wallMesh = meshGenerator.CreateWallMesh(wallHeight);
        GameObject wall = CreateObjectFromMesh(wallMesh, "Walls", sector, wallMaterial);
        AddWallCollider(wall, wallMesh);
    }

    void AddWallCollider(GameObject walls, Mesh wallMesh)
    {
        MeshCollider wallCollider = walls.gameObject.AddComponent<MeshCollider>();
        wallCollider.sharedMesh = wallMesh;
    }
}