using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Threading.Tasks.Schedulers
{
    /// <summary>
    /// Provides a TaskScheduler that provides control over priorities, fairness, and the underlying threads utilized.
    /// </summary>
    [DebuggerDisplay("Id={Id}, Queues={DebugQueueCount}, ScheduledTasks = {DebugTaskCount}"), DebuggerTypeProxy(typeof(QueuedTaskScheduler.QueuedTaskSchedulerDebugView))]
    public sealed class QueuedTaskScheduler : TaskScheduler, IDisposable
    {
        /// <summary>The collection of tasks to be executed on our custom threads.</summary>
        private readonly BlockingCollection<Task> _blockingTaskQueue;
        /// <summary>
        /// The maximum allowed concurrency level of this scheduler.  If custom threads are
        /// used, this represents the number of created threads.
        /// </summary>
        private readonly int _concurrencyLevel;
        /// <summary>The number of Tasks that have been queued or that are running whiel using an underlying scheduler.</summary>
        private int _delegatesQueuedOrRunning;
        /// <summary>Cancellation token used for disposal.</summary>
        private readonly CancellationTokenSource _disposeCancellation;
        /// <summary>The queue of tasks to process when using an underlying target scheduler.</summary>
        private readonly Queue<Task> _nonthreadsafeTaskQueue;
        /// <summary>
        /// A sorted list of round-robin queue lists.  Tasks with the smallest priority value
        /// are preferred.  Priority groups are round-robin'd through in order of priority.
        /// </summary>
        private readonly SortedList<int, QueueGroup> _queueGroups;
        /// <summary>The scheduler onto which actual work is scheduled.</summary>
        private readonly TaskScheduler _targetScheduler;
        /// <summary>Whether we're processing tasks on the current thread.</summary>
        private static ThreadLocal<bool> _taskProcessingThread = new ThreadLocal<bool>();
        /// <summary>The threads used by the scheduler to process work.</summary>
        private readonly Thread[] _threads;

        /// <summary>Initializes the scheduler.</summary>
        public QueuedTaskScheduler() : this(TaskScheduler.Default, 0)
        {
        }

        /// <summary>Initializes the scheduler.</summary>
        /// <param name="threadCount">The number of threads to create and use for processing work items.</param>
        public QueuedTaskScheduler(int threadCount) : this(threadCount, string.Empty, false, ThreadPriority.Normal, ApartmentState.MTA, 0, null, null)
        {
        }

        /// <summary>Initializes the scheduler.</summary>
        /// <param name="targetScheduler">The target underlying scheduler onto which this sceduler's work is queued.</param>
        public QueuedTaskScheduler(TaskScheduler targetScheduler) : this(targetScheduler, 0)
        {
        }

        /// <summary>Initializes the scheduler.</summary>
        /// <param name="targetScheduler">The target underlying scheduler onto which this sceduler's work is queued.</param>
        /// <param name="maxConcurrencyLevel">The maximum degree of concurrency allowed for this scheduler's work.</param>
        public QueuedTaskScheduler(TaskScheduler targetScheduler, int maxConcurrencyLevel)
        {
            this._queueGroups = new SortedList<int, QueueGroup>();
            this._disposeCancellation = new CancellationTokenSource();
            if (targetScheduler == null)
            {
                throw new ArgumentNullException("underlyingScheduler");
            }
            if (maxConcurrencyLevel < 0)
            {
                throw new ArgumentOutOfRangeException("concurrencyLevel");
            }
            this._targetScheduler = targetScheduler;
            this._nonthreadsafeTaskQueue = new Queue<Task>();
            this._concurrencyLevel = (maxConcurrencyLevel != 0) ? maxConcurrencyLevel : Environment.ProcessorCount;
            if ((targetScheduler.MaximumConcurrencyLevel > 0) && (targetScheduler.MaximumConcurrencyLevel < this._concurrencyLevel))
            {
                this._concurrencyLevel = targetScheduler.MaximumConcurrencyLevel;
            }
        }

        /// <summary>Initializes the scheduler.</summary>
        /// <param name="threadCount">The number of threads to create and use for processing work items.</param>
        /// <param name="threadName">The name to use for each of the created threads.</param>
        /// <param name="useForegroundThreads">A Boolean value that indicates whether to use foreground threads instead of background.</param>
        /// <param name="threadPriority">The priority to assign to each thread.</param>
        /// <param name="threadApartmentState">The apartment state to use for each thread.</param>
        /// <param name="threadMaxStackSize">The stack size to use for each thread.</param>
        /// <param name="threadInit">An initialization routine to run on each thread.</param>
        /// <param name="threadFinally">A finalization routine to run on each thread.</param>
        public QueuedTaskScheduler(int threadCount, string threadName = "", bool useForegroundThreads = false, ThreadPriority threadPriority = ThreadPriority.Normal, ApartmentState threadApartmentState = ApartmentState.STA, int threadMaxStackSize = 0, Action threadInit = null, Action threadFinally = null)
        {
            ThreadStart start = null;
            this._queueGroups = new SortedList<int, QueueGroup>();
            this._disposeCancellation = new CancellationTokenSource();
            if (threadCount < 0)
            {
                throw new ArgumentOutOfRangeException("concurrencyLevel");
            }
            if (threadCount == 0)
            {
                this._concurrencyLevel = Environment.ProcessorCount;
            }
            else
            {
                this._concurrencyLevel = threadCount;
            }
            this._blockingTaskQueue = new BlockingCollection<Task>();
            this._threads = new Thread[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                if (start == null)
                {
                    start = () => this.ThreadBasedDispatchLoop(threadInit, threadFinally);
                }
                this._threads[i] = new Thread(start, threadMaxStackSize) { Priority = threadPriority, IsBackground = !useForegroundThreads };
                if (threadName != null)
                {
                    this._threads[i].Name = string.Concat(new object[] { threadName, " (", i, ")" });
                }
                this._threads[i].SetApartmentState(threadApartmentState);
            }
            foreach (Thread thread2 in this._threads)
            {
                thread2.Start();
            }
        }

        /// <summary>Creates and activates a new scheduling queue for this scheduler.</summary>
        /// <returns>The newly created and activated queue at priority 0.</returns>
        public TaskScheduler ActivateNewQueue()
        {
            return this.ActivateNewQueue(0);
        }

        /// <summary>Creates and activates a new scheduling queue for this scheduler.</summary>
        /// <param name="priority">The priority level for the new queue.</param>
        /// <returns>The newly created and activated queue at the specified priority.</returns>
        public TaskScheduler ActivateNewQueue(int priority)
        {
            QueuedTaskSchedulerQueue item = new QueuedTaskSchedulerQueue(priority, this);
            lock (this._queueGroups)
            {
                QueueGroup group;
                if (!this._queueGroups.TryGetValue(priority, out group))
                {
                    group = new QueueGroup();
                    this._queueGroups.Add(priority, group);
                }
                group.Add(item);
            }
            return item;
        }

        /// <summary>Initiates shutdown of the scheduler.</summary>
        public void Dispose()
        {
            this._disposeCancellation.Cancel();
        }

        /// <summary>Find the next task that should be executed, based on priorities and fairness and the like.</summary>
        /// <param name="targetTask">The found task, or null if none was found.</param>
        /// <param name="queueForTargetTask">
        /// The scheduler associated with the found task.  Due to security checks inside of TPL,  
        /// this scheduler needs to be used to execute that task.
        /// </param>
        private void FindNextTask_NeedsLock(out Task targetTask, out QueuedTaskSchedulerQueue queueForTargetTask)
        {
            targetTask = null;
            queueForTargetTask = null;
            foreach (KeyValuePair<int, QueueGroup> pair in this._queueGroups)
            {
                QueueGroup group = pair.Value;
                foreach (int num in group.CreateSearchOrder())
                {
                    queueForTargetTask = group[num];
                    Queue<Task> queue = queueForTargetTask._workItems;
                    if (queue.Count > 0)
                    {
                        targetTask = queue.Dequeue();
                        if (queueForTargetTask._disposed && (queue.Count == 0))
                        {
                            this.RemoveQueue_NeedsLock(queueForTargetTask);
                        }
                        group.NextQueueIndex = (group.NextQueueIndex + 1) % pair.Value.Count;
                        break;
                    }
                }
            }
        }

        /// <summary>Gets the tasks scheduled to this scheduler.</summary>
        /// <returns>An enumerable of all tasks queued to this scheduler.</returns>
        /// <remarks>This does not include the tasks on sub-schedulers.  Those will be retrieved by the debugger separately.</remarks>
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            if (this._targetScheduler == null)
            {
                return (from t in this._blockingTaskQueue
                    where t != null
                    select t).ToList<Task>();
            }
            return (from t in this._nonthreadsafeTaskQueue
                where t != null
                select t).ToList<Task>();
        }

        /// <summary>Notifies the pool that there's a new item to be executed in one of the round-robin queues.</summary>
        private void NotifyNewWorkItem()
        {
            this.QueueTask(null);
        }

        /// <summary>
        /// Process tasks one at a time in the best order.  
        /// This should be run in a Task generated by QueueTask.
        /// It's been separated out into its own method to show up better in Parallel Tasks.
        /// </summary>
        private void ProcessPrioritizedAndBatchedTasks()
        {
            bool flag = true;
            while (!this._disposeCancellation.IsCancellationRequested && flag)
            {
                try
                {
                    _taskProcessingThread.Value = true;
                    while (!this._disposeCancellation.IsCancellationRequested)
                    {
                        Task task;
                        lock (this._nonthreadsafeTaskQueue)
                        {
                            if (this._nonthreadsafeTaskQueue.Count == 0)
                            {
                                break;
                            }
                            task = this._nonthreadsafeTaskQueue.Dequeue();
                        }
                        QueuedTaskSchedulerQueue queueForTargetTask = null;
                        if (task == null)
                        {
                            lock (this._queueGroups)
                            {
                                this.FindNextTask_NeedsLock(out task, out queueForTargetTask);
                            }
                        }
                        if (task != null)
                        {
                            if (queueForTargetTask != null)
                            {
                                queueForTargetTask.ExecuteTask(task);
                            }
                            else
                            {
                                base.TryExecuteTask(task);
                            }
                        }
                    }
                    continue;
                }
                finally
                {
                    lock (this._nonthreadsafeTaskQueue)
                    {
                        if (this._nonthreadsafeTaskQueue.Count == 0)
                        {
                            this._delegatesQueuedOrRunning--;
                            flag = false;
                            _taskProcessingThread.Value = false;
                        }
                    }
                }
            }
        }

        /// <summary>Queues a task to the scheduler.</summary>
        /// <param name="task">The task to be queued.</param>
        protected override void QueueTask(Task task)
        {
            if (this._disposeCancellation.IsCancellationRequested)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            if (this._targetScheduler == null)
            {
                this._blockingTaskQueue.Add(task);
            }
            else
            {
                bool flag = false;
                lock (this._nonthreadsafeTaskQueue)
                {
                    this._nonthreadsafeTaskQueue.Enqueue(task);
                    if (this._delegatesQueuedOrRunning < this._concurrencyLevel)
                    {
                        this._delegatesQueuedOrRunning++;
                        flag = true;
                    }
                }
                if (flag)
                {
                    Task.Factory.StartNew(new Action(this.ProcessPrioritizedAndBatchedTasks), CancellationToken.None, TaskCreationOptions.None, this._targetScheduler);
                }
            }
        }

        /// <summary>Removes a scheduler from the group.</summary>
        /// <param name="queue">The scheduler to be removed.</param>
        private void RemoveQueue_NeedsLock(QueuedTaskSchedulerQueue queue)
        {
            QueueGroup group = this._queueGroups[queue._priority];
            int index = group.IndexOf(queue);
            if (group.NextQueueIndex >= index)
            {
                group.NextQueueIndex--;
            }
            group.RemoveAt(index);
        }

        /// <summary>The dispatch loop run by all threads in this scheduler.</summary>
        /// <param name="threadInit">An initialization routine to run when the thread begins.</param>
        /// <param name="threadFinally">A finalization routine to run before the thread ends.</param>
        private void ThreadBasedDispatchLoop(Action threadInit, Action threadFinally)
        {
            _taskProcessingThread.Value = true;
            if (threadInit != null)
            {
                threadInit();
            }
            try
            {
            Label_0014:
                try
                {
                    foreach (Task task in this._blockingTaskQueue.GetConsumingEnumerable(this._disposeCancellation.Token))
                    {
                        if (task != null)
                        {
                            base.TryExecuteTask(task);
                        }
                        else
                        {
                            Task task2;
                            QueuedTaskSchedulerQueue queue;
                            lock (this._queueGroups)
                            {
                                this.FindNextTask_NeedsLock(out task2, out queue);
                            }
                            if (task2 != null)
                            {
                                queue.ExecuteTask(task2);
                            }
                        }
                    }
                    goto Label_0014;
                }
                catch (ThreadAbortException)
                {
                    if (!Environment.HasShutdownStarted && !AppDomain.CurrentDomain.IsFinalizingForUnload())
                    {
                        Thread.ResetAbort();
                    }
                    goto Label_0014;
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                if (threadFinally != null)
                {
                    threadFinally();
                }
                _taskProcessingThread.Value = false;
            }
        }

        /// <summary>Tries to execute a task synchronously on the current thread.</summary>
        /// <param name="task">The task to execute.</param>
        /// <param name="taskWasPreviouslyQueued">Whether the task was previously queued.</param>
        /// <returns>true if the task was executed; otherwise, false.</returns>
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return (_taskProcessingThread.Value && base.TryExecuteTask(task));
        }

        /// <summary>Gets the number of queues currently activated.</summary>
        private int DebugQueueCount
        {
            get
            {
                int num = 0;
                foreach (KeyValuePair<int, QueueGroup> pair in this._queueGroups)
                {
                    num += pair.Value.Count;
                }
                return num;
            }
        }

        /// <summary>Gets the number of tasks currently scheduled.</summary>
        private int DebugTaskCount
        {
            get
            {
                return (from t in (this._targetScheduler != null) ? ((IEnumerable<Task>) this._nonthreadsafeTaskQueue) : ((IEnumerable<Task>) this._blockingTaskQueue)
                    where t != null
                    select t).Count<Task>();
            }
        }

        /// <summary>Gets the maximum concurrency level to use when processing tasks.</summary>
        public override int MaximumConcurrencyLevel
        {
            get
            {
                return this._concurrencyLevel;
            }
        }

        /// <summary>Debug view for the QueuedTaskScheduler.</summary>
        private class QueuedTaskSchedulerDebugView
        {
            /// <summary>The scheduler.</summary>
            private QueuedTaskScheduler _scheduler;

            /// <summary>Initializes the debug view.</summary>
            /// <param name="scheduler">The scheduler.</param>
            public QueuedTaskSchedulerDebugView(QueuedTaskScheduler scheduler)
            {
                if (scheduler == null)
                {
                    throw new ArgumentNullException("scheduler");
                }
                this._scheduler = scheduler;
            }

            /// <summary>Gets the prioritized and fair queues.</summary>
            public IEnumerable<TaskScheduler> Queues
            {
                get
                {
                    List<TaskScheduler> list = new List<TaskScheduler>();
                    foreach (KeyValuePair<int, QueuedTaskScheduler.QueueGroup> pair in this._scheduler._queueGroups)
                    {
                        list.AddRange(pair.Value);
                    }
                    return list;
                }
            }

            /// <summary>Gets all of the Tasks queued to the scheduler directly.</summary>
            public IEnumerable<Task> ScheduledTasks
            {
                get
                {
                    IEnumerable<Task> enumerable = (this._scheduler._targetScheduler != null) ? ((IEnumerable<Task>) this._scheduler._nonthreadsafeTaskQueue) : ((IEnumerable<Task>) this._scheduler._blockingTaskQueue);
                    return (from t in enumerable
                        where t != null
                        select t).ToList<Task>();
                }
            }
        }

        /// <summary>Provides a scheduling queue associatd with a QueuedTaskScheduler.</summary>
        [DebuggerTypeProxy(typeof(QueuedTaskScheduler.QueuedTaskSchedulerQueue.QueuedTaskSchedulerQueueDebugView)), DebuggerDisplay("QueuePriority = {_priority}, WaitingTasks = {WaitingTasks}")]
        private sealed class QueuedTaskSchedulerQueue : TaskScheduler, IDisposable
        {
            /// <summary>Whether this queue has been disposed.</summary>
            internal bool _disposed;
            /// <summary>The scheduler with which this pool is associated.</summary>
            private readonly QueuedTaskScheduler _pool;
            /// <summary>Gets the priority for this queue.</summary>
            internal int _priority;
            /// <summary>The work items stored in this queue.</summary>
            internal readonly Queue<Task> _workItems;

            /// <summary>Initializes the queue.</summary>
            /// <param name="priority">The priority associated with this queue.</param>
            /// <param name="pool">The scheduler with which this queue is associated.</param>
            internal QueuedTaskSchedulerQueue(int priority, QueuedTaskScheduler pool)
            {
                this._priority = priority;
                this._pool = pool;
                this._workItems = new Queue<Task>();
            }

            /// <summary>Signals that the queue should be removed from the scheduler as soon as the queue is empty.</summary>
            public void Dispose()
            {
                if (!this._disposed)
                {
                    lock (this._pool._queueGroups)
                    {
                        if (this._workItems.Count == 0)
                        {
                            this._pool.RemoveQueue_NeedsLock(this);
                        }
                    }
                    this._disposed = true;
                }
            }

            /// <summary>Runs the specified ask.</summary>
            /// <param name="task">The task to execute.</param>
            internal void ExecuteTask(Task task)
            {
                base.TryExecuteTask(task);
            }

            /// <summary>Gets the tasks scheduled to this scheduler.</summary>
            /// <returns>An enumerable of all tasks queued to this scheduler.</returns>
            protected override IEnumerable<Task> GetScheduledTasks()
            {
                return this._workItems.ToList<Task>();
            }

            /// <summary>Queues a task to the scheduler.</summary>
            /// <param name="task">The task to be queued.</param>
            protected override void QueueTask(Task task)
            {
                if (this._disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                lock (this._pool._queueGroups)
                {
                    this._workItems.Enqueue(task);
                }
                this._pool.NotifyNewWorkItem();
            }

            /// <summary>Tries to execute a task synchronously on the current thread.</summary>
            /// <param name="task">The task to execute.</param>
            /// <param name="taskWasPreviouslyQueued">Whether the task was previously queued.</param>
            /// <returns>true if the task was executed; otherwise, false.</returns>
            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                return (QueuedTaskScheduler._taskProcessingThread.Value && base.TryExecuteTask(task));
            }

            /// <summary>Gets the maximum concurrency level to use when processing tasks.</summary>
            public override int MaximumConcurrencyLevel
            {
                get
                {
                    return this._pool.MaximumConcurrencyLevel;
                }
            }

            /// <summary>Gets the number of tasks waiting in this scheduler.</summary>
            internal int WaitingTasks
            {
                get
                {
                    return this._workItems.Count;
                }
            }

            /// <summary>A debug view for the queue.</summary>
            private sealed class QueuedTaskSchedulerQueueDebugView
            {
                /// <summary>The queue.</summary>
                private readonly QueuedTaskScheduler.QueuedTaskSchedulerQueue _queue;

                /// <summary>Initializes the debug view.</summary>
                /// <param name="queue">The queue to be debugged.</param>
                public QueuedTaskSchedulerQueueDebugView(QueuedTaskScheduler.QueuedTaskSchedulerQueue queue)
                {
                    if (queue == null)
                    {
                        throw new ArgumentNullException("queue");
                    }
                    this._queue = queue;
                }

                /// <summary>Gets the QueuedTaskScheduler with which this queue is associated.</summary>
                public QueuedTaskScheduler AssociatedScheduler
                {
                    get
                    {
                        return this._queue._pool;
                    }
                }

                /// <summary>Gets the ID of this scheduler.</summary>
                public int Id
                {
                    get
                    {
                        return this._queue.Id;
                    }
                }

                /// <summary>Gets the priority of this queue in its associated scheduler.</summary>
                public int Priority
                {
                    get
                    {
                        return this._queue._priority;
                    }
                }

                /// <summary>Gets all of the tasks scheduled to this queue.</summary>
                public IEnumerable<Task> ScheduledTasks
                {
                    get
                    {
                        return this._queue.GetScheduledTasks();
                    }
                }
            }
        }

        /// <summary>A group of queues a the same priority level.</summary>
        private class QueueGroup : List<QueuedTaskScheduler.QueuedTaskSchedulerQueue>
        {
            /// <summary>The starting index for the next round-robin traversal.</summary>
            public int NextQueueIndex;

            /// <summary>Creates a search order through this group.</summary>
            /// <returns>An enumerable of indices for this group.</returns>
            public IEnumerable<int> CreateSearchOrder()
            {
                int nextQueueIndex = this.NextQueueIndex;
            Label_PostSwitchInIterator:;
                if (nextQueueIndex < this.Count)
                {
                    yield return nextQueueIndex;
                    nextQueueIndex++;
                    goto Label_PostSwitchInIterator;
                }
                for (int i = 0; i < this.NextQueueIndex; i++)
                {
                    yield return i;
                }
            }

        }
    }
}

