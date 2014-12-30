using System.Configuration.Provider;

namespace LFNet.Common.Scheduler
{
    /// <summary>
    /// A base class for JobLockProviders.
    /// </summary>
    public abstract class JobLockProvider : ProviderBase
    {
        protected JobLockProvider()
        {
        }

        /// <summary>
        /// Acquires a lock on specified job name.
        /// </summary>
        /// <param name="lockName">Name of the lock, usually the job name.</param>
        /// <returns>A <see cref="T:LFNet.Common.Scheduler.JobLock" /> object that will release the lock when disposed.</returns>
        public abstract JobLock Acquire(string lockName);
        /// <summary>
        /// Releases the specified job lock.
        /// </summary>
        /// <param name="jobLock">The job lock.</param>
        public abstract void Release(JobLock jobLock);
    }
}

