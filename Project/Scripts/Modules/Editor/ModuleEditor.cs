/* Understanding the point of this custom editor requires understanding the problem it solves. Modules are designed
 to be easily combined and aggregated. e.g. you can write a map generation module that takes an existing map gen module,
 but further processes it by adding entrances and connecting them to the existing passages in the map. This is an example
 of the decorator pattern: the original module is 'decorated' with entrance logic. But the inspector to this decorator
 will (by default) only expose the properties of the new module, and not the decorated module. This editor ensures that the
 decorated module has an editor for its properties exposed in the decorator's inspector. This works recursively, 
 ensuring that a complex composition hierarchy can all be customized at the top level.*/

using System.Linq;
using UnityEngine;
using UnityEditor;
using AKSaigyouji.EditorScripting;

namespace AKSaigyouji.Modules
{
    [CustomEditor(typeof(Module), editorForChildClasses: true)]
    public sealed class ModuleEditor : Editor
    {
        Editor[] moduleEditors;
        bool[] foldouts;


        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            serializedObject.Update();
            UpdateModuleEditors();
            int currentEditor = -1;
            foreach (SerializedProperty property in EditorHelpers.GetProperties(serializedObject).Where(IsModule))
            {
                if (property.objectReferenceValue != serializedObject.targetObject)
                {
                    currentEditor++;
                    string name = property.displayName;
                    UnityEngine.Object obj = property.objectReferenceValue;
                    EditorHelpers.DrawFoldoutEditor(name, obj, ref foldouts[currentEditor], ref moduleEditors[currentEditor]);
                }
                else
                {
                    property.objectReferenceValue = null;
                    Debug.LogWarning("Action denied: Module inserted into itself.");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        void UpdateModuleEditors()
        {
            int numModules = EditorHelpers.GetProperties(serializedObject).Where(IsModule).Count();
            if (moduleEditors == null || numModules > moduleEditors.Length)
            {
                moduleEditors = new Editor[numModules];
                foldouts = new bool[numModules];
            }
        }

        bool IsModule(SerializedProperty property)
        {
            return (property.propertyType == SerializedPropertyType.ObjectReference)
                && (property.objectReferenceValue is Module);
        }
    } 
}