using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Threading
{
    /// <summary>Provides support for pipelined data processing.</summary>
    /// <typeparam name="TInput">Specifies the type of the input data to the pipeline.</typeparam>
    /// <typeparam name="TOutput">Specifies the type of the output data from this stage of the pipeline.</typeparam>
    public class Pipeline<TInput, TOutput>
    {
        private readonly int _degreeOfParallelism;
        private readonly Func<TInput, TOutput> _stageFunc;

        internal Pipeline(int degreeOfParallelism) : this(null, degreeOfParallelism)
        {
        }

        internal Pipeline(Func<TInput, TOutput> func, int degreeOfParallelism)
        {
            this._stageFunc = func;
            this._degreeOfParallelism = degreeOfParallelism;
        }

        /// <summary>Creates a new pipeline that combines the current pipeline with a new stage.</summary>
        /// <typeparam name="TNextOutput">Specifies the new output type of the pipeline.</typeparam>
        /// <param name="func">
        /// The function used to convert the output of the current pipeline into the new
        /// output of the new pipeline.
        /// </param>
        /// <returns>A new pipeline that combines the current pipeline with the new stage.</returns>
        /// <remarks>This overload creates a parallel pipeline stage.</remarks>
        public Pipeline<TInput, TNextOutput> Next<TNextOutput>(Func<TOutput, TNextOutput> func)
        {
            return this.Next<TNextOutput>(func, 1);
        }

        /// <summary>Creates a new pipeline that combines the current pipeline with a new stage.</summary>
        /// <typeparam name="TNextOutput">Specifies the new output type of the pipeline.</typeparam>
        /// <param name="func">
        /// The function used to convert the output of the current pipeline into the new
        /// output of the new pipeline.
        /// </param>
        /// <param name="degreeOfParallelism">The concurrency level for this stage of the pipeline.</param>
        /// <returns>A new pipeline that combines the current pipeline with the new stage.</returns>
        public Pipeline<TInput, TNextOutput> Next<TNextOutput>(Func<TOutput, TNextOutput> func, int degreeOfParallelism)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }
            if (degreeOfParallelism < 1)
            {
                throw new ArgumentOutOfRangeException("degreeOfParallelism");
            }
            return new InternalPipeline<TNextOutput>((Pipeline<TInput, TOutput>) this, func, degreeOfParallelism);
        }

        /// <summary>Runs the pipeline and returns an enumerable over the results.</summary>
        /// <param name="source">The source data to be processed by the pipeline.</param>
        /// <returns>An enumerable of the results of the pipeline.</returns>
        public IEnumerable<TOutput> Process(IEnumerable<TInput> source)
        {
            return this.Process(source, new CancellationToken());
        }

        /// <summary>Runs the pipeline and returns an enumerable over the results.</summary>
        /// <param name="source">The source data to be processed by the pipeline.</param>
        /// <param name="cancellationToken">The cancellation token used to signal cancellation of the pipelining.</param>
        /// <returns>An enumerable of the results of the pipeline.</returns>
        public IEnumerable<TOutput> Process(IEnumerable<TInput> source, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return this.ProcessNoArgValidation(source, cancellationToken);
        }

        /// <summary>Implements the core processing for a pipeline stage.</summary>
        /// <param name="source">The source data to be processed by the pipeline.</param>
        /// <param name="cancellationToken">The cancellation token used to signal cancellation of the pipelining.</param>
        /// <param name="output">The collection into which to put the output.</param>
        protected virtual void ProcessCore(IEnumerable<TInput> source, CancellationToken cancellationToken, BlockingCollection<TOutput> output)
        {
            ParallelOptions parallelOptions = new ParallelOptions {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = this._degreeOfParallelism,
                TaskScheduler = Pipeline.Scheduler
            };
            Parallel.ForEach<TInput>(source, parallelOptions, (Action<TInput>) (item => output.Add(((Pipeline<TInput, TOutput>) this)._stageFunc(item))));
        }

        /// <summary>Runs the pipeline and returns an enumerable over the results.</summary>
        /// <param name="source">The source data to be processed by the pipeline.</param>
        /// <param name="cancellationToken">The cancellation token used to signal cancellation of the pipelining.</param>
        /// <returns>An enumerable of the results of the pipeline.</returns>
        private IEnumerable<TOutput> ProcessNoArgValidation(IEnumerable<TInput> source, CancellationToken cancellationToken)
        {
            Action action = null;
            using (BlockingCollection<TOutput> output = new BlockingCollection<TOutput>())
            {
                if (action == null)
                {
                    action = delegate {
                        try
                        {
                            ((Pipeline<TInput, TOutput>) this).ProcessCore(source, cancellationToken, output);
                        }
                        finally
                        {
                            output.CompleteAdding();
                        }
                    };
                }
                Task iteratorVariable0 = Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, Pipeline.Scheduler);
                foreach (TOutput iteratorVariable1 in output.GetConsumingEnumerable(cancellationToken))
                {
                    yield return iteratorVariable1;
                }
                iteratorVariable0.Wait();
            }
        }

       

        /// <summary>Helper used to add a new stage to a pipeline.</summary>
        /// <typeparam name="TNextOutput">Specifies the type of the output for the new pipeline.</typeparam>
        private sealed class InternalPipeline<TNextOutput> : Pipeline<TInput, TNextOutput>
        {
            private readonly Pipeline<TInput, TOutput> _beginningPipeline;
            private readonly Func<TOutput, TNextOutput> _lastStageFunc;

            public InternalPipeline(Pipeline<TInput, TOutput> beginningPipeline, Func<TOutput, TNextOutput> func, int degreeOfParallelism) : base(degreeOfParallelism)
            {
                this._beginningPipeline = beginningPipeline;
                this._lastStageFunc = func;
            }

            /// <summary>Implements the core processing for a pipeline stage.</summary>
            /// <param name="source">The source data to be processed by the pipeline.</param>
            /// <param name="cancellationToken">The cancellation token used to signal cancellation of the pipelining.</param>
            /// <param name="output">The collection into which to put the output.</param>
            protected override void ProcessCore(IEnumerable<TInput> source, CancellationToken cancellationToken, BlockingCollection<TNextOutput> output)
            {
                ParallelOptions parallelOptions = new ParallelOptions {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = base._degreeOfParallelism,
                    TaskScheduler = Pipeline.Scheduler
                };
                Parallel.ForEach<TOutput>(this._beginningPipeline.Process(source, cancellationToken), parallelOptions, (Action<TOutput>) (item => output.Add(((Pipeline<TInput, TOutput>.InternalPipeline<TNextOutput>) this)._lastStageFunc(item))));
            }
        }
    }
}

