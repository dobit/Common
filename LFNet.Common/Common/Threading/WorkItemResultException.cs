using System;
using System.Runtime.Serialization;

namespace LFNet.Common.Threading
{
    /// <summary>
    /// Represents an exception in case IWorkItemResult.GetResult has been timed out
    /// </summary>
    [Serializable]
    public sealed class WorkItemResultException : ApplicationException
    {
        public WorkItemResultException()
        {
        }

        public WorkItemResultException(string message) : base(message)
        {
        }

        public WorkItemResultException(SerializationInfo si, StreamingContext sc) : base(si, sc)
        {
        }

        public WorkItemResultException(string message, Exception e) : base(message, e)
        {
        }
    }
}

