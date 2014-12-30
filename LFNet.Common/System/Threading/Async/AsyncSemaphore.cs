using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace System.Threading.Async
{
    /// <summary>Provides an asynchronous semaphore.</summary>
    [DebuggerDisplay("CurrentCount={CurrentCount}, MaximumCount={MaximumCount}, WaitingCount={WaitingCount}")]
    public sealed class AsyncSemaphore : IDisposable
    {
        /// <summary>The current count.</summary>
        private int _currentCount;
        /// <summary>The maximum count. If _maxCount isn't positive, the instance has been disposed.</summary>
        private int _maxCount;
        /// <summary>Tasks waiting to be completed when the semaphore has count available.</summary>
        private Queue<TaskCompletionSource<object>> _waitingTasks;

        /// <summary>Initializes the SemaphoreAsync with a count of zero and a maximum count of Int32.MaxValue.</summary>
        public AsyncSemaphore() : this(0)
        {
        }

        /// <summary>Initializes the SemaphoreAsync with the specified count and a maximum count of Int32.MaxValue.</summary>
        /// <param name="initialCount">The initial count to use as the current count.</param>
        public AsyncSemaphore(int initialCount) : this(initialCount, 0x7fffffff)
        {
        }

        /// <summary>Initializes the SemaphoreAsync with the specified counts.</summary>
        /// <param name="initialCount">The initial count to use as the current count.</param>
        /// <param name="maxCount">The maximum count allowed.</param>
        public AsyncSemaphore(int initialCount, int maxCount)
        {
            if (maxCount <= 0)
            {
                throw new ArgumentOutOfRangeException("maxCount");
            }
            if ((initialCount > maxCount) || (initialCount < 0))
            {
                throw new ArgumentOutOfRangeException("initialCount");
            }
            this._currentCount = initialCount;
            this._maxCount = maxCount;
            this._waitingTasks = new Queue<TaskCompletionSource<object>>();
        }

        /// <summary>Releases the resources used by the semaphore.</summary>
        public void Dispose()
        {
            if (this._maxCount > 0)
            {
                this._maxCount = 0;
                lock (this._waitingTasks)
                {
                    while (this._waitingTasks.Count > 0)
                    {
                        this._waitingTasks.Dequeue().SetCanceled();
                    }
                }
            }
        }

        /// <summary>
        /// Queues an action that will be executed when space is available
        /// in the semaphore.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        /// <returns>
        /// A Task that represents the execution of the action.
        /// </returns>
        /// <remarks>
        /// Release does not need to be called for this action, as it will be handled implicitly
        /// by the Queue method.
        /// </remarks>
        public Task Queue(Action action)
        {
            return this.WaitAsync().ContinueWith(delegate (Task _) {
                try
                {
                    action();
                }
                finally
                {
                    this.Release();
                }
            });
        }

        /// <summary>
        /// Queues a function that will be executed when space is available
        /// in the semaphore.
        /// </summary>
        /// <param name="function">The function to be executed.</param>
        /// <returns>
        /// A Task that represents the execution of the function.
        /// </returns>
        /// <remarks>
        /// Release does not need to be called for this function, as it will be handled implicitly
        /// by the Queue method.
        /// </remarks>
        public Task<TResult> Queue<TResult>(Func<TResult> function)
        {
            return this.WaitAsync().ContinueWith<TResult>(delegate (Task _) {
                TResult local;
                try
                {
                    local = function();
                }
                finally
                {
                    this.Release();
                }
                return local;
            });
        }

        /// <summary>Releases a unit of work to the semaphore.</summary>
        public void Release()
        {
            this.ThrowIfDisposed();
            lock (this._waitingTasks)
            {
                if (this._currentCount == this._maxCount)
                {
                    throw new SemaphoreFullException();
                }
                if (this._waitingTasks.Count > 0)
                {
                    this._waitingTasks.Dequeue().SetResult(null);
                }
                else
                {
                    this._currentCount++;
                }
            }
        }

        private void ThrowIfDisposed()
        {
            if (this._maxCount <= 0)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
        }

        /// <summary>Waits for a unit to be available in the semaphore.</summary>
        /// <returns>A Task that will be completed when a unit is available and this Wait operation succeeds.</returns>
        public Task WaitAsync()
        {
            this.ThrowIfDisposed();
            lock (this._waitingTasks)
            {
                if (this._currentCount > 0)
                {
                    this._currentCount--;
                    return CompletedTask.Default;
                }
                TaskCompletionSource<object> item = new TaskCompletionSource<object>();
                this._waitingTasks.Enqueue(item);
                return item.Task;
            }
        }

        /// <summary>Gets the current count.</summary>
        public int CurrentCount
        {
            get
            {
                return this._currentCount;
            }
        }

        /// <summary>Gets the maximum count.</summary>
        public int MaximumCount
        {
            get
            {
                return this._maxCount;
            }
        }

        /// <summary>Gets the number of operations currently waiting on the semaphore.</summary>
        public int WaitingCount
        {
            get
            {
                lock (this._waitingTasks)
                {
                    return this._waitingTasks.Count;
                }
            }
        }
    }
}

