using UnityEngine;
using UnityEditor;
using CaveGeneration;
using CaveGeneration.MeshGeneration;

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
        if (caveGenerator.Cave == null)
        {
            Debug.Log("Cavegenerator: no cave object found!");
            return;
        }
        string guid = AssetDatabase.CreateFolder("Assets", "GeneratedCave");
        string path = AssetDatabase.GUIDToAssetPath(guid) + "/";
        foreach (MapMeshes meshes in caveGenerator.GeneratedMeshes)
        {
            CreateMeshAssets(meshes, path);
        }
        CreateCavePrefab(path, caveGenerator.Cave);
    }

    void CreateMeshAssets(MapMeshes mapMeshes, string path)
    {
        foreach (Mesh mesh in mapMeshes.Meshes)
        {
            AssetDatabase.CreateAsset(mesh, path + mesh.name + ".mesh");
        }
    }

    void CreateCavePrefab(string path, GameObject cave)
    {
        PrefabUtility.CreatePrefab(path + "Cave.prefab", cave);
    }
}
