/* This is the custom inspector for CaveGeneratorUI, and thus constitutes the main user interface for the cave
 generation system.*/

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using AKSaigyouji.EditorScripting;

namespace AKSaigyouji.CaveGeneration
{
    [CustomEditor(typeof(CaveGeneratorUI))]
    public class CaveGeneratorUIEditor : Editor
    {
        // Asset folder names
        const string CAVE_FOLDER = "Generated Cave";
        const string PREFAB_FOLDER = "Generated Caves";
        const string FLOOR_FOLDER = "FloorMeshes";
        const string WALL_FOLDER = "WallMeshes";
        const string CEILING_FOLDER = "CeilingMeshes";

        const string CAVE_NAME = "Cave";
        const string PREFAB_NAME = "Cave.prefab";

        // These names must reflect the names of the corresponding variables in the corresponding scripts.
        const string THREE_TIER_CONFIG_NAME = "threeTierCaveConfig";
        const string ROCK_CAVE_CONFIG_NAME = "rockCaveConfig";

        const string MAP_GEN_NAME = "mapGenerator";
        const string OUTLINE_NAME = "outlineModule";
        const string FLOOR_HEIGHTMAP_NAME = "floorHeightMap";
        const string CEILING_HEIGHTMAP_NAME = "ceilingHeightMap";

        const string CAVE_GEN_TYPE_NAME = "type";
        const string RANDOMIZE_SEED_NAME = "randomize";

        // Inspector labels
        const string RANDOMIZE_LABEL = "Randomize Seeds";
        const string OUTLINE_MODULE_LABEL = "Outline Module";
        const string MAP_GEN_MODULE_LABEL = "Map Generator Module";
        const string FLOOR_HEIGHTMAP_LABEL = "Floor Heightmap Module";
        const string CEILING_HEIGHTMAP_LABEL = "Ceiling Heightmap Module";
        const string GENERATE_CAVE_BUTTON_LABEL = "Generate Cave";
        const string CONVERT_PREFAB_BUTTON_LABEL = "Convert to Prefab";
        const string CAVE_CONFIG_LABEL = "Configuration";

        // These variables are used to display the module editors in this inspector
        Editor outlineEditor;
        Editor mapGenEditor;
        Editor floorHeightMapEditor;
        Editor ceilingHeightMapEditor;

        const bool DEFAULT_FOLDOUT = false;
        bool drawOutlineEditor = DEFAULT_FOLDOUT;
        bool drawMapGenEditor = DEFAULT_FOLDOUT;
        bool drawFloorHeightMapEditor = DEFAULT_FOLDOUT;
        bool drawCeilingHeightMapEditor = DEFAULT_FOLDOUT;

        // Which cave generator is currently selected. Normally polymorphism would be used in place of branching on an enum
        // throughout the script, but due to the nature of editor scripting and the small number of options, I decided
        // it would be more robust and easier to live with a few switches instead. This may change if a lot of different
        // generators are implemented, which is not currently planned.
        CaveGeneratorUI.CaveGeneratorType caveGenType;
        const string TYPE_ENUM_ERROR_FORMAT = "Internal error: {0} type not handled properly by custom editor.";

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawCaveGenType();
            DrawConfiguration();
            DrawRandomizeSeed();
            DrawModuleEditors();
            DrawButtons();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawCaveGenType()
        {
            SerializedProperty typeProperty = serializedObject.FindProperty(CAVE_GEN_TYPE_NAME);
            caveGenType = (CaveGeneratorUI.CaveGeneratorType)typeProperty.enumValueIndex;
            EditorGUILayout.PropertyField(typeProperty);
        }

        void DrawConfiguration()
        {
            string configName = GetConfigName();
            SerializedProperty property = serializedObject.FindProperty(configName);
            GUIContent label = new GUIContent(CAVE_CONFIG_LABEL);
            EditorGUILayout.PropertyField(property, label, true);
        }

