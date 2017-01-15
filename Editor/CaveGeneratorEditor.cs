using UnityEngine;
using UnityEditor;
using CaveGeneration;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(CaveGeneratorUI))]
public class CaveGeneratorEditor : Editor
{
    // Asset folder names
    const string ROOT_FOLDER = "Assets";
    const string CAVE_FOLDER = "GeneratedCave";
    const string FLOOR_FOLDER = "FloorMeshes";
    const string WALL_FOLDER = "WallMeshes";
    const string CEILING_FOLDER = "CeilingMeshes";

    const string CAVE_NAME = "Cave";
    const string PREFAB_NAME = "Cave.prefab";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        CaveGeneratorUI caveGenerator = (CaveGeneratorUI)target;
        if (Application.isPlaying)
        {
            if (GUILayout.Button("Generate New Map"))
            {
                DestroyCave();
                caveGenerator.Generate();
            }

            if (GUILayout.Button("Create Prefab"))
            {
                TryCreatePrefab();
                DestroyCave();
            }
        }
    }

    void DestroyCave()
    {
        Transform[] caves = FindChildCaves();
        foreach (Transform child in caves)
        {
            Destroy(child.gameObject);
        }
    }

    Transform[] FindChildCaves()
    {
        var generator = (CaveGeneratorUI)target;
        var children = new List<Transform>();
        foreach (Transform child in generator.transform)
        {
            children.Add(child);
        }
        Transform[] childCaves = children.Where(child => child.name == CAVE_NAME).ToArray();
        return childCaves;
    }

    void TryCreatePrefab()
    {
        // The cavegenerator should have only one cave as a child.
        Transform[] childCaves = FindChildCaves();

        if (childCaves.Length == 0)
        {
            Debug.LogError("No cave found to convert. Cave must be a child of this generator and labelled " + CAVE_NAME);
            return;
        }

        if (childCaves.Length > 1)
        {
            Debug.LogError("Unexpected: multiple caves found under this generator. Must have only one.");
            return;
        }

        GameObject cave = childCaves[0].gameObject;
        CreatePrefab(cave);
    }

    void CreatePrefab(GameObject cave)
    {
        string caveFolderPath = EditorHelpers.CreateFolder(ROOT_FOLDER, CAVE_FOLDER);

        cave = CreateCavePrefab(cave, caveFolderPath);
        try
        {
            CreateMeshAssets(cave.transform, caveFolderPath);
        }
        catch (System.InvalidOperationException)
        {
            AssetDatabase.DeleteAsset(caveFolderPath); 
            throw;
        }
    }

    void CreateMeshAssets(Transform cave, string path)
    {
        string floorFolder   = EditorHelpers.CreateFolder(path, FLOOR_FOLDER);
        string ceilingFolder = EditorHelpers.CreateFolder(path, CEILING_FOLDER);
        string wallFolder    = EditorHelpers.CreateFolder(path, WALL_FOLDER);
        foreach (Transform sector in cave.transform)
        {
            foreach (Transform component in sector)
            {
                string name = component.name;

                if (name.Contains(Sector.floorName))
                {
                    CreateMeshAsset(component, floorFolder);
                }
                else if (name.Contains(Sector.ceilingName))
                {
                    CreateMeshAsset(component, ceilingFolder);
                }
                else if (name.Contains(Sector.wallName))
                {
                    CreateMeshAsset(component, wallFolder);
                }
                else
                {
                    throw new System.InvalidOperationException("Unexpected cave hierarchy: unidentified sector child.");
                }
            }
        }
    }

    void CreateMeshAsset(Transform component, string path)
    {
        Mesh mesh = ExtractMesh(component);
        string name = string.Format("{0}.asset", mesh.name);
        string assetPath = EditorHelpers.AppendToPath(path, name);
        AssetDatabase.CreateAsset(mesh, assetPath);
    }

    GameObject CreateCavePrefab(GameObject cave, string path)
    {
        string cavePath = EditorHelpers.AppendToPath(path, PREFAB_NAME);
        return PrefabUtility.CreatePrefab(cavePath, cave);
    }

    Mesh ExtractMesh(Transform component)
    {
        const string errorMessage = "Prefab creation failed, unexpected cave hierarchy: sector child with no mesh.";
        MeshFilter meshFilter = component.GetComponent<MeshFilter>();

        if (meshFilter == null)
            throw new System.InvalidOperationException(errorMessage);

        Mesh mesh = meshFilter.sharedMesh;

        if (mesh == null)
            throw new System.InvalidOperationException(errorMessage);

        return mesh;
    }
}
