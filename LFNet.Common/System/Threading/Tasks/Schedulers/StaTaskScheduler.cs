using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Threading.Tasks.Schedulers
{
    /// <summary>Provides a scheduler that uses STA threads.</summary>
    public sealed class StaTaskScheduler : TaskScheduler, IDisposable
    {
        /// <summary>Stores the queued tasks to be executed by our pool of STA threads.</summary>
        private BlockingCollection<Task> _tasks;
        /// <summary>The STA threads used by the scheduler.</summary>
        private readonly List<Thread> _threads;
        /// <summary>Gets the maximum concurrency level supported by this scheduler.</summary>
        public override int MaximumConcurrencyLevel
        {
            get
            {
                return this._threads.Count;
            }
        }
        /// <summary>Initializes a new instance of the StaTaskScheduler class with the specified concurrency level.</summary>
        /// <param name="numberOfThreads">The number of threads that should be created and used by this scheduler.</param>
        public StaTaskScheduler(int numberOfThreads)
        {
            if (numberOfThreads < 1)
            {
                throw new ArgumentOutOfRangeException("concurrencyLevel");
            }
            this._tasks = new BlockingCollection<Task>();
            this._threads = Enumerable.Range(0, numberOfThreads).Select(delegate(int i)
            {
                Thread thread = new Thread(new ThreadStart(delegate
                {
                    foreach (Task current in this._tasks.GetConsumingEnumerable())
                    {
                        base.TryExecuteTask(current);
                    }
                }));
                thread.IsBackground = true;
                thread.SetApartmentState(ApartmentState.STA);
                return thread;
            }).ToList<Thread>();
            this._threads.ForEach(delegate(Thread t)
            {
                t.Start();
            });
        }
        /// <summary>Queues a Task to be executed by this scheduler.</summary>
        /// <param name="task">The task to be executed.</param>
        protected override void QueueTask(Task task)
        {
            this._tasks.Add(task);
        }
        /// <summary>Provides a list of the scheduled tasks for the debugger to consume.</summary>
        /// <returns>An enumerable of all tasks currently scheduled.</returns>
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return this._tasks.ToArray();
        }
        /// <summary>Determines whether a Task may be inlined.</summary>
        /// <param name="task">The task to be executed.</param>
        /// <param name="taskWasPreviouslyQueued">Whether the task was previously queued.</param>
        /// <returns>true if the task was successfully inlined; otherwise, false.</returns>
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return Thread.CurrentThread.GetApartmentState() == ApartmentState.STA && base.TryExecuteTask(task);
        }
        /// <summary>
        /// Cleans up the scheduler by indicating that no more tasks will be queued.
        /// This method blocks until all threads successfully shutdown.
        /// </summary>
        public void Dispose()
        {
            if (this._tasks != null)
            {
                this._tasks.CompleteAdding();
                foreach (Thread current in this._threads)
                {
                    current.Join();
                }
                this._tasks.Dispose();
                this._tasks = null;
            }
        }
    }
}
