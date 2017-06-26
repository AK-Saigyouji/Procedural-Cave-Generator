using UnityEngine;

namespace AKSaigyouji.Modules.MapGeneration
{
    public static class WindowHelpers
    {
        /// <summary>
        /// The integer Unity associates with the left mouse button.
        /// </summary>
        public const int LEFT_MOUSE_BUTTON = 0;

        /// <summary>
        /// The integer Unity associates with the right mouse button.
        /// </summary>
        public const int RIGHT_MOUSE_BUTTON = 1;

        /// <summary>
        /// Approximate height of a default window title.
        /// </summary>
        public const int WINDOW_TITLE_HEIGHT = 14;

        public const int WINDOW_TASKBAR_HEIGHT = 21;

        /// <summary>
        /// Rounds raw so that it sits on the lattice of granularity as defined by the ratio of grid units to world units
        /// configured in the node editor settings.
        /// </summary>
        public static Vector2 RoundToLattice(Vector2 raw)
        {
            const int SQUARE_SIZE = NodeEditorSettings.GRID_UNITS_TO_WORLD_UNITS;
            return new Vector2((int)(raw.x / SQUARE_SIZE) * SQUARE_SIZE, (int)(raw.y / SQUARE_SIZE) * SQUARE_SIZE);
        }

        /// <summary>
        /// Rounds the rect's position so that it sits on the lattice of granularity as defined by the ratio of grid 
        /// units to world units configured in the node editor settings.
        /// </summary>
        public static Rect RoundToLattice(Rect rect)
        {
            Rect roundedRect = new Rect(rect);
            roundedRect.position = RoundToLattice(roundedRect.position);
            return roundedRect;
        }
    } 
}