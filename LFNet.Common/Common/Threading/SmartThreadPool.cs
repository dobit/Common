using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading;
using LFNet.Common.Threading.Internal;

namespace LFNet.Common.Threading
{
    /// <summary>
    /// Smart thread pool class.
    /// </summary>
    public class SmartThreadPool : IWorkItemsGroup, IDisposable
    {
        /// <summary>
        /// A reference to the current work item a thread from the thread pool 
        /// is executing.
        /// </summary>
        [ThreadStatic]
        private static WorkItem _currentWorkItem;
        /// <summary>
        /// Total number of work items that are stored in the work items queue 
        /// plus the work items that the threads in the pool are working on.
        /// </summary>
        private int _currentWorkItemsCount;
        /// <summary>
        /// Number of threads that currently work (not idle).
        /// </summary>
        private int _inUseWorkerThreads;
        /// <summary>
        /// Indicate that the SmartThreadPool has been disposed
        /// </summary>
        private bool _isDisposed;
        /// <summary>
        /// Signaled when the thread pool is idle, i.e. no thread is busy
        /// and the work items queue is empty
        /// </summary>
        private ManualResetEvent _isIdleWaitHandle;
        /// <summary>
        /// Contains the name of this instance of SmartThreadPool.
        /// Can be changed by the user.
        /// </summary>
        private string _name;
        /// <summary>
        /// STP performance counters
        /// </summary>
        private ISTPInstancePerformanceCounters _pcs;
        /// <summary>
        /// A flag to indicate the threads to quit.
        /// </summary>
        private bool _shutdown;
        /// <summary>
        /// An event to signal all the threads to quit immediately.
        /// </summary>
        private ManualResetEvent _shuttingDownEvent;
        /// <summary>
        /// A reference from each thread in the thread pool to its SmartThreadPool
        /// object container.
        /// With this variable a thread can know whatever it belongs to a 
        /// SmartThreadPool.
        /// </summary>
        [ThreadStatic]
        private static SmartThreadPool _smartThreadPool;
        /// <summary>
        /// Start information to use. 
        /// It is simpler than providing many constructors.
        /// </summary>
        private STPStartInfo _stpStartInfo;
        /// <summary>
        /// Counts the threads created in the pool.
        /// It is used to name the threads.
        /// </summary>
        private int _threadCounter;
        /// <summary>
        /// Hashtable of all the threads in the thread pool.
        /// </summary>
        private Hashtable _workerThreads;
        /// <summary>
        /// Holds all the WorkItemsGroup instances that have at least one 
        /// work item int the SmartThreadPool
        /// This variable is used in case of Shutdown
        /// </summary>
        private Hashtable _workItemsGroups;
        /// <summary>
        /// Count the work items handled.
        /// Used by the performance counter.
        /// </summary>
        private long _workItemsProcessed;
        /// <summary>
        /// Queue of work items.
        /// </summary>
        private WorkItemsQueue _workItemsQueue;
        /// <summary>
        /// The default option to run the post execute
        /// </summary>
        public const CallToPostExecute DefaultCallToPostExecute = CallToPostExecute.Always;
        /// <summary>
        /// Indicate to dispose of the state objects if they support the IDispose interface. (false)
        /// </summary>
        public const bool DefaultDisposeOfStateObjects = false;
        /// <summary>
        /// Default idle timeout in milliseconds. (One minute)
        /// </summary>
        public const int DefaultIdleTimeout = 0xea60;
        /// <summary>
        /// Default maximum number of threads the thread pool contains. (25)
        /// </summary>
        public const int DefaultMaxWorkerThreads = 0x19;
        /// <summary>
        /// Default minimum number of threads the thread pool contains. (0)
        /// </summary>
        public const int DefaultMinWorkerThreads = 0;
        /// <summary>
        /// The default is not to use the performance counters
        /// </summary>
        public static readonly string DefaultPerformanceCounterInstanceName = null;
        /// <summary>
        /// The default post execute method to run. 
        /// When null it means not to call it.
        /// </summary>
        public static readonly PostExecuteWorkItemCallback DefaultPostExecuteWorkItemCallback = null;
        /// <summary>
        /// The default is to work on work items as soon as they arrive
        /// and not to wait for the start.
        /// </summary>
        public const bool DefaultStartSuspended = false;
        /// <summary>
        /// The default thread priority
        /// </summary>
        public const ThreadPriority DefaultThreadPriority = ThreadPriority.Normal;
        /// <summary>
        /// Indicate to copy the security context of the caller and then use it in the call. (false)
        /// </summary>
        public const bool DefaultUseCallerCallContext = false;
        /// <summary>
        /// Indicate to copy the HTTP context of the caller and then use it in the call. (false)
        /// </summary>
        public const bool DefaultUseCallerHttpContext = false;
        /// <summary>
        /// The default work item priority
        /// </summary>
        public const WorkItemPriority DefaultWorkItemPriority = WorkItemPriority.Normal;

