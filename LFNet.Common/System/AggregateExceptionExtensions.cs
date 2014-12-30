using System;
using System.Collections.Generic;

namespace System
{
    /// <summary>Extension methods for AggregateException.</summary>
    public static class AggregateExceptionExtensions
    {
        /// <summary>Invokes a handler on each Exception contained by this AggregateException.</summary>
        /// <param name="aggregateException">The AggregateException.</param>
        /// <param name="predicate">
        /// The predicate to execute for each exception. The predicate accepts as an argument the Exception
        /// to be processed and returns a Boolean to indicate whether the exception was handled.
        /// </param>
        /// <param name="leaveStructureIntact">
        /// Whether the rethrown AggregateException should maintain the same hierarchy as the original.
        /// </param>
        public static void Handle(this AggregateException aggregateException, Func<Exception, bool> predicate, bool leaveStructureIntact)
        {
            if (aggregateException == null)
            {
                throw new ArgumentNullException("aggregateException");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            if (leaveStructureIntact)
            {
                AggregateException exception = HandleRecursively(aggregateException, predicate);
                if (exception != null)
                {
                    throw exception;
                }
            }
            else
            {
                aggregateException.Handle(predicate);
            }
        }

        private static AggregateException HandleRecursively(AggregateException aggregateException, Func<Exception, bool> predicate)
        {
            List<Exception> innerExceptions = null;
            foreach (Exception exception in aggregateException.InnerExceptions)
            {
                AggregateException exception2 = exception as AggregateException;
                if (exception2 != null)
                {
                    AggregateException item = HandleRecursively(exception2, predicate);
                    if (item != null)
                    {
                        if (innerExceptions != null)
                        {
                            innerExceptions = new List<Exception>();
                        }
                        innerExceptions.Add(item);
                    }
                }
                else if (!predicate(exception))
                {
                    if (innerExceptions != null)
                    {
                        innerExceptions = new List<Exception>();
                    }
                    innerExceptions.Add(exception);
                }
            }
            if (innerExceptions.Count <= 0)
            {
                return null;
            }
            return new AggregateException(aggregateException.Message, innerExceptions);
        }
    }
}

