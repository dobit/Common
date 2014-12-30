namespace LFNet.Common.Scheduler
{
    /// <summary>
    /// A base class for jobs
    /// </summary>
    public abstract class JobBase : IJob
    {
        private volatile bool _cancelPending;

        protected JobBase()
        {
        }

        /// <summary>
        /// Cancels this job.
        /// </summary>
        public virtual void Cancel()
        {
            this._cancelPending = true;
        }

        /// <summary>
        /// Runs this job.
        /// </summary>
        /// <param name="context">The job context.</param>
        /// <returns>
        /// A <see cref="T:LFNet.Common.Scheduler.JobResult" /> instance indicating the results of the job.
        /// </returns>
        public abstract JobResult Run(JobContext context);

        /// <summary>
        /// Gets a value indicating whether a cancel request is pending.
        /// </summary>
        /// <value><c>true</c> if cancel is pending; otherwise, <c>false</c>.</value>
        protected bool CancelPending
        {
            get
            {
                return this._cancelPending;
            }
        }
    }
}

