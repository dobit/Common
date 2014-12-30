using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace System.Collections.Concurrent
{
    /// <summary>Extension methods for BlockingCollection.</summary>
    public static class BlockingCollectionExtensions
    {
        /// <summary>Adds the contents of an enumerable to the BlockingCollection.</summary>
        /// <typeparam name="T">Specifies the type of the elements in the collection.</typeparam>
        /// <param name="target">The target BlockingCollection to be augmented.</param>
        /// <param name="source">The source enumerable containing the data to be added.</param>
        /// <param name="completeAddingWhenDone">
        /// Whether to mark the target BlockingCollection as complete for adding when 
        /// all elements of the source enumerable have been transfered.
        /// </param>
        public static void AddFromEnumerable<T>(this BlockingCollection<T> target, IEnumerable<T> source, bool completeAddingWhenDone)
        {
            try
            {
                foreach (T local in source)
                {
                    target.Add(local);
                }
            }
            finally
            {
                if (completeAddingWhenDone)
                {
                    target.CompleteAdding();
                }
            }
        }

        /// <summary>Adds the contents of an observable to the BlockingCollection.</summary>
        /// <typeparam name="T">Specifies the type of the elements in the collection.</typeparam>
        /// <param name="target">The target BlockingCollection to be augmented.</param>
        /// <param name="source">The source observable containing the data to be added.</param>
        /// <param name="completeAddingWhenDone">
        /// Whether to mark the target BlockingCollection as complete for adding when 
        /// all elements of the source observable have been transfered.
        /// </param>
        /// <returns>An IDisposable that may be used to cancel the transfer.</returns>
        public static IDisposable AddFromObservable<T>(this BlockingCollection<T> target, IObservable<T> source, bool completeAddingWhenDone)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            Action<T> onNext = delegate (T item) {
                target.Add(item);
            };
            Action<Exception> onError = delegate (Exception error) {
                if (completeAddingWhenDone)
                {
                    target.CompleteAdding();
                }
            };
            Action onCompleted = delegate {
                if (completeAddingWhenDone)
                {
                    target.CompleteAdding();
                }
            };
            return source.Subscribe(new DelegateBasedObserver<T>(onNext, onError, onCompleted));
        }

        /// <summary>
        /// Gets a partitioner for a BlockingCollection that consumes and yields the contents of the BlockingCollection.</summary>
        /// <typeparam name="T">Specifies the type of data in the collection.</typeparam>
        /// <param name="collection">The collection for which to create a partitioner.</param>
        /// <returns>A partitioner that completely consumes and enumerates the contents of the collection.</returns>
        /// <remarks>
        /// Using this partitioner with a Parallel.ForEach loop or with PLINQ eliminates the need for those
        /// constructs to do any additional locking.  The only synchronization in place is that used by the
        /// BlockingCollection internally.
        /// </remarks>
        public static Partitioner<T> GetConsumingPartitioner<T>(this BlockingCollection<T> collection)
        {
            return new BlockingCollectionPartitioner<T>(collection);
        }

        /// <summary>Creates an IProducerConsumerCollection-facade for a BlockingCollection instance.</summary>
        /// <typeparam name="T">Specifies the type of the elements in the collection.</typeparam>
        /// <param name="collection">The BlockingCollection.</param>
        /// <returns>
        /// An IProducerConsumerCollection that wraps the provided BlockingCollection.
        /// </returns>
        public static IProducerConsumerCollection<T> ToProducerConsumerCollection<T>(this BlockingCollection<T> collection)
        {
            return collection.ToProducerConsumerCollection<T>(-1);
        }

        /// <summary>Creates an IProducerConsumerCollection-facade for a BlockingCollection instance.</summary>
        /// <typeparam name="T">Specifies the type of the elements in the collection.</typeparam>
        /// <param name="collection">The BlockingCollection.</param>
        /// <param name="millisecondsTimeout">-1 for infinite blocking add and take operations. 0 for non-blocking, 1 or greater for blocking with timeout.</param>
        /// <returns>An IProducerConsumerCollection that wraps the provided BlockingCollection.</returns>
        public static IProducerConsumerCollection<T> ToProducerConsumerCollection<T>(this BlockingCollection<T> collection, int millisecondsTimeout)
        {
            return new ProducerConsumerWrapper<T>(collection, millisecondsTimeout, new CancellationToken());
        }

        /// <summary>Creates an IProducerConsumerCollection-facade for a BlockingCollection instance.</summary>
        /// <typeparam name="T">Specifies the type of the elements in the collection.</typeparam>
        /// <param name="collection">The BlockingCollection.</param>
        /// <param name="millisecondsTimeout">-1 for infinite blocking add and take operations. 0 for non-blocking, 1 or greater for blocking with timeout.</param>
        /// <param name="cancellationToken">The CancellationToken to use for any blocking operations.</param>
        /// <returns>An IProducerConsumerCollection that wraps the provided BlockingCollection.</returns>
        public static IProducerConsumerCollection<T> ToProducerConsumerCollection<T>(this BlockingCollection<T> collection, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            return new ProducerConsumerWrapper<T>(collection, millisecondsTimeout, cancellationToken);
        }

        /// <summary>Provides a partitioner that consumes a blocking collection and yields its contents.</summary>
        /// <typeparam name="T">Specifies the type of data in the collection.</typeparam>
        private class BlockingCollectionPartitioner<T> : Partitioner<T>
        {
            /// <summary>The target collection.</summary>
            private BlockingCollection<T> _collection;

            /// <summary>Initializes the partitioner.</summary>
            /// <param name="collection">The collection to be enumerated and consumed.</param>
            internal BlockingCollectionPartitioner(BlockingCollection<T> collection)
            {
                if (collection == null)
                {
                    throw new ArgumentNullException("collection");
                }
                this._collection = collection;
            }

            /// <summary>
            /// Creates an object that can partition the underlying collection into a variable number of partitions.
            /// </summary>
            /// <returns>An object that can create partitions over the underlying data source.</returns>
            public override IEnumerable<T> GetDynamicPartitions()
            {
                return this._collection.GetConsumingEnumerable();
            }

            /// <summary>Partitions the underlying collection into the given number of partitions.</summary>
            /// <param name="partitionCount">The number of partitions to create.</param>
            /// <returns>A list containing partitionCount enumerators.</returns>
            public override IList<IEnumerator<T>> GetPartitions(int partitionCount)
            {
                if (partitionCount < 1)
                {
                    throw new ArgumentOutOfRangeException("partitionCount");
                }
                IEnumerable<T> dynamicPartitioner = this.GetDynamicPartitions();
                return (from _ in Enumerable.Range(0, partitionCount) select dynamicPartitioner.GetEnumerator()).ToArray<IEnumerator<T>>();
            }

            /// <summary>Gets whether additional partitions can be created dynamically.</summary>
            public override bool SupportsDynamicPartitions
            {
                get
                {
                    return true;
                }
            }
        }

        /// <summary>Provides a producer-consumer collection facade for a BlockingCollection.</summary>
        /// <typeparam name="T">Specifies the type of the elements in the collection.</typeparam>
        internal sealed class ProducerConsumerWrapper<T> : IProducerConsumerCollection<T>, IEnumerable<T>, ICollection, IEnumerable
        {
            private readonly CancellationToken _cancellationToken;
            private readonly BlockingCollection<T> _collection;
            private readonly int _millisecondsTimeout;

            public ProducerConsumerWrapper(BlockingCollection<T> collection, int millisecondsTimeout, CancellationToken cancellationToken)
            {
                if (collection == null)
                {
                    throw new ArgumentNullException("bc");
                }
                if (millisecondsTimeout < -1)
                {
                    throw new ArgumentOutOfRangeException("millisecondsTimeout");
                }
                this._collection = collection;
                this._millisecondsTimeout = millisecondsTimeout;
                this._cancellationToken = cancellationToken;
            }

            public void CopyTo(T[] array, int index)
            {
                this._collection.CopyTo(array, index);
            }

            public void CopyTo(Array array, int index)
            {
                ((ICollection)this._collection).CopyTo(array, index);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return ((IEnumerable<T>)this._collection).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public T[] ToArray()
            {
                return this._collection.ToArray();
            }

            public bool TryAdd(T item)
            {
                return this._collection.TryAdd(item, this._millisecondsTimeout, this._cancellationToken);
            }

            public bool TryTake(out T item)
            {
                return this._collection.TryTake(out item, this._millisecondsTimeout, this._cancellationToken);
            }

            public int Count
            {
                get
                {
                    return this._collection.Count;
                }
            }

            public bool IsSynchronized
            {
                get
                {
                    return ((ICollection) this._collection).IsSynchronized;
                }
            }

            public object SyncRoot
            {
                get
                {
                    return ((ICollection) this._collection).SyncRoot;
                }
            }
        }
    }
}

