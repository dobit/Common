using System;
using System.Threading;

namespace LFNet.Common.Threading
{
    /// <summary>
    /// IWorkItemResult interface
    /// </summary>
    public interface IWorkItemResult
    {
        /// <summary>
        /// Cancel the work item if it didn't start running yet.
        /// </summary>
        /// <returns>Returns true on success or false if the work item is in progress or already completed</returns>
        bool Cancel();
        /// <summary>
        /// Get the result of the work item.
        /// If the work item didn't run yet then the caller waits.
        /// </summary>
        /// <returns>The result of the work item</returns>
        object GetResult();
        /// <summary>
        /// Get the result of the work item.
        /// If the work item didn't run yet then the caller waits.
        /// </summary>
        /// <param name="e">Filled with the exception if one was thrown</param>
        /// <returns>The result of the work item</returns>
        object GetResult(out global::System.Exception e);
        /// <summary>
        /// Get the result of the work item.
        /// If the work item didn't run yet then the caller waits until timeout.
        /// </summary>
        /// <returns>The result of the work item</returns>
        /// On timeout throws WorkItemTimeoutException
        object GetResult(int millisecondsTimeout, bool exitContext);
        /// <summary>
        /// Get the result of the work item.
        /// If the work item didn't run yet then the caller waits until timeout.
        /// </summary>
        /// <returns>The result of the work item</returns>
        /// On timeout throws WorkItemTimeoutException
        object GetResult(TimeSpan timeout, bool exitContext);
        /// <summary>
        /// Get the result of the work item.
        /// If the work item didn't run yet then the caller waits until timeout or until the cancelWaitHandle is signaled.
        /// </summary>
        /// <param name="millisecondsTimeout">Timeout in milliseconds, or -1 for infinite</param>
        /// <param name="exitContext">
        /// true to exit the synchronization domain for the context before the wait (if in a synchronized context), and reacquire it; otherwise, false. 
        /// </param>
        /// <param name="cancelWaitHandle">A cancel wait handle to interrupt the blocking if needed</param>
        /// <returns>The result of the work item</returns>
        /// On timeout throws WorkItemTimeoutException
        /// On cancel throws WorkItemCancelException
        object GetResult(int millisecondsTimeout, bool exitContext, WaitHandle cancelWaitHandle);
        /// <summary>
        /// Get the result of the work item.
        /// If the work item didn't run yet then the caller waits until timeout.
        /// </summary>
        /// <param name="millisecondsTimeout">The milliseconds timeout.</param>
        /// <param name="exitContext">if set to <c>true</c> [exit context].</param>
        /// <param name="e">Filled with the exception if one was thrown</param>
        /// <returns>The result of the work item</returns>
        /// On timeout throws WorkItemTimeoutException
        object GetResult(int millisecondsTimeout, bool exitContext, out global::System.Exception e);
        /// <summary>
        /// Get the result of the work item.
        /// If the work item didn't run yet then the caller waits until timeout or until the cancelWaitHandle is signaled.
        /// </summary>
        /// <returns>The result of the work item</returns>
        /// On timeout throws WorkItemTimeoutException
        /// On cancel throws WorkItemCancelException
        object GetResult(TimeSpan timeout, bool exitContext, WaitHandle cancelWaitHandle);
        /// <summary>
        /// Get the result of the work item.
        /// If the work item didn't run yet then the caller waits until timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="exitContext">if set to <c>true</c> [exit context].</param>
        /// <param name="e">Filled with the exception if one was thrown</param>
        /// <returns>The result of the work item</returns>
        /// On timeout throws WorkItemTimeoutException
        object GetResult(TimeSpan timeout, bool exitContext, out global::System.Exception e);
        /// <summary>
        /// Get the result of the work item.
        /// If the work item didn't run yet then the caller waits until timeout or until the cancelWaitHandle is signaled.
        /// </summary>
        /// <param name="millisecondsTimeout">Timeout in milliseconds, or -1 for infinite</param>
        /// <param name="exitContext">
        /// true to exit the synchronization domain for the context before the wait (if in a synchronized context), and reacquire it; otherwise, false. 
        /// </param>
        /// <param name="cancelWaitHandle">A cancel wait handle to interrupt the blocking if needed</param>
        /// <param name="e">Filled with the exception if one was thrown</param>
        /// <returns>The result of the work item</returns>
        /// On timeout throws WorkItemTimeoutException
        /// On cancel throws WorkItemCancelException
        object GetResult(int millisecondsTimeout, bool exitContext, WaitHandle cancelWaitHandle, out global::System.Exception e);
        /// <summary>
        /// Get the result of the work item.
        /// If the work item didn't run yet then the caller waits until timeout or until the cancelWaitHandle is signaled.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="exitContext">if set to <c>true</c> [exit context].</param>
        /// <param name="cancelWaitHandle">The cancel wait handle.</param>
        /// <param name="e">Filled with the exception if one was thrown</param>
        /// <returns>The result of the work item</returns>
        /// On timeout throws WorkItemTimeoutException
        /// On cancel throws WorkItemCancelException
        object GetResult(TimeSpan timeout, bool exitContext, WaitHandle cancelWaitHandle, out global::System.Exception e);

        /// <summary>
        /// Returns the exception if occured otherwise returns null.
        /// </summary>
        object Exception { get; }

        /// <summary>
        /// Gets an indication whether the asynchronous operation has been canceled.
        /// </summary>
        bool IsCanceled { get; }

        /// <summary>
        /// Gets an indication whether the asynchronous operation has completed.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Return the result, same as GetResult()
        /// </summary>
        object Result { get; }

        /// <summary>
        /// Gets a user-defined object that qualifies or contains information about an asynchronous operation.
        /// </summary>
        object State { get; }

        /// <summary>
        /// Get the work item's priority
        /// </summary>
        WorkItemPriority WorkItemPriority { get; }
    }
}

