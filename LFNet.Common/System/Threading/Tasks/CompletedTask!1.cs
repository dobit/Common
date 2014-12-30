using System.Threading.Tasks;

namespace System.Threading.Tasks
{
    /// <summary>Provides access to an already completed task.</summary>
    /// <remarks>A completed task can be useful for using ContinueWith overloads where there aren't StartNew equivalents.</remarks>
    public static class CompletedTask<TResult>
    {
        /// <summary>Gets a completed Task.</summary>
        public static readonly Task<TResult> Default;

        /// <summary>Initializes a Task.</summary>
        static CompletedTask()
        {
            TaskCompletionSource<TResult> source = new TaskCompletionSource<TResult>();
            source.TrySetResult(default(TResult));
            CompletedTask<TResult>.Default = source.Task;
        }
    }
}

