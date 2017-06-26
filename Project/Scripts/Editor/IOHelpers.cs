using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;

namespace AKSaigyouji.EditorScripting
{
    /// <summary>
    /// Library of helper methods for working with directories and files in Unity.
    /// </summary>
    public static class IOHelpers
    {
        // Unity paths use "/" regardless of operating system.
        const string PATH_SEPARATOR = "/";

        public static string CombinePath(string prefix, string suffix)
        {
            if (prefix.EndsWith(PATH_SEPARATOR))
            {
                return prefix + suffix;
            }
            else
            {
                return string.Format("{0}{1}{2}", prefix, PATH_SEPARATOR, suffix);
            }
        }

        public static string CombinePath(params string[] pathComponents)
        {
            return string.Join(PATH_SEPARATOR, pathComponents);
        }

        // For the following method, trying to use File.Exists is risky, as directory structures and access can differ quite
        // a bit based on build target and OS. AssetDatabase hides this complexity, so that's why that API is used despite
        // the clumsiness of this approach. 

        /// <summary>
        /// Searches for all assets in the folder whose name 'matches' the given filter, and returns true if any are found.
        /// Can give false positives if the filter does not match the searched asset uniquely, so use with caution.
        /// </summary>
        public static bool AssetExists(string filter, string folderPath)
        {
            Assert.IsNotNull(filter);
            Assert.IsNotNull(folderPath);
            int numAssetsFound = AssetDatabase.FindAssets(filter, new[] { folderPath }).Length;
            if (numAssetsFound > 1)
            {
                Debug.LogWarning("Multiple assets matched filter.");
            }
            return numAssetsFound > 0;
        }

        public static void SaveTextureAsPNG(Texture2D texture, string path)
        {
            Assert.IsNotNull(texture);
            Assert.IsFalse(string.IsNullOrEmpty(path));
            string assetString = "Assets/";
            if (path.StartsWith(assetString))
            {
                // Application.dataPath includes Assets/, so we strip it if it's in the provided path.
                path = path.Remove(0, assetString.Length);
            }
            path = CombinePath(Application.dataPath, path);
            File.WriteAllBytes(path, texture.EncodeToPNG());
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Create a folder at the top level if it doesn't exist. If it does, do nothing. Returns path to folder.
        /// </summary>
        public static string RequireFolder(string folderName)
        {
            return RequireFolder("Assets", folderName);
        }

        /// <summary>
        /// Create a folder if it doesn't exist. If it does, do nothing. Returns path to folder.
        /// </summary>
        public static string RequireFolder(string parentFolder, string folderName)
        {
            Assert.IsFalse(string.IsNullOrEmpty(parentFolder));
            Assert.IsFalse(string.IsNullOrEmpty(folderName));
            string path = CombinePath(parentFolder, folderName);
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parentFolder, folderName);
            }
            return path;
        }

        /// <summary>
        /// If the path is unavailable, with add the smallest integer to the file name in the path such that
        /// the resulting path does not already exist. This ensures that AssetDatabase.CreateAsset will not 
        /// overwrite an existing file. Example: Assets/Materials/floor.mat -> Assets/Materials/floor 1.asset ->
        /// Assets/Material/floor 2.asset etc.
        /// </summary>
        public static string GetAvailableAssetPath(string folderPath, string fileName)
        {
            Assert.IsFalse(string.IsNullOrEmpty(fileName));
            string[] filesInFolder = Directory.GetFiles(folderPath);
            int i = 1;
            string extension = Path.GetExtension(fileName);
            string rootName = Path.GetFileNameWithoutExtension(fileName);
            string newFileName = rootName;
            while (filesInFolder.Any(path => path.Contains(newFileName)))
            {
                newFileName = string.Format("{0} {1}", rootName, i);
                i++;
            }
            string availablePath = CombinePath(folderPath, newFileName + extension);
            return availablePath;
        }

        public static string GetFolderContainingAsset(UnityEngine.Object asset)
        {
            Assert.IsNotNull(asset);
            Assert.IsFalse(string.IsNullOrEmpty(asset.name));
            string pathToModule = AssetDatabase.GetAssetPath(asset);
            string folderContainingModule = pathToModule.Remove(pathToModule.LastIndexOf(asset.name) - 1);
            return folderContainingModule;
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
    } 
}