using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace System.Threading.Tasks
{
    internal class EAPCommon
    {
        internal static void HandleCompletion<T>(TaskCompletionSource<T> tcs, AsyncCompletedEventArgs e, Func<T> getResult, Action unregisterHandler)
        {
            if (e.UserState == tcs)
            {
                if (e.Cancelled)
                {
                    tcs.TrySetCanceled();
                }
                else if (e.Error != null)
                {
                    tcs.TrySetException(e.Error);
                }
                else
                {
                    tcs.TrySetResult(getResult());
                }
                unregisterHandler();
            }
        }
    }
}

