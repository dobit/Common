using System;
using System.Threading;

namespace LFNet.Common.Threading.Internal
{
    /// <summary>
    /// WorkItemsQueue class.
    /// </summary>
    public class WorkItemsQueue : IDisposable
    {
        /// <summary>
        /// Waiters queue (implemented as stack).
        /// </summary>
        private WaiterEntry _headWaiterEntry = new WaiterEntry();
        /// <summary>
        /// A flag that indicates if the WorkItemsQueue has been disposed.
        /// </summary>
        private bool _isDisposed;
        /// <summary>
        /// Indicate that work items are allowed to be queued
        /// </summary>
        private bool _isWorkItemsQueueActive = true;
        /// <summary>
        /// Each thread in the thread pool keeps its own waiter entry.
        /// </summary>
        [ThreadStatic]
        private static WaiterEntry _waiterEntry;
        /// <summary>
        /// Waiters count
        /// </summary>
        private int _waitersCount;
        /// <summary>
        /// Work items queue
        /// </summary>
        private PriorityQueue _workItems = new PriorityQueue();

        /// <summary>
        /// Cleanup the work items queue, hence no more work 
        /// items are allowed to be queue
        /// </summary>
        protected virtual void Cleanup()
        {
            lock (this)
            {
                if (this._isWorkItemsQueueActive)
                {
                    this._isWorkItemsQueueActive = false;
                    foreach (WorkItem item in this._workItems)
                    {
                        item.DisposeOfState();
                    }
                    this._workItems.Clear();
                    while (this._waitersCount > 0)
                    {
                        this.PopWaiter().Timeout();
                    }
                }
            }
        }

        /// <summary>
        /// Waits for a work item or exits on timeout or cancel
        /// </summary>
        /// <param name="millisecondsTimeout">Timeout in milliseconds</param>
        /// <param name="cancelEvent">Cancel wait handle</param>
        /// <returns>Returns true if the resource was granted</returns>
        public WorkItem DequeueWorkItem(int millisecondsTimeout, WaitHandle cancelEvent)
        {
            WaiterEntry newWaiterEntry = null;
            WorkItem workItem = null;
            lock (this)
            {
                this.ValidateNotDisposed();
                if (this._workItems.Count > 0)
                {
                    return (this._workItems.Dequeue() as WorkItem);
                }
                newWaiterEntry = this.GetThreadWaiterEntry();
                this.PushWaiter(newWaiterEntry);
            }
            WaitHandle[] waitHandles = new WaitHandle[] { newWaiterEntry.WaitHandle, cancelEvent };
            int num = WaitHandle.WaitAny(waitHandles, millisecondsTimeout, true);
            lock (this)
            {
                bool flag = 0 == num;
                if (!flag)
                {
                    bool flag2 = newWaiterEntry.Timeout();
                    if (flag2)
                    {
                        this.RemoveWaiter(newWaiterEntry, false);
                    }
                    flag = !flag2;
                }
                if (flag)
                {
                    workItem = newWaiterEntry.WorkItem;
                    if (workItem == null)
                    {
                        workItem = this._workItems.Dequeue() as WorkItem;
                    }
                }
            }
            return workItem;
        }

