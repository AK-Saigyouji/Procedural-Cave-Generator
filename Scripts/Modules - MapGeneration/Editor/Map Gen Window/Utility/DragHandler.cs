using AKSaigyouji.EditorScripting;
using UnityEngine;

namespace AKSaigyouji.Modules.MapGeneration
{
    /// <summary>
    /// Responsible for determining whether an item is being dragged - does not do the actual dragging.
    /// </summary>
    public sealed class DragHandler
    {
        bool isDragging = false;

        public bool IsDragging(Event e, Rect draggedItem)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == WindowHelpers.LEFT_MOUSE_BUTTON)
                    {
                        if (draggedItem.Contains(e.mousePosition))
                        {
                            isDragging = true;
                        }
                    }
                    break;

                case EventType.MouseUp:
                    isDragging = false;
                    break;

                case EventType.MouseDrag:
                    if (e.button == WindowHelpers.LEFT_MOUSE_BUTTON && isDragging)
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }
    } 
}