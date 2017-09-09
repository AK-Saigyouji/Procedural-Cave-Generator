using UnityEngine;

namespace AKSaigyouji.EditorScripting
{
    /// <summary>
    /// Responsible for determining whether an item is being dragged - does not do the actual dragging.
    /// </summary>
    [System.Serializable]
    public sealed class DragHandler
    {
        bool isDragging = false;

        public bool IsDragging(Event e, Rect draggedItem)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0)
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
                    if (e.button == 0 && isDragging)
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }
    } 
}