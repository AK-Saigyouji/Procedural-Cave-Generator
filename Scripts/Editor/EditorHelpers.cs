using UnityEditor;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Helper functions for editor scripts.
/// </summary>
public static class EditorHelpers
{
    /// <summary>
    /// Returns all the children of this property that are one level deep. i.e. will not return children of children, etc.
    /// </summary>
    public static IEnumerable<SerializedProperty> GetChildren(this SerializedProperty property)
    {
        var currentProperty = property.Copy();
        var finalProperty = property.GetEndProperty();
        bool hasNext = currentProperty.Next(true);
        while (hasNext && !SerializedProperty.EqualContents(currentProperty, finalProperty))
        {
            yield return currentProperty;
            hasNext = currentProperty.Next(false);
        }
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
