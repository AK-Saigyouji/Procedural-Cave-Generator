using System;
using System.Collections;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace AKSaigyouji.Threading
{
    /// <summary>
    /// Offers multithreading functionality. Will become obsolete once Unity upgrades to .NET 4.6. 
    /// </summary>
    static class Threading
    {
        // Max number of work items to send to threadpool. Exceeding 64 will throw an exception due to limitation
        // on number of reset handles.
        const int MAX_WORK_ITEM_COUNT = 8;

        static readonly object locker = new object();

        /// <summary>
        /// Implementation of a parallel foreach.
        /// Distributes the actions across multiple threads for faster computation. Actions must not touch the Unity 
        /// API, or else Unity will lock up.
        /// </summary>
        static public void ParallelExecute(params Action[] actions)
        {
            if (actions == null)
                throw new ArgumentNullException("actions");

            if (actions.Contains(null))
                throw new ArgumentException("Cannot execute null action");

            int workItemCount = Math.Min(MAX_WORK_ITEM_COUNT, actions.Length);
            var resetEvents = new ManualResetEvent[workItemCount];
            Exception workerException = null;
            for (int i = 0; i < workItemCount; i++)
            {
                resetEvents[i] = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(new WaitCallback((object index) =>
                {
                    int workerIndex = (int)index;
                    try
                    {
                        for (int actionIndex = workerIndex; actionIndex < actions.Length; actionIndex += workItemCount)
                        {
                            actions[actionIndex]();
                        }
                        resetEvents[workerIndex].Set();
                    }
                    catch (Exception e)
                    {
                        lock (locker)
                        {
                            workerException = e;
                        }
                        Array.ForEach(resetEvents, ev => ev.Set()); 
                    }
                }), i);
            }
            WaitHandle.WaitAll(resetEvents);
            // Control returns to main thread, so exception can be thrown.
            lock (locker) // Prevent any lingering threads from writing a new exception mid-throw.
            {
                if (workerException != null)
                {
                    throw workerException;
                }
            }
        }

        /// <summary>
        /// A very basic substitute for async/await usable in coroutines. May use secondary threads, so ensure
        /// the action does not touch the Unity API. Intended for relatively long-running operations, as
        /// it may introduce 20+ ms of overhead.
        /// </summary>
        public static IEnumerator ExecuteAndAwait(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            var pause = new WaitForSecondsRealtime(0.016f); // Checks slightly faster than 60 times per second.
            IAsyncResult result = action.BeginInvoke(null, null);
            while (!result.IsCompleted)
            {
                yield return pause;
            }
            // EndInvoke is called purely to ensure unhandled exceptions caused by the action are thrown
            // on the main thread (otherwise, action will have failed silently). 
            action.EndInvoke(result); 
        }
    }
}