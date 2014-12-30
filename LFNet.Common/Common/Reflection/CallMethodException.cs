using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace LFNet.Common.Reflection
{
    /// <summary>
    /// This exception is returned from the 
    /// CallMethod method in the server-side DataPortal
    /// and contains the exception thrown by the
    /// underlying business object method that was
    /// being invoked.
    /// </summary>
    [Serializable]
    public class CallMethodException : Exception
    {
        private readonly string _innerStackTrace;

        /// <summary>
        /// Creates an instance of the object for deserialization.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Serialiation context.</param>
        protected CallMethodException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._innerStackTrace = info.GetString("_innerStackTrace");
        }

        /// <summary>
        /// Creates an instance of the object.
        /// </summary>
        /// <param name="message">Message text describing the exception.</param>
        /// <param name="ex">Inner exception object.</param>
        public CallMethodException(string message, Exception ex) : base(message, ex)
        {
            this._innerStackTrace = ex.StackTrace;
        }

        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Serialization context.</param>
        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.SerializationFormatter), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_innerStackTrace", this._innerStackTrace);
        }

        /// <summary>
        /// Get the stack trace from the original
        /// exception.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public override string StackTrace
        {
            get
            {
                return string.Format("{0}{1}{2}", this._innerStackTrace, Environment.NewLine, base.StackTrace);
            }
        }
    }
}

