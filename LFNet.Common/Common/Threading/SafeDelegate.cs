using System;
using System.Threading;

namespace LFNet.Common.Threading
{
    /// <summary>
    /// Class that implements a wrapper for a delegate to support 
    /// fire and forget asynchronous invoke of a delegate.
    /// </summary>
    public static class SafeDelegate
    {
        private static readonly AsyncCallback _callback = new AsyncCallback(SafeDelegate.EndWrapperInvoke);
        private static readonly DelegateWrapper _wrapperInstance = new DelegateWrapper(SafeDelegate.InvokeWrappedDelegate);

        private static void EndWrapperInvoke(IAsyncResult ar)
        {
            _wrapperInstance.EndInvoke(ar);
            WaitHandle asyncWaitHandle = ar.AsyncWaitHandle;
            if (asyncWaitHandle != null)
            {
                asyncWaitHandle.Close();
                asyncWaitHandle.Dispose();
            }
        }

        /// <summary>
        /// Invoke the specified delegate with the specified arguments
        /// asynchronously on a thread pool thread. EndInvoke is automatically 
        /// called to prevent resource leaks.
        /// </summary>
        public static void InvokeAsync(Delegate d, params object[] args)
        {
            _wrapperInstance.BeginInvoke(d, args, _callback, null);
        }

        private static void InvokeWrappedDelegate(Delegate d, object[] args)
        {
            d.DynamicInvoke(args);
        }

        private delegate void DelegateWrapper(Delegate d, object[] args);
    }
}

