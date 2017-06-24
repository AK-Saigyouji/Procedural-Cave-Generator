using UnityEngine;
using UnityEditor;
using AKSaigyouji.EditorScripting;
using AKSaigyouji.Maps;

namespace AKSaigyouji.Modules.MapGeneration
{
    sealed class MapGenWindow
    {
        public MapGenModule MapGenModule { get { return mapGenModule; } }
        public Rect Rect { get { return rect; } }

        Rect rect = new Rect(5, 5, 300, 300);
        Editor mapGenEditor;
        MapGenModule mapGenModule;

        readonly RectResizer infoResizer;
        readonly int id;

        readonly Boundary infoWindowLimits = new Boundary(100, 500, 100, 800);

        public MapGenWindow(int id, MapGenModule mapGenModule)
        {
            this.id = id;
            this.mapGenModule = mapGenModule;
            infoResizer = new RectResizer(NodeEditorSettings.RESIZE_HANDLE_SIZE, infoWindowLimits.BotLeft, infoWindowLimits.TopRight);
        }

        public void Draw()
        {
            rect = GUI.Window(id, rect, WindowFunction, string.Format("Map (ID:{0})", id));
            rect = infoResizer.Resize(Event.current, rect);
        }

        void WindowFunction(int id)
        {
            infoResizer.Draw(rect);
            mapGenModule = (MapGenModule)EditorGUILayout.ObjectField("Map Gen Module:", mapGenModule, typeof(MapGenModule), false);
            if (mapGenModule != null)
            {
                Editor.CreateCachedEditor(mapGenModule, null, ref mapGenEditor);
                mapGenEditor.OnInspectorGUI();
            }
            GUI.DragWindow(new Rect(0, 0, rect.width, WindowHelpers.WINDOW_TITLE_HEIGHT));
        }
    } 
}