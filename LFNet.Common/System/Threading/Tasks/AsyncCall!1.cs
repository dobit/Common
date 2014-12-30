using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Threading.Tasks
{
    /// <summary>Asynchronously invokes a handler for every posted item.</summary>
    /// <typeparam name="T">Specifies the type of data processed by the instance.</typeparam>
    public sealed class AsyncCall<T> : MarshalByRefObject
    {
        /// <summary>The delegate to invoke for every element.</summary>
        private readonly Delegate _handler;
        /// <summary>The maximum number of items that should be processed by an individual task.</summary>
        private readonly int _maxItemsPerTask;
        /// <summary>The options to use for parallel processing of data.</summary>
        private readonly ParallelOptions _parallelOptions;
        /// <summary>Whether a processing task has been scheduled.</summary>
        private int _processingCount;
        /// <summary>
        /// A queue that stores the posted data.  Also serves as the syncObj for protected instance state.
        /// A ConcurrentQueue is used to enable lock-free dequeues while running with a single consumer task.
        /// </summary>
        private readonly ConcurrentQueue<T> _queue;
        /// <summary>The TaskFactory to use to launch new tasks.</summary>
        private readonly TaskFactory _tf;

        /// <summary>General initialization of the AsyncCall.  Another constructor must initialize the delegate.</summary>
        /// <param name="maxDegreeOfParallelism">The maximum degree of parallelism to use.  If not specified, 1 is used for serial execution.</param>
        /// <param name="maxItemsPerTask">The maximum number of items to be processed per task.  If not specified, Int32.MaxValue is used.</param>
        /// <param name="scheduler">The scheduler to use.  If null, the default scheduler is used.</param>
        private AsyncCall(int maxDegreeOfParallelism = 1, int maxItemsPerTask = 0x7fffffff, TaskScheduler scheduler = null)
        {
            if (maxDegreeOfParallelism < 1)
            {
                throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
            }
            if (maxItemsPerTask < 1)
            {
                throw new ArgumentOutOfRangeException("maxItemsPerTask");
            }
            if (scheduler == null)
            {
                scheduler = TaskScheduler.Default;
            }
            this._queue = new ConcurrentQueue<T>();
            this._maxItemsPerTask = maxItemsPerTask;
            this._tf = new TaskFactory(scheduler);
            if (maxDegreeOfParallelism != 1)
            {
                ParallelOptions options = new ParallelOptions {
                    MaxDegreeOfParallelism = maxDegreeOfParallelism,
                    TaskScheduler = scheduler
                };
                this._parallelOptions = options;
            }
        }

        /// <summary>
        /// Initializes the AsyncCall with a function to execute for each element.  The function returns an Task 
        /// that represents the asynchronous completion of that element's processing.
        /// </summary>
        /// <param name="functionHandler">The function to run for every posted item.</param>
        /// <param name="maxDegreeOfParallelism">The maximum degree of parallelism to use.  If not specified, 1 is used for serial execution.</param>
        /// <param name="scheduler">The scheduler to use.  If null, the default scheduler is used.</param>
        public AsyncCall(Func<T, Task> functionHandler, int maxDegreeOfParallelism = 1, TaskScheduler scheduler = null) : this(maxDegreeOfParallelism, 1, scheduler)
        {
            if (functionHandler == null)
            {
                throw new ArgumentNullException("handler");
            }
            this._handler = functionHandler;
        }

        /// <summary>Initializes the AsyncCall with an action to execute for each element.</summary>
        /// <param name="actionHandler">The action to run for every posted item.</param>
        /// <param name="maxDegreeOfParallelism">The maximum degree of parallelism to use.  If not specified, 1 is used for serial execution.</param>
        /// <param name="scheduler">The scheduler to use.  If null, the default scheduler is used.</param>
        /// <param name="maxItemsPerTask">The maximum number of items to be processed per task.  If not specified, Int32.MaxValue is used.</param>
        public AsyncCall(Action<T> actionHandler, int maxDegreeOfParallelism = 1, int maxItemsPerTask = 0x7fffffff, TaskScheduler scheduler = null) : this(maxDegreeOfParallelism, maxItemsPerTask, scheduler)
        {
            if (actionHandler == null)
            {
                throw new ArgumentNullException("handler");
            }
            this._handler = actionHandler;
        }

        /// <summary>Gets an enumerable that yields the items to be processed at this time.</summary>
        /// <returns>An enumerable of items.</returns>
        private IEnumerable<T> GetItemsToProcess()
        {
            int iteratorVariable0 = 0;
            while (true)
            {
                T iteratorVariable1;
                if ((iteratorVariable0 >= this._maxItemsPerTask) || !this._queue.TryDequeue(out iteratorVariable1))
                {
                    yield break;
                }
                yield return iteratorVariable1;
                iteratorVariable0++;
            }
        }

        /// <summary>Post an item for processing.</summary>
        /// <param name="item">The item to be processed.</param>
        public void Post(T item)
        {
            lock (this._queue)
            {
                this._queue.Enqueue(item);
                if (this._handler is Action<T>)
                {
                    if (this._processingCount == 0)
                    {
                        this._processingCount = 1;
                        this._tf.StartNew(new Action(this.ProcessItemsActionTaskBody));
                    }
                }
                else if ((this._handler is Func<T, Task>) && ((this._processingCount == 0) || (((this._parallelOptions != null) && (this._processingCount < this._parallelOptions.MaxDegreeOfParallelism)) && !this._queue.IsEmpty)))
                {
                    this._processingCount++;
                    this._tf.StartNew(new Action<object>(this.ProcessItemFunctionTaskBody), null);
                }
            }
        }

        /// <summary>Used as the body of a function task to process items in the queue.</summary>
        private void ProcessItemFunctionTaskBody(object ignored)
        {
            bool flag = false;
            try
            {
                T local;
                Func<T, Task> func = (Func<T, Task>) this._handler;
                if (this._queue.TryDequeue(out local))
                {
                    Task task = func(local);
                    if (task != null)
                    {
                        task.ContinueWith(new Action<Task>(this.ProcessItemFunctionTaskBody), this._tf.Scheduler);
                    }
                    else
                    {
                        this._tf.StartNew(new Action<object>(this.ProcessItemFunctionTaskBody), null);
                    }
                    flag = true;
                }
            }
            finally
            {
                if (!flag)
                {
                    lock (this._queue)
                    {
                        if (!this._queue.IsEmpty)
                        {
                            this._tf.StartNew(new Action<object>(this.ProcessItemFunctionTaskBody), null);
                        }
                        else
                        {
                            this._processingCount--;
                        }
                    }
                }
            }
        }

        /// <summary>Used as the body of an action task to process items in the queue.</summary>
        private void ProcessItemsActionTaskBody()
        {
            try
            {
                Action<T> body = (Action<T>) this._handler;
                if (this._parallelOptions == null)
                {
                    foreach (T local in this.GetItemsToProcess())
                    {
                        body(local);
                    }
                }
                else
                {
                    Parallel.ForEach<T>(this.GetItemsToProcess(), this._parallelOptions, body);
                }
            }
            finally
            {
                lock (this._queue)
                {
                    if (!this._queue.IsEmpty)
                    {
                        this._tf.StartNew(new Action(this.ProcessItemsActionTaskBody), TaskCreationOptions.PreferFairness);
                    }
                    else
                    {
                        this._processingCount = 0;
                    }
                }
            }
        }

     
    }
}

