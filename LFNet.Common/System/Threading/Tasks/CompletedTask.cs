using System.Threading.Tasks;

namespace System.Threading.Tasks
{
    /// <summary>Provides access to an already completed task.</summary>
    /// <remarks>A completed task can be useful for using ContinueWith overloads where there aren't StartNew equivalents.</remarks>
    public static class CompletedTask
    {
        /// <summary>Gets a completed Task.</summary>
        public static readonly Task Default = CompletedTask<object>.Default;
    }
}

