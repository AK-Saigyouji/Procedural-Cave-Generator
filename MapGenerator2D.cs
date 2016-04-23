using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator2D : MapGenerator {

    [SerializeField]
    Material wallMaterial;

    protected override void GenerateMeshFromMap(Map map)
    {
        cave = new GameObject("Cave2D");
        cave.transform.parent = transform;
        generatedMeshes = new List<MapMeshes>();
        foreach (Map subMap in map.SubdivideMap())
        {
            GameObject sector = new GameObject("Sector " + subMap.index);
            sector.transform.parent = cave.transform;
            MeshGenerator meshGenerator = GetComponent<MeshGenerator>();
            MapMeshes mapMeshes = meshGenerator.Generate2D(subMap);
            GameObject ceiling = CreateObjectFromMesh(mapMeshes.ceilingMesh, "Wall", sector, wallMaterial);
            ceiling.transform.localRotation = Quaternion.Euler(270f, 0f, 0f);
            List<Vector2[]> edgePointsList = meshGenerator.Generate2DColliders();
            foreach (Vector2[] edgePoints in edgePointsList)
            {
                EdgeCollider2D edgeCollider = sector.AddComponent<EdgeCollider2D>();
                edgeCollider.points = edgePoints;
            }
            generatedMeshes.Add(mapMeshes);
        }
    }
}