        /// <summary>
        /// Event to send that the thread pool is idle
        /// </summary>
        private event EventHandler _stpIdle;

        public event EventHandler Idle
        {
            add
            {
                this._stpIdle += value;
            }
            remove
            {
                this._stpIdle -= value;
            }
        }

        public event WorkItemsGroupIdleHandler OnIdle
        {
            add
            {
                throw new NotImplementedException("This event is not implemented in the SmartThreadPool class. Please create a WorkItemsGroup in order to use this feature.");
            }
            remove
            {
                throw new NotImplementedException("This event is not implemented in the SmartThreadPool class. Please create a WorkItemsGroup in order to use this feature.");
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SmartThreadPool()
        {
            this._name = "SmartThreadPool";
            this._workerThreads = Hashtable.Synchronized(new Hashtable());
            this._workItemsQueue = new WorkItemsQueue();
            this._stpStartInfo = new STPStartInfo();
            this._isIdleWaitHandle = new ManualResetEvent(true);
            this._shuttingDownEvent = new ManualResetEvent(false);
            this._workItemsGroups = Hashtable.Synchronized(new Hashtable());
            this._pcs = NullSTPInstancePerformanceCounters.Instance;
            this.Initialize();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SmartThreadPool(STPStartInfo stpStartInfo)
        {
            this._name = "SmartThreadPool";
            this._workerThreads = Hashtable.Synchronized(new Hashtable());
            this._workItemsQueue = new WorkItemsQueue();
            this._stpStartInfo = new STPStartInfo();
            this._isIdleWaitHandle = new ManualResetEvent(true);
            this._shuttingDownEvent = new ManualResetEvent(false);
            this._workItemsGroups = Hashtable.Synchronized(new Hashtable());
            this._pcs = NullSTPInstancePerformanceCounters.Instance;
            this._stpStartInfo = new STPStartInfo(stpStartInfo);
            this.Initialize();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="idleTimeout">Idle timeout in milliseconds</param>
        public SmartThreadPool(int idleTimeout)
        {
            this._name = "SmartThreadPool";
            this._workerThreads = Hashtable.Synchronized(new Hashtable());
            this._workItemsQueue = new WorkItemsQueue();
            this._stpStartInfo = new STPStartInfo();
            this._isIdleWaitHandle = new ManualResetEvent(true);
            this._shuttingDownEvent = new ManualResetEvent(false);
            this._workItemsGroups = Hashtable.Synchronized(new Hashtable());
            this._pcs = NullSTPInstancePerformanceCounters.Instance;
            this._stpStartInfo.IdleTimeout = idleTimeout;
            this.Initialize();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="idleTimeout">Idle timeout in milliseconds</param>
        /// <param name="maxWorkerThreads">Upper limit of threads in the pool</param>
        public SmartThreadPool(int idleTimeout, int maxWorkerThreads)
        {
            this._name = "SmartThreadPool";
            this._workerThreads = Hashtable.Synchronized(new Hashtable());
            this._workItemsQueue = new WorkItemsQueue();
            this._stpStartInfo = new STPStartInfo();
            this._isIdleWaitHandle = new ManualResetEvent(true);
            this._shuttingDownEvent = new ManualResetEvent(false);
            this._workItemsGroups = Hashtable.Synchronized(new Hashtable());
            this._pcs = NullSTPInstancePerformanceCounters.Instance;
            this._stpStartInfo.IdleTimeout = idleTimeout;
            this._stpStartInfo.MaxWorkerThreads = maxWorkerThreads;
            this.Initialize();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="idleTimeout">Idle timeout in milliseconds</param>
        /// <param name="maxWorkerThreads">Upper limit of threads in the pool</param>
        /// <param name="minWorkerThreads">Lower limit of threads in the pool</param>
        public SmartThreadPool(int idleTimeout, int maxWorkerThreads, int minWorkerThreads)
        {
            this._name = "SmartThreadPool";
            this._workerThreads = Hashtable.Synchronized(new Hashtable());
            this._workItemsQueue = new WorkItemsQueue();
            this._stpStartInfo = new STPStartInfo();
            this._isIdleWaitHandle = new ManualResetEvent(true);
            this._shuttingDownEvent = new ManualResetEvent(false);
            this._workItemsGroups = Hashtable.Synchronized(new Hashtable());
            this._pcs = NullSTPInstancePerformanceCounters.Instance;
            this._stpStartInfo.IdleTimeout = idleTimeout;
            this._stpStartInfo.MaxWorkerThreads = maxWorkerThreads;
            this._stpStartInfo.MinWorkerThreads = minWorkerThreads;
            this.Initialize();
        }

        public void Cancel()
        {
            foreach (WorkItemsGroup group in this._workItemsGroups.Values)
            {
                group.Cancel();
            }
        }

        public IWorkItemsGroup CreateWorkItemsGroup(int concurrency)
        {
            return new WorkItemsGroup(this, concurrency, this._stpStartInfo);
        }

        public IWorkItemsGroup CreateWorkItemsGroup(int concurrency, WIGStartInfo wigStartInfo)
        {
            return new WorkItemsGroup(this, concurrency, wigStartInfo);
        }

        private void DecrementWorkItemsCount()
        {
            this._workItemsProcessed += 1L;
            this._pcs.SampleWorkItems((long) this._workItemsQueue.Count, this._workItemsProcessed);
            if (Interlocked.Decrement(ref this._currentWorkItemsCount) == 0)
            {
                this._isIdleWaitHandle.Set();
            }
        }

        /// <summary>
        /// Waits on the queue for a work item, shutdown, or timeout.
        /// </summary>
        /// <returns>
        /// Returns the WaitingCallback or null in case of timeout or shutdown.
        /// </returns>
        private WorkItem Dequeue()
        {
            return this._workItemsQueue.DequeueWorkItem(this._stpStartInfo.IdleTimeout, this._shuttingDownEvent);
        }

        public void Dispose()
        {
            if (!this._isDisposed)
            {
                if (!this._shutdown)
                {
                    this.Shutdown();
                }
                if (this._shuttingDownEvent != null)
                {
                    this._shuttingDownEvent.Close();
                    this._shuttingDownEvent = null;
                }
                this._workerThreads.Clear();
                this._isDisposed = true;
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Put a new work item in the queue
        /// </summary>
        /// <param name="workItem">A work item to queue</param>
        private void Enqueue(WorkItem workItem)
        {
            this.Enqueue(workItem, true);
        }

        /// <summary>
        /// Put a new work item in the queue
        /// </summary>
        /// <param name="workItem">A work item to queue</param>
        /// <param name="incrementWorkItems">if set to <c>true</c> increment work items.</param>
        internal void Enqueue(WorkItem workItem, bool incrementWorkItems)
        {
            if (incrementWorkItems)
            {
                this.IncrementWorkItemsCount();
            }
            this._workItemsQueue.EnqueueWorkItem(workItem);
            workItem.WorkItemIsQueued();
            if ((this.InUseThreads + this.WaitingCallbacks) > this._workerThreads.Count)
            {
                this.StartThreads(1);
            }
        }

        private void ExecuteWorkItem(WorkItem workItem)
        {
            this._pcs.SampleWorkItemsWaitTime(workItem.WaitingTime);
            try
            {
                workItem.Execute();
            }
            catch
            {
                throw;
            }
            finally
            {
                this._pcs.SampleWorkItemsProcessTime(workItem.ProcessTime);
            }
        }

        ~SmartThreadPool()
        {
            this.Dispose();
        }

        private void IncrementWorkItemsCount()
        {
            this._pcs.SampleWorkItems((long) this._workItemsQueue.Count, this._workItemsProcessed);
            if (Interlocked.Increment(ref this._currentWorkItemsCount) == 1)
            {
                this._isIdleWaitHandle.Reset();
            }
        }

        /// <summary>
        /// Inform that the current thread is about to quit or quiting.
        /// The same thread may call this method more than once.
        /// </summary>
        private void InformCompleted()
        {
            if (this._workerThreads.Contains(Thread.CurrentThread))
            {
                this._workerThreads.Remove(Thread.CurrentThread);
                this._pcs.SampleThreads((long) this._workerThreads.Count, (long) this._inUseWorkerThreads);
            }
        }

        private void Initialize()
        {
            this.ValidateSTPStartInfo();
            if (this._stpStartInfo.PerformanceCounterInstanceName != null)
            {
                try
                {
                    this._pcs = new STPInstancePerformanceCounters(this._stpStartInfo.PerformanceCounterInstanceName);
                }
                catch (Exception)
                {
                    this._pcs = NullSTPInstancePerformanceCounters.Instance;
                }
            }
            this.StartOptimalNumberOfThreads();
        }

        /// <summary>
        /// A worker thread method that processes work items from the work items queue.
        /// </summary>
        private void ProcessQueuedItems()
        {
            _smartThreadPool = this;
            try
            {
                bool flag = false;
                while (!this._shutdown)
                {
                    this._workerThreads[Thread.CurrentThread] = DateTime.Now;
                    WorkItem workItem = this.Dequeue();
                    this._workerThreads[Thread.CurrentThread] = DateTime.Now;
                    if ((workItem == null) && (this._workerThreads.Count > this._stpStartInfo.MinWorkerThreads))
                    {
                        lock (this._workerThreads.SyncRoot)
                        {
                            if (this._workerThreads.Count > this._stpStartInfo.MinWorkerThreads)
                            {
                                this.InformCompleted();
                                return;
                            }
                        }
                    }
                    if (workItem != null)
                    {
                        try
                        {
                            try
                            {
                                flag = false;
                                if (workItem.StartingWorkItem())
                                {
                                    int num = Interlocked.Increment(ref this._inUseWorkerThreads);
                                    this._pcs.SampleThreads((long) this._workerThreads.Count, (long) num);
                                    flag = true;
                                    _currentWorkItem = workItem;
                                    this.ExecuteWorkItem(workItem);
                                }
                            }
                            catch (Exception exception)
                            {
                                exception.GetHashCode();
                            }
                            continue;
                        }
                        finally
                        {
                            if (workItem != null)
                            {
                                workItem.DisposeOfState();
                            }
                            _currentWorkItem = null;
                            if (flag)
                            {
                                int num2 = Interlocked.Decrement(ref this._inUseWorkerThreads);
                                this._pcs.SampleThreads((long) this._workerThreads.Count, (long) num2);
                            }
                            workItem.FireWorkItemCompleted();
                            this.DecrementWorkItemsCount();
                        }
                    }
                }
            }
            catch (ThreadAbortException exception2)
            {
                exception2.GetHashCode();
                Thread.ResetAbort();
            }
            catch (Exception)
            {
            }
            finally
            {
                this.InformCompleted();
            }
        }

        /// <summary>
        /// Queue a work item
        /// </summary>
        /// <param name="callback">A callback to execute</param>
        /// <returns>Returns a work item result</returns>
        public IWorkItemResult QueueWorkItem(WorkItemCallback callback)
        {
            this.ValidateNotDisposed();
            this.ValidateCallback(callback);
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, this._stpStartInfo, callback);
            this.Enqueue(workItem);
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
            this.ValidateNotDisposed();
            this.ValidateCallback(callback);
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, this._stpStartInfo, callback, workItemPriority);
            this.Enqueue(workItem);
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
            this.ValidateNotDisposed();
            this.ValidateCallback(callback);
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, this._stpStartInfo, callback, state);
            this.Enqueue(workItem);
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
            this.ValidateNotDisposed();
            this.ValidateCallback(callback);
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, this._stpStartInfo, workItemInfo, callback);
            this.Enqueue(workItem);
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
            this.ValidateNotDisposed();
            this.ValidateCallback(callback);
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, this._stpStartInfo, callback, state, postExecuteWorkItemCallback);
            this.Enqueue(workItem);
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
            this.ValidateNotDisposed();
            this.ValidateCallback(callback);
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, this._stpStartInfo, callback, state, workItemPriority);
            this.Enqueue(workItem);
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
            this.ValidateNotDisposed();
            this.ValidateCallback(callback);
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, this._stpStartInfo, workItemInfo, callback, state);
            this.Enqueue(workItem);
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
            this.ValidateNotDisposed();
            this.ValidateCallback(callback);
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, this._stpStartInfo, callback, state, postExecuteWorkItemCallback, callToPostExecute);
            this.Enqueue(workItem);
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
            this.ValidateNotDisposed();
            this.ValidateCallback(callback);
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, this._stpStartInfo, callback, state, postExecuteWorkItemCallback, workItemPriority);
            this.Enqueue(workItem);
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
            this.ValidateNotDisposed();
            this.ValidateCallback(callback);
            WorkItem workItem = WorkItemFactory.CreateWorkItem(this, this._stpStartInfo, callback, state, postExecuteWorkItemCallback, callToPostExecute, workItemPriority);
            this.Enqueue(workItem);
            return workItem.GetWorkItemResult();
        }

