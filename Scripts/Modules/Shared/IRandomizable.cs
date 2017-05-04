/* The motivation for this interface was to allow modules to be automatically randomized without letting 
 the abstraction of the corresponding modules leak. The cave generator operates on abstract Module classes,
 and it's up to individual implementations to decide whether they admit randomization and how. The cave generator
 can test for this interface at run-time, and use it to randomize components that can be randomized.*/

namespace CaveGeneration.Modules
{
    /// <summary>
    /// Interface for randomized components whose randomization can be controlled via a seed value.
    /// </summary>
    public interface IRandomizable
    {
        /// <summary>
        /// Seed value to control the randomness of this module. A given module with the same properties set to
        /// the same seed should produce the same output. 
        /// </summary>
        int Seed { set; }
    }
}