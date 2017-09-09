/* This object holds the data necessary to store and load the node editor between sessions. The SavedNode type
 handles serializing the nodes themselves, as that requires diving into a tree of references. */

using AKSaigyouji.EditorScripting;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;

namespace AKSaigyouji.Modules.MapGeneration
{
    public sealed class NodeEditorSave : ScriptableObject
    {
        public IEnumerable<SavedNode> Nodes
        {
            get { return nodes.AsReadOnly(); }
            set { nodes = value.ToList(); }
        }

        public Vector2 Offset { get { return offset; } set { offset = value; } }

        [SerializeField] List<SavedNode> nodes = new List<SavedNode>();
        [SerializeField] Vector2 offset;

        const string assets = "Assets";
        const string resources = "Editor Default Resources";
        const string assetName = "AKSNodeEditorState";
        const string assetNameWithExtension = assetName + ".asset";

        /// <summary>
        /// Load the editor from the last session. If a previous editor is not found, returns an empty save which
        /// can be consumed like a non-empty one - in particular, never returns null.
        /// </summary>
        public static NodeEditorSave Load()
        {
            NodeEditorSave save = null;
            save = EditorGUIUtility.Load(IOHelpers.CombinePath(assets, resources, assetNameWithExtension)) as NodeEditorSave;
            if (save == null)
            {
                save = CreateInstance<NodeEditorSave>();
            }
            Assert.IsNotNull(save);
            return save;
        }

        public static void Save(NodeEditorSave save)
        {
            Assert.IsNotNull(save);
            Assert.IsTrue(save.Nodes.All(node => node != null));
            string folderPath = Path.Combine(assets, resources);
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder(assets, resources);
            }
            string assetPath = Path.Combine(folderPath, assetNameWithExtension);
            if (IOHelpers.AssetExists(assetName, folderPath))
            {
                AssetDatabase.DeleteAsset(assetPath);
            }
            AssetDatabase.CreateAsset(save, assetPath);
            AssetDatabase.SaveAssets();
        }

        public static NodeEditorSave CreateSave(IEnumerable<NodeGroup> nodes, Vector2 offset)
        {
            Assert.IsNotNull(nodes);
            Assert.IsTrue(nodes.All(node => node != null));
            var savedEditor = CreateInstance<NodeEditorSave>();
            savedEditor.Nodes = nodes.Select(node => new SavedNode(node.MapGenModule, node.Position)).ToList();
            savedEditor.Offset = offset;
            savedEditor.hideFlags = HideFlags.NotEditable;
            return savedEditor;
        }
    } 
}