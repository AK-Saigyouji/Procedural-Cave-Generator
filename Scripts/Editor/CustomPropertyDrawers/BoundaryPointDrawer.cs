/* This custom property drawer serves two purposes. The first is to clean up the appearance of the property so that it
 gets laid out on a single line, rather than taking up three lines with a foldout. The second is to automatically indicate
 whether the 'magnitude' property corresponds to the X axis or Y axis. This makes it more intuitive to work with. */

using UnityEngine;
using UnityEditor;
using CaveGeneration.Modules;

[CustomPropertyDrawer(typeof(BoundaryPoint))]
public sealed class BoundaryPointDrawer : PropertyDrawer
{
    const string SIDE = "side";
    const string MAGNITUDE = "magnitude";
    const float CHAR_LABEL_WIDTH = 14;
    const float SIDE_LABEL_WIDTH = 45f;
    const float HORIZONTAL_PADDING = 2f;

    const string SIDE_TOOLTIP = "Which side of the boundary is this point on?";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        label = EditorGUI.BeginProperty(position, label, property);
        Rect propertyRect = EditorGUI.PrefixLabel(position, label);
        EditorGUI.indentLevel = 0;
        propertyRect.width *= 0.75f;
        SerializedProperty side = property.FindPropertyRelative(SIDE);
        string magnitudeLabel = DetermineMagnitudeAxis(side);
        EditorGUIUtility.labelWidth = SIDE_LABEL_WIDTH;
        EditorGUI.PropertyField(propertyRect, side, new GUIContent("Side", SIDE_TOOLTIP));
        propertyRect.x += propertyRect.width + HORIZONTAL_PADDING;
        propertyRect.width /= 3;
        EditorGUIUtility.labelWidth = CHAR_LABEL_WIDTH;
        EditorGUI.PropertyField(propertyRect, property.FindPropertyRelative(MAGNITUDE), new GUIContent(magnitudeLabel));
        EditorGUI.EndProperty();
    }

    // This determines whether the magnitude corresponds to the x axis or y axis, based on the chosen side.
    // i.e. if the point is on the left or right, then magnitude corresponds to the y-axis. Top or bot corresponds to x.
    // Thus this method returns 'X' or 'Y' accordingly.
    string DetermineMagnitudeAxis(SerializedProperty sideProperty)
    {
        int val = sideProperty.enumValueIndex;
        BoundaryPoint.Side side = (BoundaryPoint.Side)val;
        if (side == BoundaryPoint.Side.Left || side == BoundaryPoint.Side.Right)
        {
            return "Y";
        }
        else
        {
            return "X";
        }
    }
}