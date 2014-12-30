using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Threading.Tasks
{
    /// <summary>Represents a queue of tasks to be started and executed serially.</summary>
    public class SerialTaskQueue
    {
        /// <summary>The task currently executing, or null if there is none.</summary>
        private Task _taskInFlight;
        /// <summary>The ordered queue of tasks to be executed. Also serves as a lock protecting all shared state.</summary>
        private Queue<object> _tasks = new Queue<object>();

        /// <summary>Gets a Task that represents the completion of all previously queued tasks.</summary>
        public Task Completed()
        {
            return this.Enqueue(new Task(delegate {
            }));
        }

        /// <summary>Enqueues the task to be processed serially and in order.</summary>
        /// <param name="taskGenerator">The function that generates a non-started task.</param>
        public void Enqueue(Func<Task> taskGenerator)
        {
            this.EnqueueInternal(taskGenerator);
        }

        /// <summary>Enqueues the non-started task to be processed serially and in order.</summary>
        /// <param name="task">The task.</param>
        public Task Enqueue(Task task)
        {
            this.EnqueueInternal(task);
            return task;
        }

        /// <summary>Enqueues the task to be processed serially and in order.</summary>
        /// <param name="taskOrFunction">The task or functino that generates a task.</param>
        /// <remarks>The task must not be started and must only be started by this instance.</remarks>
        private void EnqueueInternal(object taskOrFunction)
        {
            if (taskOrFunction == null)
            {
                throw new ArgumentNullException("task");
            }
            lock (this._tasks)
            {
                if (this._taskInFlight == null)
                {
                    this.StartTask_CallUnderLock(taskOrFunction);
                }
                else
                {
                    this._tasks.Enqueue(taskOrFunction);
                }
            }
        }

        /// <summary>Called when a Task completes to potentially start the next in the queue.</summary>
        /// <param name="ignored">The task that completed.</param>
        private void OnTaskCompletion(Task ignored)
        {
            lock (this._tasks)
            {
                this._taskInFlight = null;
                if (this._tasks.Count > 0)
                {
                    this.StartTask_CallUnderLock(this._tasks.Dequeue());
                }
            }
        }

        /// <summary>Starts the provided task (or function that returns a task).</summary>
        /// <param name="nextItem">The next task or function that returns a task.</param>
        private void StartTask_CallUnderLock(object nextItem)
        {
            Task task = nextItem as Task;
            if (task == null)
            {
                task = ((Func<Task>) nextItem)();
            }
            if (task.Status == TaskStatus.Created)
            {
                task.Start();
            }
            this._taskInFlight = task;
            task.ContinueWith(new Action<Task>(this.OnTaskCompletion));
        }
    }
}

