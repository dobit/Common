using LFNet.Common.Component;

namespace LFNet.Common.Scheduler
{
    /// <summary>
    /// A base class representing a job lock.
    /// </summary>
    public class JobLock : DisposableBase
    {
        private bool _lockAcquired;
        private readonly string _lockName;
        private readonly JobLockProvider _provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Scheduler.JobLock" /> class.
        /// </summary>
        /// <param name="provider">The lock provider to call when disposing.</param>
        /// <param name="lockName">Name of the lock.</param>
        /// <param name="lockAcquired">if set to <c>true</c> lock was acquired.</param>
        public JobLock(JobLockProvider provider, string lockName, bool lockAcquired)
        {
            this._lockAcquired = lockAcquired;
            this._lockName = lockName;
            this._provider = provider;
        }

        /// <summary>
        /// Disposes the unmanaged resources.
        /// </summary>
        protected override void DisposeUnmanagedResources()
        {
            if (this.LockAcquired)
            {
                this._provider.Release(this);
            }
        }

        /// <summary>
        /// Sets the lock to released.
        /// </summary>
        public void SetReleased()
        {
            this._lockAcquired = false;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// </returns>
        public override string ToString()
        {
            return string.Format("LockName: {0}, LockAcquired: {1}", this.LockName, this.LockAcquired);
        }

        /// <summary>
        /// Gets a value indicating whether the lock was acquired successfully.
        /// </summary>
        /// <value><c>true</c> if the was lock acquired; otherwise, <c>false</c>.</value>
        public bool LockAcquired
        {
            get
            {
                return this._lockAcquired;
            }
        }

        /// <summary>
        /// Gets the name of the lock.
        /// </summary>
        /// <value>The name of the lock.</value>
        public string LockName
        {
            get
            {
                return this._lockName;
            }
        }
    }
}

