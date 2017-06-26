using System;
using UnityEngine;
using AKSaigyouji.EditorScripting;

namespace AKSaigyouji.Modules.MapGeneration
{
    /// <summary>
    /// Packages together all the GUI elements associated with a node.
    /// </summary>
    public sealed class NodeGroup
    {
        public Rect Position { get { return node.Rect; } }
        public MapGenModule MapGenModule { get { return info.MapGenModule; } }

        readonly Node node;
        readonly MapGenWindow info;
        readonly BoundaryDrawer boundaryDrawer;

        int id;

        public NodeGroup(int id, Rect position, Action<NodeGroup> onClickRemoveNode, MapGenModule mapGenModule)
        {
            this.id = id;
            node = new Node(position, () => onClickRemoveNode(this));
            info = new MapGenWindow(id, mapGenModule);
            boundaryDrawer = new BoundaryDrawer();
        }

        public void Draw()
        {
            node.Draw();
            if (MapGenModule != null)
            {
                boundaryDrawer.Draw(node.Rect, info.MapGenModule, node.IsSelected);
            }
        }

        public void DrawWindows()
        {
            // Needs to be drawn in a Window block, hence separated from the rest of the draw commands.
            if (node.IsSelected)
            {
                info.Draw();
            }
        }

        public bool UpdateSelection(Event e, NodeSelection selection)
        {
            bool selected = false;
            // selection logic is tricky: we only want one node to be selected at a time, and we have to handle
            // the case where two nodes overlap. The obvious way to do this is to consume the event upon selection,
            // but then the deselection logic won't process. This is why the NodeSelection object exists, to keep track
            // of whether we have clicked on a different node (which will force all other nodes to deselect). 
            if (e.type == EventType.MouseDown && e.button == WindowHelpers.LEFT_MOUSE_BUTTON)
            {
                Vector2 mouse = e.mousePosition;
                if (!selection.MadeThisEvent && (node.Rect.Contains(mouse) || (node.IsSelected && info.Rect.Contains(mouse))))
                {
                    selection.MadeThisEvent = true;
                    selection.Current = id;
                    node.Select(); ;
                    selected = true;
                }
                else
                {
                    node.Deselect();
                }
                GUI.changed = true;
            }
            node.Update(e);
            return selected;
        }
    } 
}