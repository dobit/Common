using System;
using System.Threading;

namespace LFNet.Common.Threading.Internal
{
    /// <summary>
    /// Holds a callback delegate and the state for that delegate.
    /// </summary>
    public class WorkItem : IHasWorkItemPriority, IWorkItem
    {
        /// <summary>
        /// The time when the work items starts its execution.
        /// Used with the performance counter.
        /// </summary>
        private DateTime _beginProcessTime;
        /// <summary>
        /// Callback delegate for the callback.
        /// </summary>
        private WorkItemCallback _callback;
        /// <summary>
        /// Stores the caller's context
        /// </summary>
        private CallerThreadContext _callerContext;
        /// <summary>
        /// A reference to an object that indicates whatever the 
        /// WorkItemsGroup has been canceled
        /// </summary>
        private CanceledWorkItemsGroup _canceledWorkItemsGroup = CanceledWorkItemsGroup.NotCanceledWorkItemsGroup;
        /// <summary>
        /// The time when the work items ends its execution.
        /// Used with the performance counter.
        /// </summary>
        private DateTime _endProcessTime;
        /// <summary>
        /// Hold the exception if the method threw it
        /// </summary>
        private Exception _exception;
        /// <summary>
        /// The time when the work items is queued.
        /// Used with the performance counter.
        /// </summary>
        private DateTime _queuedTime;
        /// <summary>
        /// Holds the result of the mehtod
        /// </summary>
        private object _result;
        /// <summary>
        /// State with which to call the callback delegate.
        /// </summary>
        private object _state;
        /// <summary>
        /// A ManualResetEvent to indicate that the result is ready
        /// </summary>
        private ManualResetEvent _workItemCompleted;
        /// <summary>
        /// A reference count to the _workItemCompleted. 
        /// When it reaches to zero _workItemCompleted is Closed
        /// </summary>
        private int _workItemCompletedRefCount;
        /// <summary>
        /// Work item info
        /// </summary>
        private WorkItemInfo _workItemInfo;
        /// <summary>
        /// Represents the result state of the work item
        /// </summary>
        private WorkItemResult _workItemResult;
        /// <summary>
        /// The work item group this work item belong to.
        /// 
        /// </summary>
        private IWorkItemsGroup _workItemsGroup;
        /// <summary>
        /// Hold the state of the work item
        /// </summary>
        private WorkItemState _workItemState;

        /// <summary>
        /// Called when the WorkItem completes
        /// </summary>
        private event WorkItemStateCallback _workItemCompletedEvent;

        /// <summary>
        /// Called when the WorkItem starts
        /// </summary>
        private event WorkItemStateCallback _workItemStartedEvent;

        internal event WorkItemStateCallback OnWorkItemCompleted
        {
            add
            {
                this._workItemCompletedEvent += value;
            }
            remove
            {
                this._workItemCompletedEvent -= value;
            }
        }

        internal event WorkItemStateCallback OnWorkItemStarted
        {
            add
            {
                this._workItemStartedEvent += value;
            }
            remove
            {
                this._workItemStartedEvent -= value;
            }
        }

        /// <summary>
        /// Initialize the callback holding object.
        /// </summary>
        /// <param name="workItemsGroup"></param>
        /// <param name="workItemInfo"></param>
        /// <param name="callback">Callback delegate for the callback.</param>
        /// <param name="state">State with which to call the callback delegate.</param>
        /// 
        /// We assume that the WorkItem object is created within the thread
        /// that meant to run the callback
        public WorkItem(IWorkItemsGroup workItemsGroup, WorkItemInfo workItemInfo, WorkItemCallback callback, object state)
        {
            this._workItemsGroup = workItemsGroup;
            this._workItemInfo = workItemInfo;
            if (this._workItemInfo.UseCallerCallContext || this._workItemInfo.UseCallerHttpContext)
            {
                this._callerContext = CallerThreadContext.Capture(this._workItemInfo.UseCallerCallContext, this._workItemInfo.UseCallerHttpContext);
            }
            this._callback = callback;
            this._state = state;
            this._workItemResult = new WorkItemResult(this);
            this.Initialize();
        }

        /// <summary>
        /// Cancel the work item if it didn't start running yet.
        /// </summary>
        /// <returns>Returns true on success or false if the work item is in progress or already completed</returns>
        private bool Cancel()
        {
            lock (this)
            {
                switch (this.GetWorkItemState())
                {
                    case WorkItemState.InQueue:
                        this.SignalComplete(true);
                        return true;

                    case WorkItemState.InProgress:
                    case WorkItemState.Completed:
                        return false;

                    case WorkItemState.Canceled:
                        return true;
                }
            }
            return false;
        }

