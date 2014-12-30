using System;

namespace LFNet.Common.Threading.Internal
{
    internal class NullSTPInstancePerformanceCounters : ISTPInstancePerformanceCounters, IDisposable
    {
        private static NullSTPInstancePerformanceCounters _instance = new NullSTPInstancePerformanceCounters(null);

        public NullSTPInstancePerformanceCounters(string instance)
        {
        }

        public void Close()
        {
        }

        public void Dispose()
        {
        }

        public void SampleThreads(long activeThreads, long inUseThreads)
        {
        }

        public void SampleWorkItems(long workItemsQueued, long workItemsProcessed)
        {
        }

        public void SampleWorkItemsProcessTime(TimeSpan workItemProcessTime)
        {
        }

        public void SampleWorkItemsWaitTime(TimeSpan workItemWaitTime)
        {
        }

        public static NullSTPInstancePerformanceCounters Instance
        {
            get
            {
                return _instance;
            }
        }
    }
}

