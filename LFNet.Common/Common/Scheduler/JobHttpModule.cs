using System;
using System.Diagnostics;
using System.Threading;
using System.Web;

namespace LFNet.Common.Scheduler
{
    /// <summary>
    /// A Http module class to start the <see cref="T:LFNet.Common.Scheduler.JobManager" />.
    /// </summary>
    public class JobHttpModule : IHttpModule
    {
        private static long _initCount;

        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule" />.
        /// </summary>
        public void Dispose()
        {
            Trace.TraceInformation("JobModule.Dispose called at {0}.", new object[] { DateTime.Now });
        }

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication" /> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application</param>
        public void Init(HttpApplication context)
        {
            Trace.TraceInformation("JobModule.Init called at {0}.", new object[] { DateTime.Now });
            if (Interlocked.Increment(ref _initCount) == 1L)
            {
                JobManager.Current.Start();
            }
        }
    }
}

