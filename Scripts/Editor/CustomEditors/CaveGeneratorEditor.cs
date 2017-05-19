/* This is the custom inspector for CaveGeneratorUI, and thus constitutes the main user interface for the cave
 generation system. This custom editor was written mainly to add features, rather than customize the appearance
 of the interface. In particular, the following changes have been made:
 
  1. A button has been added to generate a new cave.
  2. A button has been added to convert an existing cave to a prefab. 
  3. Editors for the modules have been added to the inspector.
  
   The purpose of 1 is to permit the generation of caves without having to write code. The purpose of 2 is to
  allow caves to be serialized correctly. Simply dragging the cave from the hierarchy into assets would successfully 
  create a prefab, but the meshes will disappear when Unity is reloaded. The purpose of 3 is to smooth the iterative
  process of generating caves: a user may configure the modules, switch to the cave generator, generate a few caves,
  switch back to a module to tweak a property, switch back, generate a few more caves, see the changes, etc. 
  By drawing editors for the modules directly onto the inspector, this can be done all in a single inspector.*/

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

    // These names must reflect the names of the corresponding variables in CaveGeneratorUI and CaveConfiguration
    const string CONFIG_NAME = "caveConfig"; 
    const string MAP_GEN_NAME = "mapGenerator";
    const string FLOOR_HEIGHTMAP_NAME = "floorHeightMap";
    const string CEILING_HEIGHTMAP_NAME = "ceilingHeightMap";

    // These variables are used to display the module properties in this inspector (by default they don't).
    Editor mapGenEditor;
    Editor floorHeightMapEditor;
    Editor ceilingHeightMapEditor;
    const bool DEFAULT_FOLDOUT = false;
    bool drawMapGenEditor = DEFAULT_FOLDOUT;
    bool drawFloorHeightMapEditor = DEFAULT_FOLDOUT;
    bool drawCeilingHeightMapEditor = DEFAULT_FOLDOUT;

    // Inspector labels
    const string MAP_GEN_MODULE_LABEL = "Map Generator Module";
    const string FLOOR_HEIGHTMAP_LABEL = "Floor Heightmap Module";
    const string CEILING_HEIGHTMAP_LABEL = "Ceiling Heightmap Module";
    const string GENERATE_CAVE_BUTTON_LABEL = "Generate Cave";
    const string CONVERT_PREFAB_BUTTON_LABEL = "Convert to Prefab";

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();
        DrawLine();
        DrawModuleEditor(MAP_GEN_MODULE_LABEL, MAP_GEN_NAME, ref drawMapGenEditor, ref mapGenEditor);
        DrawLine();
        DrawModuleEditor(FLOOR_HEIGHTMAP_LABEL, FLOOR_HEIGHTMAP_NAME, ref drawFloorHeightMapEditor, ref floorHeightMapEditor);
        DrawLine();
        DrawModuleEditor(CEILING_HEIGHTMAP_LABEL, CEILING_HEIGHTMAP_NAME, ref drawCeilingHeightMapEditor, ref ceilingHeightMapEditor);
        DrawLine();

        DrawButtons();

        serializedObject.ApplyModifiedProperties();
    }

    void DrawButtons()
    {
        CaveGeneratorUI caveGenerator = (CaveGeneratorUI)target;
        if (Application.isPlaying)
        {
            if (GUILayout.Button(GENERATE_CAVE_BUTTON_LABEL))
            {
                DestroyCave();
                caveGenerator.Generate();
            }

            if (GUILayout.Button(CONVERT_PREFAB_BUTTON_LABEL))
            {
                TryCreatePrefab();
                DestroyCave();
            }
        }
    }

    void DrawModuleEditor(string label, string moduleName, ref bool drawEditor, ref Editor editor)
    {
        SerializedProperty module = serializedObject.FindProperty(CONFIG_NAME).FindPropertyRelative(moduleName);
        Object targetObject = module.objectReferenceValue;
        EditorHelpers.DrawFoldoutEditor(label, targetObject, ref drawEditor, ref editor);
    }

    void DrawLine()
    {
        EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
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

        try
        {
            CreateMeshAssets(cave.transform, caveFolderPath);
            CreateCavePrefab(cave, caveFolderPath);
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
                if (Sector.IsFloor(component))
                {
                    CreateMeshAsset(component, floorFolder);
                }
                else if (Sector.IsCeiling(component))
                {
                    CreateMeshAsset(component, ceilingFolder);
                }
                else if (Sector.IsWall(component))
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