        public void DisposeOfState()
        {
            if (this._workItemInfo.DisposeOfStateObjects)
            {
                IDisposable disposable = this._state as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                    this._state = null;
                }
            }
        }

        /// <summary>
        /// Execute the work item and the post execute
        /// </summary>
        public void Execute()
        {
            CallToPostExecute never = CallToPostExecute.Never;
            switch (this.GetWorkItemState())
            {
                case WorkItemState.InProgress:
                    never |= CallToPostExecute.WhenWorkItemNotCanceled;
                    this.ExecuteWorkItem();
                    break;

                case WorkItemState.Canceled:
                    never |= CallToPostExecute.WhenWorkItemCanceled;
                    break;

                default:
                    throw new NotSupportedException();
            }
            if ((never & this._workItemInfo.CallToPostExecute) != CallToPostExecute.Never)
            {
                this.PostExecute();
            }
            this._endProcessTime = DateTime.Now;
        }

        /// <summary>
        /// Execute the work item
        /// </summary>
        private void ExecuteWorkItem()
        {
            CallerThreadContext callerThreadContext = null;
            if (this._callerContext != null)
            {
                callerThreadContext = CallerThreadContext.Capture(this._callerContext.CapturedCallContext, this._callerContext.CapturedHttpContext);
                CallerThreadContext.Apply(this._callerContext);
            }
            Exception exception = null;
            object result = null;
            try
            {
                result = this._callback(this._state);
            }
            catch (Exception exception2)
            {
                exception = exception2;
            }
            if (this._callerContext != null)
            {
                CallerThreadContext.Apply(callerThreadContext);
            }
            this.SetResult(result, exception);
        }

