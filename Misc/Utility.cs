using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Utility
{
    static class Threading
    {
        // Max number of work items to send to threadpool. Must not exceed 64.
        static readonly int threadCount = 8;

        /// <summary>
        /// .NET 3.5 implementation of a parallel foreach. Distributes the actions across multiple threads for faster 
        /// computation. Actions must not touch the Unity API. 
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
}
