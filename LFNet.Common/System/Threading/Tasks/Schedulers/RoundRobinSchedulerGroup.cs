using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Threading.Tasks.Schedulers
{
    /// <summary>Enables the creation of a group of schedulers that support round-robin scheduling for fairness.</summary>
    public sealed class RoundRobinSchedulerGroup
    {
        private int _nextQueue;
        private readonly List<RoundRobinTaskSchedulerQueue> _queues = new List<RoundRobinTaskSchedulerQueue>();

        /// <summary>Creates a new scheduler as part of this group.</summary>
        /// <returns>The new scheduler.</returns>
        public TaskScheduler CreateScheduler()
        {
            RoundRobinTaskSchedulerQueue item = new RoundRobinTaskSchedulerQueue(this);
            lock (this._queues)
            {
                this._queues.Add(item);
            }
            return item;
        }

        /// <summary>Notifies the ThreadPool that there's a new item to be executed.</summary>
        private void NotifyNewWorkItem()
        {
            ThreadPool.UnsafeQueueUserWorkItem(delegate (object _) {
                Task task = null;
                RoundRobinTaskSchedulerQueue queue = null;
                lock (this._queues)
                {
                    foreach (int num in Enumerable.Range(this._nextQueue, this._queues.Count - this._nextQueue).Concat<int>(Enumerable.Range(0, this._nextQueue)))
                    {
                        queue = this._queues[num];
                        Queue<Task> queue2 = queue._workItems;
                        if (queue2.Count > 0)
                        {
                            task = queue2.Dequeue();
                            this._nextQueue = num;
                            if (queue._disposed && (queue2.Count == 0))
                            {
                                this.RemoveQueue_NeedsLock(queue);
                            }
                            break;
                        }
                    }
                    this._nextQueue = (this._nextQueue + 1) % this._queues.Count;
                }
                if (task != null)
                {
                    queue.RunQueuedTask(task);
                }
            }, null);
        }

        /// <summary>Removes a scheduler from the group.</summary>
        /// <param name="queue">The scheduler to be removed.</param>
        private void RemoveQueue_NeedsLock(RoundRobinTaskSchedulerQueue queue)
        {
            int index = this._queues.IndexOf(queue);
            if (this._nextQueue >= index)
            {
                this._nextQueue--;
            }
            this._queues.RemoveAt(index);
        }

        /// <summary>Gets a collection of all schedulers in this group.</summary>
        public ReadOnlyCollection<TaskScheduler> Schedulers
        {
            get
            {
                lock (this._queues)
                {
                    return new ReadOnlyCollection<TaskScheduler>(this._queues.Cast<TaskScheduler>().ToArray<TaskScheduler>());
                }
            }
        }

        /// <summary>A scheduler that participates in round-robin scheduling.</summary>
        private sealed class RoundRobinTaskSchedulerQueue : TaskScheduler, IDisposable
        {
            internal bool _disposed;
            private RoundRobinSchedulerGroup _pool;
            internal Queue<Task> _workItems = new Queue<Task>();

            internal RoundRobinTaskSchedulerQueue(RoundRobinSchedulerGroup pool)
            {
                this._pool = pool;
            }

            protected override IEnumerable<Task> GetScheduledTasks()
            {
                IEnumerable<Task> enumerable;
                object obj2 = this._pool._queues;
                bool lockTaken = false;
                try
                {
                    Monitor.TryEnter(obj2, ref lockTaken);
                    if (!lockTaken)
                    {
                        throw new NotSupportedException();
                    }
                    enumerable = this._workItems.ToArray();
                }
                finally
                {
                    if (lockTaken)
                    {
                        Monitor.Exit(obj2);
                    }
                }
                return enumerable;
            }

            protected override void QueueTask(Task task)
            {
                if (this._disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                lock (this._pool._queues)
                {
                    this._workItems.Enqueue(task);
                }
                this._pool.NotifyNewWorkItem();
            }

            internal void RunQueuedTask(Task task)
            {
                base.TryExecuteTask(task);
            }

            void IDisposable.Dispose()
            {
                if (!this._disposed)
                {
                    lock (this._pool._queues)
                    {
                        if (this._workItems.Count == 0)
                        {
                            this._pool.RemoveQueue_NeedsLock(this);
                        }
                        this._disposed = true;
                    }
                }
            }

            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                return base.TryExecuteTask(task);
            }
        }
    }
}

