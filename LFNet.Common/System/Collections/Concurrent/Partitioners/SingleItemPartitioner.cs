using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Collections.Concurrent.Partitioners
{
    /// <summary>Partitions a data source one item at a time.</summary>
    public static class SingleItemPartitioner
    {
        /// <summary>Creates a partitioner for an enumerable that partitions it one item at a time.</summary>
        /// <typeparam name="T">Specifies the type of data contained in the enumerable.</typeparam>
        /// <param name="source">The source enumerable to be partitioned.</param>
        /// <returns>The partitioner.</returns>
        public static OrderablePartitioner<T> Create<T>(IEnumerable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (source is IList<T>)
            {
                return new SingleItemIListPartitioner<T>((IList<T>) source);
            }
            return new SingleItemEnumerablePartitioner<T>(source);
        }

        /// <summary>Partitions an enumerable one item at a time.</summary>
        /// <typeparam name="T">Specifies the type of data contained in the list.</typeparam>
        private sealed class SingleItemEnumerablePartitioner<T> : OrderablePartitioner<T>
        {
            /// <summary>The enumerable to be partitioned.</summary>
            private readonly IEnumerable<T> _source;

            /// <summary>Initializes the partitioner.</summary>
            /// <param name="source">The enumerable to be partitioned.</param>
            internal SingleItemEnumerablePartitioner(IEnumerable<T> source) : base(true, false, true)
            {
                this._source = source;
            }

            /// <summary>Gets a list of the specified static number of partitions.</summary>
            /// <returns>The list of created partitions ready to be iterated.</returns>
            public override IEnumerable<KeyValuePair<long, T>> GetOrderableDynamicPartitions()
            {
                return new DynamicGenerator(this._source.GetEnumerator(), true);
            }

            public override IList<IEnumerator<KeyValuePair<long, T>>> GetOrderablePartitions(int partitionCount)
            {
                if (partitionCount < 1)
                {
                    throw new ArgumentOutOfRangeException("partitionCount");
                }
                DynamicGenerator dynamicPartitioner = new DynamicGenerator(this._source.GetEnumerator(), false);
                return (from i in Enumerable.Range(0, partitionCount) select dynamicPartitioner.GetEnumerator()).ToList<IEnumerator<KeyValuePair<long, T>>>();
            }

            /// <summary>Gets whether this partitioner supports dynamic partitioning (it does).</summary>
            public override bool SupportsDynamicPartitions
            {
                get
                {
                    return true;
                }
            }

            /// <summary>Dynamically generates a partitions on a shared enumerator.</summary>
            private class DynamicGenerator : IEnumerable<KeyValuePair<long, T>>, IEnumerable, IDisposable
            {
                /// <summary>Whether this dynamic partitioner has been disposed.</summary>
                private bool _disposed;
                /// <summary>The next available position to be yielded.</summary>
                private long _nextAvailablePosition;
                /// <summary>The number of partitions remaining to be disposed, potentially including this dynamic generator.</summary>
                private int _remainingPartitions;
                /// <summary>The source enumerator shared amongst all partitions.</summary>
                private readonly IEnumerator<T> _sharedEnumerator;

                /// <summary>Initializes the dynamic generator.</summary>
                /// <param name="sharedEnumerator">The enumerator shared by all partitions.</param>
                /// <param name="requiresDisposal">Whether this generator will be disposed.</param>
                public DynamicGenerator(IEnumerator<T> sharedEnumerator, bool requiresDisposal)
                {
                    this._sharedEnumerator = sharedEnumerator;
                    this._nextAvailablePosition = -1L;
                    this._remainingPartitions = requiresDisposal ? 1 : 0;
                }

                /// <summary>Increments the number of partitions in use and returns a new partition.</summary>
                /// <returns>The new partition.</returns>
                public IEnumerator<KeyValuePair<long, T>> GetEnumerator()
                {
                    Interlocked.Increment(ref this._remainingPartitions);
                    return this.GetEnumeratorCore();
                }

                /// <summary>Creates a partition.</summary>
                /// <returns>The new partition.</returns>
                private IEnumerator<KeyValuePair<long, T>> GetEnumeratorCore()
                {
                    while (true)
                    {
                        long iteratorVariable1;
                        T current;
                        lock (this._sharedEnumerator)
                        {
                            if (this._sharedEnumerator.MoveNext())
                            {
                                long num2;
                                this._nextAvailablePosition = (num2 = this._nextAvailablePosition) + 1L;
                                iteratorVariable1 = num2;
                                current = this._sharedEnumerator.Current;
                            }
                            else
                            {
                                break;
                            }
                        }
                        yield return new KeyValuePair<long, T>(iteratorVariable1, current);
                    }
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return this.GetEnumerator();
                }

                /// <summary>Closes the shared enumerator if all other partitions have completed.</summary>
                void IDisposable.Dispose()
                {
                    if (!this._disposed && (Interlocked.Decrement(ref this._remainingPartitions) == 0))
                    {
                        this._disposed = true;
                        this._sharedEnumerator.Dispose();
                    }
                }

            }
        }

        /// <summary>Partitions a list one item at a time.</summary>
        /// <typeparam name="T">Specifies the type of data contained in the list.</typeparam>
        private sealed class SingleItemIListPartitioner<T> : OrderablePartitioner<T>
        {
            /// <summary>The list to be partitioned.</summary>
            private readonly IList<T> _source;

            /// <summary>Initializes the partitioner.</summary>
            /// <param name="source">The list to be partitioned.</param>
            internal SingleItemIListPartitioner(IList<T> source) : base(true, false, true)
            {
                this._source = source;
            }

            /// <summary>Creates a dynamic partitioner for creating a dynamic number of partitions.</summary>
            /// <returns>The dynamic partitioner.</returns>
            public override IEnumerable<KeyValuePair<long, T>> GetOrderableDynamicPartitions()
            {
                return SingleItemPartitioner.SingleItemIListPartitioner<T>.GetOrderableDynamicPartitionsCore(this._source, new StrongBox<int>(0));
            }

            /// <summary>An enumerable that creates individual enumerators that all work together to partition the list.</summary>
            /// <param name="source">The list being partitioned.</param>
            /// <param name="nextIteration">An integer shared between partitions denoting the next available index in the source.</param>
            /// <returns>An enumerable that generates enumerators which participate in partitioning the list.</returns>
            private static IEnumerable<KeyValuePair<long, T>> GetOrderableDynamicPartitionsCore(IList<T> source, StrongBox<int> nextIteration)
            {
                while (true)
                {
                    int iteratorVariable0 = Interlocked.Increment(ref nextIteration.Value) - 1;
                    if ((iteratorVariable0 < 0) || (iteratorVariable0 >= source.Count))
                    {
                        yield break;
                    }
                    yield return new KeyValuePair<long, T>((long) iteratorVariable0, source[iteratorVariable0]);
                }
            }

            /// <summary>Gets a list of the specified static number of partitions.</summary>
            /// <param name="partitionCount">The static number of partitions to create.</param>
            /// <returns>The list of created partitions ready to be iterated.</returns>
            public override IList<IEnumerator<KeyValuePair<long, T>>> GetOrderablePartitions(int partitionCount)
            {
                if (partitionCount < 1)
                {
                    throw new ArgumentOutOfRangeException("partitionCount");
                }
                IEnumerable<KeyValuePair<long, T>> dynamicPartitioner = this.GetOrderableDynamicPartitions();
                return (from i in Enumerable.Range(0, partitionCount) select dynamicPartitioner.GetEnumerator()).ToList<IEnumerator<KeyValuePair<long, T>>>();
            }

            /// <summary>Gets whether this partitioner supports dynamic partitioning (it does).</summary>
            public override bool SupportsDynamicPartitions
            {
                get
                {
                    return true;
                }
            }

          
        }
    }
}

