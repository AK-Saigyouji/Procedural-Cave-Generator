using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor {
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MapGenerator mapGenerator = (MapGenerator)target;
        if (Application.isPlaying)
        {
            if (GUILayout.Button("Generate New Map"))
            {
                mapGenerator.GenerateNewMap();
            }

            if (GUILayout.Button("Create Prefab"))
            {
                CreatePrefab(mapGenerator);
            }
        }
    }

    internal void CreatePrefab(MapGenerator mapGenerator)
    {
        string guid = AssetDatabase.CreateFolder("Assets", "GeneratedCave");
        string path = AssetDatabase.GUIDToAssetPath(guid) + "/";
        foreach (MapMeshes meshes in mapGenerator.generatedMeshes)
        {
            CreateMeshAssets(meshes, path);
        }
        CreateCavePrefab(path, mapGenerator.cave);
    }

    void CreateMeshAssets(MapMeshes meshes, string path)
    {
        AssetDatabase.CreateAsset(meshes.ceilingMesh, path + meshes.ceilingMesh.name + ".mesh");
        AssetDatabase.CreateAsset(meshes.wallMesh, path + meshes.wallMesh.name + ".mesh");
    }

    void CreateCavePrefab(string path, GameObject cave)
    {
        PrefabUtility.CreatePrefab(path + "Cave.prefab", cave);
    }
}
