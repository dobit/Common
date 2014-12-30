using System;
using System.Diagnostics;
using System.Linq;

namespace System
{
    /// <summary>Parallel extensions for the Delegate class.</summary>
    public static class DelegateExtensions
    {
        /// <summary>Dynamically invokes (late-bound) in parallel the methods represented by the delegate.</summary>
        /// <param name="multicastDelegate">The delegate to be invoked.</param>
        /// <param name="args">An array of objects that are the arguments to pass to the delegates.</param>
        /// <returns>The return value of one of the delegate invocations.</returns>
        public static object ParallelDynamicInvoke(this Delegate multicastDelegate, params object[] args)
        {
            if (multicastDelegate == null)
            {
                throw new ArgumentNullException("multicastDelegate");
            }
            if (args == null)
            {
                throw new ArgumentNullException("args");
            }
            return multicastDelegate.GetInvocationList().AsParallel<Delegate>().AsOrdered<Delegate>().Select<Delegate, object>(((Func<Delegate, object>) (d => d.DynamicInvoke(args)))).Last<object>();
        }

        /// <summary>
        /// Provides a delegate that runs the specified action and fails fast if the action throws an exception.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <returns>The wrapper delegate.</returns>
        public static Action WithFailFast(this Action action)
        {
            return delegate {
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                    else
                    {
                        Environment.FailFast("An unhandled exception occurred.", exception);
                    }
                }
            };
        }

        /// <summary>
        /// Provides a delegate that runs the specified function and fails fast if the function throws an exception.
        /// </summary>
        /// <param name="function">The function to invoke.</param>
        /// <returns>The wrapper delegate.</returns>
        public static Func<T> WithFailFast<T>(this Func<T> function)
        {
            return delegate {
                try
                {
                    return function();
                }
                catch (Exception exception)
                {
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                    else
                    {
                        Environment.FailFast("An unhandled exception occurred.", exception);
                    }
                }
                throw new Exception("Will never get here");
            };
        }
    }
}

