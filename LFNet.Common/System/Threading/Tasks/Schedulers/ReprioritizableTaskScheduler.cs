using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Threading.Tasks.Schedulers
{
    /// <summary>Provides a task scheduler that supports reprioritizing previously queued tasks.</summary>
    public sealed class ReprioritizableTaskScheduler : TaskScheduler
    {
        private readonly LinkedList<Task> _tasks = new LinkedList<Task>();

        /// <summary>Reprioritizes a previously queued task to the back of the queue.</summary>
        /// <param name="task">The task to be reprioritized.</param>
        /// <returns>Whether the task could be found and moved to the back of the queue.</returns>
        public bool Deprioritize(Task task)
        {
            lock (this._tasks)
            {
                LinkedListNode<Task> node = this._tasks.Find(task);
                if (node != null)
                {
                    this._tasks.Remove(node);
                    this._tasks.AddLast(node);
                    return true;
                }
            }
            return false;
        }

        /// <summary>Gets all of the tasks currently queued to the scheduler.</summary>
        /// <returns>An enumerable of the tasks currently queued to the scheduler.</returns>
        protected override IEnumerable<Task> GetScheduledTasks()
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

        /// <summary>Reprioritizes a previously queued task to the front of the queue.</summary>
        /// <param name="task">The task to be reprioritized.</param>
        /// <returns>Whether the task could be found and moved to the front of the queue.</returns>
        public bool Prioritize(Task task)
        {
            lock (this._tasks)
            {
                LinkedListNode<Task> node = this._tasks.Find(task);
                if (node != null)
                {
                    this._tasks.Remove(node);
                    this._tasks.AddFirst(node);
                    return true;
                }
            }
            return false;
        }

        /// <summary>Picks up and executes the next item in the queue.</summary>
        /// <param name="ignored">Ignored.</param>
        private void ProcessNextQueuedItem(object ignored)
        {
            Task task;
            lock (this._tasks)
            {
                if (this._tasks.Count > 0)
                {
                    task = this._tasks.First.Value;
                    this._tasks.RemoveFirst();
                }
                else
                {
                    return;
                }
            }
            base.TryExecuteTask(task);
        }

        /// <summary>Queues a task to the scheduler.</summary>
        /// <param name="task">The task to be queued.</param>
        protected override void QueueTask(Task task)
        {
            lock (this._tasks)
            {
                this._tasks.AddLast(task);
            }
            ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(this.ProcessNextQueuedItem), null);
        }

        /// <summary>Removes a previously queued item from the scheduler.</summary>
        /// <param name="task">The task to be removed.</param>
        /// <returns>Whether the task could be removed from the scheduler.</returns>
        protected override bool TryDequeue(Task task)
        {
            lock (this._tasks)
            {
                return this._tasks.Remove(task);
            }
        }

        /// <summary>Executes the specified task inline.</summary>
        /// <param name="task">The task to be executed.</param>
        /// <param name="taskWasPreviouslyQueued">Whether the task was previously queued.</param>
        /// <returns>Whether the task could be executed inline.</returns>
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return base.TryExecuteTask(task);
        }
    }
}

