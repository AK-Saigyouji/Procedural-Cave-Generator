using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A 2D map generator, intended to be used in 2D mode. Generates flat cavernous regions and edge colliders that run along
/// the outlines of these regions.
/// </summary>
public class CaveGenerator2D : CaveGenerator {

    [SerializeField]
    Material wallMaterial;

    public CaveGenerator2D(int length, int width, Material wallMaterial, float mapDensity = 0.5f, string seed = "", 
        bool useRandomSeed = true, int borderSize = 0, int squareSize = 1)
        : base(length, width, mapDensity, seed, useRandomSeed, borderSize, squareSize)
    {
        this.wallMaterial = wallMaterial;
    }

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
            AddColliders(ceiling);
        }
    }

    void AddColliders(GameObject wall)
    {
        EdgeCollider2D[] currentColliders = wall.GetComponents<EdgeCollider2D>();
        foreach (EdgeCollider2D collider in currentColliders)
        {
            Destroy(collider);
        }

        List<Vector2[]> edgePointsList = meshGenerator.GenerateColliderEdges();
        foreach (Vector2[] edgePoints in edgePointsList)
        {
            EdgeCollider2D edgeCollider = wall.AddComponent<EdgeCollider2D>();
            edgeCollider.points = edgePoints;
        }
    }
}
