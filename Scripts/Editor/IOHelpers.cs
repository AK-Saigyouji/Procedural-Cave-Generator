using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;

/// <summary>
/// Library of helper methods for working with directories and files in Unity.
/// </summary>
public static class IOHelpers
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

    /// <summary>
    /// Does the folder at the path contain any files or folders?
    /// </summary>
    public static bool IsFolderEmpty(string path)
    {
        Assert.IsNotNull(path);
        Assert.IsTrue(AssetDatabase.IsValidFolder(path));
        return !Directory.GetFileSystemEntries(path).Any();
    }

    /// <summary>
    /// Create a prefab out of the game object at the path, with the given name.
    /// </summary>
    public static GameObject CreatePrefab(GameObject gameObject, string path, string name)
    {
        Assert.IsNotNull(gameObject);
        Assert.IsNotNull(path);
        Assert.IsNotNull(name);
        string finalPath = AppendToPath(path, name);
        return PrefabUtility.CreatePrefab(finalPath, gameObject);
    }

    /// <summary>
    /// Given an object, the path to a folder, and a name, will convert the object to an asset with an appropriate
    /// extension, and save it in that folder with that name.
    /// </summary>
    public static void CreateAsset(UnityEngine.Object asset, string folderPath, string name)
    {
        Assert.IsNotNull(asset);
        Assert.IsNotNull(folderPath);
        Assert.IsNotNull(name);
        name = string.Format("{0}.asset", name);
        string assetPath = AppendToPath(folderPath, name);
        AssetDatabase.CreateAsset(asset, assetPath);
    }
}