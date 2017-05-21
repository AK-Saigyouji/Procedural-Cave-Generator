/* This is the highest custom base class in the module class hierarchy, sitting directly below Unity's ScriptableObject.
 Its main purpose is to allow the creation of a single custom editor to apply to all modules, though it also permits the
 easy sharing of core data and functionality shared by all modules.
 
  Originally, modules could implement IRandomizable to permit their seeds to be set. I decided it would be simpler
 both to use and implement to have a virtual property to set the seed, which does nothing by default.*/

using UnityEngine;

namespace CaveGeneration.Modules
{
    /// <summary>
    /// Base class for the cave generator's swappable components.
    /// </summary>
    public abstract class Module : ScriptableObject
    {
        /// <summary>
        /// Fixes any randomness possessed by this module. Does nothing if this module does not possess randomness.
        /// </summary>
        public virtual int Seed { set { } }
    }
}