using System;
using System.Runtime.Serialization;

namespace LFNet.Common.Threading
{
    /// <summary>
    /// Represents an exception in case IWorkItemResult.GetResult has been canceled
    /// </summary>
    [Serializable]
    public sealed class WorkItemCancelException : ApplicationException
    {
        public WorkItemCancelException()
        {
        }

        public WorkItemCancelException(string message) : base(message)
        {
        }

        public WorkItemCancelException(SerializationInfo si, StreamingContext sc) : base(si, sc)
        {
        }

        public WorkItemCancelException(string message, Exception e) : base(message, e)
        {
        }
    }
}

