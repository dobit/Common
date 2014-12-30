using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent.Partitioners;

namespace System.Threading.Algorithms
{
    /// <summary>
    /// Provides parallelized algorithms for common operations.
    /// </summary>
    public static class ParallelAlgorithms
    {
        private static ParallelOptions s_defaultParallelOptions = new ParallelOptions();

        /// <summary>Computes a sequential exclusive prefix scan over the array using the specified function.</summary>
        /// <param name="arr">The data, which will be overwritten with the computed prefix scan.</param>
        /// <param name="function">The function to use for the scan.</param>
        /// <param name="lowerBoundInclusive">The inclusive lower bound of the array at which to start the scan.</param>
        /// <param name="upperBoundExclusive">The exclusive upper bound of the array at which to end the scan.</param>
        public static void ExclusiveScanInPlaceSerial<T>(T[] arr, Func<T, T, T> function, int lowerBoundInclusive, int upperBoundExclusive)
        {
            T local = arr[lowerBoundInclusive];
            arr[lowerBoundInclusive] = default(T);
            for (int i = lowerBoundInclusive + 1; i < upperBoundExclusive; i++)
            {
                T local2 = local;
                local = function(local, arr[i]);
                arr[i] = local2;
            }
        }

        /// <summary>Filters an input list, running a predicate over each element of the input.</summary>
        /// <typeparam name="T">Specifies the type of data in the list.</typeparam>
        /// <param name="input">The list to be filtered.</param>
        /// <param name="predicate">The predicate to use to determine which elements pass.</param>
        /// <returns>A new list containing all those elements from the input that passed the filter.</returns>
        public static IList<T> Filter<T>(IList<T> input, Func<T, bool> predicate)
        {
            return Filter<T>(input, s_defaultParallelOptions, predicate);
        }

        /// <summary>Filters an input list, running a predicate over each element of the input.</summary>
        /// <typeparam name="T">Specifies the type of data in the list.</typeparam>
        /// <param name="input">The list to be filtered.</param>
        /// <param name="parallelOptions">Options to use for the execution of this filter.</param>
        /// <param name="predicate">The predicate to use to determine which elements pass.</param>
        /// <returns>A new list containing all those elements from the input that passed the filter.</returns>
        public static IList<T> Filter<T>(IList<T> input, ParallelOptions parallelOptions, Func<T, bool> predicate)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            if (parallelOptions == null)
            {
                throw new ArgumentNullException("parallelOptions");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            List<T> results = new List<T>(input.Count);
            Parallel.For<List<T>>(0, input.Count, parallelOptions, () => new List<T>(input.Count), delegate (int i, ParallelLoopState loop, List<T> localList) {
                T arg = input[i];
                if (predicate(arg))
                {
                    localList.Add(arg);
                }
                return localList;
            }, delegate (List<T> localList) {
                lock (results)
                {
                    results.AddRange(localList);
                }
            });
            return results;
        }

        /// <summary>Executes a for loop in which iterations may run in parallel.</summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        public static void For(BigInteger fromInclusive, BigInteger toExclusive, Action<BigInteger> body)
        {
            For(fromInclusive, toExclusive, s_defaultParallelOptions, body);
        }

        /// <summary>Executes a for loop in which iterations may run in parallel.</summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="options">A System.Threading.Tasks.ParallelOptions instance that configures the behavior of this operation.</param>
        /// <param name="body">The delegate that is invoked once per iteration.</param>
        public static void For(BigInteger fromInclusive, BigInteger toExclusive, ParallelOptions options, Action<BigInteger> body)
        {
            Action<long> action = null;
            BigInteger integer = toExclusive - fromInclusive;
            if (integer > 0L)
            {
                if (integer <= 0x7fffffffffffffffL)
                {
                    if (action == null)
                    {
                        action = i => body(i + fromInclusive);
                    }
                    Parallel.For(0L, (long) integer, options, action);
                }
                else
                {
                    Parallel.ForEach<BigInteger>(Range(fromInclusive, toExclusive), options, body);
                }
            }
        }

        /// <summary>Executes a for loop over ranges in which iterations may run in parallel. </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="body">The delegate that is invoked once per range.</param>
        /// <returns>A ParallelLoopResult structure that contains information on what portion of the loop completed.</returns>
        public static ParallelLoopResult ForRange(int fromInclusive, int toExclusive, Action<int, int> body)
        {
            return ForRange(fromInclusive, toExclusive, s_defaultParallelOptions, body);
        }

        /// <summary>Executes a for loop over ranges in which iterations may run in parallel. </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="body">The delegate that is invoked once per range.</param>
        /// <returns>A ParallelLoopResult structure that contains information on what portion of the loop completed.</returns>
        public static ParallelLoopResult ForRange(int fromInclusive, int toExclusive, Action<int, int, ParallelLoopState> body)
        {
            return ForRange(fromInclusive, toExclusive, s_defaultParallelOptions, body);
        }

        /// <summary>Executes a for loop over ranges in which iterations may run in parallel. </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="body">The delegate that is invoked once per range.</param>
        /// <returns>A ParallelLoopResult structure that contains information on what portion of the loop completed.</returns>
        public static ParallelLoopResult ForRange(long fromInclusive, long toExclusive, Action<long, long> body)
        {
            return ForRange(fromInclusive, toExclusive, s_defaultParallelOptions, body);
        }

        /// <summary>Executes a for loop over ranges in which iterations may run in parallel. </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="body">The delegate that is invoked once per range.</param>
        /// <returns>A ParallelLoopResult structure that contains information on what portion of the loop completed.</returns>
        public static ParallelLoopResult ForRange(long fromInclusive, long toExclusive, Action<long, long, ParallelLoopState> body)
        {
            return ForRange(fromInclusive, toExclusive, s_defaultParallelOptions, body);
        }

