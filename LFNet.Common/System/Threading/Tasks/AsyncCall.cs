using System;
using System.Reflection;
using System.Threading.Tasks;

namespace System.Threading.Tasks
{
    /// <summary>Provides static factory methods for creating AsyncCall(Of T) instances.</summary>
    public static class AsyncCall
    {
        /// <summary>
        /// Initializes the AsyncCall with a function to execute for each element.  The function returns an Task 
        /// that represents the asynchronous completion of that element's processing.
        /// </summary>
        /// <param name="functionHandler">The function to run for every posted item.</param>
        /// <param name="maxDegreeOfParallelism">The maximum degree of parallelism to use.  If not specified, 1 is used for serial execution.</param>
        /// <param name="scheduler">The scheduler to use.  If null, the default scheduler is used.</param>
        public static AsyncCall<T> Create<T>(Func<T, Task> functionHandler, int maxDegreeOfParallelism = 1, TaskScheduler scheduler = null)
        {
            return new AsyncCall<T>(functionHandler, maxDegreeOfParallelism, scheduler);
        }

        /// <summary>Initializes the AsyncCall with an action to execute for each element.</summary>
        /// <param name="actionHandler">The action to run for every posted item.</param>
        /// <param name="maxDegreeOfParallelism">The maximum degree of parallelism to use.  If not specified, 1 is used for serial execution.</param>
        /// <param name="scheduler">The scheduler to use.  If null, the default scheduler is used.</param>
        /// <param name="maxItemsPerTask">The maximum number of items to be processed per task.  If not specified, Int32.MaxValue is used.</param>
        public static AsyncCall<T> Create<T>(Action<T> actionHandler, int maxDegreeOfParallelism = 1, int maxItemsPerTask = 0x7fffffff, TaskScheduler scheduler = null)
        {
            return new AsyncCall<T>(actionHandler, maxDegreeOfParallelism, maxItemsPerTask, scheduler);
        }

        /// <summary>
        /// Initializes the AsyncCall in the specified AppDomain with a function to execute for each element.  
        /// The function returns an Task that represents the asynchronous completion of that element's processing.
        /// </summary>
        /// <param name="targetDomain"></param>
        /// <param name="functionHandler">The action to run for every posted item.</param>
        /// <param name="maxDegreeOfParallelism">The maximum degree of parallelism to use.  If not specified, 1 is used for serial execution.</param>
        public static AsyncCall<T> CreateInTargetAppDomain<T>(AppDomain targetDomain, Func<T, Task> functionHandler, int maxDegreeOfParallelism = 1)
        {
            object[] args = new object[3];
            args[0] = functionHandler;
            args[1] = maxDegreeOfParallelism;
            return (AsyncCall<T>) targetDomain.CreateInstanceAndUnwrap(typeof(AsyncCall<T>).Assembly.FullName, typeof(AsyncCall<T>).FullName, false, BindingFlags.CreateInstance, null, args, null, null);
        }

        /// <summary>Initializes the AsyncCall in the specified AppDomain with an action to execute for each element.</summary>
        /// <param name="targetDomain"></param>
        /// <param name="actionHandler">The action to run for every posted item.</param>
        /// <param name="maxDegreeOfParallelism">The maximum degree of parallelism to use.  If not specified, 1 is used for serial execution.</param>
        /// <param name="maxItemsPerTask">The maximum number of items to be processed per task.  If not specified, Int32.MaxValue is used.</param>
        public static AsyncCall<T> CreateInTargetAppDomain<T>(AppDomain targetDomain, Action<T> actionHandler, int maxDegreeOfParallelism = 1, int maxItemsPerTask = 0x7fffffff)
        {
            object[] args = new object[4];
            args[0] = actionHandler;
            args[1] = maxDegreeOfParallelism;
            args[2] = maxItemsPerTask;
            return (AsyncCall<T>) targetDomain.CreateInstanceAndUnwrap(typeof(AsyncCall<T>).Assembly.FullName, typeof(AsyncCall<T>).FullName, false, BindingFlags.CreateInstance, null, args, null, null);
        }
    }
}

