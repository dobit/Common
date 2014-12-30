using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace System.Threading.Async
{
    /// <summary>Provides for asynchronous exclusive and concurrent execution support.</summary>
    [DebuggerDisplay("WaitingConcurrent={WaitingConcurrent}, WaitingExclusive={WaitingExclusive}, CurrentReaders={CurrentConcurrent}, Exclusive={CurrentlyExclusive}")]
    public sealed class AsyncReaderWriter
    {
        /// <summary>The number of concurrent readers currently executing.</summary>
        private int _currentConcurrent;
        /// <summary>The number of exclusive writers currently executing.</summary>
        private bool _currentlyExclusive;
        /// <summary>The non-generic factory to use for task creation.</summary>
        private TaskFactory _factory;
        /// <summary>The lock that protects all shared state in this instance.</summary>
        private readonly object _lock;
        /// <summary>The queue of concurrent readers waiting to execute.</summary>
        private readonly Queue<Task> _waitingConcurrent;
        /// <summary>The queue of exclusive writers waiting to execute.</summary>
        private readonly Queue<Task> _waitingExclusive;

        /// <summary>Initializes the ReaderWriterAsync.</summary>
        public AsyncReaderWriter()
        {
            this._lock = new object();
            this._waitingConcurrent = new Queue<Task>();
            this._waitingExclusive = new Queue<Task>();
            this._factory = Task.Factory;
        }

        /// <summary>Initializes the ReaderWriterAsync with the specified TaskFactory for us in creating all tasks.</summary>
        /// <param name="factory">The TaskFactory to use to create all tasks.</param>
        public AsyncReaderWriter(TaskFactory factory)
        {
            this._lock = new object();
            this._waitingConcurrent = new Queue<Task>();
            this._waitingExclusive = new Queue<Task>();
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            this._factory = factory;
        }

        /// <summary>Completes the processing of a concurrent reader.</summary>
        private void FinishConcurrentReader()
        {
            lock (this._lock)
            {
                this._currentConcurrent--;
                if ((this._currentConcurrent == 0) && (this._waitingExclusive.Count > 0))
                {
                    this.RunExclusive_RequiresLock(this._waitingExclusive.Dequeue());
                }
                else if ((this._waitingExclusive.Count == 0) && (this._waitingConcurrent.Count > 0))
                {
                    this.RunConcurrent_RequiresLock();
                }
            }
        }

        /// <summary>Completes the processing of an exclusive writer.</summary>
        private void FinishExclusiveWriter()
        {
            lock (this._lock)
            {
                this._currentlyExclusive = false;
                if (this._waitingExclusive.Count > 0)
                {
                    this.RunExclusive_RequiresLock(this._waitingExclusive.Dequeue());
                }
                else if (this._waitingConcurrent.Count > 0)
                {
                    this.RunConcurrent_RequiresLock();
                }
            }
        }

        /// <summary>Queues a concurrent reader action to the ReaderWriterAsync.</summary>
        /// <param name="action">The action to be executed concurrently.</param>
        /// <returns>A Task that represents the execution of the provided action.</returns>
        public Task QueueConcurrentReader(Action action)
        {
            Task item = this._factory.Create(delegate (object state) {
                try
                {
                    ((Action) state)();
                }
                finally
                {
                    this.FinishConcurrentReader();
                }
            }, action);
            lock (this._lock)
            {
                if (this._currentlyExclusive || (this._waitingExclusive.Count > 0))
                {
                    this._waitingConcurrent.Enqueue(item);
                    return item;
                }
                this.RunConcurrent_RequiresLock(item);
            }
            return item;
        }

        /// <summary>Queues a concurrent reader function to the ReaderWriterAsync.</summary>
        /// <param name="function">The function to be executed concurrently.</param>
        /// <returns>A Task that represents the execution of the provided function.</returns>
        public Task<TResult> QueueConcurrentReader<TResult>(Func<TResult> function)
        {
            Task<TResult> item = this._factory.Create<TResult>(delegate (object state) {
                TResult local;
                try
                {
                    local = ((Func<TResult>) state)();
                }
                finally
                {
                    this.FinishConcurrentReader();
                }
                return local;
            }, function);
            lock (this._lock)
            {
                if (this._currentlyExclusive || (this._waitingExclusive.Count > 0))
                {
                    this._waitingConcurrent.Enqueue(item);
                    return item;
                }
                this.RunConcurrent_RequiresLock(item);
            }
            return item;
        }

        /// <summary>Queues an exclusive writer action to the ReaderWriterAsync.</summary>
        /// <param name="action">The action to be executed exclusively.</param>
        /// <returns>A Task that represents the execution of the provided action.</returns>
        public Task QueueExclusiveWriter(Action action)
        {
            Task item = this._factory.Create(delegate (object state) {
                try
                {
                    ((Action) state)();
                }
                finally
                {
                    this.FinishExclusiveWriter();
                }
            }, action);
            lock (this._lock)
            {
                if ((this._currentlyExclusive || (this._currentConcurrent > 0)) || (this._waitingExclusive.Count > 0))
                {
                    this._waitingExclusive.Enqueue(item);
                    return item;
                }
                this.RunExclusive_RequiresLock(item);
            }
            return item;
        }

        /// <summary>Queues an exclusive writer function to the ReaderWriterAsync.</summary>
        /// <param name="function">The function to be executed exclusively.</param>
        /// <returns>A Task that represents the execution of the provided function.</returns>
        public Task<TResult> QueueExclusiveWriter<TResult>(Func<TResult> function)
        {
            Task<TResult> item = this._factory.Create<TResult>(delegate (object state) {
                TResult local;
                try
                {
                    local = ((Func<TResult>) state)();
                }
                finally
                {
                    this.FinishExclusiveWriter();
                }
                return local;
            }, function);
            lock (this._lock)
            {
                if ((this._currentlyExclusive || (this._currentConcurrent > 0)) || (this._waitingExclusive.Count > 0))
                {
                    this._waitingExclusive.Enqueue(item);
                    return item;
                }
                this.RunExclusive_RequiresLock(item);
            }
            return item;
        }

        /// <summary>Starts all queued concurrent tasks.</summary>
        /// <remarks>This must only be executed while holding the instance's lock.</remarks>
        private void RunConcurrent_RequiresLock()
        {
            while (this._waitingConcurrent.Count > 0)
            {
                this.RunConcurrent_RequiresLock(this._waitingConcurrent.Dequeue());
            }
        }

        /// <summary>Starts the specified concurrent task.</summary>
        /// <param name="concurrent">The exclusive task to be started.</param>
        /// <remarks>This must only be executed while holding the instance's lock.</remarks>
        private void RunConcurrent_RequiresLock(Task concurrent)
        {
            this._currentConcurrent++;
            concurrent.Start(this._factory.GetTargetScheduler());
        }

        /// <summary>Starts the specified exclusive task.</summary>
        /// <param name="exclusive">The exclusive task to be started.</param>
        /// <remarks>This must only be executed while holding the instance's lock.</remarks>
        private void RunExclusive_RequiresLock(Task exclusive)
        {
            this._currentlyExclusive = true;
            exclusive.Start(this._factory.GetTargetScheduler());
        }

        /// <summary>Gets the number of concurrent operations currently executing.</summary>
        public int CurrentConcurrent
        {
            get
            {
                lock (this._lock)
                {
                    return this._currentConcurrent;
                }
            }
        }

        /// <summary>Gets whether an exclusive operation is currently executing.</summary>
        public bool CurrentlyExclusive
        {
            get
            {
                lock (this._lock)
                {
                    return this._currentlyExclusive;
                }
            }
        }

        /// <summary>Gets the number of concurrent operations currently queued.</summary>
        public int WaitingConcurrent
        {
            get
            {
                lock (this._lock)
                {
                    return this._waitingConcurrent.Count;
                }
            }
        }

        /// <summary>Gets the number of exclusive operations currently queued.</summary>
        public int WaitingExclusive
        {
            get
            {
                lock (this._lock)
                {
                    return this._waitingExclusive.Count;
                }
            }
        }
    }
}

