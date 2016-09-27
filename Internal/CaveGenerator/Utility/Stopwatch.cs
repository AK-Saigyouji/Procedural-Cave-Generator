using UnityEngine;

namespace CaveGeneration.Utility
{
    static class Stopwatch
    {
        /// <summary>
        /// Prints the current time on the stopwatch along with the provided message, then restarts it. Convenience
        /// method for repeatedly querying and resetting a stopwatch to profile a set of methods.
        /// </summary>
        static public void Query(System.Diagnostics.Stopwatch sw, string message = "")
        {
            Debug.Log(message + sw.Elapsed.TotalSeconds);
            sw.Reset();
            sw.Start();
        }
    }
}