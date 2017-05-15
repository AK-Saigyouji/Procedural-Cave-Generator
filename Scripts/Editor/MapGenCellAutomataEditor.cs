using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CaveGeneration.Modules;

[CustomEditor(typeof(MapGenCellAutomata))]
public sealed class MapGenCellAutomataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();
        var properties = serializedObject.FindProperty("properties");
        foreach (var item in properties.GetChildren())
        {
            EditorGUILayout.PropertyField(item, includeChildren: false);
        }
        serializedObject.ApplyModifiedProperties();
    }
}