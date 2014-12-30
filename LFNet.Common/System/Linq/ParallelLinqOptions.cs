using System;
using System.Linq;
using System.Threading.Tasks;

namespace System.Linq
{
    /// <summary>Provides a grouping for common Parallel LINQ options.</summary>
    public sealed class ParallelLinqOptions : ParallelOptions
    {
        private ParallelExecutionMode _executionMode;
        private ParallelMergeOptions _mergeOptions;
        private bool _ordered;

        /// <summary>Gets or sets the execution mode.</summary>
        public ParallelExecutionMode ExecutionMode
        {
            get
            {
                return this._executionMode;
            }
            set
            {
                if ((value != ParallelExecutionMode.Default) && (value != ParallelExecutionMode.ForceParallelism))
                {
                    throw new ArgumentOutOfRangeException("ExecutionMode");
                }
                this._executionMode = value;
            }
        }

        /// <summary>Gets or sets the merge options.</summary>
        public ParallelMergeOptions MergeOptions
        {
            get
            {
                return this._mergeOptions;
            }
            set
            {
                if (((value != ParallelMergeOptions.AutoBuffered) && (value != ParallelMergeOptions.Default)) && ((value != ParallelMergeOptions.FullyBuffered) && (value != ParallelMergeOptions.NotBuffered)))
                {
                    throw new ArgumentOutOfRangeException("MergeOptions");
                }
                this._mergeOptions = value;
            }
        }

        /// <summary>Gets or sets whether the query should retain ordering.</summary>
        public bool Ordered
        {
            get
            {
                return this._ordered;
            }
            set
            {
                this._ordered = value;
            }
        }
    }
}

