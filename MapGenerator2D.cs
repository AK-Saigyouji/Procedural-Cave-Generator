using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator2D : MapGenerator {

    [SerializeField]
    Material wallMaterial;

    protected override void GenerateMeshFromMap(Map map)
    {
        cave = CreateChild("Cave2D", transform);
        generatedMeshes = new List<MapMeshes>();
        foreach (Map subMap in map.SubdivideMap())
        {
            GameObject sector = CreateChild("sector " + subMap.index, cave.transform);
            MapMeshes mapMeshes = meshGenerator.Generate2D(subMap);
            GameObject ceiling = CreateObjectFromMesh(mapMeshes.ceilingMesh, "Wall", sector, wallMaterial);
            ceiling.transform.localRotation = Quaternion.Euler(270f, 0f, 0f);
            generatedMeshes.Add(mapMeshes);
        }
    }

    void AddColliders(GameObject gameObject)
    {
        List<Vector2[]> edgePointsList = meshGenerator.Generate2DColliders();
        foreach (Vector2[] edgePoints in edgePointsList)
        {
            EdgeCollider2D edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
            edgeCollider.points = edgePoints;
        }
    }
}
