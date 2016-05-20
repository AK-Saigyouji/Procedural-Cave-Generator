using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A 3D map generator. Generates flat cavernous regions and perpendicular walls along these outlines of those regions.
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
        cave = CreateChild("Cave3D", transform);
        generatedMeshes = new List<MapMeshes>();
        foreach (Map subMap in map.SubdivideMap())
        {
            GameObject sector = CreateChild("Sector " + subMap.index, cave.transform);
            MapMeshes mapMeshes = meshGenerator.Generate3D(subMap, wallHeight);
            CreateObjectFromMesh(mapMeshes.ceilingMesh, "Ceiling", sector, ceilingMaterial);
            GameObject walls = CreateObjectFromMesh(mapMeshes.wallMesh, "Walls", sector, wallMaterial);
            AddWallCollider(walls, mapMeshes);
            generatedMeshes.Add(mapMeshes);
        }
    }

    void AddWallCollider(GameObject walls, MapMeshes mapMeshes)
    {
        MeshCollider wallCollider = walls.gameObject.AddComponent<MeshCollider>();
        wallCollider.sharedMesh = mapMeshes.wallMesh;
    }
}