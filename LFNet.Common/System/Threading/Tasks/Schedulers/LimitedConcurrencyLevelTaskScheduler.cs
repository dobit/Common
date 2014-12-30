using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Threading.Tasks.Schedulers
{
    /// <summary>
    /// Provides a task scheduler that ensures a maximum concurrency level while
    /// running on top of the ThreadPool.
    /// </summary>
    public class LimitedConcurrencyLevelTaskScheduler : TaskScheduler
    {
        /// <summary>Whether the current thread is processing work items.</summary>
        [ThreadStatic]
        private static bool _currentThreadIsProcessingItems;
        /// <summary>Whether the scheduler is currently processing work items.</summary>
        private int _delegatesQueuedOrRunning;
        /// <summary>The maximum concurrency level allowed by this scheduler.</summary>
        private readonly int _maxDegreeOfParallelism;
        /// <summary>The list of tasks to be executed.</summary>
        private readonly LinkedList<Task> _tasks = new LinkedList<Task>();

        /// <summary>
        /// Initializes an instance of the LimitedConcurrencyLevelTaskScheduler class with the
        /// specified degree of parallelism.
        /// </summary>
        /// <param name="maxDegreeOfParallelism">The maximum degree of parallelism provided by this scheduler.</param>
        public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism < 1)
            {
                throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
            }
            this._maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        /// <summary>Gets an enumerable of the tasks currently scheduled on this scheduler.</summary>
        /// <returns>An enumerable of the tasks currently scheduled.</returns>
        protected sealed override IEnumerable<Task> GetScheduledTasks()
        {
            IEnumerable<Task> enumerable;
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(this._tasks, ref lockTaken);
                if (!lockTaken)
                {
                    throw new NotSupportedException();
                }
                enumerable = this._tasks.ToArray<Task>();
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(this._tasks);
                }
            }
            return enumerable;
        }

        /// <summary>
        /// Informs the ThreadPool that there's work to be executed for this scheduler.
        /// </summary>
        private void NotifyThreadPoolOfPendingWork()
        {
            ThreadPool.UnsafeQueueUserWorkItem(delegate (object _) {
                _currentThreadIsProcessingItems = true;
                try
                {
                    while (true)
                    {
                        Task task;
                        lock (this._tasks)
                        {
                            if (this._tasks.Count == 0)
                            {
                                this._delegatesQueuedOrRunning--;
                                return;
                            }
                            task = this._tasks.First.Value;
                            this._tasks.RemoveFirst();
                        }
                        base.TryExecuteTask(task);
                    }
                }
                finally
                {
                    _currentThreadIsProcessingItems = false;
                }
            }, null);
        }

        /// <summary>Queues a task to the scheduler.</summary>
        /// <param name="task">The task to be queued.</param>
        protected sealed override void QueueTask(Task task)
        {
            lock (this._tasks)
            {
                this._tasks.AddLast(task);
                if (this._delegatesQueuedOrRunning < this._maxDegreeOfParallelism)
                {
                    this._delegatesQueuedOrRunning++;
                    this.NotifyThreadPoolOfPendingWork();
                }
            }
        }

        /// <summary>Attempts to remove a previously scheduled task from the scheduler.</summary>
        /// <param name="task">The task to be removed.</param>
        /// <returns>Whether the task could be found and removed.</returns>
        protected sealed override bool TryDequeue(Task task)
        {
            lock (this._tasks)
            {
                return this._tasks.Remove(task);
            }
        }

        /// <summary>Attempts to execute the specified task on the current thread.</summary>
        /// <param name="task">The task to be executed.</param>
        /// <param name="taskWasPreviouslyQueued"></param>
        /// <returns>Whether the task could be executed on the current thread.</returns>
        protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (!_currentThreadIsProcessingItems)
            {
                return false;
            }
            if (taskWasPreviouslyQueued)
            {
                this.TryDequeue(task);
            }
            return base.TryExecuteTask(task);
        }

        /// <summary>Gets the maximum concurrency level supported by this scheduler.</summary>
        public sealed override int MaximumConcurrencyLevel
        {
            get
            {
                return this._maxDegreeOfParallelism;
            }
        }
    }
}

