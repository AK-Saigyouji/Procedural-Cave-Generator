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
        const string WALL_MODULE_NAME = "wallModule";

        const string FLOOR_HEIGHTMAP_LABEL = "Floor Heightmap Module";
        const string CEILING_HEIGHTMAP_LABEL = "Ceiling Heightmap Module";
        const string WALL_MODULE_LABEL = "Wall Module";

        const string CONFIG_NAME = "threeTierCaveConfig";

        const string FLOOR_FOLDER = "FloorMeshes";
        const string WALL_FOLDER = "WallMeshes";
        const string CEILING_FOLDER = "CeilingMeshes";

        Editor floorEditor;
        Editor ceilingEditor;
        Editor wallEditor;

        bool drawFloorEditor;
        bool drawCeilingEditor;
        bool drawWallEditor;

        protected override MapGenModule GetMapGenModule()
        {
            return (MapGenModule)serializedObject.FindProperty(GetPath(MAP_GENERATOR_NAME))
                                                 .objectReferenceValue;
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
            string ceilingPath = GetPath(CEILING_HEIGHTMAP_NAME);
            string wallPath = GetPath(WALL_MODULE_NAME);

            DrawModuleEditor(floorPath, FLOOR_HEIGHTMAP_LABEL, ref drawFloorEditor, ref floorEditor);
            DrawModuleEditor(ceilingPath, CEILING_HEIGHTMAP_LABEL, ref drawCeilingEditor, ref ceilingEditor);
            DrawModuleEditor(wallPath, WALL_MODULE_LABEL, ref drawWallEditor, ref wallEditor);

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

        static string GetPath(string propertyName)
        {
            return GetPath(CONFIG_NAME, propertyName);
        }
    }
}