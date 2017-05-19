/* Understanding the point of this custom editor requires understanding the problem it solves. Modules are designed
 to be easily combined and aggregated. e.g. you can write a map generation module that takes an existing map gen module,
 but further processes it by adding entrances and connecting them to the existing passages in the map. This is an example
 of the decorator pattern: the original module is 'decorated' with entrance logic. But the inspector to this decorator
 will (by default) only expose the properties of the new module, and not the decorated module. This editor ensures that the
 decorated module has an editor for its properties exposed in the decorator's inspector. This works recursively, 
 ensuring that a complex composition hierarchy can all be customized at the top level.*/

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CaveGeneration.Modules;

[CustomEditor(typeof(Module), editorForChildClasses: true)]
public sealed class ModuleEditor : Editor
{
    Editor[] moduleEditors;
    bool[] foldouts;

    void OnEnable()
    {
        int numModules = EditorHelpers.GetProperties(serializedObject).Count();
        moduleEditors = new Editor[numModules];
        foldouts = new bool[numModules];
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        serializedObject.Update();
        int currentEditor = -1;
        foreach (SerializedProperty property in EditorHelpers.GetProperties(serializedObject))
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                UnityEngine.Object obj = property.objectReferenceValue;
                if (obj is Module)
                {
                    currentEditor++;
                    EditorHelpers.DrawFoldoutEditor(
                        property.displayName, obj, ref foldouts[currentEditor], ref moduleEditors[currentEditor]);
                }
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}