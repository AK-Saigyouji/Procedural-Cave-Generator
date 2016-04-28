using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A 3D map generator. Generates flat cavernous regions and perpendicular walls along these outlines of those regions.
/// The walls receive a mesh collider for collision detection.
/// </summary>
public class MapGenerator3D : MapGenerator
{
    [SerializeField]
    Material ceilingMaterial;
    [SerializeField]
    Material wallMaterial;

    protected override void GenerateMeshFromMap(Map map)
    {
        cave = CreateChild("Cave3D", transform);
        generatedMeshes = new List<MapMeshes>();
        foreach (Map subMap in map.SubdivideMap(MAP_CHUNK_SIZE))
        {
            GameObject sector = CreateChild("Sector " + subMap.index, cave.transform);
            MapMeshes mapMeshes = meshGenerator.Generate3D(subMap);
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