        /// <summary>Executes a for loop over ranges in which iterations may run in parallel. </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="parallelOptions">A ParallelOptions instance that configures the behavior of this operation.</param>
        /// <param name="body">The delegate that is invoked once per range.</param>
        /// <returns>A ParallelLoopResult structure that contains information on what portion of the loop completed.</returns>
        public static ParallelLoopResult ForRange(int fromInclusive, int toExclusive, ParallelOptions parallelOptions, Action<int, int> body)
        {
            if (parallelOptions == null)
            {
                throw new ArgumentNullException("parallelOptions");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            return Parallel.ForEach<Tuple<int, int>>(Partitioner.Create(fromInclusive, toExclusive), parallelOptions, delegate (Tuple<int, int> range) {
                body(range.Item1, range.Item2);
            });
        }

        /// <summary>Executes a for loop over ranges in which iterations may run in parallel. </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="parallelOptions">A ParallelOptions instance that configures the behavior of this operation.</param>
        /// <param name="body">The delegate that is invoked once per range.</param>
        /// <returns>A ParallelLoopResult structure that contains information on what portion of the loop completed.</returns>
        public static ParallelLoopResult ForRange(int fromInclusive, int toExclusive, ParallelOptions parallelOptions, Action<int, int, ParallelLoopState> body)
        {
            if (parallelOptions == null)
            {
                throw new ArgumentNullException("parallelOptions");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            return Parallel.ForEach<Tuple<int, int>>(Partitioner.Create(fromInclusive, toExclusive), parallelOptions, delegate (Tuple<int, int> range, ParallelLoopState loopState) {
                body(range.Item1, range.Item2, loopState);
            });
        }

        /// <summary>Executes a for loop over ranges in which iterations may run in parallel. </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="parallelOptions">A ParallelOptions instance that configures the behavior of this operation.</param>
        /// <param name="body">The delegate that is invoked once per range.</param>
        /// <returns>A ParallelLoopResult structure that contains information on what portion of the loop completed.</returns>
        public static ParallelLoopResult ForRange(long fromInclusive, long toExclusive, ParallelOptions parallelOptions, Action<long, long> body)
        {
            if (parallelOptions == null)
            {
                throw new ArgumentNullException("parallelOptions");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            return Parallel.ForEach<Tuple<long, long>>(Partitioner.Create(fromInclusive, toExclusive), parallelOptions, delegate (Tuple<long, long> range) {
                body(range.Item1, range.Item2);
            });
        }

        /// <summary>Executes a for loop over ranges in which iterations may run in parallel. </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="parallelOptions">A ParallelOptions instance that configures the behavior of this operation.</param>
        /// <param name="body">The delegate that is invoked once per range.</param>
        /// <returns>A ParallelLoopResult structure that contains information on what portion of the loop completed.</returns>
        public static ParallelLoopResult ForRange(long fromInclusive, long toExclusive, ParallelOptions parallelOptions, Action<long, long, ParallelLoopState> body)
        {
            if (parallelOptions == null)
            {
                throw new ArgumentNullException("parallelOptions");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            return Parallel.ForEach<Tuple<long, long>>(Partitioner.Create(fromInclusive, toExclusive), parallelOptions, delegate (Tuple<long, long> range, ParallelLoopState loopState) {
                body(range.Item1, range.Item2, loopState);
            });
        }

        /// <summary>Executes a for loop over ranges in which iterations may run in parallel. </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="localInit">The function delegate that returns the initial state of the local data for each thread.</param>
        /// <param name="body">The delegate that is invoked once per range.</param>
        /// <param name="localFinally">The delegate that performs a final action on the local state of each thread.</param>
        /// <returns>A ParallelLoopResult structure that contains information on what portion of the loop completed.</returns>
        public static ParallelLoopResult ForRange<TLocal>(int fromInclusive, int toExclusive, Func<TLocal> localInit, Func<int, int, ParallelLoopState, TLocal, TLocal> body, Action<TLocal> localFinally)
        {
            return ForRange<TLocal>(fromInclusive, toExclusive, s_defaultParallelOptions, localInit, body, localFinally);
        }

        /// <summary>Executes a for loop over ranges in which iterations may run in parallel. </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="localInit">The function delegate that returns the initial state of the local data for each thread.</param>
        /// <param name="body">The delegate that is invoked once per range.</param>
        /// <param name="localFinally">The delegate that performs a final action on the local state of each thread.</param>
        /// <returns>A ParallelLoopResult structure that contains information on what portion of the loop completed.</returns>
        public static ParallelLoopResult ForRange<TLocal>(long fromInclusive, long toExclusive, Func<TLocal> localInit, Func<long, long, ParallelLoopState, TLocal, TLocal> body, Action<TLocal> localFinally)
        {
            return ForRange<TLocal>(fromInclusive, toExclusive, s_defaultParallelOptions, localInit, body, localFinally);
        }

        /// <summary>Executes a for loop over ranges in which iterations may run in parallel. </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="parallelOptions"></param>
        /// <param name="localInit">The function delegate that returns the initial state of the local data for each thread.</param>
        /// <param name="body">The delegate that is invoked once per range.</param>
        /// <param name="localFinally">The delegate that performs a final action on the local state of each thread.</param>
        /// <returns>A ParallelLoopResult structure that contains information on what portion of the loop completed.</returns>
        public static ParallelLoopResult ForRange<TLocal>(int fromInclusive, int toExclusive, ParallelOptions parallelOptions, Func<TLocal> localInit, Func<int, int, ParallelLoopState, TLocal, TLocal> body, Action<TLocal> localFinally)
        {
            if (parallelOptions == null)
            {
                throw new ArgumentNullException("parallelOptions");
            }
            if (localInit == null)
            {
                throw new ArgumentNullException("localInit");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            if (localFinally == null)
            {
                throw new ArgumentNullException("localFinally");
            }
            return Parallel.ForEach<Tuple<int, int>, TLocal>(Partitioner.Create(fromInclusive, toExclusive), parallelOptions, localInit, (range, loopState, x) => body(range.Item1, range.Item2, loopState, x), localFinally);
        }

        /// <summary>Executes a for loop over ranges in which iterations may run in parallel. </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="parallelOptions"></param>
        /// <param name="localInit">The function delegate that returns the initial state of the local data for each thread.</param>
        /// <param name="body">The delegate that is invoked once per range.</param>
        /// <param name="localFinally">The delegate that performs a final action on the local state of each thread.</param>
        /// <returns>A ParallelLoopResult structure that contains information on what portion of the loop completed.</returns>
        public static ParallelLoopResult ForRange<TLocal>(long fromInclusive, long toExclusive, ParallelOptions parallelOptions, Func<TLocal> localInit, Func<long, long, ParallelLoopState, TLocal, TLocal> body, Action<TLocal> localFinally)
        {
            if (parallelOptions == null)
            {
                throw new ArgumentNullException("parallelOptions");
            }
            if (localInit == null)
            {
                throw new ArgumentNullException("localInit");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            if (localFinally == null)
            {
                throw new ArgumentNullException("localFinally");
            }
            return Parallel.ForEach<Tuple<long, long>, TLocal>(Partitioner.Create(fromInclusive, toExclusive), parallelOptions, localInit, (range, loopState, x) => body(range.Item1, range.Item2, loopState, x), localFinally);
        }

        /// <summary>Computes a parallel inclusive prefix scan over the array using the specified function.</summary>
        public static void InclusiveScanInPlaceParallel<T>(T[] arr, Func<T, T, T> function)
        {
            Action<Barrier> postPhaseAction = null;
            int processorCount = Environment.ProcessorCount;
            T[] intermediatePartials = new T[processorCount];
            if (postPhaseAction == null)
            {
                postPhaseAction = _ => ExclusiveScanInPlaceSerial<T>(intermediatePartials, function, 0, intermediatePartials.Length);
            }
            using (Barrier phaseBarrier = new Barrier(processorCount, postPhaseAction))
            {
                int num2 = arr.Length / processorCount;
                int num3 = 0;
                Task[] tasks = new Task[processorCount];
                int num4 = 0;
                while (num4 < processorCount)
                {
                    int rangeNum = num4;
                    int lowerRangeInclusive = num3;
                    int upperRangeExclusive = (num4 < (processorCount - 1)) ? (num3 + num2) : arr.Length;
                    tasks[rangeNum] = Task.Factory.StartNew(delegate {
                        InclusiveScanInPlaceSerial<T>(arr, function, lowerRangeInclusive, upperRangeExclusive, 1);
                        intermediatePartials[rangeNum] = arr[upperRangeExclusive - 1];
                        phaseBarrier.SignalAndWait();
                        if (rangeNum != 0)
                        {
                            for (int j = lowerRangeInclusive; j < upperRangeExclusive; j++)
                            {
                                arr[j] = function(intermediatePartials[rangeNum], arr[j]);
                            }
                        }
                    });
                    num4++;
                    num3 += num2;
                }
                Task.WaitAll(tasks);
            }
        }

        /// <summary>Computes a sequential prefix scan over the array using the specified function.</summary>
        /// <typeparam name="T">The type of the data in the array.</typeparam>
        /// <param name="arr">The data, which will be overwritten with the computed prefix scan.</param>
        /// <param name="function">The function to use for the scan.</param>
        /// <param name="arrStart">The start of the data in arr over which the scan is being computed.</param>
        /// <param name="arrLength">The length of the data in arr over which the scan is being computed.</param>
        /// <param name="skip">The inclusive distance between elements over which the scan is being computed.</param>
        /// <remarks>No parameter validation is performed.</remarks>
        private static void InclusiveScanInPlaceSerial<T>(T[] arr, Func<T, T, T> function, int arrStart, int arrLength, int skip)
        {
            for (int i = arrStart; (i + skip) < arrLength; i += skip)
            {
                arr[i + skip] = function(arr[i], arr[i + skip]);
            }
        }

        /// <summary>Computes a parallel prefix scan over the array using the specified function.</summary>
        /// <typeparam name="T">The type of the data in the array.</typeparam>
        /// <param name="arr">The data, which will be overwritten with the computed prefix scan.</param>
        /// <param name="function">The function to use for the scan.</param>
        /// <param name="arrStart">The start of the data in arr over which the scan is being computed.</param>
        /// <param name="arrLength">The length of the data in arr over which the scan is being computed.</param>
        /// <param name="skip">The inclusive distance between elements over which the scan is being computed.</param>
        /// <remarks>No parameter validation is performed.</remarks>
        private static void InclusiveScanInPlaceWithLoadBalancingParallel<T>(T[] arr, Func<T, T, T> function, int arrStart, int arrLength, int skip)
        {
            if (arrLength > 1)
            {
                int toExclusive = arrLength / 2;
                Parallel.For(0, toExclusive, delegate (int i) {
                    int index = arrStart + ((i * 2) * skip);
                    arr[index + skip] = function(arr[index], arr[index + skip]);
                });
                InclusiveScanInPlaceWithLoadBalancingParallel<T>(arr, function, arrStart + skip, toExclusive, skip * 2);
                Parallel.For(0, ((arrLength % 2) == 0) ? (toExclusive - 1) : toExclusive, delegate (int i) {
                    int index = (arrStart + ((i * 2) * skip)) + skip;
                    arr[index + skip] = function(arr[index], arr[index + skip]);
                });
            }
        }

        private static IEnumerable<bool> IterateUntilFalse(Func<bool> condition)
        {
            while (true)
            {
                if (!condition())
                {
                    yield break;
                }
                yield return true;
            }
        }

        /// <summary>Executes a map operation, converting an input list into an output list, in parallel.</summary>
        /// <typeparam name="TInput">Specifies the type of the input data.</typeparam>
        /// <typeparam name="TOutput">Specifies the type of the output data.</typeparam>
        /// <param name="input">The input list to be mapped used the transform function.</param>
        /// <param name="transform">The transform function to use to map the input data to the output data.</param>
        /// <returns>The output data, transformed using the transform function.</returns>
        public static TOutput[] Map<TInput, TOutput>(IList<TInput> input, Func<TInput, TOutput> transform)
        {
            return Map<TInput, TOutput>(input, s_defaultParallelOptions, transform);
        }

        /// <summary>Executes a map operation, converting an input list into an output list, in parallel.</summary>
        /// <typeparam name="TInput">Specifies the type of the input data.</typeparam>
        /// <typeparam name="TOutput">Specifies the type of the output data.</typeparam>
        /// <param name="input">The input list to be mapped used the transform function.</param>
        /// <param name="parallelOptions">A ParallelOptions instance that configures the behavior of this operation.</param>
        /// <param name="transform">The transform function to use to map the input data to the output data.</param>
        /// <returns>The output data, transformed using the transform function.</returns>
        public static TOutput[] Map<TInput, TOutput>(IList<TInput> input, ParallelOptions parallelOptions, Func<TInput, TOutput> transform)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            if (parallelOptions == null)
            {
                throw new ArgumentNullException("parallelOptions");
            }
            if (transform == null)
            {
                throw new ArgumentNullException("transform");
            }
            TOutput[] output = new TOutput[input.Count];
            Parallel.For(0, input.Count, parallelOptions, delegate (int i) {
                output[i] = transform(input[i]);
            });
            return output;
        }

        /// <summary>Repeatedly executes an operation in parallel while the specified condition evaluates to true.</summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="body">The loop body.</param>
        public static void ParallelWhile(Func<bool> condition, Action body)
        {
            ParallelWhile(s_defaultParallelOptions, condition, body);
        }

        /// <summary>Repeatedly executes an operation in parallel while the specified condition evaluates to true.</summary>
        /// <param name="parallelOptions">A ParallelOptions instance that configures the behavior of this operation.</param>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="body">The loop body.</param>
        public static void ParallelWhile(ParallelOptions parallelOptions, Func<bool> condition, Action body)
        {
            if (parallelOptions == null)
            {
                throw new ArgumentNullException("parallelOptions");
            }
            if (condition == null)
            {
                throw new ArgumentNullException("condition");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            Parallel.ForEach<bool>((Partitioner<bool>) SingleItemPartitioner.Create<bool>(IterateUntilFalse(condition)), parallelOptions, (Action<bool>) (ignored => body()));
        }

        /// <summary>Creates an enumerable that iterates the range [fromInclusive, toExclusive).</summary>
        /// <param name="fromInclusive">The lower bound, inclusive.</param>
        /// <param name="toExclusive">The upper bound, exclusive.</param>
        /// <returns>The enumerable of the range.</returns>
        private static IEnumerable<BigInteger> Range(BigInteger fromInclusive, BigInteger toExclusive)
        {
            BigInteger iteratorVariable0 = fromInclusive;
            while (true)
            {
                if (iteratorVariable0 >= toExclusive)
                {
                    yield break;
                }
                yield return iteratorVariable0;
                iteratorVariable0 ++;
            }
        }

        /// <summary>Reduces the input data using the specified aggregation operation.</summary>
        /// <typeparam name="T">Specifies the type of data being aggregated.</typeparam>
        /// <param name="input">The input data to be reduced.</param>
        /// <param name="seed">The seed to use to initialize the operation; this seed may be used multiple times.</param>
        /// <param name="associativeCommutativeOperation">The reduction operation.</param>
        /// <returns>The reduced value.</returns>
        public static T Reduce<T>(IList<T> input, T seed, Func<T, T, T> associativeCommutativeOperation)
        {
            return Reduce<T>(input, s_defaultParallelOptions, seed, associativeCommutativeOperation);
        }

        /// <summary>Reduces the input data using the specified aggregation operation.</summary>
        /// <typeparam name="T">Specifies the type of data being aggregated.</typeparam>
        /// <param name="input">The input data to be reduced.</param>
        /// <param name="parallelOptions">A ParallelOptions instance that configures the behavior of this operation.</param>
        /// <param name="seed">The seed to use to initialize the operation; this seed may be used multiple times.</param>
        /// <param name="associativeCommutativeOperation">The reduction operation.</param>
        /// <returns>The reduced value.</returns>
        public static T Reduce<T>(IList<T> input, ParallelOptions parallelOptions, T seed, Func<T, T, T> associativeCommutativeOperation)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            return Reduce<T>(0, input.Count, parallelOptions, i => input[i], seed, associativeCommutativeOperation);
        }

        /// <summary>Reduces the input range using the specified aggregation operation.</summary>
        /// <typeparam name="T">Specifies the type of data being aggregated.</typeparam>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="mapOperation">The function used to retrieve the data to be reduced for a given index.</param>
        /// <param name="seed">The seed to use to initialize the operation; this seed may be used multiple times.</param>
        /// <param name="associativeCommutativeOperation">The reduction operation.</param>
        /// <returns>The reduced value.</returns>
        public static T Reduce<T>(int fromInclusive, int toExclusive, Func<int, T> mapOperation, T seed, Func<T, T, T> associativeCommutativeOperation)
        {
            return Reduce<T>(fromInclusive, toExclusive, s_defaultParallelOptions, mapOperation, seed, associativeCommutativeOperation);
        }

        /// <summary>Reduces the input range using the specified aggregation operation.</summary>
        /// <typeparam name="T">Specifies the type of data being aggregated.</typeparam>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="parallelOptions">A ParallelOptions instance that configures the behavior of this operation.</param>
        /// <param name="mapOperation">The function used to retrieve the data to be reduced for a given index.</param>
        /// <param name="seed">The seed to use to initialize the operation; this seed may be used multiple times.</param>
        /// <param name="associativeCommutativeOperation">The reduction operation.</param>
        /// <returns>The reduced value.</returns>
        public static T Reduce<T>(int fromInclusive, int toExclusive, ParallelOptions parallelOptions, Func<int, T> mapOperation, T seed, Func<T, T, T> associativeCommutativeOperation)
        {
            if (parallelOptions == null)
            {
                throw new ArgumentNullException("parallelOptions");
            }
            if (mapOperation == null)
            {
                throw new ArgumentNullException("mapOperation");
            }
            if (associativeCommutativeOperation == null)
            {
                throw new ArgumentNullException("associativeCommutativeOperation");
            }
            if (toExclusive < fromInclusive)
            {
                throw new ArgumentOutOfRangeException("toExclusive");
            }
            object obj = new object();
            T result = seed;
            Parallel.For<T>(fromInclusive, toExclusive, parallelOptions, () => seed, (i, loop, localResult) => associativeCommutativeOperation(mapOperation(i), localResult), delegate (T localResult) {
                lock (obj)
                {
                    result = associativeCommutativeOperation(localResult, result);
                }
            });
            return result;
        }

        /// <summary>Computes a parallel prefix scan over the source enumerable using the specified function.</summary>
        /// <typeparam name="T">The type of the data in the source.</typeparam>
        /// <param name="source">The source data over which a prefix scan should be computed.</param>
        /// <param name="function">The function to use for the scan.</param>
        /// <returns>The results of the scan operation.</returns>
        /// <remarks>
        /// For very small functions, such as additions, an implementation targeted
        /// at the relevant type and operation will perform significantly better than
        /// this generalized implementation.
        /// </remarks>
        public static T[] Scan<T>(IEnumerable<T> source, Func<T, T, T> function)
        {
            bool loadBalance = false;
            return Scan<T>(source, function, loadBalance);
        }

        /// <summary>Computes a parallel prefix scan over the source enumerable using the specified function.</summary>
        /// <typeparam name="T">The type of the data in the source.</typeparam>
        /// <param name="source">The source data over which a prefix scan should be computed.</param>
        /// <param name="function">The function to use for the scan.</param>
        /// <param name="loadBalance">Whether to load-balance during process.</param>
        /// <returns>The results of the scan operation.</returns>
        /// <remarks>
        /// For very small functions, such as additions, an implementation targeted
        /// at the relevant type and operation will perform significantly better than
        /// this generalized implementation.
        /// </remarks>
        public static T[] Scan<T>(IEnumerable<T> source, Func<T, T, T> function, bool loadBalance)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            T[] data = source.ToArray<T>();
            ScanInPlace<T>(data, function, loadBalance);
            return data;
        }

        /// <summary>Computes a parallel prefix scan in-place on an array using the specified function.</summary>
        /// <typeparam name="T">The type of the data in the source.</typeparam>
        /// <param name="data">The data over which a prefix scan should be computed. Upon exit, stores the results.</param>
        /// <param name="function">The function to use for the scan.</param>
        /// <returns>The results of the scan operation.</returns>
        /// <remarks>
        /// For very small functions, such as additions, an implementation targeted
        /// at the relevant type and operation will perform significantly better than
        /// this generalized implementation.
        /// </remarks>
        public static void ScanInPlace<T>(T[] data, Func<T, T, T> function)
        {
            bool loadBalance = false;
            ScanInPlace<T>(data, function, loadBalance);
        }

        /// <summary>Computes a parallel prefix scan in-place on an array using the specified function.</summary>
        /// <typeparam name="T">The type of the data in the source.</typeparam>
        /// <param name="data">The data over which a prefix scan should be computed. Upon exit, stores the results.</param>
        /// <param name="function">The function to use for the scan.</param>
        /// <param name="loadBalance">Whether to load-balance during process.</param>
        /// <returns>The results of the scan operation.</returns>
        /// <remarks>
        /// For very small functions, such as additions, an implementation targeted
        /// at the relevant type and operation will perform significantly better than
        /// this generalized implementation.
        /// </remarks>
        public static void ScanInPlace<T>(T[] data, Func<T, T, T> function, bool loadBalance)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            if (function == null)
            {
                throw new ArgumentNullException("function");
            }
            if (Environment.ProcessorCount <= 2)
            {
                InclusiveScanInPlaceSerial<T>(data, function, 0, data.Length, 1);
            }
            else if (loadBalance)
            {
                InclusiveScanInPlaceWithLoadBalancingParallel<T>(data, function, 0, data.Length, 1);
            }
            else
            {
                InclusiveScanInPlaceParallel<T>(data, function);
            }
        }

        /// <summary>Sorts an array in parallel.</summary>
        /// <typeparam name="T">Specifies the type of data in the array.</typeparam>
        /// <param name="array">The array to be sorted.</param>
        public static void Sort<T>(T[] array)
        {
            Sort<T>(array, null);
        }

        /// <summary>Sorts an array in parallel.</summary>
        /// <typeparam name="T">Specifies the type of data in the array.</typeparam>
        /// <param name="array">The array to be sorted.</param>
        /// <param name="comparer">The comparer used to compare two elements during the sort operation.</param>
        public static void Sort<T>(T[] array, IComparer<T> comparer)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            Sort<T, object>(array, null, 0, array.Length, comparer);
        }

        /// <summary>Sorts key/value arrays in parallel.</summary>
        /// <typeparam name="TKey">Specifies the type of the data in the keys array.</typeparam>
        /// <typeparam name="TValue">Specifies the type of the data in the items array.</typeparam>
        /// <param name="keys">The keys to be sorted.</param>
        /// <param name="items">The items to be sorted based on the corresponding keys.</param>
        public static void Sort<TKey, TValue>(TKey[] keys, TValue[] items)
        {
            Sort<TKey, TValue>(keys, items, 0, keys.Length, null);
        }

        /// <summary>Sorts an array in parallel.</summary>
        /// <typeparam name="T">Specifies the type of data in the array.</typeparam>
        /// <param name="array">The array to be sorted.</param>
        /// <param name="index">The index at which to start the sort, inclusive.</param>
        /// <param name="length">The number of elements to be sorted, starting at the start index.</param>
        public static void Sort<T>(T[] array, int index, int length)
        {
            Sort<T, object>(array, null, index, length, null);
        }

        /// <summary>Sorts key/value arrays in parallel.</summary>
        /// <typeparam name="TKey">Specifies the type of the data in the keys array.</typeparam>
        /// <typeparam name="TValue">Specifies the type of the data in the items array.</typeparam>
        /// <param name="keys">The keys to be sorted.</param>
        /// <param name="items">The items to be sorted based on the corresponding keys.</param>
        /// <param name="comparer">The comparer used to compare two elements during the sort operation.</param>
        public static void Sort<TKey, TValue>(TKey[] keys, TValue[] items, IComparer<TKey> comparer)
        {
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }
            Sort<TKey, TValue>(keys, items, 0, keys.Length, comparer);
        }

        /// <summary>Sorts an array in parallel.</summary>
        /// <typeparam name="T">Specifies the type of data in the array.</typeparam>
        /// <param name="array">The array to be sorted.</param>
        /// <param name="index">The index at which to start the sort, inclusive.</param>
        /// <param name="length">The number of elements to be sorted, starting at the start index.</param>
        /// <param name="comparer">The comparer used to compare two elements during the sort operation.</param>
        public static void Sort<T>(T[] array, int index, int length, IComparer<T> comparer)
        {
            Sort<T, object>(array, null, index, length, comparer);
        }

        /// <summary>Sorts key/value arrays in parallel.</summary>
        /// <typeparam name="TKey">Specifies the type of the data in the keys array.</typeparam>
        /// <typeparam name="TValue">Specifies the type of the data in the items array.</typeparam>
        /// <param name="keys">The keys to be sorted.</param>
        /// <param name="items">The items to be sorted based on the corresponding keys.</param>
        /// <param name="index">The index at which to start the sort, inclusive.</param>
        /// <param name="length">The number of elements to be sorted, starting at the start index.</param>
        public static void Sort<TKey, TValue>(TKey[] keys, TValue[] items, int index, int length)
        {
            Sort<TKey, TValue>(keys, items, index, length, null);
        }

        /// <summary>Sorts key/value arrays in parallel.</summary>
        /// <typeparam name="TKey">Specifies the type of the data in the keys array.</typeparam>
        /// <typeparam name="TValue">Specifies the type of the data in the items array.</typeparam>
        /// <param name="keys">The keys to be sorted.</param>
        /// <param name="items">The items to be sorted based on the corresponding keys.</param>
        /// <param name="index">The index at which to start the sort, inclusive.</param>
        /// <param name="length">The number of elements to be sorted, starting at the start index.</param>
        /// <param name="comparer">The comparer used to compare two elements during the sort operation.</param>
        public static void Sort<TKey, TValue>(TKey[] keys, TValue[] items, int index, int length, IComparer<TKey> comparer)
        {
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }
            if ((index < 0) || (length < 0))
            {
                throw new ArgumentOutOfRangeException((length < 0) ? "length" : "index");
            }
            if (((keys.Length - index) < length) || ((items != null) && (index > (items.Length - length))))
            {
                throw new ArgumentException("index");
            }
            new Sorter<TKey, TValue>(keys, items, comparer).QuickSort(index, (index + length) - 1);
        }

