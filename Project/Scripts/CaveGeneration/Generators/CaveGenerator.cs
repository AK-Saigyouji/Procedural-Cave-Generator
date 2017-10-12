// Uncomment this preprocessor directive to disable multithreading. Alternatively, add it to the list of directives
// in the player settings.

// #define SINGLE_THREAD

using System;
using UnityEngine;

namespace AKSaigyouji.CaveGeneration
{
    /// <summary>
    /// Base class for cave generators. 
    /// </summary>
    public abstract class CaveGenerator
    {
        public abstract GameObject Generate();

        protected static void Execute(Action[] actions)
        {
#if SINGLE_THREAD
            Array.ForEach(actions, action => action.Invoke());
#else
            Threading.Threading.ParallelExecute(actions);
#endif
        }
    }
}