        internal void FireWorkItemCompleted()
        {
            try
            {
                if (this._workItemCompletedEvent != null)
                {
                    this._workItemCompletedEvent(this);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Get the result of the work item.
        /// If the work item didn't run yet then the caller waits for the result, timeout, or cancel.
        /// In case of error the method throws and exception
        /// </summary>
        /// <returns>The result of the work item</returns>
        private object GetResult(int millisecondsTimeout, bool exitContext, WaitHandle cancelWaitHandle)
        {
            Exception e = null;
            object obj2 = this.GetResult(millisecondsTimeout, exitContext, cancelWaitHandle, out e);
            if (e != null)
            {
                throw new WorkItemResultException("The work item caused an excpetion, see the inner exception for details", e);
            }
            return obj2;
        }

        /// <summary>
        /// Get the result of the work item.
        /// If the work item didn't run yet then the caller waits for the result, timeout, or cancel.
        /// In case of error the e argument is filled with the exception
        /// </summary>
        /// <returns>The result of the work item</returns>
        private object GetResult(int millisecondsTimeout, bool exitContext, WaitHandle cancelWaitHandle, out Exception e)
        {
            e = null;
            if (WorkItemState.Canceled == this.GetWorkItemState())
            {
                throw new WorkItemCancelException("Work item canceled");
            }
            if (this.IsCompleted)
            {
                e = this._exception;
                return this._result;
            }
            if (cancelWaitHandle == null)
            {
                bool flag = !this.GetWaitHandle().WaitOne(millisecondsTimeout, exitContext);
                this.ReleaseWaitHandle();
                if (flag)
                {
                    throw new WorkItemTimeoutException("Work item timeout");
                }
            }
            else
            {
                WaitHandle waitHandle = this.GetWaitHandle();
                int num = WaitHandle.WaitAny(new WaitHandle[] { waitHandle, cancelWaitHandle });
                this.ReleaseWaitHandle();
                switch (num)
                {
                    case 1:
                    case 0x102:
                        throw new WorkItemTimeoutException("Work item timeout");
                }
            }
            if (WorkItemState.Canceled == this.GetWorkItemState())
            {
                throw new WorkItemCancelException("Work item canceled");
            }
            e = this._exception;
            return this._result;
        }

        /// <summary>
        /// A wait handle to wait for completion, cancel, or timeout 
        /// </summary>
        private WaitHandle GetWaitHandle()
        {
            lock (this)
            {
                if (this._workItemCompleted == null)
                {
                    this._workItemCompleted = new ManualResetEvent(this.IsCompleted);
                }
                this._workItemCompletedRefCount++;
            }
            return this._workItemCompleted;
        }

        /// <summary>
        /// Fill an array of wait handles with the work items wait handles.
        /// </summary>
        /// <param name="workItemResults">An array of work item results</param>
        /// <param name="waitHandles">An array of wait handles to fill</param>
        private static void GetWaitHandles(IWorkItemResult[] workItemResults, WaitHandle[] waitHandles)
        {
            for (int i = 0; i < workItemResults.Length; i++)
            {
                WorkItemResult result = workItemResults[i] as WorkItemResult;
                waitHandles[i] = result.GetWorkItem().GetWaitHandle();
            }
        }

        /// <summary>
        /// Returns the work item result
        /// </summary>
        /// <returns>The work item result</returns>
        internal IWorkItemResult GetWorkItemResult()
        {
            return this._workItemResult;
        }

        private WorkItemState GetWorkItemState()
        {
            if (this._canceledWorkItemsGroup.IsCanceled)
            {
                return WorkItemState.Canceled;
            }
            return this._workItemState;
        }

        internal void Initialize()
        {
            this._workItemState = WorkItemState.InQueue;
            this._workItemCompleted = null;
            this._workItemCompletedRefCount = 0;
        }

        /// <summary>
        /// Runs the post execute callback
        /// </summary>
        private void PostExecute()
        {
            if (this._workItemInfo.PostExecuteWorkItemCallback != null)
            {
                try
                {
                    this._workItemInfo.PostExecuteWorkItemCallback(this._workItemResult);
                }
                catch (Exception)
                {
                }
            }
        }

        private void ReleaseWaitHandle()
        {
            lock (this)
            {
                if (this._workItemCompleted != null)
                {
                    this._workItemCompletedRefCount--;
                    if (this._workItemCompletedRefCount == 0)
                    {
                        this._workItemCompleted.Close();
                        this._workItemCompleted = null;
                    }
                }
            }
        }

        /// <summary>
        /// Release the work items' wait handles
        /// </summary>
        /// <param name="workItemResults">An array of work item results</param>
        private static void ReleaseWaitHandles(IWorkItemResult[] workItemResults)
        {
            for (int i = 0; i < workItemResults.Length; i++)
            {
                WorkItemResult result = workItemResults[i] as WorkItemResult;
                result.GetWorkItem().ReleaseWaitHandle();
            }
        }

        /// <summary>
        /// Set the result of the work item to return
        /// </summary>
        /// <param name="result">The result of the work item</param>
        /// <param name="exception">The exception.</param>
        internal void SetResult(object result, Exception exception)
        {
            this._result = result;
            this._exception = exception;
            this.SignalComplete(false);
        }

        /// <summary>
        /// Sets the work item's state
        /// </summary>
        /// <param name="workItemState">The state to set the work item to</param>
        private void SetWorkItemState(WorkItemState workItemState)
        {
            lock (this)
            {
                this._workItemState = workItemState;
            }
        }

        /// <summary>
        /// Signals that work item has been completed or canceled
        /// </summary>
        /// <param name="canceled">Indicates that the work item has been canceled</param>
        private void SignalComplete(bool canceled)
        {
            this.SetWorkItemState(canceled ? WorkItemState.Canceled : WorkItemState.Completed);
            lock (this)
            {
                if (this._workItemCompleted != null)
                {
                    this._workItemCompleted.Set();
                }
            }
        }

        /// <summary>
        /// Change the state of the work item to in progress if it wasn't canceled.
        /// </summary>
        /// <returns>
        /// Return true on success or false in case the work item was canceled.
        /// If the work item needs to run a post execute then the method will return true.
        /// </returns>
        public bool StartingWorkItem()
        {
            this._beginProcessTime = DateTime.Now;
            lock (this)
            {
                if (this.IsCanceled)
                {
                    bool flag = false;
                    if ((this._workItemInfo.PostExecuteWorkItemCallback != null) && ((this._workItemInfo.CallToPostExecute & CallToPostExecute.WhenWorkItemCanceled) == CallToPostExecute.WhenWorkItemCanceled))
                    {
                        flag = true;
                    }
                    return flag;
                }
                this.SetWorkItemState(WorkItemState.InProgress);
            }
            return true;
        }

        /// <summary>
        /// Wait for all work items to complete
        /// </summary>
        /// <param name="workItemResults">Array of work item result objects</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or Timeout.Infinite (-1) to wait indefinitely.</param>
        /// <param name="exitContext">
        /// true to exit the synchronization domain for the context before the wait (if in a synchronized context), and reacquire it; otherwise, false. 
        /// </param>
        /// <param name="cancelWaitHandle">A cancel wait handle to interrupt the wait if needed</param>
        /// <returns>
        /// true when every work item in workItemResults has completed; otherwise false.
        /// </returns>
        internal static bool WaitAll(IWorkItemResult[] workItemResults, int millisecondsTimeout, bool exitContext, WaitHandle cancelWaitHandle)
        {
            bool flag;
            if (workItemResults.Length == 0)
            {
                return true;
            }
            WaitHandle[] waitHandles = new WaitHandle[workItemResults.Length];
            GetWaitHandles(workItemResults, waitHandles);
            if ((cancelWaitHandle == null) && (waitHandles.Length <= 0x40))
            {
                flag = WaitHandle.WaitAll(waitHandles, millisecondsTimeout, exitContext);
            }
            else
            {
                WaitHandle[] handleArray2;
                flag = true;
                int num = millisecondsTimeout;
                DateTime now = DateTime.Now;
                if (cancelWaitHandle != null)
                {
                    WaitHandle[] handleArray3 = new WaitHandle[2];
                    handleArray3[1] = cancelWaitHandle;
                    handleArray2 = handleArray3;
                }
                else
                {
                    handleArray2 = new WaitHandle[1];
                }
                bool flag2 = -1 == millisecondsTimeout;
                for (int i = 0; i < workItemResults.Length; i++)
                {
                    if (!flag2 && (num < 0))
                    {
                        flag = false;
                        break;
                    }
                    handleArray2[0] = waitHandles[i];
                    int num3 = WaitHandle.WaitAny(handleArray2, num, exitContext);
                    if ((num3 > 0) || (0x102 == num3))
                    {
                        flag = false;
                        break;
                    }
                    if (!flag2)
                    {
                        TimeSpan span = (TimeSpan) (DateTime.Now - now);
                        num = millisecondsTimeout - ((int) span.TotalMilliseconds);
                    }
                }
            }
            ReleaseWaitHandles(workItemResults);
            return flag;
        }

        /// <summary>
        /// Waits for any of the work items in the specified array to complete, cancel, or timeout
        /// </summary>
        /// <param name="workItemResults">Array of work item result objects</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or Timeout.Infinite (-1) to wait indefinitely.</param>
        /// <param name="exitContext">
        /// true to exit the synchronization domain for the context before the wait (if in a synchronized context), and reacquire it; otherwise, false. 
        /// </param>
        /// <param name="cancelWaitHandle">A cancel wait handle to interrupt the wait if needed</param>
        /// <returns>
        /// The array index of the work item result that satisfied the wait, or WaitTimeout if no work item result satisfied the wait and a time interval equivalent to millisecondsTimeout has passed or the work item has been canceled.
        /// </returns>
        internal static int WaitAny(IWorkItemResult[] workItemResults, int millisecondsTimeout, bool exitContext, WaitHandle cancelWaitHandle)
        {
            WaitHandle[] waitHandles = null;
            if (cancelWaitHandle != null)
            {
                waitHandles = new WaitHandle[workItemResults.Length + 1];
                GetWaitHandles(workItemResults, waitHandles);
                waitHandles[workItemResults.Length] = cancelWaitHandle;
            }
            else
            {
                waitHandles = new WaitHandle[workItemResults.Length];
                GetWaitHandles(workItemResults, waitHandles);
            }
            int num = WaitHandle.WaitAny(waitHandles, millisecondsTimeout, exitContext);
            if ((cancelWaitHandle != null) && (num == workItemResults.Length))
            {
                num = 0x102;
            }
            ReleaseWaitHandles(workItemResults);
            return num;
        }

        internal bool WasQueuedBy(IWorkItemsGroup workItemsGroup)
        {
            return (workItemsGroup == this._workItemsGroup);
        }

        internal void WorkItemIsQueued()
        {
            this._queuedTime = DateTime.Now;
        }

        public CanceledWorkItemsGroup CanceledWorkItemsGroup
        {
            get
            {
                return this._canceledWorkItemsGroup;
            }
            set
            {
                this._canceledWorkItemsGroup = value;
            }
        }

        /// <summary>
        /// Returns true when the work item has canceled
        /// </summary>
        public bool IsCanceled
        {
            get
            {
                lock (this)
                {
                    return (this.GetWorkItemState() == WorkItemState.Canceled);
                }
            }
        }

        /// <summary>
        /// Returns true when the work item has completed or canceled
        /// </summary>
        private bool IsCompleted
        {
            get
            {
                lock (this)
                {
                    WorkItemState workItemState = this.GetWorkItemState();
                    return ((workItemState == WorkItemState.Completed) || (workItemState == WorkItemState.Canceled));
                }
            }
        }

        public TimeSpan ProcessTime
        {
            get
            {
                return (TimeSpan) (this._endProcessTime - this._beginProcessTime);
            }
        }

        public TimeSpan WaitingTime
        {
            get
            {
                return (TimeSpan) (this._beginProcessTime - this._queuedTime);
            }
        }

        /// <summary>
        /// Returns the priority of the work item
        /// </summary>
        public WorkItemPriority WorkItemPriority
        {
            get
            {
                return this._workItemInfo.WorkItemPriority;
            }
        }

        private class WorkItemResult : IWorkItemResult, IInternalWorkItemResult
        {
            /// <summary>
            /// A back reference to the work item
            /// </summary>
            private WorkItem _workItem;

            public event WorkItemStateCallback OnWorkItemCompleted
            {
                add
                {
                    this._workItem.OnWorkItemCompleted += value;
                }
                remove
                {
                    this._workItem.OnWorkItemCompleted -= value;
                }
            }

            public event WorkItemStateCallback OnWorkItemStarted
            {
                add
                {
                    this._workItem.OnWorkItemStarted += value;
                }
                remove
                {
                    this._workItem.OnWorkItemStarted -= value;
                }
            }

            public WorkItemResult(WorkItem workItem)
            {
                this._workItem = workItem;
            }

            public bool Cancel()
            {
                return this._workItem.Cancel();
            }

            public object GetResult()
            {
                return this._workItem.GetResult(-1, true, null);
            }

            public object GetResult(out global::System.Exception e)
            {
                return this._workItem.GetResult(-1, true, null, out e);
            }

            public object GetResult(int millisecondsTimeout, bool exitContext)
            {
                return this._workItem.GetResult(millisecondsTimeout, exitContext, null);
            }

            public object GetResult(TimeSpan timeout, bool exitContext)
            {
                return this._workItem.GetResult((int) timeout.TotalMilliseconds, exitContext, null);
            }

            public object GetResult(int millisecondsTimeout, bool exitContext, WaitHandle cancelWaitHandle)
            {
                return this._workItem.GetResult(millisecondsTimeout, exitContext, cancelWaitHandle);
            }

            public object GetResult(int millisecondsTimeout, bool exitContext, out global::System.Exception e)
            {
                return this._workItem.GetResult(millisecondsTimeout, exitContext, null, out e);
            }

            public object GetResult(TimeSpan timeout, bool exitContext, WaitHandle cancelWaitHandle)
            {
                return this._workItem.GetResult((int) timeout.TotalMilliseconds, exitContext, cancelWaitHandle);
            }

            public object GetResult(TimeSpan timeout, bool exitContext, out global::System.Exception e)
            {
                return this._workItem.GetResult((int) timeout.TotalMilliseconds, exitContext, null, out e);
            }

            public object GetResult(int millisecondsTimeout, bool exitContext, WaitHandle cancelWaitHandle, out global::System.Exception e)
            {
                return this._workItem.GetResult(millisecondsTimeout, exitContext, cancelWaitHandle, out e);
            }

            public object GetResult(TimeSpan timeout, bool exitContext, WaitHandle cancelWaitHandle, out global::System.Exception e)
            {
                return this._workItem.GetResult((int) timeout.TotalMilliseconds, exitContext, cancelWaitHandle, out e);
            }

            internal WorkItem GetWorkItem()
            {
                return this._workItem;
            }

            /// <summary>
            /// Returns the exception if occured otherwise returns null.
            /// This value is valid only after the work item completed,
            /// before that it is always null.
            /// </summary>
            public object Exception
            {
                get
                {
                    return this._workItem._exception;
                }
            }

            public bool IsCanceled
            {
                get
                {
                    return this._workItem.IsCanceled;
                }
            }

            public bool IsCompleted
            {
                get
                {
                    return this._workItem.IsCompleted;
                }
            }

            /// <summary>
            /// Return the result, same as GetResult()
            /// </summary>
            public object Result
            {
                get
                {
                    return this.GetResult();
                }
            }

            public object State
            {
                get
                {
                    return this._workItem._state;
                }
            }

            public WorkItemPriority WorkItemPriority
            {
                get
                {
                    return this._workItem._workItemInfo.WorkItemPriority;
                }
            }
        }

        /// <summary>
        /// Indicates the state of the work item in the thread pool
        /// </summary>
        private enum WorkItemState
        {
            InQueue,
            InProgress,
            Completed,
            Canceled
        }
    }
}

