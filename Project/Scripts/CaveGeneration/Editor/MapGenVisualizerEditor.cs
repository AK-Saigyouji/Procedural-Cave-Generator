using System;
using UnityEngine;
using UnityEditor;
using AKSaigyouji.Maps;
using AKSaigyouji.EditorScripting;

namespace AKSaigyouji.Modules.MapGeneration
{
    [CustomEditor(typeof(MapGenVisualizer))]
    public sealed class MapGenVisualizerEditor : Editor
    {
        Editor editor;
        bool foldout;

        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            MapGenModule mapGenerator = GetMapGenModule();
            if (mapGenerator != null)
            {
                EditorHelpers.DrawFoldoutEditor("Map Generator", mapGenerator, ref foldout, ref editor);
            }
        }

        public override void OnPreviewGUI(Rect position, GUIStyle background)
        {
            if (Event.current.type == EventType.Repaint)
            {
                MapGenModule mapGenerator = GetMapGenModule();
                if (mapGenerator != null)
                {
                    try
                    {
                        Map map = mapGenerator.Generate();
                        if (map != null)
                        {
                            Texture texture = map.ToTexture();
                            GUI.DrawTexture(position, texture, ScaleMode.StretchToFill, false);
                        }
                    }
                    // If the generator is not fully configured in the inspector yet, then a number of exceptions may occur,
                    // which are caught and suppressed here. This results in the behaviour that the editor will only visualize
                    // a map once the generator has a valid configuration. Doing so may suppress actual errors in a module's
                    // code, in which case the module should be executed in the cave generator, or directly, for debugging
                    // purposes.
                    catch (InvalidOperationException) { }
                    catch (NullReferenceException) { }
                    catch (ArgumentException) { }
                }
            }
        }

        MapGenModule GetMapGenModule()
        {
            return (MapGenModule)serializedObject.FindProperty("mapGenModule").objectReferenceValue;
        }
    } 
}