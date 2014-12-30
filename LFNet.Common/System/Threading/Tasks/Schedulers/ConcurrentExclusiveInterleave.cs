using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace System.Threading.Tasks.Schedulers
{
    /// <summary>Provides concurrent and exclusive task schedulers that coordinate.</summary>
    [DebuggerDisplay("ConcurrentTasksWaiting={ConcurrentTaskCount}, ExclusiveTasksWaiting={ExclusiveTaskCount}"), DebuggerTypeProxy(typeof(ConcurrentExclusiveInterleave.ConcurrentExclusiveInterleaveDebugView))]
    public sealed class ConcurrentExclusiveInterleave
    {
        /// <summary>The scheduler used to queue and execute "reader" tasks that may run concurrently with other readers.</summary>
        private ConcurrentExclusiveTaskScheduler _concurrentTaskScheduler;
        /// <summary>Whether the exclusive processing of a task should include all of its children as well.</summary>
        private bool _exclusiveProcessingIncludesChildren;
        /// <summary>The scheduler used to queue and execute "writer" tasks that must run exclusively while no other tasks for this interleave are running.</summary>
        private ConcurrentExclusiveTaskScheduler _exclusiveTaskScheduler;
        /// <summary>Synchronizes all activity in this type and its generated schedulers.</summary>
        private readonly object _internalLock;
        /// <summary>The parallel options used by the asynchronous task and parallel loops.</summary>
        private ParallelOptions _parallelOptions;
        /// <summary>Whether this interleave has queued its processing task.</summary>
        private Task _taskExecuting;

        /// <summary>Initialies the ConcurrentExclusiveInterleave.</summary>
        public ConcurrentExclusiveInterleave() : this(TaskScheduler.Current, false)
        {
        }

        /// <summary>Initialies the ConcurrentExclusiveInterleave.</summary>
        /// <param name="exclusiveProcessingIncludesChildren">Whether the exclusive processing of a task should include all of its children as well.</param>
        public ConcurrentExclusiveInterleave(bool exclusiveProcessingIncludesChildren) : this(TaskScheduler.Current, exclusiveProcessingIncludesChildren)
        {
        }

        /// <summary>Initialies the ConcurrentExclusiveInterleave.</summary>
        /// <param name="targetScheduler">The target scheduler on which this interleave should execute.</param>
        public ConcurrentExclusiveInterleave(TaskScheduler targetScheduler) : this(targetScheduler, false)
        {
        }

        /// <summary>Initialies the ConcurrentExclusiveInterleave.</summary>
        /// <param name="targetScheduler">The target scheduler on which this interleave should execute.</param>
        /// <param name="exclusiveProcessingIncludesChildren">Whether the exclusive processing of a task should include all of its children as well.</param>
        public ConcurrentExclusiveInterleave(TaskScheduler targetScheduler, bool exclusiveProcessingIncludesChildren)
        {
            if (targetScheduler == null)
            {
                throw new ArgumentNullException("targetScheduler");
            }
            this._internalLock = new object();
            this._exclusiveProcessingIncludesChildren = exclusiveProcessingIncludesChildren;
            ParallelOptions options = new ParallelOptions {
                TaskScheduler = targetScheduler
            };
            this._parallelOptions = options;
            this._concurrentTaskScheduler = new ConcurrentExclusiveTaskScheduler(this, new Queue<Task>(), targetScheduler.MaximumConcurrencyLevel);
            this._exclusiveTaskScheduler = new ConcurrentExclusiveTaskScheduler(this, new Queue<Task>(), 1);
        }

        /// <summary>The body of the async processor to be run in a Task.  Only one should be running at a time.</summary>
        /// <remarks>This has been separated out into its own method to improve the Parallel Tasks window experience.</remarks>
        private void ConcurrentExclusiveInterleaveProcessor()
        {
            Action<Task> continuationAction = null;
            bool flag = true;
            bool flag2 = true;
            while (flag)
            {
                try
                {
                    foreach (Task task in this.GetExclusiveTasks())
                    {
                        this._exclusiveTaskScheduler.ExecuteTask(task);
                        if (this._exclusiveProcessingIncludesChildren && !task.IsCompleted)
                        {
                            flag2 = false;
                            if (continuationAction == null)
                            {
                                continuationAction = _ => this.ConcurrentExclusiveInterleaveProcessor();
                            }
                            task.ContinueWith(continuationAction, this._parallelOptions.TaskScheduler);
                            break;
                        }
                    }
                    Parallel.ForEach<Task>(this.GetConcurrentTasksUntilExclusiveExists(), this._parallelOptions, new Action<Task>(this.ExecuteConcurrentTask));
                    continue;
                }
                finally
                {
                    if (flag2)
                    {
                        lock (this._internalLock)
                        {
                            if ((this._concurrentTaskScheduler.Tasks.Count == 0) && (this._exclusiveTaskScheduler.Tasks.Count == 0))
                            {
                                this._taskExecuting = null;
                                flag = false;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>Runs a concurrent task.</summary>
        /// <param name="task">The task to execute.</param>
        /// <remarks>This has been separated out into its own method to improve the Parallel Tasks window experience.</remarks>
        private void ExecuteConcurrentTask(Task task)
        {
            this._concurrentTaskScheduler.ExecuteTask(task);
        }

        /// <summary>
        /// Gets an enumerable that yields waiting concurrent tasks one at a time until
        /// either there are no more concurrent tasks or there are any exclusive tasks.
        /// </summary>
        private IEnumerable<Task> GetConcurrentTasksUntilExclusiveExists()
        {
            while (true)
            {
                Task iteratorVariable0 = null;
                lock (this._internalLock)
                {
                    if ((this._exclusiveTaskScheduler.Tasks.Count == 0) && (this._concurrentTaskScheduler.Tasks.Count > 0))
                    {
                        iteratorVariable0 = this._concurrentTaskScheduler.Tasks.Dequeue();
                    }
                }
                if (iteratorVariable0 == null)
                {
                    yield break;
                }
                yield return iteratorVariable0;
            }
        }

        /// <summary>
        /// Gets an enumerable that yields all of the exclusive tasks one at a time.
        /// </summary>
        private IEnumerable<Task> GetExclusiveTasks()
        {
            while (true)
            {
                Task iteratorVariable0 = null;
                lock (this._internalLock)
                {
                    if (this._exclusiveTaskScheduler.Tasks.Count > 0)
                    {
                        iteratorVariable0 = this._exclusiveTaskScheduler.Tasks.Dequeue();
                    }
                }
                if (iteratorVariable0 == null)
                {
                    yield break;
                }
                yield return iteratorVariable0;
            }
        }

        /// <summary>Notifies the interleave that new work has arrived to be processed.</summary>
        /// <remarks>Must only be called while holding the lock.</remarks>
        internal void NotifyOfNewWork()
        {
            if (this._taskExecuting == null)
            {
                this._taskExecuting = new Task(new Action(this.ConcurrentExclusiveInterleaveProcessor), CancellationToken.None, TaskCreationOptions.None);
                this._taskExecuting.Start(this._parallelOptions.TaskScheduler);
            }
        }

        /// <summary>Gets the number of tasks waiting to run concurrently.</summary>
        private int ConcurrentTaskCount
        {
            get
            {
                lock (this._internalLock)
                {
                    return this._concurrentTaskScheduler.Tasks.Count;
                }
            }
        }

        /// <summary>
        /// Gets a TaskScheduler that can be used to schedule tasks to this interleave
        /// that may run concurrently with other tasks on this interleave.
        /// </summary>
        public TaskScheduler ConcurrentTaskScheduler
        {
            get
            {
                return this._concurrentTaskScheduler;
            }
        }

        /// <summary>Gets the number of tasks waiting to run exclusively.</summary>
        private int ExclusiveTaskCount
        {
            get
            {
                lock (this._internalLock)
                {
                    return this._exclusiveTaskScheduler.Tasks.Count;
                }
            }
        }

        /// <summary>
        /// Gets a TaskScheduler that can be used to schedule tasks to this interleave
        /// that must run exclusively with regards to other tasks on this interleave.
        /// </summary>
        public TaskScheduler ExclusiveTaskScheduler
        {
            get
            {
                return this._exclusiveTaskScheduler;
            }
        }



        /// <summary>Provides a debug view for ConcurrentExclusiveInterleave.</summary>
        internal class ConcurrentExclusiveInterleaveDebugView
        {
            /// <summary>The interleave being debugged.</summary>
            private ConcurrentExclusiveInterleave _interleave;

            /// <summary>Initializes the debug view.</summary>
            /// <param name="interleave">The interleave being debugged.</param>
            public ConcurrentExclusiveInterleaveDebugView(ConcurrentExclusiveInterleave interleave)
            {
                if (interleave == null)
                {
                    throw new ArgumentNullException("interleave");
                }
                this._interleave = interleave;
            }

            /// <summary>Gets the number of tasks waiting to run concurrently.</summary>
            public IEnumerable<Task> ConcurrentTasksWaiting
            {
                get
                {
                    return this._interleave._concurrentTaskScheduler.Tasks;
                }
            }

            public IEnumerable<Task> ExclusiveTasksWaiting
            {
                get
                {
                    return this._interleave._exclusiveTaskScheduler.Tasks;
                }
            }

            /// <summary>Gets a description of the processing task for debugging purposes.</summary>
            public Task InterleaveTask
            {
                get
                {
                    return this._interleave._taskExecuting;
                }
            }
        }

        /// <summary>
        /// A scheduler shim used to queue tasks to the interleave and execute those tasks on request of the interleave.
        /// </summary>
        private class ConcurrentExclusiveTaskScheduler : TaskScheduler
        {
            /// <summary>The parent interleave.</summary>
            private readonly ConcurrentExclusiveInterleave _interleave;
            /// <summary>The maximum concurrency level for the scheduler.</summary>
            private readonly int _maximumConcurrencyLevel;
            /// <summary>Whether a Task is currently being processed on this thread.</summary>
            private ThreadLocal<bool> _processingTaskOnCurrentThread = new ThreadLocal<bool>();

            /// <summary>Initializes the scheduler.</summary>
            /// <param name="interleave">The parent interleave.</param>
            /// <param name="tasks">The queue to store queued tasks into.</param>
            /// <param name="maximumConcurrencyLevel"></param>
            internal ConcurrentExclusiveTaskScheduler(ConcurrentExclusiveInterleave interleave, Queue<Task> tasks, int maximumConcurrencyLevel)
            {
                if (interleave == null)
                {
                    throw new ArgumentNullException("interleave");
                }
                if (tasks == null)
                {
                    throw new ArgumentNullException("tasks");
                }
                this._interleave = interleave;
                this._maximumConcurrencyLevel = maximumConcurrencyLevel;
                this.Tasks = tasks;
            }

            /// <summary>Executes a task on this scheduler.</summary>
            /// <param name="task">The task to be executed.</param>
            internal void ExecuteTask(Task task)
            {
                bool flag = this._processingTaskOnCurrentThread.Value;
                if (!flag)
                {
                    this._processingTaskOnCurrentThread.Value = true;
                }
                base.TryExecuteTask(task);
                if (!flag)
                {
                    this._processingTaskOnCurrentThread.Value = false;
                }
            }

            /// <summary>Gets for debugging purposes the tasks scheduled to this scheduler.</summary>
            /// <returns>An enumerable of the tasks queued.</returns>
            protected override IEnumerable<Task> GetScheduledTasks()
            {
                return this.Tasks;
            }

            /// <summary>Queues a task to the scheduler.</summary>
            /// <param name="task">The task to be queued.</param>
            protected override void QueueTask(Task task)
            {
                lock (this._interleave._internalLock)
                {
                    this.Tasks.Enqueue(task);
                    this._interleave.NotifyOfNewWork();
                }
            }

            /// <summary>Tries to execute the task synchronously on this scheduler.</summary>
            /// <param name="task">The task to execute.</param>
            /// <param name="taskWasPreviouslyQueued">Whether the task was previously queued to the scheduler.</param>
            /// <returns>true if the task could be executed; otherwise, false.</returns>
            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                Func<object, bool> function = null;
                if (!this._processingTaskOnCurrentThread.Value)
                {
                    return false;
                }
                if (function == null)
                {
                    function = state => base.TryExecuteTask((Task) state);
                }
                Task<bool> task2 = new Task<bool>(function, task);
                task2.RunSynchronously(this._interleave._parallelOptions.TaskScheduler);
                return task2.Result;
            }

            /// <summary>Gets the maximum concurrency level this scheduler is able to support.</summary>
            public override int MaximumConcurrencyLevel
            {
                get
                {
                    return this._maximumConcurrencyLevel;
                }
            }

            /// <summary>Gets the queue of tasks for this scheduler.</summary>
            internal Queue<Task> Tasks { get; private set; }
        }
    }
}

