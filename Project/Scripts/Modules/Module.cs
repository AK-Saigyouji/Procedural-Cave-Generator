/* Module is an extension of ScriptableObject with similar use cases: modular, swappable assets. The power of modules
 comes from improved capacity for composition. A custom inspector provides access to all modules referenced within a module,
 allowing modification of all modules in its reference graph from a single inspector. Additionally, an added menu item provides
 a deep copy which will make copies of all reachable modules in its reference graph, storing them as assets within the
 copy. These extensions facilitate a more SOLID-friendly approach to the design of modules.*/

using UnityEngine;

namespace AKSaigyouji.Modules
{
    /// <summary>
    /// Base class for the cave generator's swappable components.
    /// </summary>
    public abstract class Module : ScriptableObject
    {
        protected const string MODULE_ASSET_PATH = "AKSaigyouji/";

        /// <summary>
        /// Fixes any randomness possessed by this module. Does nothing if this module does not possess randomness.
        /// Must be overriden when implementing a module that needs to permit its randomness to be fixed by an external
        /// actor.
        /// </summary>
        public virtual int Seed { get { return 0; } set { } }
    }
}