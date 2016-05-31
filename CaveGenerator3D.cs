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
    [SerializeField]
    int wallsPerTextureTile = 5;

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
        MeshGenerator[] meshGenerators = PrepareMeshGenerators(submaps);
        cave = CreateChild("Cave3D", transform);
        List<MapMeshes> meshes = new List<MapMeshes>();
        for (int i = 0; i < submaps.Count; i++)
        {
            GameObject sector = CreateSector(submaps[i].index);
            Mesh ceilingMesh = CreateCeiling(meshGenerators[i], sector);
            Mesh wallMesh = CreateWall(meshGenerators[i], sector);
            meshes.Add(new MapMeshes(ceilingMesh, wallMesh));
        }
        generatedMeshes = meshes;
    }

    override protected void PrepareMeshGenerator(MeshGenerator meshGenerator, Map map)
    {
        meshGenerator.GenerateCeiling(map, ceilingTextureDimensions);
        meshGenerator.GenerateWalls(wallHeight, wallsPerTextureTile);
    }

    Mesh CreateCeiling(MeshGenerator meshGenerator, GameObject sector)
    {
        Mesh ceilingMesh = meshGenerator.CreateCeilingMesh();
        CreateObjectFromMesh(ceilingMesh, "Ceiling", sector, ceilingMaterial);
        return ceilingMesh;
    }

    Mesh CreateWall(MeshGenerator meshGenerator, GameObject sector)
    {
        Mesh wallMesh = meshGenerator.CreateWallMesh();
        GameObject wall = CreateObjectFromMesh(wallMesh, "Walls", sector, wallMaterial);
        AddWallCollider(wall, wallMesh);
        return wallMesh;
    }

    void AddWallCollider(GameObject walls, Mesh wallMesh)
    {
        MeshCollider wallCollider = walls.gameObject.AddComponent<MeshCollider>();
        wallCollider.sharedMesh = wallMesh;
    }
}