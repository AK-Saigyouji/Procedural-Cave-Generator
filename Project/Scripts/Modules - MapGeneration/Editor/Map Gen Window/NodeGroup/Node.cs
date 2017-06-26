using System;
using UnityEngine;
using UnityEditor;
using AKSaigyouji.EditorScripting;

namespace AKSaigyouji.Modules.MapGeneration
{
    public sealed class Node
    {
        public Rect Rect
        {
            get
            {
                Rect rect = new Rect(nodeRect);
                rect.position += MapEditorWindow.DragOffset;
                return WindowHelpers.RoundToLattice(rect);
            }
        }

        public bool IsSelected { get { return style == NodeEditorSettings.SelectedNodeStyle; } }

        readonly Action onRemoveNode;
        readonly DragHandler dragHandler;

        GUIStyle style;
        Rect nodeRect; // use Rect, which adds the offset from drag actions.

        public Node(Rect position, Action onClickRemoveNode)
        {
            nodeRect = position;
            nodeRect.position -= MapEditorWindow.DragOffset;
            style = NodeEditorSettings.DefaultNodeStyle;
            onRemoveNode = onClickRemoveNode;
            dragHandler = new DragHandler();
        }

        public void Draw()
        {
            GUI.Box(Rect, string.Empty, style);
        }

        public void Select()
        {
            style = NodeEditorSettings.SelectedNodeStyle;
        }

        public void Deselect()
        {
            style = NodeEditorSettings.DefaultNodeStyle;
        }

        public void Update(Event e)
        {
            if (IsSelected && dragHandler.IsDragging(e, Rect))
            {
                Drag(e.delta);
                e.Use();
            }
            if (e.type == EventType.MouseDown && e.button == WindowHelpers.RIGHT_MOUSE_BUTTON)
            {
                if (Rect.Contains(e.mousePosition))
                {
                    OpenContextMenu();
                    e.Use();
                }
            }
        }

        // This is for dragging the node itself and not drag from dragging the window around.
        void Drag(Vector2 delta)
        {
            nodeRect.position += delta;
        }

        void OpenContextMenu()
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Remove node"), false, () => onRemoveNode());
            genericMenu.ShowAsContext();
        }
    } 
}