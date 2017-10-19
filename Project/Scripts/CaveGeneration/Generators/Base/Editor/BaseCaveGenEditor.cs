using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using AKSaigyouji.EditorScripting;
using AKSaigyouji.Modules.MapGeneration;

namespace AKSaigyouji.CaveGeneration
{
    public abstract class BaseCaveGenEditor : Editor
    {
        // Asset folder names
        const string ROOT_FOLDER = "AKSaigyouji";
        const string CAVE_FOLDER = "Generated Cave";
        const string MAP_FOLDER = "Maps";
        const string PREFAB_FOLDER = "Generated Caves";

        const string CAVE_NAME = "Cave";
        const string PREFAB_NAME = "Cave.prefab";
        const string MAP_NAME = "SavedCaveGenerator_map.png";

        const string MAP_GEN_NAME = "mapGenerator";

        const string CAVE_GEN_TYPE_NAME = "type";
        const string RANDOMIZE_SEED_NAME = "randomize";

        // Inspector labels
        const string RANDOMIZE_LABEL = "Randomize Seeds";
        const string MAP_GEN_MODULE_LABEL = "Map Generator Module";
        const string GENERATE_CAVE_BUTTON_LABEL = "Generate Cave";
        const string SAVE_MAP_LABEL = "Save Single Map";
        const string CONVERT_PREFAB_BUTTON_LABEL = "Convert to Prefab";
        const string CAVE_CONFIG_LABEL = "Configuration";

        // controls visibility of the map gen editor
        Editor mapGenEditor;
        bool drawMapGenEditor;

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

        protected abstract MapGenModule GetMapGenModule();

        protected virtual void DrawModuleEditors()
        {
            EditorHelpers.DrawLine();
            EditorHelpers.DrawFoldoutEditor(MAP_GEN_MODULE_LABEL, GetMapGenModule(), ref drawMapGenEditor, ref mapGenEditor);
        }

        /// <summary>
        /// Draw the configurable properties for this type of cave generator here.
        /// </summary>
        protected abstract void DrawConfiguration();

        protected static string GetPath(string configName, string propertyName)
        {
            return string.Format("{0}.{1}", configName, propertyName);
        }

        protected void DrawModuleEditor(string path, string label, ref bool toggled, ref Editor editor)
        {
            EditorHelpers.DrawLine();
            var module = serializedObject.FindProperty(path).objectReferenceValue;
            EditorHelpers.DrawFoldoutEditor(label, module, ref toggled, ref editor);
        }

        void DrawCaveGenType()
        {
            SerializedProperty typeProperty = serializedObject.FindProperty(CAVE_GEN_TYPE_NAME);
            EditorGUILayout.PropertyField(typeProperty);
        }

        void DrawRandomizeSeed()
        {
            var property = serializedObject.FindProperty(RANDOMIZE_SEED_NAME);
            EditorGUILayout.PropertyField(property, new GUIContent(RANDOMIZE_LABEL));
        }

        void DrawButtons()
        {
            if (Application.isPlaying)
            {
                if (GUILayout.Button(GENERATE_CAVE_BUTTON_LABEL))
                {
                    DestroyCave();
                    CaveGeneratorUI caveGenerator = (CaveGeneratorUI)target;
                    caveGenerator.Generate();
                }
                if (GUILayout.Button(SAVE_MAP_LABEL))
                {
                    CreateMap();
                }
                if (GUILayout.Button(CONVERT_PREFAB_BUTTON_LABEL))
                {
                    TryCreatePrefab();
                    DestroyCave();
                }
            }
        }

        void CreateMap()
        {
            string rootPath = IOHelpers.RequireFolder(ROOT_FOLDER);
            string mapFolderPath = IOHelpers.RequireFolder(rootPath, MAP_FOLDER);
            string mapPath = IOHelpers.GetAvailableAssetPath(mapFolderPath, MAP_NAME);
            var mapGen = GetMapGenModule();
            var map = mapGen.Generate();
            var texture = map.ToTexture();
            IOHelpers.SaveTextureAsPNG(texture, mapPath);
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
                throw new InvalidOperationException("No cave found to convert. Cave must be a child of this generator and labelled " + CAVE_NAME);
            }

            if (childCaves.Length > 1)
            {
                throw new InvalidOperationException("Unexpected: multiple caves found under this generator. Must have only one.");
            }

            GameObject cave = childCaves[0].gameObject;
            CreatePrefab(cave);
        }

        void CreatePrefab(GameObject cave)
        {
            string rootFolderPath = IOHelpers.RequireFolder(ROOT_FOLDER);
            string caveFolderPath = IOHelpers.RequireFolder(rootFolderPath, PREFAB_FOLDER);
            string prefabFolderPath = IOHelpers.CreateFolder(caveFolderPath, CAVE_FOLDER);

            try
            {
                SaveAdditionalAssets(cave, prefabFolderPath);
                string path = IOHelpers.CombinePath(prefabFolderPath, PREFAB_NAME);
                PrefabUtility.CreatePrefab(path, cave);
            }
            catch (InvalidOperationException)
            {
                AssetDatabase.DeleteAsset(prefabFolderPath);
                throw;
            }
        }

        /// <summary>
        /// If the cave requires any assets to be serialized, this method should be overridden, and they should
        /// be saved in this method. If no additional assets are required, the method need not be implemented.
        /// </summary>
        /// <param name="cave">The saved cave gameobject.</param>
        /// <param name="path">Saved assets should be in this directory.</param>
        protected virtual void SaveAdditionalAssets(GameObject cave, string path)
        {

        }

        /// <summary>
        /// Creates a mesh asset out of the mesh on this component's mesh filter. 
        /// </summary>
        /// <param name="component">Must have a mesh filter with a valid (not null) mesh.</param>
        /// <param name="path">Mesh will be saved to this path.</param>
        protected void CreateMeshAsset(Transform component, string path)
        {
            Mesh mesh = ExtractMesh(component.gameObject);
            path = IOHelpers.CombinePath(path, component.name);
            AssetDatabase.CreateAsset(mesh, path);
        }

        static Mesh ExtractMesh(GameObject component)
        {
            MeshFilter meshFilter = component.GetComponent<MeshFilter>();

            if (meshFilter == null || meshFilter.sharedMesh == null)
                throw new InvalidOperationException("Prefab creation failed, unexpected cave hierarchy: sector child with no mesh.");

            return meshFilter.sharedMesh;
        }
    }
}