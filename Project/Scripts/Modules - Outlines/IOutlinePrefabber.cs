using UnityEngine;

namespace AKSaigyouji.Modules.Outlines
{
    /// <summary>
    /// Instantiates prefabs along the outlines of a cave.
    /// </summary>
    public interface IOutlinePrefabber
    {
        void ProcessOutline(Vector3[] outline, Transform parent);
    } 
}