        /// <summary>Executes a function for each value in a range, returning the first result achieved and ceasing execution.</summary>
        /// <typeparam name="TResult">The type of the data returned.</typeparam>
        /// <param name="fromInclusive">The start of the range, inclusive.</param>
        /// <param name="toExclusive">The end of the range, exclusive.</param>
        /// <param name="body">The function to execute for each element.</param>
        /// <returns>The result computed.</returns>
        public static TResult SpeculativeFor<TResult>(int fromInclusive, int toExclusive, Func<int, TResult> body)
        {
            return SpeculativeFor<TResult>(fromInclusive, toExclusive, s_defaultParallelOptions, body);
        }

        /// <summary>Executes a function for each value in a range, returning the first result achieved and ceasing execution.</summary>
        /// <typeparam name="TResult">The type of the data returned.</typeparam>
        /// <param name="fromInclusive">The start of the range, inclusive.</param>
        /// <param name="toExclusive">The end of the range, exclusive.</param>
        /// <param name="options">The options to use for processing the loop.</param>
        /// <param name="body">The function to execute for each element.</param>
        /// <returns>The result computed.</returns>
        public static TResult SpeculativeFor<TResult>(int fromInclusive, int toExclusive, ParallelOptions options, Func<int, TResult> body)
        {
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            object result = null;
            Parallel.For(fromInclusive, toExclusive, options, delegate (int i, ParallelLoopState loopState) {
                Interlocked.CompareExchange(ref result, body(i), null);
                loopState.Stop();
            });
            return (TResult) result;
        }

