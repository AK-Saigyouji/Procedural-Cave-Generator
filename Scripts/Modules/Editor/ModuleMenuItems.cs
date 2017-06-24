using UnityEditor;
using AKSaigyouji.EditorScripting;

namespace AKSaigyouji.Modules
{
    public static class ModuleMenuItems
    {
        [MenuItem("CONTEXT/Module/Deep Copy")]
        static void Duplicate(MenuCommand command)
        {
            Module module = (Module)command.context;
            string folderPath = IOHelpers.GetFolderContainingAsset(module);
            string path = IOHelpers.GetAvailableAssetPath(folderPath, module.name + ".asset");
            Module copiedAsset = module.CopyDeep();
            ScriptableObjectHelpers.SaveCompoundScriptableObject(copiedAsset, path);
        }
    } 
}