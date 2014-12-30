using System;
using System.Reflection;
using System.Text;

namespace LFNet.Common.Extensions
{
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Formats an exception with the stack trace included.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static string FormatMessageWithStackTrace(this Exception exception)
        {
            return string.Format("{0}\r\nStack Trace:\r\n{1}{2}", exception.Message, exception.StackTrace, Environment.NewLine);
        }

        public static string GetAllMessages(this Exception exception)
        {
            return exception.GetAllMessages(false);
        }

        public static string GetAllMessages(this Exception exception, bool includeStackTrace)
        {
            StringBuilder builder = new StringBuilder();
            for (Exception exception2 = (exception is AggregateException) ? ((AggregateException) exception).GetInnerException() : exception; exception2 != null; exception2 = exception2.InnerException)
            {
                string str = includeStackTrace ? exception2.FormatMessageWithStackTrace() : exception2.Message;
                builder.Append(str);
                if (exception2.InnerException != null)
                {
                    builder.Append(" --> ");
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Gets the exception that is wrapped by an AggregateException.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static Exception GetInnerException(this AggregateException exception)
        {
            if ((exception == null) || (exception.InnerException == null))
            {
                return null;
            }
            exception.Handle(e => true);
            exception = exception.Flatten();
            if ((exception.InnerException is TargetInvocationException) && (exception.InnerException.InnerException != null))
            {
                return exception.InnerException.InnerException;
            }
            if ((exception.InnerException is ApplicationException) && (exception.InnerException.InnerException is ApplicationException))
            {
                return exception.InnerException.InnerException;
            }
            return exception.InnerException;
        }
    }
}

