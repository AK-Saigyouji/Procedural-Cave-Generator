/* This is the custom inspector for CaveGeneratorUI, and the main user interface for the cave generation system.*/

using UnityEngine;
using UnityEditor;

namespace AKSaigyouji.CaveGeneration
{
    [CustomEditor(typeof(CaveGeneratorUI))]
    public sealed class CaveGeneratorUIEditor : Editor
    {
        Editor outlineEditor;
        Editor threeTierEditor;

        void OnEnable()
        {
            outlineEditor = CreateEditor(serializedObject.targetObject, typeof(OutlineCaveGenEditor));
            threeTierEditor = CreateEditor(serializedObject.targetObject, typeof(ThreeTierCaveGenEditor));
        }

        public override void OnInspectorGUI()
        {
            switch ((CaveGeneratorUI.CaveGeneratorType)serializedObject.FindProperty("type").enumValueIndex)
            {
                case CaveGeneratorUI.CaveGeneratorType.RockOutline:
                    outlineEditor.OnInspectorGUI();
                    break;
                case CaveGeneratorUI.CaveGeneratorType.ThreeTiered:
                    threeTierEditor.OnInspectorGUI();
                    break;
            }
        }
    }
}
