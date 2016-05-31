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
        IList<Map> submaps = map.SubdivideMap();
        MeshGenerator[] meshGenerators = PrepareMeshGenerators(submaps);
        List<MapMeshes> meshes = new List<MapMeshes>();
        for (int i = 0; i < submaps.Count; i++)
        {
            GameObject sector = CreateSector(submaps[i].index);
            Mesh mesh = CreateWall(meshGenerators[i], sector);
            meshes.Add(new MapMeshes(ceilingMesh: mesh));
        }
        generatedMeshes = meshes;
    }

    Mesh CreateWall(MeshGenerator meshGenerator, GameObject parent)
    {
        Mesh ceilingMesh = meshGenerator.CreateCeilingMesh();
        GameObject wall = CreateObjectFromMesh(ceilingMesh, "Walls", parent, wallMaterial);
        OrientWall(wall);
        RemoveExistingColliders(wall);
        AddColliders(wall, meshGenerator);
        return ceilingMesh;
    }

    void OrientWall(GameObject wall)
    {
        wall.transform.localRotation = Quaternion.Euler(270f, 0f, 0f);
    }
    
    void RemoveExistingColliders(GameObject wall)
    {
        EdgeCollider2D[] currentColliders = wall.GetComponents<EdgeCollider2D>();
        foreach (EdgeCollider2D collider in currentColliders)
        {
            Destroy(collider);
        }
    }

    void AddColliders(GameObject wall, MeshGenerator meshGenerator)
    {
        List<Vector2[]> edgePointsList = meshGenerator.GenerateColliderEdges();
        foreach (Vector2[] edgePoints in edgePointsList)
        {
            EdgeCollider2D edgeCollider = wall.AddComponent<EdgeCollider2D>();
            edgeCollider.points = edgePoints;
        }
    }
}
