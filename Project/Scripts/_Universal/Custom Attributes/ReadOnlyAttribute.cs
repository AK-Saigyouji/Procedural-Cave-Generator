using UnityEngine;

namespace AKSaigyouji
{
    /// <summary>
    /// Variables marked with this attribute will show up as readonly in the inspector. Useful for revealing
    /// run-time information which is not supposed to be altered by hand.
    /// </summary>
    public sealed class ReadOnlyAttribute : PropertyAttribute
    {
        // The code responsible for making the field readonly is in ReadOnlyDrawer, an editor script. The attribute
        // itself has no logic.
    } 
}