        internal void RegisterWorkItemsGroup(IWorkItemsGroup workItemsGroup)
        {
            this._workItemsGroups[workItemsGroup] = workItemsGroup;
        }

        /// <summary>
        /// Force the SmartThreadPool to shutdown
        /// </summary>
        public void Shutdown()
        {
            this.Shutdown(true, 0);
        }

        /// <summary>
        /// Empties the queue of work items and abort the threads in the pool.
        /// </summary>
        public void Shutdown(bool forceAbort, int millisecondsTimeout)
        {
            this.ValidateNotDisposed();
            ISTPInstancePerformanceCounters counters = this._pcs;
            if (NullSTPInstancePerformanceCounters.Instance != this._pcs)
            {
                this._pcs.Dispose();
                this._pcs = NullSTPInstancePerformanceCounters.Instance;
            }
            Thread[] array = null;
            lock (this._workerThreads.SyncRoot)
            {
                this._workItemsQueue.Dispose();
                this._shutdown = true;
                this._shuttingDownEvent.Set();
                array = new Thread[this._workerThreads.Count];
                this._workerThreads.Keys.CopyTo(array, 0);
            }
            int num = millisecondsTimeout;
            DateTime now = DateTime.Now;
            bool flag = -1 == millisecondsTimeout;
            bool flag2 = false;
            foreach (Thread thread in array)
            {
                if (!flag && (num < 0))
                {
                    flag2 = true;
                    break;
                }
                if (!thread.Join(num))
                {
                    flag2 = true;
                    break;
                }
                if (!flag)
                {
                    TimeSpan span = (TimeSpan) (DateTime.Now - now);
                    num = millisecondsTimeout - ((int) span.TotalMilliseconds);
                }
            }
            if (flag2 && forceAbort)
            {
                foreach (Thread thread2 in array)
                {
                    if ((thread2 != null) && thread2.IsAlive)
                    {
                        try
                        {
                            thread2.Abort("Shutdown");
                        }
                        catch (SecurityException exception)
                        {
                            exception.GetHashCode();
                        }
                        catch (ThreadStateException exception2)
                        {
                            exception2.GetHashCode();
                        }
                    }
                }
            }
            counters.Dispose();
        }

