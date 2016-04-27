using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
        foreach (Map subMap in map.SubdivideMap())
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