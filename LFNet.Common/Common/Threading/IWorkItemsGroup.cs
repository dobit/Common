using System;

namespace LFNet.Common.Threading
{
    /// <summary>
    /// IWorkItemsGroup interface
    /// </summary>
    public interface IWorkItemsGroup
    {
        event WorkItemsGroupIdleHandler OnIdle;

        void Cancel();
        IWorkItemResult QueueWorkItem(WorkItemCallback callback);
        IWorkItemResult QueueWorkItem(WorkItemCallback callback, WorkItemPriority workItemPriority);
        IWorkItemResult QueueWorkItem(WorkItemCallback callback, object state);
        IWorkItemResult QueueWorkItem(WorkItemInfo workItemInfo, WorkItemCallback callback);
        IWorkItemResult QueueWorkItem(WorkItemCallback callback, object state, PostExecuteWorkItemCallback postExecuteWorkItemCallback);
        IWorkItemResult QueueWorkItem(WorkItemCallback callback, object state, WorkItemPriority workItemPriority);
        IWorkItemResult QueueWorkItem(WorkItemInfo workItemInfo, WorkItemCallback callback, object state);
        IWorkItemResult QueueWorkItem(WorkItemCallback callback, object state, PostExecuteWorkItemCallback postExecuteWorkItemCallback, CallToPostExecute callToPostExecute);
        IWorkItemResult QueueWorkItem(WorkItemCallback callback, object state, PostExecuteWorkItemCallback postExecuteWorkItemCallback, WorkItemPriority workItemPriority);
        IWorkItemResult QueueWorkItem(WorkItemCallback callback, object state, PostExecuteWorkItemCallback postExecuteWorkItemCallback, CallToPostExecute callToPostExecute, WorkItemPriority workItemPriority);
        void Start();
        void WaitForIdle();
        bool WaitForIdle(int millisecondsTimeout);
        bool WaitForIdle(TimeSpan timeout);

        /// <summary>
        /// Get/Set the name of the WorkItemsGroup
        /// </summary>
        string Name { get; set; }

        int WaitingCallbacks { get; }
    }
}

