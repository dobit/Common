using System;
using System.Runtime.Serialization;

namespace LFNet.Common.Threading
{
    /// <summary>
    /// Represents an exception in case IWorkItemResult.GetResult has been timed out
    /// </summary>
    [Serializable]
    public sealed class WorkItemTimeoutException : ApplicationException
    {
        public WorkItemTimeoutException()
        {
        }

        public WorkItemTimeoutException(string message) : base(message)
        {
        }

        public WorkItemTimeoutException(SerializationInfo si, StreamingContext sc) : base(si, sc)
        {
        }

        public WorkItemTimeoutException(string message, Exception e) : base(message, e)
        {
        }
    }
}

