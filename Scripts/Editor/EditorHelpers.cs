using UnityEditor;
using UnityEngine.Assertions;

/// <summary>
/// Helper functions for editor scripts.
/// </summary>
public static class EditorHelpers
{
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
