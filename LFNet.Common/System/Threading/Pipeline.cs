using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;

namespace System.Threading
{
    /// <summary>Provides support for pipelined data processing.</summary>
    public static class Pipeline
    {
        internal static readonly TaskScheduler Scheduler = new ThreadPerTaskScheduler();

        /// <summary>Creates a new pipeline, with the specified function as the sole stage.</summary>
        /// <typeparam name="TInput">Specifies the type of the input data to the pipeline.</typeparam>
        /// <typeparam name="TOutput">Specifies the type of the output data from this stage of the pipeline.</typeparam>
        /// <param name="func">The function used to process input data into output data.</param>
        /// <returns>A pipeline for converting from input data to output data.</returns>
        public static Pipeline<TInput, TOutput> Create<TInput, TOutput>(Func<TInput, TOutput> func)
        {
            return Create<TInput, TOutput>(func, 1);
        }

        /// <summary>Creates a new pipeline, with the specified function as the sole stage.</summary>
        /// <typeparam name="TInput">Specifies the type of the input data to the pipeline.</typeparam>
        /// <typeparam name="TOutput">Specifies the type of the output data from this stage of the pipeline.</typeparam>
        /// <param name="func">The function used to process input data into output data.</param>
        /// <param name="degreeOfParallelism">The concurrency level for this stage of the pipeline.</param>
        /// <returns>A pipeline for converting from input data to output data.</returns>
        public static Pipeline<TInput, TOutput> Create<TInput, TOutput>(Func<TInput, TOutput> func, int degreeOfParallelism)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }
            if (degreeOfParallelism < 1)
            {
                throw new ArgumentOutOfRangeException("degreeOfParallelism");
            }
            return new Pipeline<TInput, TOutput>(func, degreeOfParallelism);
        }
    }
}

