using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CaveGenerator), true)]
public class MapGeneratorEditor : Editor {
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        CaveGenerator caveGenerator = (CaveGenerator)target;
        if (Application.isPlaying)
        {
            if (GUILayout.Button("Generate New Map"))
            {
                caveGenerator.GenerateCave();
            }

            if (GUILayout.Button("Create Prefab"))
            {
                CreatePrefab(caveGenerator);
            }
        }
    }

    public void CreatePrefab(CaveGenerator caveGenerator)
    {
        string guid = AssetDatabase.CreateFolder("Assets", "GeneratedCave");
        string path = AssetDatabase.GUIDToAssetPath(guid) + "/";
        foreach (MapMeshes meshes in caveGenerator.generatedMeshes)
        {
            CreateMeshAssets(meshes, path);
        }
        CreateCavePrefab(path, caveGenerator.cave);
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
