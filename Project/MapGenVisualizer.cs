/* A MonoBehaviour that allows immediate visualization of a map generation module. The behaviour is all in the custom
 editor for this class. In the future, this may be replaced with an editor window.*/

using UnityEngine;

namespace AKSaigyouji.Modules.MapGeneration
{
    public sealed class MapGenVisualizer : MonoBehaviour
    {
        [SerializeField] MapGenModule mapGenModule;
    }
}