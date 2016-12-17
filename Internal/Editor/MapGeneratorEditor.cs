using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;
using CaveGeneration;
using CaveGeneration.MeshGeneration;
using System.Collections.Generic;

[CustomEditor(typeof(CaveGenerator), true)]
public class MapGeneratorEditor : Editor {

    const string ROOT_FOLDER = "Assets";
    const string CAVE_FOLDER = "GeneratedCave";
    const string FLOOR_FOLDER = "FloorMeshes";
    const string WALL_FOLDER = "WallMeshes";
    const string CEILING_FOLDER = "CeilingMeshes";

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        CaveGenerator caveGenerator = (CaveGenerator)target;
        if (Application.isPlaying)
        {
            if (GUILayout.Button("Generate New Map"))
            {
                caveGenerator.Generate();
            }

            if (GUILayout.Button("Create Prefab"))
            {
                CreatePrefab(caveGenerator);
            }
        }
    }

    void CreatePrefab(CaveGenerator caveGenerator)
    {
        Cave cave = caveGenerator.ExtractCave();
        Assert.IsNotNull(cave, "Internal error: extracted null cave.");

        string path = CreateFolder(ROOT_FOLDER, CAVE_FOLDER);

        CreateMeshAssets(cave.GetFloors(), FLOOR_FOLDER, path);
        CreateMeshAssets(cave.GetCeilings(), CEILING_FOLDER, path);
        CreateMeshAssets(cave.GetWalls(), WALL_FOLDER, path);
        CreateCavePrefab(cave.GameObject, path);

        Destroy(cave.GameObject);
    }

    void CreateMeshAssets(IEnumerable<CaveComponent> components, string folderName, string path)
    {
        string folderPath = CreateFolder(path, folderName);
        foreach (CaveComponent component in components)
        {
            CreateMeshAsset(component.Mesh, component.Name, folderPath);
        }
    }

    /// <summary>
    /// Similar to AssetDatabase.CreateFolder but returns the path to the created folder instead of the guid.
    /// </summary>
    string CreateFolder(string path, string name)
    {
        string guid = AssetDatabase.CreateFolder(path, name);
        return AssetDatabase.GUIDToAssetPath(guid);
    }

    void CreateMeshAsset(Mesh mesh, string name, string path)
    {
        string fullName = string.Format("{0}.asset", name);
        string assetPath = AppendToPath(path, fullName);
        AssetDatabase.CreateAsset(mesh, assetPath);
    }

    void CreateCavePrefab(GameObject cave, string path)
    {
        string name = "Cave.prefab";
        string cavePath = AppendToPath(path, name);
        PrefabUtility.CreatePrefab(cavePath, cave);
    }

    string AppendToPath(string path, string toAppend)
    {
        return string.Format("{0}/{1}", path, toAppend);
    }
}
