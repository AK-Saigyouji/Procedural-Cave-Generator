/* This is the main entry point to the map editor window. It provides a set of tools for visually building
 a map generator module out of other modules. */

using UnityEngine;
using UnityEditor;
using AKSaigyouji.EditorScripting;
using System.Collections.Generic;
using System.Linq;

namespace AKSaigyouji.Modules.MapGeneration
{
    public sealed class MapEditorWindow : EditorWindow
    {
        /// <summary>
        /// Add this to the position of any object whose position should be affected by drag actions.
        /// </summary>
        public static Vector2 DragOffset { get { return WindowHelpers.RoundToLattice(dragOffset); } }
        static Vector2 dragOffset;

        readonly Zoom zoom = new Zoom();
        readonly NodeSelection selection = new NodeSelection();

        int nodeNumber; // next id for a node. Increments on node creation, does not decrement on removal. Resets on reload.
        NodeEditorSave savedEditor;
        List<NodeGroup> nodes; // convention: nodes deeper in the list are drawn in front of shallow nodes.

        readonly KeyCode ZOOM_IN_KEY = KeyCode.Equals;
        readonly KeyCode ZOOM_OUT_KEY = KeyCode.Minus;

        const string SAVED_MODULE_NAME = "Compound.asset";
        readonly string SAVED_TEXTURE_NAME = string.Format("SavedNodeEditor{0}.png", MapImporter.MAP_SUBSTRING);

        [MenuItem("Window/AKS - Map Gen Editor")]
        static void OpenWindow()
        {
            var window = GetWindow<MapEditorWindow>();
            window.titleContent = new GUIContent("Map Designer");
        }
        
        void OnGUI()
        {
            UpdateZoom();

            zoom.Begin();
            DrawGrid(NodeEditorSettings.MINOR_GRID_SPACING, new Color(0.5f, 0.5f, 0.5f, 0.2f));
            DrawGrid(NodeEditorSettings.MAJOR_GRID_SPACING, new Color(0.5f, 0.5f, 0.5f, 0.4f));
            DrawNodes();
            zoom.End();

            BeginWindows();
            DrawWindows();
            EndWindows();

            zoom.Begin();
            ProcessNodeEvents();
            HandleContextClicks();
            HandleDrag();
            zoom.End();

            selection.MadeThisEvent = false;
            if (GUI.changed)
            {
                Repaint();
            }
        }

        void OnEnable()
        {
            zoom.Reset();
            nodeNumber = 0;
            Load();
        }

        void OnDisable()
        {
            Save();
        }

        void Save()
        {
            savedEditor = NodeEditorSave.CreateSave(nodes, dragOffset);
            NodeEditorSave.Save(savedEditor);
        }

        void Load()
        {
            var savedEditor = NodeEditorSave.Load();
            if (savedEditor == null)
            {
                nodes = new List<NodeGroup>();
                return;
            }

            dragOffset = savedEditor.Offset; // Important: restore offset before restoring nodes.
            nodes = new List<NodeGroup>();
            foreach (SavedNode savedNode in savedEditor.Nodes)
            {
                AddNode(savedNode.NodeRect, savedNode.MapGenModule);
            }
        }

        void SaveModule()
        {
            var compoundModule = BuildModule();
            string folderPath = NodeEditorSettings.PathToModuleFolder;
            string modulePath = IOHelpers.GetAvailableAssetPath(folderPath, SAVED_MODULE_NAME);
            AssetDatabase.CreateAsset(compoundModule, modulePath);
        }

        void SaveMap()
        {
            var compoundModule = BuildModule();
            string folderPath = NodeEditorSettings.PathToMapFolder;
            Texture2D texture = compoundModule.Generate().ToTexture();
            string texturePath = IOHelpers.GetAvailableAssetPath(folderPath, SAVED_TEXTURE_NAME);
            IOHelpers.SaveTextureAsPNG(texture, texturePath);
        }

        MapGenCompound BuildModule()
        {
            var modules = nodes.Select(node => node.MapGenModule);
            var offsets = nodes.Select(node => node.Position.position)
                               .Select(vec => new Vector2(vec.x, -vec.y) / NodeEditorSettings.GRID_UNITS_TO_WORLD_UNITS);
            var compoundModule = MapGenCompound.Construct(modules, offsets);
            return compoundModule;
        }

        void UpdateZoom()
        {
            Event current = Event.current;
            if (current.type == EventType.KeyDown)
            {
                if (current.keyCode == ZOOM_IN_KEY)
                {
                    zoom.In();
                }
                if (current.keyCode == ZOOM_OUT_KEY)
                {
                    zoom.Out();
                }
                GUI.changed = true;
            }
        }

        void DrawGrid(float gridSpacing, Color color)
        {
            Rect area = new Rect(Vector2.zero, position.size);
            HandleHelpers.DrawGrid(area, gridSpacing, color, DragOffset);
        }

        void DrawWindows()
        {
            foreach (var nodes in nodes)
            {
                nodes.DrawWindows();
            }
        }

        void DrawNodes()
        {
            foreach (var node in nodes)
            {
                node.Draw();
            }
        }

        void ProcessNodeEvents()
        {
            NodeGroup selected = null;
            foreach (var node in Enumerable.Reverse(nodes))
            {
                if (node.UpdateSelection(Event.current, selection))
                {
                    selected = node;
                }
            }
            if (selected != null)
            {
                // push node to the back of the list so it gets drawn on top
                nodes.Remove(selected);
                nodes.Add(selected);
            }
        }

        void HandleContextClicks()
        {
            Event e = Event.current;
            if (e.type == EventType.ContextClick)
            {
                ShowContextMenu(e.mousePosition);
            }
        }

        void HandleDrag()
        {
            Event e = Event.current;
            if (e.type == EventType.MouseDrag)
            {
                if (e.mousePosition.y > WindowHelpers.WINDOW_TASKBAR_HEIGHT && e.button == WindowHelpers.LEFT_MOUSE_BUTTON)
                {
                    dragOffset += e.delta;
                    GUI.changed = true;
                }
            }
        }

        void ShowContextMenu(Vector2 mousePosition)
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Add node"), false, () => AddNode(mousePosition));
            genericMenu.AddItem(new GUIContent("Clear all"), false, ClearAll);
            genericMenu.AddItem(new GUIContent("Preview"), false, Preview);
            genericMenu.AddItem(new GUIContent("Build Map"), false, SaveMap);
            genericMenu.AddItem(new GUIContent("Build Module"), false, SaveModule);
            genericMenu.ShowAsContext();
        }

        void ClearAll()
        {
            if (EditorUtility.DisplayDialog("Clear All Nodes", "Remove all nodes?", "Yes", "Cancel"))
            {
                nodes.Clear();
            }
        }

        void Preview()
        {
            var preview = GetWindow<PreviewWindow>();
            preview.UpdateWindow(BuildModule());
        }

        void AddNode(Vector2 mousePosition)
        {
            Rect position = new Rect(mousePosition, NodeEditorSettings.DEFAULT_NODE_SIZE * Vector2.one);
            AddNode(position, mapGenModule: null);
        }

        void AddNode(Rect position, MapGenModule mapGenModule)
        {
            nodes.Add(new NodeGroup(nodeNumber, position, RemoveNode, mapGenModule));
            nodeNumber++;
        }

        void RemoveNode(NodeGroup node)
        {
            nodes.Remove(node);
        }
    } 
}

