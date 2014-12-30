using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace System.Collections.Concurrent.Partitioners
{
    /// <summary>
    /// Partitions an enumerable into chunks based on user-supplied criteria.
    /// </summary>
    internal sealed class ChunkPartitioner<T> : OrderablePartitioner<T>
    {
        private readonly Func<int, int> _nextChunkSizeFunc;
        private readonly IEnumerable<T> _source;

        public ChunkPartitioner(IEnumerable<T> source, Func<int, int> nextChunkSizeFunc) : base(true, true, true)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (nextChunkSizeFunc == null)
            {
                throw new ArgumentNullException("nextChunkSizeFunc");
            }
            this._source = source;
            this._nextChunkSizeFunc = nextChunkSizeFunc;
        }

        public ChunkPartitioner(IEnumerable<T> source, int chunkSize)
            : this(source, (int prev) => chunkSize)
        {
            if (chunkSize <= 0)
            {
                throw new ArgumentOutOfRangeException("chunkSize");
            }
        }

        public ChunkPartitioner(IEnumerable<T> source, int minChunkSize, int maxChunkSize) : this(source, ChunkPartitioner<T>.CreateFuncFromMinAndMax(minChunkSize, maxChunkSize))
        {
            if ((minChunkSize <= 0) || (minChunkSize > maxChunkSize))
            {
                throw new ArgumentOutOfRangeException("minChunkSize");
            }
        }

        private static Func<int, int> CreateFuncFromMinAndMax(int minChunkSize, int maxChunkSize)
        {
            return delegate (int prev) {
                if (prev < minChunkSize)
                {
                    return minChunkSize;
                }
                if (prev < maxChunkSize)
                {
                    int num = prev * 2;
                    if ((num < maxChunkSize) && (num >= 0))
                    {
                        return num;
                    }
                }
                return maxChunkSize;
            };
        }

        /// <summary>
        /// Creates an object that can partition the underlying collection into a variable number of
        /// partitions.
        /// </summary>
        /// <returns>
        /// An object that can create partitions over the underlying data source.
        /// </returns>
        public override IEnumerable<KeyValuePair<long, T>> GetOrderableDynamicPartitions()
        {
            return new EnumerableOfEnumerators((ChunkPartitioner<T>) this, false);
        }

        private IEnumerable<KeyValuePair<long, T>> GetOrderableDynamicPartitions(bool referenceCountForDisposal)
        {
            return new EnumerableOfEnumerators((ChunkPartitioner<T>) this, referenceCountForDisposal);
        }

        /// <summary>
        /// Partitions the underlying collection into the specified number of orderable partitions.
        /// </summary>
        /// <param name="partitionCount">The number of partitions to create.</param>
        /// <returns>An object that can create partitions over the underlying data source.</returns>
        public override IList<IEnumerator<KeyValuePair<long, T>>> GetOrderablePartitions(int partitionCount)
        {
            if (partitionCount <= 0)
            {
                throw new ArgumentOutOfRangeException("partitionCount");
            }
            IEnumerator<KeyValuePair<long, T>>[] enumeratorArray = new IEnumerator<KeyValuePair<long, T>>[partitionCount];
            IEnumerable<KeyValuePair<long, T>> orderableDynamicPartitions = this.GetOrderableDynamicPartitions(true);
            for (int i = 0; i < partitionCount; i++)
            {
                enumeratorArray[i] = orderableDynamicPartitions.GetEnumerator();
            }
            return enumeratorArray;
        }

        /// <summary>Gets whether additional partitions can be created dynamically.</summary>
        public override bool SupportsDynamicPartitions
        {
            get
            {
                return true;
            }
        }

        private class EnumerableOfEnumerators : IEnumerable<KeyValuePair<long, T>>, IEnumerable, IDisposable
        {
            private int _activeEnumerators;
            private bool _disposed;
            private long _nextSharedIndex;
            private bool _noMoreElements;
            private readonly ChunkPartitioner<T> _parentPartitioner;
            private bool _referenceCountForDisposal;
            private readonly IEnumerator<T> _sharedEnumerator;
            private readonly object _sharedLock;

            public EnumerableOfEnumerators(ChunkPartitioner<T> parentPartitioner, bool referenceCountForDisposal)
            {
                this._sharedLock = new object();
                if (parentPartitioner == null)
                {
                    throw new ArgumentNullException("parentPartitioner");
                }
                this._parentPartitioner = parentPartitioner;
                this._sharedEnumerator = parentPartitioner._source.GetEnumerator();
                this._nextSharedIndex = -1L;
                this._referenceCountForDisposal = referenceCountForDisposal;
            }

            public void Dispose()
            {
                if (!this._disposed)
                {
                    if (!this._referenceCountForDisposal)
                    {
                        this._sharedEnumerator.Dispose();
                    }
                    this._disposed = true;
                }
            }

            private void DisposeEnumerator(Enumerator enumerator)
            {
                if (this._referenceCountForDisposal && (Interlocked.Decrement(ref this._activeEnumerators) == 0))
                {
                    this._sharedEnumerator.Dispose();
                }
            }

            public IEnumerator<KeyValuePair<long, T>> GetEnumerator()
            {
                if (this._referenceCountForDisposal)
                {
                    Interlocked.Increment(ref this._activeEnumerators);
                }
                return new Enumerator((ChunkPartitioner<T>.EnumerableOfEnumerators) this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            private class Enumerator : IEnumerator<KeyValuePair<long, T>>, IDisposable, IEnumerator
            {
                private List<KeyValuePair<long, T>> _currentChunk;
                private int _currentChunkCurrentIndex;
                private bool _disposed;
                private int _lastRequestedChunkSize;
                private ChunkPartitioner<T>.EnumerableOfEnumerators _parentEnumerable;

                public Enumerator(ChunkPartitioner<T>.EnumerableOfEnumerators parentEnumerable)
                {
                    this._currentChunk = new List<KeyValuePair<long, T>>();
                    if (parentEnumerable == null)
                    {
                        throw new ArgumentNullException("parentEnumerable");
                    }
                    this._parentEnumerable = parentEnumerable;
                }

                public void Dispose()
                {
                    if (!this._disposed)
                    {
                        this._parentEnumerable.DisposeEnumerator((ChunkPartitioner<T>.EnumerableOfEnumerators.Enumerator) this);
                        this._disposed = true;
                    }
                }

                public bool MoveNext()
                {
                    if (this._disposed)
                    {
                        throw new ObjectDisposedException(base.GetType().Name);
                    }
                    this._currentChunkCurrentIndex++;
                    if ((this._currentChunkCurrentIndex < 0) || (this._currentChunkCurrentIndex >= this._currentChunk.Count))
                    {
                        int num = this._parentEnumerable._parentPartitioner._nextChunkSizeFunc(this._lastRequestedChunkSize);
                        if (num <= 0)
                        {
                            throw new InvalidOperationException("Invalid chunk size requested: chunk sizes must be positive.");
                        }
                        this._lastRequestedChunkSize = num;
                        this._currentChunk.Clear();
                        this._currentChunkCurrentIndex = 0;
                        if (num > this._currentChunk.Capacity)
                        {
                            this._currentChunk.Capacity = num;
                        }
                        lock (this._parentEnumerable._sharedEnumerator)
                        {
                            if (this._parentEnumerable._noMoreElements)
                            {
                                return false;
                            }
                            for (int i = 0; i < num; i++)
                            {
                                if (!this._parentEnumerable._sharedEnumerator.MoveNext())
                                {
                                    this._parentEnumerable._noMoreElements = true;
                                    return (this._currentChunk.Count > 0);
                                }
                                this._parentEnumerable._nextSharedIndex += 1L;
                                this._currentChunk.Add(new KeyValuePair<long, T>(this._parentEnumerable._nextSharedIndex, this._parentEnumerable._sharedEnumerator.Current));
                            }
                        }
                    }
                    return true;
                }

                public void Reset()
                {
                    throw new NotSupportedException();
                }

                public KeyValuePair<long, T> Current
                {
                    get
                    {
                        if (this._currentChunkCurrentIndex >= this._currentChunk.Count)
                        {
                            throw new InvalidOperationException("There is no current item.");
                        }
                        return this._currentChunk[this._currentChunkCurrentIndex];
                    }
                }

                object IEnumerator.Current
                {
                    get
                    {
                        return this.Current;
                    }
                }
            }
        }
    }
}

