using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A 2D map generator, intended to be used in 2D mode. Generates flat cavernous regions and edge colliders that run along
/// the outlines of these regions for collision detection.
/// </summary>
public class MapGenerator2D : MapGenerator {

    [SerializeField]
    Material wallMaterial;

    protected override void GenerateMeshFromMap(Map map)
    {
        cave = CreateChild("Cave2D", transform);
        generatedMeshes = new List<MapMeshes>();
        foreach (Map subMap in map.SubdivideMap(MAP_CHUNK_SIZE))
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
        EdgeCollider2D[] currentColliders = gameObject.GetComponents<EdgeCollider2D>();
        foreach (EdgeCollider2D collider in currentColliders)
        {
            Destroy(collider);
        }

        List<Vector2[]> edgePointsList = meshGenerator.GenerateColliderEdges();
        foreach (Vector2[] edgePoints in edgePointsList)
        {
            EdgeCollider2D edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
            edgeCollider.points = edgePoints;
        }
    }
}
