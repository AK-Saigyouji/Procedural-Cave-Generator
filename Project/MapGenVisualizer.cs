/* A MonoBehaviour that allows immediate visualization of a map generation module: when a valid map gen module is slotted,
  the preview window for the object containing this behaviour will display an instance of the generated map. 
  Note that the visualization logic is contained entirely in the custom inspector for this class. */

using UnityEngine;

namespace AKSaigyouji.Modules.MapGeneration
{
    public sealed class MapGenVisualizer : MonoBehaviour
    {
        [SerializeField] MapGenModule mapGenModule;

        // The following warning disable/restore suppresses the warning that this variable is assigned but never used.
        // It is used by the editor script, which accesses it using reflection. This could be avoided by additional
        // editor scripting to create and expose a fake field, but this is far simpler.
#pragma warning disable 0414
        [SerializeField] bool suppressErrors = true;
#pragma warning restore 0414
    }
}