        void DrawRandomizeSeed()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(RANDOMIZE_SEED_NAME), new GUIContent(RANDOMIZE_LABEL));
        }

        void DrawModuleEditors()
        {
            // Note: this will almost definitely need rewriting once new cave gen types are implemented - we're taking 
            // advantage of the fact that both current types make use of both a map gen module and a floor heightmap module.
            EditorHelpers.DrawLine();
            DrawModuleEditor(MAP_GEN_MODULE_LABEL, MAP_GEN_NAME, ref drawMapGenEditor, ref mapGenEditor);
            EditorHelpers.DrawLine();
            DrawModuleEditor(FLOOR_HEIGHTMAP_LABEL, FLOOR_HEIGHTMAP_NAME, ref drawFloorHeightMapEditor, ref floorHeightMapEditor);
            EditorHelpers.DrawLine();
            if (caveGenType == CaveGeneratorUI.CaveGeneratorType.ThreeTiered)
            {
                DrawModuleEditor(CEILING_HEIGHTMAP_LABEL, CEILING_HEIGHTMAP_NAME, ref drawCeilingHeightMapEditor, ref ceilingHeightMapEditor);
                EditorHelpers.DrawLine();
            }
            if (caveGenType == CaveGeneratorUI.CaveGeneratorType.RockOutline)
            {
                DrawModuleEditor(OUTLINE_MODULE_LABEL, OUTLINE_NAME, ref drawOutlineEditor, ref outlineEditor);
                EditorHelpers.DrawLine();
            }
        }

        void DrawButtons()
        {
            if (Application.isPlaying)
            {
                if (GUILayout.Button(GENERATE_CAVE_BUTTON_LABEL))
                {
                    DestroyCave();
                    Generate();
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
            string configName = GetConfigName();
            SerializedProperty module = serializedObject.FindProperty(configName).FindPropertyRelative(moduleName);
            Object targetObject = module.objectReferenceValue;
            EditorHelpers.DrawFoldoutEditor(label, targetObject, ref drawEditor, ref editor);
        }

        string GetConfigName()
        {
            string configName;
            switch (caveGenType)
            {
                case CaveGeneratorUI.CaveGeneratorType.ThreeTiered:
                    configName = THREE_TIER_CONFIG_NAME;
                    break;
                case CaveGeneratorUI.CaveGeneratorType.RockOutline:
                    configName = ROCK_CAVE_CONFIG_NAME;
                    break;
                default:
                    throw new System.InvalidOperationException(string.Format(TYPE_ENUM_ERROR_FORMAT, caveGenType));
            }
            return configName;
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
                if (child.name == CAVE_NAME)
                {
                    children.Add(child);
                }
            }
            return children.ToArray();
        }

        void TryCreatePrefab()
        {
            // The cavegenerator should have only one cave as a child.
            Transform[] childCaves = FindChildCaves();

            if (childCaves.Length == 0)
            {
                Debug.LogErrorFormat("No cave found to convert. Cave must be a child of this generator and labelled {0}.", CAVE_NAME);
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

        void Generate()
        {
            CaveGeneratorUI caveGenerator = (CaveGeneratorUI)target;
            switch (caveGenType)
            {
                case CaveGeneratorUI.CaveGeneratorType.ThreeTiered:
                    caveGenerator.GenerateThreeTier();
                    break;
                case CaveGeneratorUI.CaveGeneratorType.RockOutline:
                    caveGenerator.GenerateRockCave();
                    break;
                default:
                    break;
            }
        }

        void CreatePrefab(GameObject cave)
        {
            IOHelpers.RequireFolder("Assets", CAVE_FOLDER);
            string caveFolderPath = IOHelpers.RequireFolder(CAVE_FOLDER, PREFAB_FOLDER);
            string prefabFolderPath = IOHelpers.CreateFolder(caveFolderPath, CAVE_FOLDER);

            try
            {
                CreateMeshAssets(cave.transform, prefabFolderPath);
                string path = IOHelpers.CombinePath(prefabFolderPath, PREFAB_NAME);
                PrefabUtility.CreatePrefab(path, cave);
            }
            catch (System.InvalidOperationException)
            {
                AssetDatabase.DeleteAsset(prefabFolderPath);
                throw;
            }
        }

        void CreateMeshAssets(Transform cave, string path)
        {
            string floorFolder = IOHelpers.CreateFolder(path, FLOOR_FOLDER);
            string ceilingFolder = IOHelpers.CreateFolder(path, CEILING_FOLDER);
            string wallFolder = IOHelpers.CreateFolder(path, WALL_FOLDER);
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
                }
            }
            foreach (string folder in new[] { floorFolder, wallFolder, ceilingFolder })
            {
                if (IOHelpers.IsFolderEmpty(folder))
                {
                    AssetDatabase.DeleteAsset(folder);
                }
            }
        }

        void CreateMeshAsset(Transform component, string path)
        {
            Mesh mesh = ExtractMesh(component.gameObject);
            path = IOHelpers.CombinePath(path, component.name);
            AssetDatabase.CreateAsset(mesh, path);
        }

        static Mesh ExtractMesh(GameObject component)
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
}
