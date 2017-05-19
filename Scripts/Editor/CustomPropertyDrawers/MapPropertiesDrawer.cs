using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CaveGeneration.MapGeneration;
using CaveGeneration.Modules;

[CustomPropertyDrawer(typeof(MapParameters))]
public sealed class MapPropertiesDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorHelpers.GetHeightForSimpleGUI(property);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorHelpers.DrawSimpleGUI(position, property, label);
    }
}