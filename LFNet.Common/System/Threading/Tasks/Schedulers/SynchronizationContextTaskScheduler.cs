using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Threading.Tasks.Schedulers
{
    /// <summary>Provides a task scheduler that targets a specific SynchronizationContext.</summary>
    public sealed class SynchronizationContextTaskScheduler : TaskScheduler
    {
        /// <summary>The target context under which to execute the queued tasks.</summary>
        private readonly SynchronizationContext _context;
        /// <summary>The queue of tasks to execute, maintained for debugging purposes.</summary>
        private readonly ConcurrentQueue<Task> _tasks;

        /// <summary>Initializes an instance of the SynchronizationContextTaskScheduler class.</summary>
        public SynchronizationContextTaskScheduler() : this(SynchronizationContext.Current)
        {
        }

        /// <summary>
        /// Initializes an instance of the SynchronizationContextTaskScheduler class
        /// with the specified SynchronizationContext.
        /// </summary>
        /// <param name="context">The SynchronizationContext under which to execute tasks.</param>
        public SynchronizationContextTaskScheduler(SynchronizationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            this._context = context;
            this._tasks = new ConcurrentQueue<Task>();
        }

        /// <summary>Gets an enumerable of tasks queued to the scheduler.</summary>
        /// <returns>An enumerable of tasks queued to the scheduler.</returns>
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return this._tasks.ToArray();
        }

        /// <summary>Queues a task to the scheduler for execution on the I/O ThreadPool.</summary>
        /// <param name="task">The Task to queue.</param>
        protected override void QueueTask(Task task)
        {
            this._tasks.Enqueue(task);
            this._context.Post(delegate
            {
                Task task2;
                if (this._tasks.TryDequeue(out task2))
                {
                    base.TryExecuteTask(task2);
                }
            }, null);
        }

        /// <summary>Tries to execute a task on the current thread.</summary>
        /// <param name="task">The task to be executed.</param>
        /// <param name="taskWasPreviouslyQueued">Ignored.</param>
        /// <returns>Whether the task could be executed.</returns>
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return ((this._context != SynchronizationContext.Current) && base.TryExecuteTask(task));
        }

        /// <summary>Gets the maximum concurrency level supported by this scheduler.</summary>
        public override int MaximumConcurrencyLevel
        {
            get
            {
                return 1;
            }
        }
    }
}

