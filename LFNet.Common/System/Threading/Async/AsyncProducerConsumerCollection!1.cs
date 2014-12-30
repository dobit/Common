using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;

namespace System.Threading.Async
{
    /// <summary>Provides an asynchronous producer/consumer collection.</summary>
    [DebuggerDisplay("Count={CurrentCount}")]
    public sealed class AsyncProducerConsumerCollection<T> : IDisposable
    {
        /// <summary>The data stored in the collection.</summary>
        private IProducerConsumerCollection<T> _collection;
        /// <summary>Asynchronous semaphore used to keep track of asynchronous work.</summary>
        private AsyncSemaphore _semaphore;

        /// <summary>Initializes the asynchronous producer/consumer collection to store data in a first-in-first-out (FIFO) order.</summary>
        public AsyncProducerConsumerCollection() : this(new ConcurrentQueue<T>())
        {
        }

        /// <summary>Initializes the asynchronous producer/consumer collection.</summary>
        /// <param name="collection">The underlying collection to use to store data.</param>
        public AsyncProducerConsumerCollection(IProducerConsumerCollection<T> collection)
        {
            this._semaphore = new AsyncSemaphore();
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            this._collection = collection;
        }

        /// <summary>Adds an element to the collection.</summary>
        /// <param name="item">The item to be added.</param>
        public void Add(T item)
        {
            if (!this._collection.TryAdd(item))
            {
                throw new InvalidOperationException("Invalid collection");
            }
            this._semaphore.Release();
        }

        /// <summary>Disposes of the collection.</summary>
        public void Dispose()
        {
            if (this._semaphore != null)
            {
                this._semaphore.Dispose();
                this._semaphore = null;
            }
        }

        /// <summary>Takes an element from the collection asynchronously.</summary>
        /// <returns>A Task that represents the element removed from the collection.</returns>
        public Task<T> Take()
        {
            return this._semaphore.WaitAsync().ContinueWith<T>(delegate (Task _) {
                T local;
                if (!_collection.TryTake(out local))
                {
                    throw new InvalidOperationException("Invalid collection");
                }
                return local;
            }, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        /// <summary>Gets the number of elements in the collection.</summary>
        public int Count
        {
            get
            {
                return this._collection.Count;
            }
        }
    }
}

