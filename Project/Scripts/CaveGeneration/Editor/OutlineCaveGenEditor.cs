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
    public sealed class OutlineCaveGenEditor : BaseCaveGenEditor
    {
        const string MAP_GENERATOR_NAME = "mapGenerator";
        const string OUTLINE_NAME = "outlineModule";
        const string FLOOR_HEIGHTMAP_NAME = "floorHeightMap";

        const string OUTLINE_MODULE_LABEL = "Outline Module";
        const string FLOOR_HEIGHTMAP_LABEL = "Floor Heightmap Module";

        const string CONFIG_NAME = "rockCaveConfig";

        const string FLOOR_FOLDER = "FloorMeshes";

        const string MAP_GENERATOR_PATH = CONFIG_NAME + "." + MAP_GENERATOR_NAME;
        const string FLOOR_HEIGHTMAP_PATH = CONFIG_NAME + "." + FLOOR_HEIGHTMAP_NAME;
        const string OUTLINE_MODULE_PATH = CONFIG_NAME + "." + OUTLINE_NAME;

        Editor outlineEditor;
        Editor floorHeightMapEditor;

        bool drawOutlineEditor;
        bool drawFloorHeightMapEditor;

        protected override MapGenModule GetMapGenModule()
        {
            return (MapGenModule)serializedObject.FindProperty(MAP_GENERATOR_PATH).objectReferenceValue;
        }

        protected override void DrawConfiguration()
        {
            SerializedProperty property = serializedObject.FindProperty(CONFIG_NAME);
            GUIContent label = new GUIContent("Configuration");
            EditorGUILayout.PropertyField(property, label, true);
        }

        protected override void DrawModuleEditors()
        {
            base.DrawModuleEditors();

            EditorHelpers.DrawLine();
            var floorHeightMap = serializedObject.FindProperty(FLOOR_HEIGHTMAP_PATH).objectReferenceValue;
            EditorHelpers.DrawFoldoutEditor(FLOOR_HEIGHTMAP_LABEL, floorHeightMap, ref drawFloorHeightMapEditor, ref floorHeightMapEditor);

            EditorHelpers.DrawLine();
            var outlineModule = serializedObject.FindProperty(OUTLINE_MODULE_PATH).objectReferenceValue;
            EditorHelpers.DrawFoldoutEditor(OUTLINE_MODULE_LABEL, outlineModule, ref drawOutlineEditor, ref outlineEditor);

            EditorHelpers.DrawLine();
        }

        protected override void SaveAdditionalAssets(GameObject cave, string path)
        {
            string floorFolder = IOHelpers.CreateFolder(path, FLOOR_FOLDER);
            foreach (Transform sector in cave.transform)
            {
                foreach (Transform component in sector)
                {
                    if (Sector.IsFloor(component))
                    {
                        CreateMeshAsset(component, floorFolder);
                    }
                }
            }
        }
    }
}