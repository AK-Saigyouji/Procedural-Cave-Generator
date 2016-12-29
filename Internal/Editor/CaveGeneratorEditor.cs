using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;
using CaveGeneration;
using System.Collections.Generic;

[CustomEditor(typeof(CaveGenerator))]
public class CaveGeneratorEditor : Editor
{
    // Asset folder names
    const string ROOT_FOLDER = "Assets";
    const string CAVE_FOLDER = "GeneratedCave";
    const string FLOOR_FOLDER = "FloorMeshes";
    const string WALL_FOLDER = "WallMeshes";
    const string CEILING_FOLDER = "CeilingMeshes";

    // Property names. If the inspector breaks after changing a property name, update it here.
    const string CONFIG = "config";
    const string CAVE_TYPE = "caveType";
    const string MAP_PARAMS = "mapParameters";
    const string CEILING_MAT = "ceilingMaterial";
    const string FLOOR_MAT = "floorMaterial";
    const string WALL_MAT = "wallMaterial";
    const string SCALE = "scale";
    const string DEBUG_MODE = "debugMode";
    const string FLOOR_HEIGHT_MAP = "floorHeight";
    const string CEILING_HEIGHT_MAP = "ceilingHeight";
    const string HEIGHT_MAP_IS_CONSTANT = "isConstant";
    const string HEIGHT_MAP_HEIGHT = "minHeight";

    const string CONSTANT_HEIGHT_LABEL = "Height";

    SerializedProperty config, caveType, mapParameters, ceilingMat, wallMat, floorMat, scale, debugMode;
    SerializedProperty floorHeight, ceilingHeight;

    void OnEnable()
    {
        /* Using strings for the property names like this is troublesome, as it means it will break if the names or paths
        in the respective classes are ever changed. But given the need to use the reflection API to find private
        properties, it's generally unavoidable. The alternative is to iterate over all properties and dynamically
        determine which need to be drawn and how, but that becomes much harder to fix if something breaks.*/
        config        = serializedObject.FindProperty(CONFIG);
        caveType      = FindProperty(CAVE_TYPE);
        mapParameters = FindProperty(MAP_PARAMS);
        ceilingMat    = FindProperty(CEILING_MAT);
        wallMat       = FindProperty(WALL_MAT);
        floorMat      = FindProperty(FLOOR_MAT);
        scale         = FindProperty(SCALE);
        debugMode     = FindProperty(DEBUG_MODE);
        floorHeight   = FindProperty(FLOOR_HEIGHT_MAP);
        ceilingHeight = FindProperty(CEILING_HEIGHT_MAP);
    }

    public override void OnInspectorGUI()
    {
        DrawProperties();
        serializedObject.ApplyModifiedProperties();

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

    void DrawProperties()
    {
        DrawProperty(caveType);
        DrawProperty(mapParameters);

        DrawHeightMapProperty(floorHeight);
        DrawHeightMapProperty(ceilingHeight);

        DrawProperty(ceilingMat);
        DrawProperty(wallMat);
        DrawProperty(floorMat);
        DrawProperty(scale);
        DrawProperty(debugMode);
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

    void DrawHeightMapProperty(SerializedProperty property)
    {
        // Editor scripting is generally a hacky ordeal. Here we want to either reveal
        // all the properties for height maps, or just a single height value, based on whether the
        // variable Constant is flagged.
        SerializedProperty isConstant = property.FindPropertyRelative(HEIGHT_MAP_IS_CONSTANT);
        if (isConstant.boolValue)
        {
            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, property.displayName);
            if (property.isExpanded)
            {
                SerializedProperty height = property.FindPropertyRelative(HEIGHT_MAP_HEIGHT);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(height, new GUIContent(CONSTANT_HEIGHT_LABEL));
                DrawProperty(isConstant);
                EditorGUI.indentLevel--;
            }
        }
        else
        {
            DrawProperty(property);
        }
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
        string folderPath = AssetDatabase.GUIDToAssetPath(guid);
        return folderPath;
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

    SerializedProperty FindProperty(string relativePath)
    {
        return config.FindPropertyRelative(relativePath);
    }

    void DrawProperty(SerializedProperty property)
    {
        EditorGUILayout.PropertyField(property, true);
    }
}
