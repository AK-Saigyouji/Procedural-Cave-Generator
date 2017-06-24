using System;
using UnityEngine;

namespace AKSaigyouji.Modules.MapGeneration
{
    [Serializable]
    public sealed class SavedNode
    {
        public MapGenModule MapGenModule { get { return mapGenModule; } set { mapGenModule = value; } }
        public Rect NodeRect { get { return rect; } set { rect = value; } }

        [SerializeField] MapGenModule mapGenModule;
        [SerializeField] Rect rect;

        public SavedNode(MapGenModule mapGenModule, Rect rect)
        {
            this.mapGenModule = mapGenModule;
            this.rect = rect;
        }
    } 
}