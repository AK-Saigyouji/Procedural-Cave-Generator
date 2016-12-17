using System;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace CaveGeneration.Utility
{
    /// <summary>
    /// This class provides some basic multi-threading capabilities, modelled after functionality that already
    /// exists in modern .NET but unavailable in the current version of Unity. As of 12/16/2016 Unity has 
    /// an update to .NET 4.6 on their roadmap, at which point this class will become entirely redundant
    /// and should be replaced.
    /// </summary>
    static class Threading
    {
        // Max number of work items to send to threadpool. Exceeding 64 will throw an exception due to limitation
        // on number of reset handles.
        const int threadCount = 8;

        /// <summary>
        /// Implementation of a parallel foreach.
        /// Distributes the actions across multiple threads for faster computation. Actions must not touch the Unity 
        /// API, or else Unity will lock up.
        /// </summary>
        static public void ParallelExecute(params Action[] actions)
        {
            int workItemCount = Math.Min(threadCount, actions.Length);
            UnityEngine.Assertions.Assert.IsTrue(workItemCount <= 64);
            var resetEvents = new ManualResetEvent[workItemCount];
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

        /// <summary>
        /// A very basic substitute for async/await usable in coroutines. May use secondary threads, so ensure
        /// the action does not touch the Unity API. 
        /// </summary>
        public static IEnumerator ExecuteAndAwait(Action action)
        {
            const float PAUSE_DURATION = 0.005f;
            var pause = new WaitForSeconds(PAUSE_DURATION);
            IAsyncResult result = action.BeginInvoke(null, null);
            while (!result.IsCompleted)
            {
                yield return pause;
            }
        }
    }
}