/* Editor scripting in Unity is a bit of a mess, with a lot of magic numbers, undocumented features, and
 a less than intuitive API based on .NET reflection. This is a collection of commonly used functionality and 
 constants for editor scripting. Much of the functionality wraps existing functions, providing a more
 intuitive API, albeit one that is still very error prone.
 
  The iteration logic over properties requires some explanation. In the context of editors, a SerializedObject
 represents a serialized stream for an object being inspected. A SerializedProperty, naturally, is a serialized stream
 for a property on such an object. To iterate over the properties of an object, we access an iterator by calling 
 GetIterator on the SerializedObject. On the surface, the iterator works a lot like a normal C# Enumerator, albeit with
 Next and NextVisible (with a bool parameter as to whether the properties of a property should be entered) instead of 
 MoveNext. But instead of having a Current property that points to the current property, the iterator itself is the
 current SerializedProperty. i.e the properties themselves form the chain between all of the properties in the object.
 A significant practical consequence of this implementation is that any reference to a given property becomes invalid
 once we call Next or NextVisible on that property. We could copy every property before returning it, but this would
 result in a lot of garbage being produced just from inspecting objects in the editor. SerializedProperty has an instance
 method Copy specifically for this purpose.
 
  The main upshot of the above paragraph is that one has to be very careful about making assumptions about the state of a 
 given SerializedProperty.*/ 

using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A library of helper functions for editor scripts.
/// </summary>
public static class EditorHelpers
{
    /// <summary>
    /// The height of a property field in the inspector for just the field itself, with no padding.
    /// </summary>
    public const int PROPERTY_HEIGHT_BASE = 16;

    /// <summary>
    /// The height of property padding.
    /// </summary>
    public const int PROPERTY_HEIGHT_PADDING = 2;

    /// <summary>
    /// The total height of a property field.
    /// </summary>
    public const int PROPERTY_HEIGHT_TOTAL = PROPERTY_HEIGHT_BASE + PROPERTY_HEIGHT_PADDING;

    /// <summary>
    /// Return all the top level properties for this serialized object.
    /// </summary>
    public static IEnumerable<SerializedProperty> GetProperties(this SerializedObject serializedObject)
    {
        var iterator = serializedObject.GetIterator();
        iterator.Next(true);
        while (iterator.NextVisible(false))
        {
            // The iterator is also the serializedproperty itself. 
            yield return iterator;
        }
    }

    /// <summary>
    /// Return all the children of this property that are one level deep. i.e. will not return children of children, etc.
    /// </summary>
    public static IEnumerable<SerializedProperty> GetChildren(this SerializedProperty property)
    {
        var currentProperty = property.Copy();
        var finalProperty = property.GetEndProperty();
        bool hasNext = currentProperty.NextVisible(true);
        while (hasNext && !SerializedProperty.EqualContents(currentProperty, finalProperty))
        {
            yield return currentProperty;
            hasNext = currentProperty.NextVisible(false);
        }
    }

    /// <summary>
    /// Create a cached editor here, under a foldout.
    /// </summary>
    /// <param name="label">Label for the foldout.</param>
    /// <param name="targetObject">The object for which an editor is being created.</param>
    /// <param name="foldout">Current state of the foldout (true is open, false is closed).</param>
    public static void DrawFoldoutEditor(string label, Object targetObject, ref bool foldout, ref Editor editor)
    {
        if (foldout = EditorGUILayout.Foldout(foldout, label))
        {
            Editor.CreateCachedEditor(targetObject, null, ref editor);
            EditorGUI.indentLevel++;
            editor.OnInspectorGUI();
            EditorGUI.indentLevel--;
        }
    }

    /// <summary>
    /// Returns the height necessary to house the GUI provided by DrawSimpleGUI.
    /// </summary>
    /// <returns></returns>
    public static int GetHeightForSimpleGUI(SerializedProperty property)
    {
        int numChildren = GetChildren(property).Count();
        return numChildren * PROPERTY_HEIGHT_TOTAL - PROPERTY_HEIGHT_PADDING;
    }

    /// <summary>
    /// Draws all the children, ignoring the property label. Useful for writing a custom drawer for an aggregation
    /// class meant to display just the properties without the extraneous label.
    /// </summary>
    public static void DrawSimpleGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        position.height = PROPERTY_HEIGHT_BASE;
        foreach (var child in GetChildren(property))
        {
            EditorGUI.PropertyField(position, child);
            position.y += PROPERTY_HEIGHT_TOTAL;
        }
        EditorGUI.EndProperty();
    }

    public static string AppendToPath(string path, string toAppend)
    {
        Assert.IsNotNull(path);
        Assert.IsNotNull(toAppend);
        return string.Format("{0}/{1}", path, toAppend);
    }

    /// <summary>
    /// Similar to AssetDatabase.CreateFolder but returns the path to the created folder instead of the guid.
    /// </summary>
    public static string CreateFolder(string path, string name)
    {
        Assert.IsNotNull(path);
        Assert.IsNotNull(name);
        string guid = AssetDatabase.CreateFolder(path, name);
        string folderPath = AssetDatabase.GUIDToAssetPath(guid);
        return folderPath;
    }
}
