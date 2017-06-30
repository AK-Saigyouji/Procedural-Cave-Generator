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
                bool suppressErrors = serializedObject.FindProperty("suppressErrors").boolValue;
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
                    // If the generator is not configured properly yet, then errors may occur. The following
                    // likely, non-fatal exceptions are caught and (optionally) suppressed here, to avoid flooding
                    // the editor console while configuring. This behaviour is optional to avoid suppressing
                    // useful diagnostic information if using this visualizer while writing new code.
                    catch (InvalidOperationException) { if (!suppressErrors) throw; }
                    catch (NullReferenceException) { if (!suppressErrors) throw; }
                    catch (ArgumentException) { if (!suppressErrors) throw; }
                }
            }
        }

        MapGenModule GetMapGenModule()
        {
            return (MapGenModule)serializedObject.FindProperty("mapGenModule").objectReferenceValue;
        }
    } 
}