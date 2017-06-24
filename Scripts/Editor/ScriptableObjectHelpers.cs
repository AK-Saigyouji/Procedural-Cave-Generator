using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;

namespace AKSaigyouji.EditorScripting
{
    public static class ScriptableObjectHelpers
    {
        /// <summary>
        /// Similar to Instantiate and MemberwiseClone, but doesn't add "(Clone)" to the name.
        /// </summary>
        public static T CopyShallow<T>(this T obj) where T : ScriptableObject
        {
            var copy = ScriptableObject.Instantiate(obj);
            copy.name = obj.name;
            return copy;
        }

        /// <summary>
        /// Will create copies of everything in the object's reference graph using the following rules: anything
        /// Unity can serialize (see Unity documentation on serialization for a complete list), but also
        /// ScriptableObjects that are either direct references, or otherwise contained in a serializable type 
        /// belonging to the previous category. So as of this writing, a field containing a scriptable object adorned
        /// with [SerializeField] or public, contained in an array or list, or a field in a serializable custom class 
        /// will be copied. Examples where a scriptable object will not be copied: as keys or values in a dictionary,
        /// in a non-public field not marked with [SerializeField], in a non-serializable class. 
        /// </summary>
        public static T CopyDeep<T>(this T obj) where T : ScriptableObject
        {
            obj = obj.CopyShallow();
            var serializedObject = new SerializedObject(obj);
            var iterator = serializedObject.GetIterator();
            iterator.Next(true);
            while (iterator.NextVisible(true))
            {
                if (iterator.propertyType == SerializedPropertyType.ObjectReference)
                {
                    ScriptableObject so = iterator.objectReferenceValue as ScriptableObject;
                    if (so != null)
                    {
                        so = so.CopyDeep();
                        iterator.objectReferenceValue = so;
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
            return (T)serializedObject.targetObject;
        }

        /// <summary>
        /// Serializes a scriptable object that references other scriptable objects: all scriptable objects
        /// in the root's reference graph are stored with the root object. Will not work if any of the
        /// ScriptableObjects already exist as assets. Consider creating a deep copy first.
        /// </summary>
        public static void SaveCompoundScriptableObject(ScriptableObject obj, string path)
        {
            Assert.IsNotNull(obj);
            Assert.IsNotNull(path);
            AssetDatabase.CreateAsset(obj, path);
            foreach (ScriptableObject childScriptableObject in GetChildScriptableObjects(obj))
            {
                AssetDatabase.AddObjectToAsset(childScriptableObject, path);
            }
            AssetDatabase.ImportAsset(path); // This will ensure the asset shows up in the editor.
        }

        public static IEnumerable<ScriptableObject> GetChildScriptableObjects(ScriptableObject obj)
        {
            Assert.IsNotNull(obj);
            var serializedObject = new SerializedObject(obj);

            var children = GetProperties(serializedObject)
                .Where(sObj => sObj.propertyType == SerializedPropertyType.ObjectReference)
                .Select(sObj => sObj.objectReferenceValue as ScriptableObject)
                .Where(property => property != null);

            foreach (ScriptableObject child in children)
            {
                yield return child;
                foreach (ScriptableObject childOfChild in GetChildScriptableObjects(child))
                {
                    yield return childOfChild;
                }
            }
        }

        static IEnumerable<SerializedProperty> GetProperties(this SerializedObject serializedObject)
        {
            Assert.IsNotNull(serializedObject);
            var iterator = serializedObject.GetIterator();
            iterator.Next(true);
            while (iterator.NextVisible(true))
            {
                // The iterator is also the serializedproperty itself. 
                yield return iterator;
            }
        }
    } 
}