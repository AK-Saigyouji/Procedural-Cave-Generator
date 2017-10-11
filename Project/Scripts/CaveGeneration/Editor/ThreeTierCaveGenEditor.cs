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
    public sealed class ThreeTierCaveGenEditor : BaseCaveGenEditor
    {
        const string MAP_GENERATOR_NAME = "mapGenerator";
        const string FLOOR_HEIGHTMAP_NAME = "floorHeightMap";
        const string CEILING_HEIGHTMAP_NAME = "ceilingHeightMap";

        const string FLOOR_HEIGHTMAP_LABEL = "Floor Heightmap Module";
        const string CEILING_HEIGHTMAP_LABEL = "Ceiling Heightmap Module";

        const string CONFIG_NAME = "threeTierCaveConfig";

        const string FLOOR_FOLDER = "FloorMeshes";
        const string WALL_FOLDER = "WallMeshes";
        const string CEILING_FOLDER = "CeilingMeshes";

        const string MAP_GEN_PATH = CONFIG_NAME + "." + MAP_GENERATOR_NAME;
        const string FLOOR_HEIGHTMAP_PATH = CONFIG_NAME + "." + FLOOR_HEIGHTMAP_NAME;
        const string CEILING_HEIGHTMAP_PATH = CONFIG_NAME + "." + CEILING_HEIGHTMAP_NAME;

        Editor floorHeightMapEditor;
        Editor ceilingHeightMapEditor;

        bool drawFloorHeightMapEditor;
        bool drawCeilingHeightMapEditor;

        protected override MapGenModule GetMapGenModule()
        {
            return (MapGenModule)serializedObject.FindProperty(MAP_GEN_PATH).objectReferenceValue;
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
            var ceilingModule = serializedObject.FindProperty(CEILING_HEIGHTMAP_PATH).objectReferenceValue;
            EditorHelpers.DrawFoldoutEditor(CEILING_HEIGHTMAP_LABEL, ceilingModule, ref drawCeilingHeightMapEditor, ref ceilingHeightMapEditor);

            EditorHelpers.DrawLine();
        }

        protected override void SaveAdditionalAssets(GameObject cave, string path)
        {
            string ceilingFolder = IOHelpers.CreateFolder(path, CEILING_FOLDER);
            string floorFolder = IOHelpers.CreateFolder(path, FLOOR_FOLDER);
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
        }
    }
}