        public void Dispose()
        {
            if (!this._isDisposed)
            {
                this.Cleanup();
                this._isDisposed = true;
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Enqueue a work item to the queue.
        /// </summary>
        public bool EnqueueWorkItem(WorkItem workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException("workItem", "workItem cannot be null");
            }
            bool flag = true;
            lock (this)
            {
                this.ValidateNotDisposed();
                if (this._isWorkItemsQueueActive)
                {
                    goto Label_0048;
                }
                return false;
            Label_0034:
                if (this.PopWaiter().Signal(workItem))
                {
                    flag = false;
                    goto Label_0051;
                }
            Label_0048:
                if (this._waitersCount > 0)
                {
                    goto Label_0034;
                }
            Label_0051:
                if (flag)
                {
                    this._workItems.Enqueue(workItem);
                }
            }
            return true;
        }

        ~WorkItemsQueue()
        {
            this.Cleanup();
        }

        /// <summary>
        /// Returns the WaiterEntry of the current thread
        /// </summary>
        /// <returns></returns>
        /// In order to avoid creation and destuction of WaiterEntry
        /// objects each thread has its own WaiterEntry object.
        private WaiterEntry GetThreadWaiterEntry()
        {
            if (_waiterEntry == null)
            {
                _waiterEntry = new WaiterEntry();
            }
            _waiterEntry.Reset();
            return _waiterEntry;
        }

        /// <summary>
        /// Pop a waiter from the waiter's stack
        /// </summary>
        /// <returns>Returns the first waiter in the stack</returns>
        private WaiterEntry PopWaiter()
        {
            WaiterEntry waiterEntry = this._headWaiterEntry._nextWaiterEntry;
            WaiterEntry entry2 = waiterEntry._nextWaiterEntry;
            this.RemoveWaiter(waiterEntry, true);
            this._headWaiterEntry._nextWaiterEntry = entry2;
            if (entry2 != null)
            {
                entry2._prevWaiterEntry = this._headWaiterEntry;
            }
            return waiterEntry;
        }

        /// <summary>
        /// Push a new waiter into the waiter's stack
        /// </summary>
        /// <param name="newWaiterEntry">A waiter to put in the stack</param>
        public void PushWaiter(WaiterEntry newWaiterEntry)
        {
            this.RemoveWaiter(newWaiterEntry, false);
            if (this._headWaiterEntry._nextWaiterEntry == null)
            {
                this._headWaiterEntry._nextWaiterEntry = newWaiterEntry;
                newWaiterEntry._prevWaiterEntry = this._headWaiterEntry;
            }
            else
            {
                WaiterEntry entry = this._headWaiterEntry._nextWaiterEntry;
                this._headWaiterEntry._nextWaiterEntry = newWaiterEntry;
                newWaiterEntry._nextWaiterEntry = entry;
                newWaiterEntry._prevWaiterEntry = this._headWaiterEntry;
                entry._prevWaiterEntry = newWaiterEntry;
            }
            this._waitersCount++;
        }

        /// <summary>
        /// Remove a waiter from the stack
        /// </summary>
        /// <param name="waiterEntry">A waiter entry to remove</param>
        /// <param name="popDecrement">If true the waiter count is always decremented</param>
        private void RemoveWaiter(WaiterEntry waiterEntry, bool popDecrement)
        {
            WaiterEntry entry = waiterEntry._prevWaiterEntry;
            WaiterEntry entry2 = waiterEntry._nextWaiterEntry;
            bool flag = popDecrement;
            waiterEntry._prevWaiterEntry = null;
            waiterEntry._nextWaiterEntry = null;
            if (entry != null)
            {
                entry._nextWaiterEntry = entry2;
                flag = true;
            }
            if (entry2 != null)
            {
                entry2._prevWaiterEntry = entry;
                flag = true;
            }
            if (flag)
            {
                this._waitersCount--;
            }
        }

        private void ValidateNotDisposed()
        {
            if (this._isDisposed)
            {
                throw new ObjectDisposedException(base.GetType().ToString(), "The SmartThreadPool has been shutdown");
            }
        }

        /// <summary>
        /// Returns the current number of work items in the queue
        /// </summary>
        public int Count
        {
            get
            {
                lock (this)
                {
                    this.ValidateNotDisposed();
                    return this._workItems.Count;
                }
            }
        }

        /// <summary>
        /// Returns the current number of waiters
        /// </summary>
        public int WaitersCount
        {
            get
            {
                lock (this)
                {
                    this.ValidateNotDisposed();
                    return this._waitersCount;
                }
            }
        }

        public class WaiterEntry : IDisposable
        {
            private bool _isDisposed;
            /// <summary>
            /// Flag to know if the waiter was signaled and got a work item. 
            /// </summary>
            private bool _isSignaled;
            /// <summary>
            /// Flag to know if this waiter already quited from the queue 
            /// because of a timeout.
            /// </summary>
            private bool _isTimedout;
            internal WorkItemsQueue.WaiterEntry _nextWaiterEntry;
            internal WorkItemsQueue.WaiterEntry _prevWaiterEntry;
            /// <summary>
            /// Event to signal the waiter that it got the work item.
            /// </summary>
            private AutoResetEvent _waitHandle = new AutoResetEvent(false);
            /// <summary>
            /// A work item that passed directly to the waiter withou going 
            /// through the queue
            /// </summary>
            private WorkItem _workItem;

            public WaiterEntry()
            {
                this.Reset();
            }

            /// <summary>
            /// Free resources
            /// </summary>
            public void Close()
            {
                if (this._waitHandle != null)
                {
                    this._waitHandle.Close();
                    this._waitHandle = null;
                }
            }

            public void Dispose()
            {
                if (!this._isDisposed)
                {
                    this.Close();
                    this._isDisposed = true;
                }
            }

            ~WaiterEntry()
            {
                this.Dispose();
            }

            /// <summary>
            /// Reset the wait entry so it can be used again
            /// </summary>
            public void Reset()
            {
                this._workItem = null;
                this._isTimedout = false;
                this._isSignaled = false;
                this._waitHandle.Reset();
            }

            /// <summary>
            /// Signal the waiter that it got a work item.
            /// </summary>
            /// <returns>Return true on success</returns>
            /// The method fails if Timeout() preceded its call
            public bool Signal(WorkItem workItem)
            {
                lock (this)
                {
                    if (!this._isTimedout)
                    {
                        this._workItem = workItem;
                        this._isSignaled = true;
                        this._waitHandle.Set();
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// Mark the wait entry that it has been timed out
            /// </summary>
            /// <returns>Return true on success</returns>
            /// The method fails if Signal() preceded its call
            public bool Timeout()
            {
                lock (this)
                {
                    if (!this._isSignaled)
                    {
                        this._isTimedout = true;
                        return true;
                    }
                }
                return false;
            }

            public global::System.Threading.WaitHandle WaitHandle
            {
                get
                {
                    return this._waitHandle;
                }
            }

            public WorkItem WorkItem
            {
                get
                {
                    lock (this)
                    {
                        return this._workItem;
                    }
                }
            }
        }
    }
}

