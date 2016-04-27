using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator), true)]
public class MapGeneratorEditor : Editor {
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MapGenerator mapGenerator = (MapGenerator)target;
        if (Application.isPlaying)
        {
            if (GUILayout.Button("Generate New Map"))
            {
                mapGenerator.GenerateNewMapWithMesh();
            }

            if (GUILayout.Button("Create Prefab"))
            {
                CreatePrefab(mapGenerator);
            }
        }
    }

    public void CreatePrefab(MapGenerator mapGenerator)
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
        if (meshes.ceilingMesh != null)
        {
            AssetDatabase.CreateAsset(meshes.ceilingMesh, path + meshes.ceilingMesh.name + ".mesh");
        }
        if (meshes.wallMesh != null)
        {
            AssetDatabase.CreateAsset(meshes.wallMesh, path + meshes.wallMesh.name + ".mesh");
        }
    }

    void CreateCavePrefab(string path, GameObject cave)
    {
        PrefabUtility.CreatePrefab(path + "Cave.prefab", cave);
    }
}
