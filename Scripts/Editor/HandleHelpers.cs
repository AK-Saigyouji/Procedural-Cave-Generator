using UnityEngine;
using UnityEditor;

namespace AKSaigyouji.EditorScripting
{
    public static class HandleHelpers
    {
        public static void DrawGrid(Rect position, float gridSpacing, Color color, Vector2 offset)
        {
            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

            Handles.BeginGUI();
            Handles.color = color;

            Vector2 newOffset = new Vector2(offset.x % gridSpacing, offset.y % gridSpacing);

            for (int i = 0; i <= widthDivs; i++)
            {
                Vector3 start = new Vector2(gridSpacing * i, -gridSpacing) + newOffset;
                Vector3 end = new Vector2(gridSpacing * i, position.height + gridSpacing) + newOffset;
                Handles.DrawLine(start, end);
            }

            for (int j = 0; j <= heightDivs; j++)
            {
                Vector3 start = new Vector2(-gridSpacing, gridSpacing * j) + newOffset;
                Vector3 end = new Vector2(position.width + gridSpacing, gridSpacing * j) + newOffset;
                Handles.DrawLine(start, end);
            }
            Handles.EndGUI();
        }
    } 
}