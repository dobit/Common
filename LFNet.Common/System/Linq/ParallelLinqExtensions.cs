using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace System.Linq
{
    /// <summary>Extension methods for Parallel LINQ.</summary>
    public static class ParallelLinqExtensions
    {
        /// <summary>This is the method to opt into Parallel LINQ.</summary>
        /// <typeparam name="TSource">Specifies the type of elements provided to the query.</typeparam>
        /// <param name="source">The source query.</param>
        /// <param name="parallelOptions">The options to use for query processing.</param>
        /// <returns>The source as a ParallelQuery to bind to ParallelEnumerable extension methods.</returns>
        public static ParallelQuery<TSource> AsParallel<TSource>(this IEnumerable<TSource> source, ParallelLinqOptions parallelOptions)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if ((parallelOptions.TaskScheduler != null) && (parallelOptions.TaskScheduler != TaskScheduler.Default))
            {
                throw new ArgumentException("Parallel LINQ only supports the default TaskScheduler.");
            }
            ParallelQuery<TSource> query = source.AsParallel<TSource>();
            if (parallelOptions.Ordered)
            {
                query = query.AsOrdered<TSource>();
            }
            if (parallelOptions.CancellationToken.CanBeCanceled)
            {
                query = query.WithCancellation<TSource>(parallelOptions.CancellationToken);
            }
            if (parallelOptions.MaxDegreeOfParallelism >= 1)
            {
                query = query.WithDegreeOfParallelism<TSource>(parallelOptions.MaxDegreeOfParallelism);
            }
            if (parallelOptions.ExecutionMode != ParallelExecutionMode.Default)
            {
                query = query.WithExecutionMode<TSource>(parallelOptions.ExecutionMode);
            }
            if (parallelOptions.MergeOptions != ParallelMergeOptions.Default)
            {
                query = query.WithMergeOptions<TSource>(parallelOptions.MergeOptions);
            }
            return query;
        }

        /// <summary>Implements a map-reduce operation.</summary>
        /// <typeparam name="TSource">Specifies the type of the source elements.</typeparam>
        /// <typeparam name="TMapped">Specifies the type of the mapped elements.</typeparam>
        /// <typeparam name="TKey">Specifies the type of the element keys.</typeparam>
        /// <typeparam name="TResult">Specifies the type of the results.</typeparam>
        /// <param name="source">The source elements.</param>
        /// <param name="map">A function used to get the target data from a source element.</param>
        /// <param name="keySelector">A function used to get a key from the target data.</param>
        /// <param name="reduce">A function used to reduce a group of elements.</param>
        /// <returns>The result elements of the reductions.</returns>
        public static ParallelQuery<TResult> MapReduce<TSource, TMapped, TKey, TResult>(this ParallelQuery<TSource> source, Func<TSource, TMapped> map, Func<TMapped, TKey> keySelector, Func<IGrouping<TKey, TMapped>, TResult> reduce)
        {
            return source.Select<TSource, TMapped>(map).GroupBy<TMapped, TKey>(keySelector).Select<IGrouping<TKey, TMapped>, TResult>(reduce);
        }

        /// <summary>Implements a map-reduce operation.</summary>
        /// <typeparam name="TSource">Specifies the type of the source elements.</typeparam>
        /// <typeparam name="TMapped">Specifies the type of the mapped elements.</typeparam>
        /// <typeparam name="TKey">Specifies the type of the element keys.</typeparam>
        /// <typeparam name="TResult">Specifies the type of the results.</typeparam>
        /// <param name="source">The source elements.</param>
        /// <param name="map">A function used to get an enumerable of target data from a source element.</param>
        /// <param name="keySelector">A function used to get a key from target data.</param>
        /// <param name="reduce">A function used to reduce a group of elements to an enumerable of results.</param>
        /// <returns>The result elements of the reductions.</returns>
        public static ParallelQuery<TResult> MapReduce<TSource, TMapped, TKey, TResult>(this ParallelQuery<TSource> source, Func<TSource, IEnumerable<TMapped>> map, Func<TMapped, TKey> keySelector, Func<IGrouping<TKey, TMapped>, IEnumerable<TResult>> reduce)
        {
            return source.SelectMany<TSource, TMapped>(map).GroupBy<TMapped, TKey>(keySelector).SelectMany<IGrouping<TKey, TMapped>, TResult>(reduce);
        }

        /// <summary>Runs the query and outputs its results into the target collection.</summary>
        /// <typeparam name="TSource">Specifies the type of elements output from the query.</typeparam>
        /// <param name="source">The source query.</param>
        /// <param name="target">The target collection.</param>
        public static void OutputToProducerConsumerCollection<TSource>(this ParallelQuery<TSource> source, IProducerConsumerCollection<TSource> target)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            source.ForAll<TSource>(item => target.TryAdd(item));
        }

        /// <summary>Takes the top elements as if they were sorted.</summary>
        /// <typeparam name="TSource">Specifies the type of the elements.</typeparam>
        /// <typeparam name="TKey">Specifies the type of the keys used to compare elements.</typeparam>
        /// <param name="source">The source elements.</param>
        /// <param name="keySelector">A function used to extract a key from each element.</param>
        /// <param name="count">The number of elements to take.</param>
        /// <returns></returns>
        public static IEnumerable<TSource> TakeTop<TSource, TKey>(this ParallelQuery<TSource> source, Func<TSource, TKey> keySelector, int count)
        {
            DescendingDefaultComparer<TKey> comparer = new DescendingDefaultComparer<TKey>();
            return source.Aggregate<TSource, SortedTopN<TKey, TSource>, IEnumerable<TSource>>(() => new SortedTopN<TKey, TSource>(count, comparer), delegate (SortedTopN<TKey, TSource> accum, TSource item) {
                accum.Add(keySelector(item), item);
                return accum;
            }, delegate (SortedTopN<TKey, TSource> accum1, SortedTopN<TKey, TSource> accum2) {
                foreach (KeyValuePair<TKey, TSource> pair in accum2)
                {
                    accum1.Add(pair);
                }
                return accum1;
            }, accum => accum.Values);
        }

        /// <summary>A comparer that comparers using the inverse of the default comparer.</summary>
        /// <typeparam name="T">Specifies the type being compared.</typeparam>
        private class DescendingDefaultComparer<T> : IComparer<T>
        {
            private static Comparer<T> _defaultComparer;

            static DescendingDefaultComparer()
            {
                ParallelLinqExtensions.DescendingDefaultComparer<T>._defaultComparer = Comparer<T>.Default;
            }

            public int Compare(T x, T y)
            {
                return ParallelLinqExtensions.DescendingDefaultComparer<T>._defaultComparer.Compare(y, x);
            }
        }
    }
}

