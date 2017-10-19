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

        Editor outlineEditor;
        Editor floorHeightMapEditor;

        bool drawOutlineEditor;
        bool drawFloorHeightMapEditor;

        protected override MapGenModule GetMapGenModule()
        {
            return (MapGenModule)serializedObject.FindProperty(GetPath(MAP_GENERATOR_NAME)).objectReferenceValue;
        }

        protected override void DrawConfiguration()
        {
            SerializedProperty property = serializedObject.FindProperty(CONFIG_NAME);
            var label = new GUIContent("Configuration");
            EditorGUILayout.PropertyField(property, label, true);
        }

        protected override void DrawModuleEditors()
        {
            base.DrawModuleEditors();

            string floorPath = GetPath(FLOOR_HEIGHTMAP_NAME);
            string outlinePath = GetPath(OUTLINE_NAME);

            DrawModuleEditor(floorPath, FLOOR_HEIGHTMAP_LABEL, ref drawFloorHeightMapEditor, ref floorHeightMapEditor);
            DrawModuleEditor(outlinePath, OUTLINE_MODULE_LABEL, ref drawOutlineEditor, ref outlineEditor);

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

        static string GetPath(string propertyName)
        {
            return GetPath(CONFIG_NAME, propertyName);
        }
    }
}