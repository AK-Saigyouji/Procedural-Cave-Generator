using System;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace CaveGeneration.Utility
{
    static class Threading
    {
        // Max number of work items to send to threadpool. Exceeding 64 will throw an exception due to limitation
        // on number of reset handles.
        const int threadCount = 8;

        /// <summary>
        /// Implementation of a parallel foreach. Distributes the actions across multiple threads for faster 
        /// computation. Actions must not touch the Unity API, or else Unity will lock up.
        /// </summary>
        static public void ParallelExecute(params Action[] actions)
        {
            int workItemCount = Math.Min(threadCount, actions.Length);
            ManualResetEvent[] resetEvents = new ManualResetEvent[workItemCount];
            for (int i = 0; i < workItemCount; i++)
            {
                resetEvents[i] = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(new WaitCallback((object index) =>
                {
                    int workerIndex = (int)index;
                    for (int actionIndex = workerIndex; actionIndex < actions.Length; actionIndex += workItemCount)
                    {
                        actions[actionIndex]();
                    }
                    resetEvents[workerIndex].Set();
                }), i);
            }
            WaitHandle.WaitAll(resetEvents);
        }

        // Async/await and even tasks are not available in the .NET used by Unity. The following is a wrapper
        // for invoking an action asynchronously and not terminating until it's complete.

        /// <summary>
        /// A basic substitute for async/await usable in coroutines. May use secondary threads, so ensure
        /// the action does not touch the Unity API.
        /// </summary>
        public static IEnumerator ExecuteAndAwait(Action action)
        {
            WaitForSeconds pause = new WaitForSeconds(0.005f);
            var result = action.BeginInvoke(null, null);
            while (!result.IsCompleted)
            {
                yield return pause;
            }
        }
    }

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
