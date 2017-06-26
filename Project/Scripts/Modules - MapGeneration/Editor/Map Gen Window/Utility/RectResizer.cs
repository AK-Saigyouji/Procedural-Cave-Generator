using UnityEngine;

namespace AKSaigyouji.Modules.MapGeneration
{
    sealed class RectResizer
    {
        bool isResizing = false;

        readonly int size;
        readonly float xMin, xMax, yMin, yMax;

        const int PADDING = 3;

        public RectResizer(int size, Vector2 botLeft, Vector2 topRight)
        {
            xMin = botLeft.x;
            xMax = topRight.x;
            yMin = botLeft.y;
            yMax = topRight.y;
            this.size = size;
        }

        public Rect Resize(Event e, Rect rect)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (ComputeResizeRect(rect).Contains(e.mousePosition))
                    {
                        isResizing = true;
                        e.Use();
                    }
                    break;

                case EventType.MouseUp:
                    isResizing = false;
                    break;

                case EventType.MouseDrag:
                    if (isResizing)
                    {
                        rect.width = Mathf.Clamp(rect.width + e.delta.x, xMin, xMax);
                        rect.height = Mathf.Clamp(rect.height + e.delta.y, yMin, yMax);
                        e.Use();
                    }
                    break;
            }
            return rect;
        }

        public void Draw(Rect containingRect)
        {
            GUI.Box(ComputeResizeRect(containingRect), string.Empty);
        }

        Rect ComputeResizeRect(Rect containingRect)
        {
            return new Rect(
                x: containingRect.width - size - PADDING,
                y: containingRect.height - size - PADDING,
                width: size,
                height: size);
        }
    } 
}