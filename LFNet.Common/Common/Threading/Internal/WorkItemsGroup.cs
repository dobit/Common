using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LFNet.Common.Threading.Internal
{
    /// <summary>
    /// Summary description for WorkItemsGroup.
    /// </summary>
    public class WorkItemsGroup : IWorkItemsGroup
    {
        /// <summary>
        /// A common object for all the work items that this work items group
        /// generate so we can mark them to cancel in O(1)
        /// </summary>
        private CanceledWorkItemsGroup _canceledWorkItemsGroup = new CanceledWorkItemsGroup();
        /// <summary>
        /// Defines how many work items of this WorkItemsGroup can run at once.
        /// </summary>
        private int _concurrency;
        /// <summary>
        /// Signaled when all of the WorkItemsGroup's work item completed.
        /// </summary>
        private ManualResetEvent _isIdleWaitHandle = new ManualResetEvent(true);
        private object _lock = new object();
        /// <summary>
        /// Contains the name of this instance of SmartThreadPool.
        /// Can be changed by the user.
        /// </summary>
        private string _name = "WorkItemsGroup";
        /// <summary>
        /// A reference to the SmartThreadPool instance that created this 
        /// WorkItemsGroup.
        /// </summary>
        private SmartThreadPool _stp;
        /// <summary>
        /// Indicate how many work items are currently running in the SmartThreadPool.
        /// This value is used with the Cancel, to calculate if we can send new 
        /// work items to the STP.
        /// </summary>
        private int _workItemsExecutingInStp;
        /// <summary>
        /// WorkItemsGroup start information
        /// </summary>
        private WIGStartInfo _workItemsGroupStartInfo;
        /// <summary>
        /// Indicate how many work items are waiting in the SmartThreadPool
        /// queue.
        /// This value is used to apply the concurrency.
        /// </summary>
        private int _workItemsInStpQueue;
        /// <summary>
        /// Priority queue to hold work items before they are passed 
        /// to the SmartThreadPool.
        /// </summary>
        private PriorityQueue _workItemsQueue;

        /// <summary>
        /// The OnIdle event
        /// </summary>
        private event WorkItemsGroupIdleHandler _onIdle;

        public event WorkItemsGroupIdleHandler OnIdle
        {
            add
            {
                this._onIdle += value;
            }
            remove
            {
                this._onIdle -= value;
            }
        }

        public WorkItemsGroup(SmartThreadPool stp, int concurrency, WIGStartInfo wigStartInfo)
        {
            if (concurrency <= 0)
            {
                throw new ArgumentOutOfRangeException("concurrency", concurrency, "concurrency must be greater than zero");
            }
            this._stp = stp;
            this._concurrency = concurrency;
            this._workItemsGroupStartInfo = new WIGStartInfo(wigStartInfo);
            this._workItemsQueue = new PriorityQueue();
            this._workItemsInStpQueue = this._workItemsExecutingInStp;
        }

        public void Cancel()
        {
            lock (this._lock)
            {
                this._canceledWorkItemsGroup.IsCanceled = true;
                this._workItemsQueue.Clear();
                this._workItemsInStpQueue = 0;
                this._canceledWorkItemsGroup = new CanceledWorkItemsGroup();
            }
        }

        private void EnqueueToSTPNextWorkItem(WorkItem workItem)
        {
            this.EnqueueToSTPNextWorkItem(workItem, false);
        }

        private void EnqueueToSTPNextWorkItem(WorkItem workItem, bool decrementWorkItemsInStpQueue)
        {
            lock (this._lock)
            {
                if (decrementWorkItemsInStpQueue)
                {
                    this._workItemsInStpQueue--;
                    if (this._workItemsInStpQueue < 0)
                    {
                        this._workItemsInStpQueue = 0;
                    }
                    this._workItemsExecutingInStp--;
                    if (this._workItemsExecutingInStp < 0)
                    {
                        this._workItemsExecutingInStp = 0;
                    }
                }
                if (workItem != null)
                {
                    workItem.CanceledWorkItemsGroup = this._canceledWorkItemsGroup;
                    this.RegisterToWorkItemCompletion(workItem.GetWorkItemResult());
                    this._workItemsQueue.Enqueue(workItem);
                    if ((1 == this._workItemsQueue.Count) && (this._workItemsInStpQueue == 0))
                    {
                        this._stp.RegisterWorkItemsGroup(this);
                        Trace.WriteLine("WorkItemsGroup " + this.Name + " is NOT idle");
                        this._isIdleWaitHandle.Reset();
                    }
                }
                if (this._workItemsQueue.Count == 0)
                {
                    if (this._workItemsInStpQueue == 0)
                    {
                        this._stp.UnregisterWorkItemsGroup(this);
                        Trace.WriteLine("WorkItemsGroup " + this.Name + " is idle");
                        this._isIdleWaitHandle.Set();
                        this._stp.QueueWorkItem(new WorkItemCallback(this.FireOnIdle));
                    }
                }
                else if (!this._workItemsGroupStartInfo.StartSuspended && (this._workItemsInStpQueue < this._concurrency))
                {
                    WorkItem item = this._workItemsQueue.Dequeue() as WorkItem;
                    this._stp.Enqueue(item, true);
                    this._workItemsInStpQueue++;
                }
            }
        }

        private object FireOnIdle(object state)
        {
            this.FireOnIdleImpl(this._onIdle);
            return null;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void FireOnIdleImpl(WorkItemsGroupIdleHandler onIdle)
        {
            if (onIdle != null)
            {
                foreach (WorkItemsGroupIdleHandler handler in onIdle.GetInvocationList())
                {
                    try
                    {
                        handler(this);
                    }
                    catch
                    {
                    }
                }
            }
        }

        public void OnSTPIsStarting()
        {
            lock (this)
            {
                if (this._workItemsGroupStartInfo.StartSuspended)
                {
                    return;
                }
            }
            for (int i = 0; i < this._concurrency; i++)
            {
                this.EnqueueToSTPNextWorkItem(null, false);
            }
        }

        private void OnWorkItemCompletedCallback(WorkItem workItem)
        {
            this.EnqueueToSTPNextWorkItem(null, true);
        }

        private void OnWorkItemStartedCallback(WorkItem workItem)
        {
            lock (this._lock)
            {
                this._workItemsExecutingInStp++;
            }
        }

        /// <summary>
        /// Queue a work item
        /// </summary>
        /// <param name="callback">A callback to execute</param>
        /// <returns>Returns a work item result</returns>
        public IWorkItemResult QueueWorkItem(WorkItemCallback callback)
        {
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, this._workItemsGroupStartInfo, callback);
            this.EnqueueToSTPNextWorkItem(workItem);
            return workItem.GetWorkItemResult();
        }

        /// <summary>
        /// Queue a work item
        /// </summary>
        /// <param name="callback">A callback to execute</param>
        /// <param name="workItemPriority">The priority of the work item</param>
        /// <returns>Returns a work item result</returns>
        public IWorkItemResult QueueWorkItem(WorkItemCallback callback, WorkItemPriority workItemPriority)
        {
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, this._workItemsGroupStartInfo, callback, workItemPriority);
            this.EnqueueToSTPNextWorkItem(workItem);
            return workItem.GetWorkItemResult();
        }

        /// <summary>
        /// Queue a work item
        /// </summary>
        /// <param name="callback">A callback to execute</param>
        /// <param name="state">
        /// The context object of the work item. Used for passing arguments to the work item. 
        /// </param>
        /// <returns>Returns a work item result</returns>
        public IWorkItemResult QueueWorkItem(WorkItemCallback callback, object state)
        {
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, this._workItemsGroupStartInfo, callback, state);
            this.EnqueueToSTPNextWorkItem(workItem);
            return workItem.GetWorkItemResult();
        }

        /// <summary>
        /// Queue a work item
        /// </summary>
        /// <param name="workItemInfo">Work item info</param>
        /// <param name="callback">A callback to execute</param>
        /// <returns>Returns a work item result</returns>
        public IWorkItemResult QueueWorkItem(WorkItemInfo workItemInfo, WorkItemCallback callback)
        {
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, this._workItemsGroupStartInfo, workItemInfo, callback);
            this.EnqueueToSTPNextWorkItem(workItem);
            return workItem.GetWorkItemResult();
        }

        /// <summary>
        /// Queue a work item
        /// </summary>
        /// <param name="callback">A callback to execute</param>
        /// <param name="state">
        /// The context object of the work item. Used for passing arguments to the work item. 
        /// </param>
        /// <param name="postExecuteWorkItemCallback">
        /// A delegate to call after the callback completion
        /// </param>
        /// <returns>Returns a work item result</returns>
        public IWorkItemResult QueueWorkItem(WorkItemCallback callback, object state, PostExecuteWorkItemCallback postExecuteWorkItemCallback)
        {
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, this._workItemsGroupStartInfo, callback, state, postExecuteWorkItemCallback);
            this.EnqueueToSTPNextWorkItem(workItem);
            return workItem.GetWorkItemResult();
        }

        /// <summary>
        /// Queue a work item
        /// </summary>
        /// <param name="callback">A callback to execute</param>
        /// <param name="state">
        /// The context object of the work item. Used for passing arguments to the work item. 
        /// </param>
        /// <param name="workItemPriority">The work item priority</param>
        /// <returns>Returns a work item result</returns>
        public IWorkItemResult QueueWorkItem(WorkItemCallback callback, object state, WorkItemPriority workItemPriority)
        {
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, this._workItemsGroupStartInfo, callback, state, workItemPriority);
            this.EnqueueToSTPNextWorkItem(workItem);
            return workItem.GetWorkItemResult();
        }

        /// <summary>
        /// Queue a work item
        /// </summary>
        /// <param name="workItemInfo">Work item information</param>
        /// <param name="callback">A callback to execute</param>
        /// <param name="state">
        /// The context object of the work item. Used for passing arguments to the work item. 
        /// </param>
        /// <returns>Returns a work item result</returns>
        public IWorkItemResult QueueWorkItem(WorkItemInfo workItemInfo, WorkItemCallback callback, object state)
        {
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, this._workItemsGroupStartInfo, workItemInfo, callback, state);
            this.EnqueueToSTPNextWorkItem(workItem);
            return workItem.GetWorkItemResult();
        }

        /// <summary>
        /// Queue a work item
        /// </summary>
        /// <param name="callback">A callback to execute</param>
        /// <param name="state">
        /// The context object of the work item. Used for passing arguments to the work item. 
        /// </param>
        /// <param name="postExecuteWorkItemCallback">
        /// A delegate to call after the callback completion
        /// </param>
        /// <param name="callToPostExecute">Indicates on which cases to call to the post execute callback</param>
        /// <returns>Returns a work item result</returns>
        public IWorkItemResult QueueWorkItem(WorkItemCallback callback, object state, PostExecuteWorkItemCallback postExecuteWorkItemCallback, CallToPostExecute callToPostExecute)
        {
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, this._workItemsGroupStartInfo, callback, state, postExecuteWorkItemCallback, callToPostExecute);
            this.EnqueueToSTPNextWorkItem(workItem);
            return workItem.GetWorkItemResult();
        }

        /// <summary>
        /// Queue a work item
        /// </summary>
        /// <param name="callback">A callback to execute</param>
        /// <param name="state">
        /// The context object of the work item. Used for passing arguments to the work item. 
        /// </param>
        /// <param name="postExecuteWorkItemCallback">
        /// A delegate to call after the callback completion
        /// </param>
        /// <param name="workItemPriority">The work item priority</param>
        /// <returns>Returns a work item result</returns>
        public IWorkItemResult QueueWorkItem(WorkItemCallback callback, object state, PostExecuteWorkItemCallback postExecuteWorkItemCallback, WorkItemPriority workItemPriority)
        {
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, this._workItemsGroupStartInfo, callback, state, postExecuteWorkItemCallback, workItemPriority);
            this.EnqueueToSTPNextWorkItem(workItem);
            return workItem.GetWorkItemResult();
        }

        /// <summary>
        /// Queue a work item
        /// </summary>
        /// <param name="callback">A callback to execute</param>
        /// <param name="state">
        /// The context object of the work item. Used for passing arguments to the work item. 
        /// </param>
        /// <param name="postExecuteWorkItemCallback">
        /// A delegate to call after the callback completion
        /// </param>
        /// <param name="callToPostExecute">Indicates on which cases to call to the post execute callback</param>
        /// <param name="workItemPriority">The work item priority</param>
        /// <returns>Returns a work item result</returns>
        public IWorkItemResult QueueWorkItem(WorkItemCallback callback, object state, PostExecuteWorkItemCallback postExecuteWorkItemCallback, CallToPostExecute callToPostExecute, WorkItemPriority workItemPriority)
        {
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, this._workItemsGroupStartInfo, callback, state, postExecuteWorkItemCallback, callToPostExecute, workItemPriority);
            this.EnqueueToSTPNextWorkItem(workItem);
            return workItem.GetWorkItemResult();
        }

        private void RegisterToWorkItemCompletion(IWorkItemResult wir)
        {
            IInternalWorkItemResult result = wir as IInternalWorkItemResult;
            result.OnWorkItemStarted += new WorkItemStateCallback(this.OnWorkItemStartedCallback);
            result.OnWorkItemCompleted += new WorkItemStateCallback(this.OnWorkItemCompletedCallback);
        }

        public void Start()
        {
            lock (this)
            {
                if (!this._workItemsGroupStartInfo.StartSuspended)
                {
                    return;
                }
                this._workItemsGroupStartInfo.StartSuspended = false;
            }
            for (int i = 0; i < this._concurrency; i++)
            {
                this.EnqueueToSTPNextWorkItem(null, false);
            }
        }

        /// <summary>
        /// Wait for the thread pool to be idle
        /// </summary>
        public void WaitForIdle()
        {
            this.WaitForIdle(-1);
        }

        /// <summary>
        /// Wait for the thread pool to be idle
        /// </summary>
        public bool WaitForIdle(int millisecondsTimeout)
        {
            this._stp.ValidateWorkItemsGroupWaitForIdle(this);
            return this._isIdleWaitHandle.WaitOne(millisecondsTimeout, false);
        }

        /// <summary>
        /// Wait for the thread pool to be idle
        /// </summary>
        public bool WaitForIdle(TimeSpan timeout)
        {
            return this.WaitForIdle((int) timeout.TotalMilliseconds);
        }

        /// <summary>
        /// Get/Set the name of the SmartThreadPool instance
        /// </summary>
        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }

        public int WaitingCallbacks
        {
            get
            {
                return this._workItemsQueue.Count;
            }
        }
    }
}