        /// <summary>Executes a function for each element in a source, returning the first result achieved and ceasing execution.</summary>
        /// <typeparam name="TSource">The type of the data in the source.</typeparam>
        /// <typeparam name="TResult">The type of the data returned.</typeparam>
        /// <param name="source">The input elements to be processed.</param>
        /// <param name="body">The function to execute for each element.</param>
        /// <returns>The result computed.</returns>
        public static TResult SpeculativeForEach<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> body)
        {
            return SpeculativeForEach<TSource, TResult>(source, s_defaultParallelOptions, body);
        }

        /// <summary>Executes a function for each element in a source, returning the first result achieved and ceasing execution.</summary>
        /// <typeparam name="TSource">The type of the data in the source.</typeparam>
        /// <typeparam name="TResult">The type of the data returned.</typeparam>
        /// <param name="source">The input elements to be processed.</param>
        /// <param name="options">The options to use for processing the loop.</param>
        /// <param name="body">The function to execute for each element.</param>
        /// <returns>The result computed.</returns>
        public static TResult SpeculativeForEach<TSource, TResult>(IEnumerable<TSource> source, ParallelOptions options, Func<TSource, TResult> body)
        {
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            object result = null;
            Parallel.ForEach<TSource>(source, options, delegate (TSource item, ParallelLoopState loopState) {
                Interlocked.CompareExchange(ref result, body(item), null);
                loopState.Stop();
            });
            return (TResult) result;
        }

