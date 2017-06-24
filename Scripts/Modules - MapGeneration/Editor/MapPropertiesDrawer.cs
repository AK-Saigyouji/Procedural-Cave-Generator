using UnityEngine;
using UnityEditor;
using AKSaigyouji.EditorScripting;

namespace AKSaigyouji.Modules.MapGeneration
{
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
}