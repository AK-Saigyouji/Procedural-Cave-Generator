using UnityEngine;

namespace CaveGeneration.Utility
{
    static class StopwatchExtensions
    {
        /// <summary>
        /// Prints the current time on the stopwatch along with the provided message, then restarts it. Convenience
        /// method for repeatedly querying and resetting a stopwatch to profile a set of methods. Note: Uses Unity's 
        /// default logging mechanism and is thus very inefficient.
        /// </summary>
        static public void Query(this System.Diagnostics.Stopwatch sw, string message = "")
        {
            string prefix = string.IsNullOrEmpty(message) ? string.Empty : message + ": ";
            Debug.Log(prefix + sw.Elapsed.TotalSeconds);
            sw.Reset();
            sw.Start();
        }
    }
}