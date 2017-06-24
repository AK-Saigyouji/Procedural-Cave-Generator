using UnityEngine;
using UnityEditor;
using AKSaigyouji.EditorScripting;

namespace AKSaigyouji.Modules.MapGeneration
{
    public static class MapGenModuleMenuItems
    {
        [MenuItem("CONTEXT/MapGenModule/Save Single Map")]
        static void SaveAsPNG(MenuCommand command)
        {
            var module = (MapGenModule)command.context;
            string folderPath = IOHelpers.GetFolderContainingAsset(module);
            Texture2D texture = module.Generate().ToTexture();
            string name = string.Format("{0}{1}.png", module.name, MapImporter.MAP_SUBSTRING);
            string texturePath = IOHelpers.GetAvailableAssetPath(folderPath, name);
            IOHelpers.SaveTextureAsPNG(texture, texturePath);
        }
    } 
}