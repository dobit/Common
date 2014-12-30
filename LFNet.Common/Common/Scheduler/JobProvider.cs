using System;
using System.Collections.Generic;
using System.Configuration.Provider;

namespace LFNet.Common.Scheduler
{
    /// <summary>
    /// A base class for job providers.
    /// </summary>
    public abstract class JobProvider : ProviderBase
    {
        protected JobProvider()
        {
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.IEnumerable" /> list <see cref="T:LFNet.Common.Scheduler.IJobConfiguration" /> jobs.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerable" /> list <see cref="T:LFNet.Common.Scheduler.IJobConfiguration" /> jobs.</returns>
        public abstract IEnumerable<IJobConfiguration> GetJobs();
        /// <summary>
        /// Determines whether a job reload is required.
        /// </summary>
        /// <param name="lastLoad">The last load.</param>
        /// <returns>
        /// <c>true</c> if a job reload is required; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// If true is return, the <see cref="T:LFNet.Common.Scheduler.JobManager" /> will call Reload that will 
        /// in turn call <see cref="M:LFNet.Common.Scheduler.JobProvider.GetJobs" /> on this provider.
        /// </remarks>
        public virtual bool IsReloadRequired(DateTime lastLoad)
        {
            return false;
        }
    }
}