        public void Shutdown(bool forceAbort, TimeSpan timeout)
        {
            this.Shutdown(forceAbort, (int) timeout.TotalMilliseconds);
        }

        public void Start()
        {
            lock (this)
            {
                if (!this._stpStartInfo.StartSuspended)
                {
                    return;
                }
                this._stpStartInfo.StartSuspended = false;
            }
            foreach (WorkItemsGroup group in this._workItemsGroups.Values)
            {
                group.OnSTPIsStarting();
            }
            this.StartOptimalNumberOfThreads();
        }

        private void StartOptimalNumberOfThreads()
        {
            int threadsCount = Math.Min(Math.Max(this._workItemsQueue.Count, this._stpStartInfo.MinWorkerThreads), this._stpStartInfo.MaxWorkerThreads);
            this.StartThreads(threadsCount);
        }

        /// <summary>
        /// Starts new threads
        /// </summary>
        /// <param name="threadsCount">The number of threads to start</param>
        private void StartThreads(int threadsCount)
        {
            if (!this._stpStartInfo.StartSuspended)
            {
                lock (this._workerThreads.SyncRoot)
                {
                    if (!this._shutdown)
                    {
                        for (int i = 0; i < threadsCount; i++)
                        {
                            if (this._workerThreads.Count >= this._stpStartInfo.MaxWorkerThreads)
                            {
                                break;
                            }
                            Thread thread = new Thread(new ThreadStart(this.ProcessQueuedItems)) {
                                Name = string.Concat(new object[] { "STP ", this.Name, " Thread #", this._threadCounter }),
                                IsBackground = true,
                                Priority = this._stpStartInfo.ThreadPriority
                            };
                            thread.Start();
                            this._threadCounter++;
                            this._workerThreads[thread] = DateTime.Now;
                            this._pcs.SampleThreads((long) this._workerThreads.Count, (long) this._inUseWorkerThreads);
                        }
                    }
                }
            }
        }

