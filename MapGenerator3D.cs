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
        cave = new GameObject("Cave3D");
        cave.transform.parent = transform;
        generatedMeshes = new List<MapMeshes>();
        foreach (Map subMap in map.SubdivideMap())
        {
            GameObject sector = new GameObject("Sector " + subMap.index);
            sector.transform.parent = cave.transform;
            MapMeshes mapMeshes = GetComponent<MeshGenerator>().Generate3D(subMap);
            CreateObjectFromMesh(mapMeshes.ceilingMesh, "Ceiling", sector, ceilingMaterial);
            GameObject walls = CreateObjectFromMesh(mapMeshes.wallMesh, "Walls", sector, wallMaterial);
            MeshCollider wallCollider = walls.gameObject.AddComponent<MeshCollider>();
            wallCollider.sharedMesh = mapMeshes.wallMesh;
            generatedMeshes.Add(mapMeshes);
        }
    }
}