        /// <summary>Invokes the specified functions, potentially in parallel, canceling outstanding invocations once one completes.</summary>
        /// <typeparam name="T">Specifies the type of data returned by the functions.</typeparam>
        /// <param name="functions">The functions to be executed.</param>
        /// <returns>A result from executing one of the functions.</returns>
        public static T SpeculativeInvoke<T>(params Func<T>[] functions)
        {
            return SpeculativeInvoke<T>(s_defaultParallelOptions, functions);
        }

        /// <summary>Invokes the specified functions, potentially in parallel, canceling outstanding invocations once one completes.</summary>
        /// <typeparam name="T">Specifies the type of data returned by the functions.</typeparam>
        /// <param name="options">The options to use for the execution.</param>
        /// <param name="functions">The functions to be executed.</param>
        /// <returns>A result from executing one of the functions.</returns>
        public static T SpeculativeInvoke<T>(ParallelOptions options, params Func<T>[] functions)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            if (functions == null)
            {
                throw new ArgumentNullException("functions");
            }
            return SpeculativeForEach<Func<T>, T>(functions, options, function => function());
        }

        /// <summary>Process in parallel a matrix where every cell has a dependency on the cell above it and to its left.</summary>
        /// <param name="numRows">The number of rows in the matrix.</param>
        /// <param name="numColumns">The number of columns in the matrix.</param>
        /// <param name="processRowColumnCell">The action to invoke for every cell, supplied with the row and column indices.</param>
        public static void Wavefront(int numRows, int numColumns, Action<int, int> processRowColumnCell)
        {
            if (numRows <= 0)
            {
                throw new ArgumentOutOfRangeException("numRows");
            }
            if (numColumns <= 0)
            {
                throw new ArgumentOutOfRangeException("numColumns");
            }
            if (processRowColumnCell == null)
            {
                throw new ArgumentNullException("processRowColumnCell");
            }
            Task[] taskArray = new Task[numColumns];
            Task task = null;
            Task[] tasks = new Task[2];
            for (int k = 0; k < numRows; k++)
            {
                task = null;
                for (int m = 0; m < numColumns; m++)
                {
                    Task task2;
                    Action action = null;
                    Action<Task> continuationAction = null;
                    Action<Task[]> action3 = null;
                    int j = k;
                    int i = m;
                    if ((k == 0) && (m == 0))
                    {
                        if (action == null)
                        {
                            action = () => processRowColumnCell(j, i);
                        }
                        task2 = Task.Factory.StartNew(action);
                    }
                    else if ((k == 0) || (m == 0))
                    {
                        Task task3 = (m == 0) ? taskArray[0] : task;
                        if (continuationAction == null)
                        {
                            continuationAction = delegate (Task p) {
                                p.Wait();
                                processRowColumnCell(j, i);
                            };
                        }
                        task2 = task3.ContinueWith(continuationAction);
                    }
                    else
                    {
                        tasks[0] = task;
                        tasks[1] = taskArray[m];
                        if (action3 == null)
                        {
                            action3 = delegate (Task[] ps) {
                                Task.WaitAll(ps);
                                processRowColumnCell(j, i);
                            };
                        }
                        task2 = Task.Factory.ContinueWhenAll(tasks, action3);
                    }
                    taskArray[m] = task = task2;
                }
            }
            task.Wait();
        }

        /// <summary>Process in parallel a matrix where every cell has a dependency on the cell above it and to its left.</summary>
        /// <param name="numRows">The number of rows in the matrix.</param>
        /// <param name="numColumns">The number of columns in the matrix.</param>
        /// <param name="numBlocksPerRow">Partition the matrix into this number of blocks along the rows.</param>
        /// <param name="numBlocksPerColumn">Partition the matrix into this number of blocks along the columns.</param>
        /// <param name="processBlock">The action to invoke for every block, supplied with the start and end indices of the rows and columns.</param>
        public static void Wavefront(int numRows, int numColumns, int numBlocksPerRow, int numBlocksPerColumn, Action<int, int, int, int> processBlock)
        {
            if (numRows <= 0)
            {
                throw new ArgumentOutOfRangeException("numRows");
            }
            if (numColumns <= 0)
            {
                throw new ArgumentOutOfRangeException("numColumns");
            }
            if ((numBlocksPerRow <= 0) || (numBlocksPerRow > numRows))
            {
                throw new ArgumentOutOfRangeException("numBlocksPerRow");
            }
            if ((numBlocksPerColumn <= 0) || (numBlocksPerColumn > numColumns))
            {
                throw new ArgumentOutOfRangeException("numBlocksPerColumn");
            }
            if (processBlock == null)
            {
                throw new ArgumentNullException("processRowColumnCell");
            }
            int rowBlockSize = numRows / numBlocksPerRow;
            int columnBlockSize = numColumns / numBlocksPerColumn;
            Wavefront(numBlocksPerRow, numBlocksPerColumn, delegate (int row, int column) {
                int num = row * rowBlockSize;
                int num2 = (row < (numBlocksPerRow - 1)) ? (num + rowBlockSize) : numRows;
                int num3 = column * columnBlockSize;
                int num4 = (column < (numBlocksPerColumn - 1)) ? (num3 + columnBlockSize) : numColumns;
                processBlock(num, num2, num3, num4);
            });
        }

        /// <summary>Processes data in parallel, allowing the processing function to add more data to be processed.</summary>
        /// <typeparam name="T">Specifies the type of data being processed.</typeparam>
        /// <param name="initialValues">The initial set of data to be processed.</param>
        /// <param name="body">The operation to execute for each value.</param>
        public static void WhileNotEmpty<T>(IEnumerable<T> initialValues, Action<T, Action<T>> body)
        {
            WhileNotEmpty<T>(s_defaultParallelOptions, initialValues, body);
        }

        /// <summary>Processes data in parallel, allowing the processing function to add more data to be processed.</summary>
        /// <typeparam name="T">Specifies the type of data being processed.</typeparam>
        /// <param name="parallelOptions">A ParallelOptions instance that configures the behavior of this operation.</param>
        /// <param name="initialValues">The initial set of data to be processed.</param>
        /// <param name="body">The operation to execute for each value.</param>
        public static void WhileNotEmpty<T>(ParallelOptions parallelOptions, IEnumerable<T> initialValues, Action<T, Action<T>> body)
        {
            if (parallelOptions == null)
            {
                throw new ArgumentNullException("parallelOptions");
            }
            if (initialValues == null)
            {
                throw new ArgumentNullException("initialValues");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            ConcurrentStack<T>[] array = new ConcurrentStack<T>[]
	{
		new ConcurrentStack<T>(initialValues),
		new ConcurrentStack<T>()
	};
            int num = 0;
            while (true)
            {
                int num2 = num % 2;
                ConcurrentStack<T> concurrentStack = array[num2];
                ConcurrentStack<T> to = array[num2 ^ 1];
                if (concurrentStack.IsEmpty)
                {
                    break;
                }
                Action<T> adder = delegate(T newItem)
                {
                    to.Push(newItem);
                };
                Parallel.ForEach<T>(concurrentStack, parallelOptions, delegate(T e)
                {
                    body(e, adder);
                });
                concurrentStack.Clear();
                num++;
            }
        }



        private sealed class Sorter<TKey, TItem>
        {
            private IComparer<TKey> _comparer;
            private TItem[] _items;
            private TKey[] _keys;

            public Sorter(TKey[] keys, TItem[] items, IComparer<TKey> comparer)
            {
                if (comparer == null)
                {
                    comparer = Comparer<TKey>.Default;
                }
                this._keys = keys;
                this._items = items;
                this._comparer = comparer;
            }

            private static int GetMaxDepth()
            {
                return (int) Math.Log((double) Environment.ProcessorCount, 2.0);
            }

            private static int GetMiddle(int low, int high)
            {
                return (low + ((high - low) >> 1));
            }

            internal void QuickSort(int left, int right)
            {
                this.QuickSort(left, right, 0, ParallelAlgorithms.Sorter<TKey, TItem>.GetMaxDepth());
            }

            internal void QuickSort(int left, int right, int depth, int maxDepth)
            {
                if ((depth >= maxDepth) || (((right - left) + 1) <= 0x1000))
                {
                    Array.Sort<TKey, TItem>(this._keys, this._items, left, (right - left) + 1, this._comparer);
                }
                else
                {
                    List<Task> list = new List<Task>();
                    do
                    {
                        int low = left;
                        int high = right;
                        int middle = ParallelAlgorithms.Sorter<TKey, TItem>.GetMiddle(low, high);
                        this.SwapIfGreaterWithItems(low, middle);
                        this.SwapIfGreaterWithItems(low, high);
                        this.SwapIfGreaterWithItems(middle, high);
                        TKey y = this._keys[middle];
                        do
                        {
                            while (this._comparer.Compare(this._keys[low], y) < 0)
                            {
                                low++;
                            }
                            while (this._comparer.Compare(y, this._keys[high]) < 0)
                            {
                                high--;
                            }
                            if (low > high)
                            {
                                break;
                            }
                            if (low < high)
                            {
                                TKey local2 = this._keys[low];
                                this._keys[low] = this._keys[high];
                                this._keys[high] = local2;
                                if (this._items != null)
                                {
                                    TItem local3 = this._items[low];
                                    this._items[low] = this._items[high];
                                    this._items[high] = local3;
                                }
                            }
                            low++;
                            high--;
                        }
                        while (low <= high);
                        if ((high - left) <= (right - low))
                        {
                            if (left < high)
                            {
                                int leftcopy = left;
                                int jcopy = high;
                                list.Add(Task.Factory.StartNew(() => ((ParallelAlgorithms.Sorter<TKey, TItem>) this).QuickSort(leftcopy, jcopy, depth + 1, maxDepth)));
                            }
                            left = low;
                        }
                        else
                        {
                            if (low < right)
                            {
                                int icopy = low;
                                int rightcopy = right;
                                list.Add(Task.Factory.StartNew(() => ((ParallelAlgorithms.Sorter<TKey, TItem>) this).QuickSort(icopy, rightcopy, depth + 1, maxDepth)));
                            }
                            right = high;
                        }
                    }
                    while (left < right);
                    Task.WaitAll(list.ToArray());
                }
            }

            internal void SwapIfGreaterWithItems(int a, int b)
            {
                if ((a != b) && (this._comparer.Compare(this._keys[a], this._keys[b]) > 0))
                {
                    TKey local = this._keys[a];
                    this._keys[a] = this._keys[b];
                    this._keys[b] = local;
                    if (this._items != null)
                    {
                        TItem local2 = this._items[a];
                        this._items[a] = this._items[b];
                        this._items[b] = local2;
                    }
                }
            }
        }
    }
}

