using UnityEngine;
using UnityEditor;

namespace AKSaigyouji.EditorScripting
{
    public static class HandleHelpers
    {
        public static void DrawGrid(Rect area, float gridSpacing, Color color, Vector2 offset)
        {
            int widthDivs = Mathf.CeilToInt(area.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(area.height / gridSpacing);

            GUI.BeginGroup(area);
            Handles.BeginGUI();
            Handles.color = color;

            Vector2 finalOffset = new Vector2(offset.x % gridSpacing, offset.y % gridSpacing);

            for (int i = 0; i <= widthDivs; i++)
            {
                Vector3 start = new Vector2(gridSpacing * i, -gridSpacing) + finalOffset;
                Vector3 end = new Vector2(gridSpacing * i, area.height + gridSpacing) + finalOffset;
                Handles.DrawLine(start, end);
            }

            for (int j = 0; j <= heightDivs; j++)
            {
                Vector3 start = new Vector2(-gridSpacing, gridSpacing * j) + finalOffset;
                Vector3 end = new Vector2(area.width + gridSpacing, gridSpacing * j) + finalOffset;
                Handles.DrawLine(start, end);
            }
            Handles.EndGUI();
            GUI.EndGroup();
        }
    } 
}