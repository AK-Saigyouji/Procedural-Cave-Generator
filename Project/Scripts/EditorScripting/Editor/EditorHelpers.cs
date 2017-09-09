/* This is a collection of commonly used functionality and constants for editor scripting. 
 Much of the functionality wraps existing functions, providing a more intuitive API, albeit one that still has to be 
 used very carefully to avoid errors.
 
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
 method Copy specifically for this purpose.*/ 

using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System.Linq;

namespace AKSaigyouji.EditorScripting
{
    /// <summary>
    /// A helper library for custom editor and property drawer scripts.
    /// </summary>
    public static class EditorHelpers
    {
        /// <summary>
        /// The total height of a property field.
        /// </summary>
        public static float PROPERTY_HEIGHT_TOTAL = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        /// <summary>
        /// The threshold for the inspector to wrap horizontal content to a second line.
        /// </summary>
        public const int MIN_WIDTH = 333;

        /// <summary>
        /// This appears to be the quantity that each indent level of the GUI provides.
        /// </summary>
        public const int HORIZONTAL_INDENT = 15;

        /// <summary>
        /// Draw a simple line in the inspector, for visual purposes.
        /// </summary>
        public static void DrawLine()
        {
            EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
        }

        /// <summary>
        /// This serializes a compound scriptable object, i.e. a scriptable object with children that are scriptable objects,
        /// who themselves may have such children, etc. All such children will have their flags set to hidden in hierarchy.
        /// </summary>
        public static void SaveScriptableObjectChildren(ScriptableObject save, string assetPath)
        {
            AssetDatabase.CreateAsset(save, assetPath);
            foreach (var child in ScriptableObjectHelpers.GetChildScriptableObjects(save))
            {
                child.hideFlags = HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(child, assetPath);
            }
        }

        /// <summary>
        /// Return all the top level properties for this serialized object. Do not alter the iterator returned
        /// by this method, and note that its state will be invalidated by the next yield.
        /// </summary>
        public static IEnumerable<SerializedProperty> GetProperties(this SerializedObject serializedObject)
        {
            Assert.IsNotNull(serializedObject);
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
            Assert.IsNotNull(property);
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
            Assert.IsNotNull(label);
            foldout = EditorGUILayout.Foldout(foldout, label);
            if (foldout && targetObject != null)
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
        public static float GetHeightForSimpleGUI(SerializedProperty property)
        {
            Assert.IsNotNull(property);
            int numChildren = GetChildren(property).Count();
            return numChildren * PROPERTY_HEIGHT_TOTAL - EditorGUIUtility.standardVerticalSpacing;
        }

        /// <summary>
        /// Draws all the children, ignoring the property label. Useful for writing a custom drawer for an aggregation
        /// class meant to display just the properties without the extraneous label.
        /// </summary>
        public static void DrawSimpleGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Assert.IsNotNull(property);
            Assert.IsNotNull(label);
            EditorGUI.BeginProperty(position, label, property);
            position.height = EditorGUIUtility.singleLineHeight;
            foreach (var child in GetChildren(property))
            {
                EditorGUI.PropertyField(position, child);
                position.y += PROPERTY_HEIGHT_TOTAL;
            }
            EditorGUI.EndProperty();
        }
    }
}