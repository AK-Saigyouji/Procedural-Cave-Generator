using UnityEngine;
using UnityEditor;
using AKSaigyouji.Modules.MapGeneration;
using AKSaigyouji.Maps;

public sealed class PreviewWindow : EditorWindow
{
    Texture texture;

    void OnGUI()
    {
        GUI.DrawTexture(new Rect(Vector2.zero, position.size), texture);
    }

    public void UpdateWindow(MapGenModule module)
    {
        Map map = module.Generate();
        texture = map.ToTexture();
    }
}