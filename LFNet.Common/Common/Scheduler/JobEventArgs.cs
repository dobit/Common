using System;
using LFNet.Common.Extensions;

namespace LFNet.Common.Scheduler
{
    /// <summary>
    /// Job event arguments.
    /// </summary>
    public class JobEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Scheduler.JobEventArgs" /> class.
        /// </summary>
        /// <param name="jobName">Name of the job.</param>
        /// <param name="action">The action.</param>
        public JobEventArgs(string jobName, JobAction action) : this(jobName, action, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Scheduler.JobEventArgs" /> class.
        /// </summary>
        /// <param name="jobName">Name of the job.</param>
        /// <param name="action">The action.</param>
        /// <param name="jobId">The job id.</param>
        public JobEventArgs(string jobName, JobAction action, string jobId)
        {
            this.JobName = jobName;
            this.Action = action;
            this.JobId = jobId;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "Job '{0}' ({1}) {2}.".FormatWith(new object[] { this.JobName, this.JobId, this.Action.ToString().ToLower() });
        }

        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        /// <value>The action.</value>
        public JobAction Action { get; private set; }

        /// <summary>
        /// Gets or sets the job id.
        /// </summary>
        /// <value>The job id.</value>
        public string JobId { get; set; }

        /// <summary>
        /// Gets or sets the name of the job.
        /// </summary>
        /// <value>The name of the job.</value>
        public string JobName { get; private set; }
    }
}

