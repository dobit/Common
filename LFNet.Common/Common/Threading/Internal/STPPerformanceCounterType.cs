namespace LFNet.Common.Threading.Internal
{
    internal enum STPPerformanceCounterType
    {
        ActiveThreads = 0,
        AvgWorkItemProcessTime = 12,
        AvgWorkItemProcessTimeBase = 13,
        AvgWorkItemWaitTime = 10,
        AvgWorkItemWaitTimeBase = 11,
        InUseThreads = 1,
        LastCounter = 14,
        OverheadThreads = 2,
        OverheadThreadsPercent = 3,
        OverheadThreadsPercentBase = 4,
        WorkItems = 5,
        WorkItemsGroups = 14,
        WorkItemsInQueue = 6,
        WorkItemsProcessed = 7,
        WorkItemsProcessedPerSecond = 9,
        WorkItemsQueuedPerSecond = 8
    }
}

