namespace LFNet.Common.Scheduler
{
    /// <summary>
    /// Interface for a scheduled job.
    /// </summary>
    public interface IJob
    {
        /// <summary>
        /// Cancels this job.
        /// </summary>
        void Cancel();
        /// <summary>
        /// Runs this job.
        /// </summary>
        /// <param name="context">The job context.</param>
        /// <returns>
        /// A <see cref="T:LFNet.Common.Scheduler.JobResult" /> instance indicating the results of the job.
        /// </returns>
        JobResult Run(JobContext context);
    }
}

