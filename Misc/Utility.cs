using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Utility
{
    static class Threading
    {
        // Max number of work items to send to threadpool. Exceeding 64 will throw an exception due to limitation
        // on number of reset handles.
        static readonly int threadCount = 8;

        /// <summary>
        /// .NET 3.5 implementation of a parallel foreach. Distributes the actions across multiple threads for faster 
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
    }

    static class Stopwatch
    {
        /// <summary>
        /// Prints the current time on the stopwatch along with the provided message, then resets it. Convenience
        /// method for repeatedly querying and resetting a stopwatch to profile a set of methods.
        /// </summary>
        static public void Query(System.Diagnostics.Stopwatch sw, string message)
        {
            UnityEngine.Debug.Log(message + sw.Elapsed.TotalSeconds);
            sw.Reset();
            sw.Start();
        }
    }
}
