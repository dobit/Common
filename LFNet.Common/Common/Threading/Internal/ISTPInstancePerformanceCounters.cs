using System;

namespace LFNet.Common.Threading.Internal
{
    internal interface ISTPInstancePerformanceCounters : IDisposable
    {
        void Close();
        void SampleThreads(long activeThreads, long inUseThreads);
        void SampleWorkItems(long workItemsQueued, long workItemsProcessed);
        void SampleWorkItemsProcessTime(TimeSpan workItemProcessTime);
        void SampleWorkItemsWaitTime(TimeSpan workItemWaitTime);
    }
}

