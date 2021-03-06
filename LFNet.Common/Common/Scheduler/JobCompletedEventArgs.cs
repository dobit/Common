﻿using System;
using LFNet.Common.Extensions;

namespace LFNet.Common.Scheduler
{
    /// <summary>
    /// Job run completed event arguments.
    /// </summary>
    public class JobCompletedEventArgs : JobEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Scheduler.JobCompletedEventArgs" /> class.
        /// </summary>
        /// <param name="jobName">Name of the job.</param>
        /// <param name="action">The action.</param>
        /// <param name="started">The time the job run started.</param>
        /// <param name="finished">The time the job run ended.</param>
        /// <param name="result">The result of the job run.</param>
        /// <param name="status">The status of the job run.</param>
        public JobCompletedEventArgs(string jobName, JobAction action, DateTime started, DateTime finished, string result, JobStatus status) : this(jobName, action, string.Empty, started, finished, result, status)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Scheduler.JobCompletedEventArgs" /> class.
        /// </summary>
        /// <param name="jobName">Name of the job.</param>
        /// <param name="action">The action.</param>
        /// <param name="jobId">The job id.</param>
        /// <param name="started">The time the job run started.</param>
        /// <param name="finished">The time the job run ended.</param>
        /// <param name="result">The result of the job run.</param>
        /// <param name="status">The status of the job run.</param>
        public JobCompletedEventArgs(string jobName, JobAction action, string jobId, DateTime started, DateTime finished, string result, JobStatus status) : base(jobName, action, jobId)
        {
            this.Started = started;
            this.Finished = finished;
            this.Result = result;
            this.Status = status;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "Job '{0}' ({1}) {2} in {3}.".FormatWith(new object[] { base.JobName, base.JobId, base.Action.ToString().ToLower(), this.Duration.ToWords(true) });
        }

        /// <summary>
        /// Gets the duration of the job run.
        /// </summary>
        /// <value>The duration of the job run.</value>
        public TimeSpan Duration
        {
            get
            {
                return this.Finished.Subtract(this.Started);
            }
        }

        /// <summary>
        /// Gets or sets the time that the job run finished.
        /// </summary>
        /// <value>The time the job run finished.</value>
        public DateTime Finished { get; private set; }

        /// <summary>
        /// Gets or sets the result of the job run.
        /// </summary>
        /// <value>The result of the job run.</value>
        public string Result { get; private set; }

        /// <summary>
        /// Gets or sets the time that the job run started.
        /// </summary>
        /// <value>The time the job run started.</value>
        public DateTime Started { get; private set; }

        /// <summary>
        /// Gets or sets the status of the job run.
        /// </summary>
        /// <value>The status of the job run.</value>
        public JobStatus Status { get; private set; }
    }
}

