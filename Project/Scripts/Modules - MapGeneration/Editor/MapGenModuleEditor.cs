using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AKSaigyouji.Maps;

namespace AKSaigyouji.Modules.MapGeneration
{
    [CustomEditor(typeof(MapGenModule), editorForChildClasses: true)]
    public sealed class MapGenModuleEditor : Editor
    {
        bool visualize = false;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override void OnPreviewSettings()
        {
            if (GUILayout.Button("Reroll"))
            {
                ((MapGenModule)target).Seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            }
            EditorGUIUtility.labelWidth = 100;
            visualize = EditorGUILayout.Toggle("Show Preview", visualize);
            EditorGUIUtility.labelWidth = 0;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if (visualize && Event.current.type == EventType.Repaint)
            {
                MapGenModule mapGenerator = (MapGenModule)serializedObject.targetObject;
                if (mapGenerator != null)
                {
                    Map map = mapGenerator.Generate();
                    if (map != null)
                    {
                        Texture texture = map.ToTexture();
                        GUI.DrawTexture(r, texture, ScaleMode.StretchToFill, false);
                    }
                }
            }
        }
    } 
}