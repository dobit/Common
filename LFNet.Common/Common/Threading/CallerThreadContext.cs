using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Web;

namespace LFNet.Common.Threading
{
    /// <summary>
    /// This class stores the caller call context in order to restore
    /// it when the work item is executed in the thread pool environment. 
    /// </summary>
    internal class CallerThreadContext
    {
        private LogicalCallContext _callContext;
        private HttpContext _httpContext;
        private static MethodInfo getLogicalCallContextMethodInfo = typeof(Thread).GetMethod("GetLogicalCallContext", BindingFlags.NonPublic | BindingFlags.Instance);
        private static string HttpContextSlotName = GetHttpContextSlotName();
        private static MethodInfo setLogicalCallContextMethodInfo = typeof(Thread).GetMethod("SetLogicalCallContext", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Constructor
        /// </summary>
        private CallerThreadContext()
        {
        }

        /// <summary>
        /// Applies the thread context stored earlier
        /// </summary>
        /// <param name="callerThreadContext"></param>
        public static void Apply(CallerThreadContext callerThreadContext)
        {
            if (callerThreadContext == null)
            {
                throw new ArgumentNullException("callerThreadContext");
            }
            if ((callerThreadContext._callContext != null) && (setLogicalCallContextMethodInfo != null))
            {
                setLogicalCallContextMethodInfo.Invoke(Thread.CurrentThread, new object[] { callerThreadContext._callContext });
            }
            if (callerThreadContext._httpContext != null)
            {
                CallContext.SetData(HttpContextSlotName, callerThreadContext._httpContext);
            }
        }

        /// <summary>
        /// Captures the current thread context
        /// </summary>
        /// <returns></returns>
        public static CallerThreadContext Capture(bool captureCallContext, bool captureHttpContext)
        {
            CallerThreadContext context = new CallerThreadContext();
            if (captureCallContext && (getLogicalCallContextMethodInfo != null))
            {
                context._callContext = (LogicalCallContext) getLogicalCallContextMethodInfo.Invoke(Thread.CurrentThread, null);
                if (context._callContext != null)
                {
                    context._callContext = (LogicalCallContext) context._callContext.Clone();
                }
            }
            if (captureHttpContext && (HttpContext.Current != null))
            {
                context._httpContext = HttpContext.Current;
            }
            return context;
        }

        private static string GetHttpContextSlotName()
        {
            FieldInfo field = typeof(HttpContext).GetField("CallContextSlotName", BindingFlags.NonPublic | BindingFlags.Static);
            if (field != null)
            {
                return (string) field.GetValue(null);
            }
            return "HttpContext";
        }

        public bool CapturedCallContext
        {
            get
            {
                return (null != this._callContext);
            }
        }

        public bool CapturedHttpContext
        {
            get
            {
                return (null != this._httpContext);
            }
        }
    }
}