        internal void UnregisterWorkItemsGroup(IWorkItemsGroup workItemsGroup)
        {
            if (this._workItemsGroups.Contains(workItemsGroup))
            {
                this._workItemsGroups.Remove(workItemsGroup);
            }
        }

        private void ValidateCallback(Delegate callback)
        {
            if (callback.GetInvocationList().Length > 1)
            {
                throw new NotSupportedException("SmartThreadPool doesn't support delegates chains");
            }
        }

        private void ValidateNotDisposed()
        {
            if (this._isDisposed)
            {
                throw new ObjectDisposedException(base.GetType().ToString(), "The SmartThreadPool has been shutdown");
            }
        }

        private void ValidateSTPStartInfo()
        {
            if (this._stpStartInfo.MinWorkerThreads < 0)
            {
                throw new ArgumentOutOfRangeException("MinWorkerThreads", "MinWorkerThreads cannot be negative");
            }
            if (this._stpStartInfo.MaxWorkerThreads <= 0)
            {
                throw new ArgumentOutOfRangeException("MaxWorkerThreads", "MaxWorkerThreads must be greater than zero");
            }
            if (this._stpStartInfo.MinWorkerThreads > this._stpStartInfo.MaxWorkerThreads)
            {
                throw new ArgumentOutOfRangeException("MinWorkerThreads, maxWorkerThreads", "MaxWorkerThreads must be greater or equal to MinWorkerThreads");
            }
        }

