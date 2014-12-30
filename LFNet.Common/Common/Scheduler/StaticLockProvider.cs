using System.Collections.Generic;

namespace LFNet.Common.Scheduler
{
    /// <summary>
    /// A lock provider that only allows one job per <see cref="T:System.AppDomain" />.
    /// </summary>
    public class StaticLockProvider : JobLockProvider
    {
        private static readonly HashSet<string> _locks = new HashSet<string>();
        private static readonly object _myLock = new object();

        /// <summary>
        /// Acquires a lock on specified job name.
        /// </summary>
        /// <param name="lockName">Name of the lock, usually the job name.</param>
        /// <returns>An <see cref="T:LFNet.Common.Scheduler.JobLock" /> object that will release the lock when disposed.</returns>
        public override JobLock Acquire(string lockName)
        {
            lock (_myLock)
            {
                return new JobLock(this, lockName, _locks.Add(lockName));
            }
        }

        /// <summary>
        /// Releases the specified job lock.
        /// </summary>
        /// <param name="jobLock">The job lock.</param>
        public override void Release(JobLock jobLock)
        {
            lock (_myLock)
            {
                if (jobLock.LockAcquired)
                {
                    _locks.Remove(jobLock.LockName);
                }
                jobLock.SetReleased();
            }
        }
    }
}

