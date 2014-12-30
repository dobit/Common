using System;

namespace LFNet.Common.Threading.Internal
{
    internal class STPInstancePerformanceCounters : ISTPInstancePerformanceCounters, IDisposable
    {
        private STPInstancePerformanceCounter[] _pcs = new STPInstancePerformanceCounter[14];
        private static STPInstancePerformanceCounter _stpInstanceNullPerformanceCounter = new STPInstanceNullPerformanceCounter();

        public STPInstancePerformanceCounters(string instance)
        {
            STPPerformanceCounters counters1 = STPPerformanceCounters.Instance;
            for (int i = 0; i < this._pcs.Length; i++)
            {
                if (instance != null)
                {
                    this._pcs[i] = new STPInstancePerformanceCounter(instance, (STPPerformanceCounterType) i);
                }
                else
                {
                    this._pcs[i] = _stpInstanceNullPerformanceCounter;
                }
            }
        }

        public void Close()
        {
            if (this._pcs != null)
            {
                for (int i = 0; i < this._pcs.Length; i++)
                {
                    if (this._pcs[i] != null)
                    {
                        this._pcs[i].Close();
                    }
                }
                this._pcs = null;
            }
        }

        public void Dispose()
        {
            this.Close();
            GC.SuppressFinalize(this);
        }

        ~STPInstancePerformanceCounters()
        {
            this.Close();
        }

        private STPInstancePerformanceCounter GetCounter(STPPerformanceCounterType spcType)
        {
            return this._pcs[(int) spcType];
        }

        public void SampleThreads(long activeThreads, long inUseThreads)
        {
            this.GetCounter(STPPerformanceCounterType.ActiveThreads).Set(activeThreads);
            this.GetCounter(STPPerformanceCounterType.InUseThreads).Set(inUseThreads);
            this.GetCounter(STPPerformanceCounterType.OverheadThreads).Set(activeThreads - inUseThreads);
            this.GetCounter(STPPerformanceCounterType.OverheadThreadsPercentBase).Set(activeThreads - inUseThreads);
            this.GetCounter(STPPerformanceCounterType.OverheadThreadsPercent).Set(inUseThreads);
        }

        public void SampleWorkItems(long workItemsQueued, long workItemsProcessed)
        {
            this.GetCounter(STPPerformanceCounterType.WorkItems).Set(workItemsQueued + workItemsProcessed);
            this.GetCounter(STPPerformanceCounterType.WorkItemsInQueue).Set(workItemsQueued);
            this.GetCounter(STPPerformanceCounterType.WorkItemsProcessed).Set(workItemsProcessed);
            this.GetCounter(STPPerformanceCounterType.WorkItemsQueuedPerSecond).Set(workItemsQueued);
            this.GetCounter(STPPerformanceCounterType.WorkItemsProcessedPerSecond).Set(workItemsProcessed);
        }

        public void SampleWorkItemsProcessTime(TimeSpan workItemProcessTime)
        {
            this.GetCounter(STPPerformanceCounterType.AvgWorkItemProcessTime).IncrementBy((long) workItemProcessTime.TotalMilliseconds);
            this.GetCounter(STPPerformanceCounterType.AvgWorkItemProcessTimeBase).Increment();
        }

        public void SampleWorkItemsWaitTime(TimeSpan workItemWaitTime)
        {
            this.GetCounter(STPPerformanceCounterType.AvgWorkItemWaitTime).IncrementBy((long) workItemWaitTime.TotalMilliseconds);
            this.GetCounter(STPPerformanceCounterType.AvgWorkItemWaitTimeBase).Increment();
        }
    }
}

