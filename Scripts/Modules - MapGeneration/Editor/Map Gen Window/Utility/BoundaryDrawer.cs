using UnityEngine;
using AKSaigyouji.Maps;
using AKSaigyouji.EditorScripting;

namespace AKSaigyouji.Modules.MapGeneration
{
    sealed class BoundaryDrawer
    {
        readonly Texture inactiveSquare;
        readonly Texture activeSquare;
        readonly Texture entranceSquare;

        readonly Color inactiveColor = new Color(0, 0, 0.5f, 0.3f); // blue, translucent (weak)
        readonly Color activeColor = new Color(1, 153f / 255, 0, 0.8f); // orange, translucent (strong)
        readonly Color entranceColor = Color.white;

        const int SQUARE_SIZE = NodeEditorSettings.GRID_UNITS_TO_WORLD_UNITS;

        public BoundaryDrawer()
        {
            inactiveSquare = BuildSquare(SQUARE_SIZE, inactiveColor, true);
            activeSquare = BuildSquare(SQUARE_SIZE, activeColor, true);
            entranceSquare = BuildSquare(SQUARE_SIZE, entranceColor, false); // non-checkered pattern makes entrances stand out
        }

        public void Draw(Rect nodeRect, MapGenModule module, bool isSelected)
        {
            if (module != null)
            {
                Texture boundaryTexture = isSelected ? activeSquare : inactiveSquare;
                Vector2 center = WindowHelpers.RoundToLattice(nodeRect.center);
                Vector2 square = SQUARE_SIZE * Vector2.one;
                Rect rect = new Rect(center, square);
                foreach (Coord coord in module.GetBoundary())
                {
                    DrawBox(center, coord, rect, boundaryTexture);
                }
                Coord mapSize = module.GetMapSize();
                Boundary boundary = new Boundary(mapSize.x, mapSize.y);
                foreach (MapEntrance entrance in module.GetOpenings())
                {
                    foreach (Coord coord in entrance.GetCoords(boundary))
                    {
                        DrawBox(center, coord, rect, entranceSquare);
                    }
                }
            }
        }

        static void DrawBox(Vector2 center, Coord offset, Rect rect, Texture texture)
        {
            Coord flippedCoord = new Coord(offset.x, -offset.y);
            rect.position = center + SQUARE_SIZE * flippedCoord;
            GUI.DrawTexture(rect, texture);
        }

        /// <summary>
        /// Build a square texture of size 
        /// </summary>
        /// <param name="size"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public Texture2D BuildSquare(int size, Color color, bool checkered)
        {
            Color offColor = new Color(color.r, color.g, color.b, color.a / 4);
            Texture2D square = new Texture2D(size, size, TextureFormat.ARGB32, false);
            square.filterMode = FilterMode.Point;
            for (int y = 0; y < SQUARE_SIZE; y++)
            {
                for (int x = 0; x < SQUARE_SIZE; x++)
                {
                    Color pixelColor;
                    if (checkered)
                    {
                        pixelColor = (x + y) % 2 == 0 ? color : offColor;
                    }
                    else
                    {
                        pixelColor = color;
                    }
                    square.SetPixel(x, y, pixelColor);
                }
            }
            square.Apply();
            return square;
        }
    } 
}