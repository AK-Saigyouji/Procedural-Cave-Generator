/* This custom property drawer serves two purposes. The first is to clean up the appearance of the property so that it
 gets laid out on a single line, rather than taking up three lines with a foldout. The second is to automatically indicate
 whether the 'magnitude' property corresponds to the X axis or Y axis. This makes it more intuitive to work with. */

using UnityEngine;
using UnityEditor;
using CaveGeneration.Modules;

[CustomPropertyDrawer(typeof(WeightedPrefab))]
public sealed class WeightedPrefabDrawer : PropertyDrawer
{
    const float LABEL_WIDTH = 45f;
    const float HORIZONTAL_PADDING = 2f;

    const string PREFAB_NAME = "prefab";
    const string WEIGHT_NAME = "weight";

    const string WEIGHT_TOOLTIP = "The probability of this rock being chosen relative to the other rocks' weights.";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        label = EditorGUI.BeginProperty(position, label, property);
        Rect propertyRect = EditorGUI.PrefixLabel(position, label);
        EditorGUI.indentLevel = 0;
        propertyRect.width *= 0.6f;
        SerializedProperty prefab = property.FindPropertyRelative(PREFAB_NAME);
        EditorGUI.PropertyField(propertyRect, prefab, GUIContent.none);

        propertyRect.x += propertyRect.width + HORIZONTAL_PADDING;
        propertyRect.width *= 0.666f;
        propertyRect.width -= HORIZONTAL_PADDING;
        EditorGUIUtility.labelWidth = LABEL_WIDTH;
        GUIContent weightLabel = new GUIContent("Weight", WEIGHT_TOOLTIP);
        SerializedProperty weight = property.FindPropertyRelative(WEIGHT_NAME);
        EditorGUI.PropertyField(propertyRect, weight, weightLabel);
        EditorGUI.EndProperty();
    }
}