        private void ValidateWaitForIdle()
        {
            if (_smartThreadPool == this)
            {
                throw new NotSupportedException("WaitForIdle cannot be called from a thread on its SmartThreadPool, it will cause may cause a deadlock");
            }
        }

        internal void ValidateWorkItemsGroupWaitForIdle(IWorkItemsGroup workItemsGroup)
        {
            this.ValidateWorkItemsGroupWaitForIdleImpl(workItemsGroup, _currentWorkItem);
            if (((workItemsGroup != null) && (_currentWorkItem != null)) && _currentWorkItem.WasQueuedBy(workItemsGroup))
            {
                throw new NotSupportedException("WaitForIdle cannot be called from a thread on its SmartThreadPool, it will cause may cause a deadlock");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ValidateWorkItemsGroupWaitForIdleImpl(IWorkItemsGroup workItemsGroup, WorkItem workItem)
        {
            if (((workItemsGroup != null) && (workItem != null)) && workItem.WasQueuedBy(workItemsGroup))
            {
                throw new NotSupportedException("WaitForIdle cannot be called from a thread on its SmartThreadPool, it will cause may cause a deadlock");
            }
        }

        /// <summary>
        /// Wait for all work items to complete
        /// </summary>
        /// <param name="workItemResults">Array of work item result objects</param>
        /// <returns>
        /// true when every work item in workItemResults has completed; otherwise false.
        /// </returns>
        public static bool WaitAll(IWorkItemResult[] workItemResults)
        {
            return WaitAll(workItemResults, -1, true);
        }

        /// <summary>
        /// Wait for all work items to complete
        /// </summary>
        /// <param name="workItemResults">Array of work item result objects</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or Timeout.Infinite (-1) to wait indefinitely.</param>
        /// <param name="exitContext">
        /// true to exit the synchronization domain for the context before the wait (if in a synchronized context), and reacquire it; otherwise, false. 
        /// </param>
        /// <returns>
        /// true when every work item in workItemResults has completed; otherwise false.
        /// </returns>
        public static bool WaitAll(IWorkItemResult[] workItemResults, int millisecondsTimeout, bool exitContext)
        {
            return WorkItem.WaitAll(workItemResults, millisecondsTimeout, exitContext, null);
        }

        /// <summary>
        /// Wait for all work items to complete
        /// </summary>
        /// <param name="workItemResults">Array of work item result objects</param>
        /// <param name="timeout">The number of milliseconds to wait, or a TimeSpan that represents -1 milliseconds to wait indefinitely. </param>
        /// <param name="exitContext">
        /// true to exit the synchronization domain for the context before the wait (if in a synchronized context), and reacquire it; otherwise, false. 
        /// </param>
        /// <returns>
        /// true when every work item in workItemResults has completed; otherwise false.
        /// </returns>
        public static bool WaitAll(IWorkItemResult[] workItemResults, TimeSpan timeout, bool exitContext)
        {
            return WaitAll(workItemResults, (int) timeout.TotalMilliseconds, exitContext);
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
        public static bool WaitAll(IWorkItemResult[] workItemResults, int millisecondsTimeout, bool exitContext, WaitHandle cancelWaitHandle)
        {
            return WorkItem.WaitAll(workItemResults, millisecondsTimeout, exitContext, cancelWaitHandle);
        }

        /// <summary>
        /// Wait for all work items to complete
        /// </summary>
        /// <param name="workItemResults">Array of work item result objects</param>
        /// <param name="timeout">The number of milliseconds to wait, or a TimeSpan that represents -1 milliseconds to wait indefinitely. </param>
        /// <param name="exitContext">
        /// true to exit the synchronization domain for the context before the wait (if in a synchronized context), and reacquire it; otherwise, false. 
        /// </param>
        /// <param name="cancelWaitHandle">A cancel wait handle to interrupt the wait if needed</param>
        /// <returns>
        /// true when every work item in workItemResults has completed; otherwise false.
        /// </returns>
        public static bool WaitAll(IWorkItemResult[] workItemResults, TimeSpan timeout, bool exitContext, WaitHandle cancelWaitHandle)
        {
            return WaitAll(workItemResults, (int) timeout.TotalMilliseconds, exitContext, cancelWaitHandle);
        }

        /// <summary>
        /// Waits for any of the work items in the specified array to complete, cancel, or timeout
        /// </summary>
        /// <param name="workItemResults">Array of work item result objects</param>
        /// <returns>
        /// The array index of the work item result that satisfied the wait, or WaitTimeout if any of the work items has been canceled.
        /// </returns>
        public static int WaitAny(IWorkItemResult[] workItemResults)
        {
            return WaitAny(workItemResults, -1, true);
        }

        /// <summary>
        /// Waits for any of the work items in the specified array to complete, cancel, or timeout
        /// </summary>
        /// <param name="workItemResults">Array of work item result objects</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or Timeout.Infinite (-1) to wait indefinitely.</param>
        /// <param name="exitContext">
        /// true to exit the synchronization domain for the context before the wait (if in a synchronized context), and reacquire it; otherwise, false. 
        /// </param>
        /// <returns>
        /// The array index of the work item result that satisfied the wait, or WaitTimeout if no work item result satisfied the wait and a time interval equivalent to millisecondsTimeout has passed or the work item has been canceled.
        /// </returns>
        public static int WaitAny(IWorkItemResult[] workItemResults, int millisecondsTimeout, bool exitContext)
        {
            return WorkItem.WaitAny(workItemResults, millisecondsTimeout, exitContext, null);
        }

        /// <summary>
        /// Waits for any of the work items in the specified array to complete, cancel, or timeout
        /// </summary>
        /// <param name="workItemResults">Array of work item result objects</param>
        /// <param name="timeout">The number of milliseconds to wait, or a TimeSpan that represents -1 milliseconds to wait indefinitely. </param>
        /// <param name="exitContext">
        /// true to exit the synchronization domain for the context before the wait (if in a synchronized context), and reacquire it; otherwise, false. 
        /// </param>
        /// <returns>
        /// The array index of the work item result that satisfied the wait, or WaitTimeout if no work item result satisfied the wait and a time interval equivalent to millisecondsTimeout has passed or the work item has been canceled.
        /// </returns>
        public static int WaitAny(IWorkItemResult[] workItemResults, TimeSpan timeout, bool exitContext)
        {
            return WaitAny(workItemResults, (int) timeout.TotalMilliseconds, exitContext);
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
        public static int WaitAny(IWorkItemResult[] workItemResults, int millisecondsTimeout, bool exitContext, WaitHandle cancelWaitHandle)
        {
            return WorkItem.WaitAny(workItemResults, millisecondsTimeout, exitContext, cancelWaitHandle);
        }

        /// <summary>
        /// Waits for any of the work items in the specified array to complete, cancel, or timeout
        /// </summary>
        /// <param name="workItemResults">Array of work item result objects</param>
        /// <param name="timeout">The number of milliseconds to wait, or a TimeSpan that represents -1 milliseconds to wait indefinitely. </param>
        /// <param name="exitContext">
        /// true to exit the synchronization domain for the context before the wait (if in a synchronized context), and reacquire it; otherwise, false. 
        /// </param>
        /// <param name="cancelWaitHandle">A cancel wait handle to interrupt the wait if needed</param>
        /// <returns>
        /// The array index of the work item result that satisfied the wait, or WaitTimeout if no work item result satisfied the wait and a time interval equivalent to millisecondsTimeout has passed or the work item has been canceled.
        /// </returns>
        public static int WaitAny(IWorkItemResult[] workItemResults, TimeSpan timeout, bool exitContext, WaitHandle cancelWaitHandle)
        {
            return WaitAny(workItemResults, (int) timeout.TotalMilliseconds, exitContext, cancelWaitHandle);
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
            this.ValidateWaitForIdle();
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
        /// Get the number of threads in the thread pool.
        /// Should be between the lower and the upper limits.
        /// </summary>
        public int ActiveThreads
        {
            get
            {
                this.ValidateNotDisposed();
                return this._workerThreads.Count;
            }
        }

        /// <summary>
        /// Get the number of busy (not idle) threads in the thread pool.
        /// </summary>
        public int InUseThreads
        {
            get
            {
                this.ValidateNotDisposed();
                return this._inUseWorkerThreads;
            }
        }

        /// <summary>
        /// Get the upper limit of threads in the pool.
        /// </summary>
        public int MaxThreads
        {
            get
            {
                this.ValidateNotDisposed();
                return this._stpStartInfo.MaxWorkerThreads;
            }
        }

        /// <summary>
        /// Get the lower limit of threads in the pool.
        /// </summary>
        public int MinThreads
        {
            get
            {
                this.ValidateNotDisposed();
                return this._stpStartInfo.MinWorkerThreads;
            }
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

        /// <summary>
        /// Get the number of work items in the queue.
        /// </summary>
        public int WaitingCallbacks
        {
            get
            {
                this.ValidateNotDisposed();
                return this._workItemsQueue.Count;
            }
        }
    }
}

