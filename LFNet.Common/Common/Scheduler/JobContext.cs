using System;
using System.Collections.Generic;

namespace LFNet.Common.Scheduler
{
    public class JobContext : MarshalByRefObject
    {
        private readonly IDictionary<string, object> _arguments;
        private readonly string _description;
        private readonly DateTime _lastRunTime;
        private readonly JobStatus _lastStatus;
        private readonly string _name;
        private readonly Action<string> _updateStatus;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.MarshalByRefObject" /> class. 
        /// </summary>
        public JobContext(string name, string description, DateTime lastRunTime, JobStatus lastStatus, IDictionary<string, object> arguments, Action<string> updateStatus)
        {
            this._updateStatus = updateStatus;
            this._name = name;
            this._description = description;
            this._lastRunTime = lastRunTime;
            this._lastStatus = lastStatus;
            this._arguments = arguments;
        }

        /// <summary>
        /// Updates the status.
        /// </summary>
        /// <param name="message">The message.</param>
        public void UpdateStatus(string message)
        {
            if (this._updateStatus != null)
            {
                this._updateStatus(message);
            }
        }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        /// <value>The arguments.</value>
        public IDictionary<string, object> Arguments
        {
            get
            {
                return this._arguments;
            }
        }

        /// <summary>
        /// Gets the description for the job.
        /// </summary>
        /// <value>The description for the job.</value>
        public string Description
        {
            get
            {
                return this._description;
            }
        }

        /// <summary>
        /// Gets the last run time.
        /// </summary>
        /// <value>The last run time.</value>
        public DateTime LastRunTime
        {
            get
            {
                return this._lastRunTime;
            }
        }

        /// <summary>
        /// Gets the last status.
        /// </summary>
        /// <value>The last status.</value>
        public JobStatus LastStatus
        {
            get
            {
                return this._lastStatus;
            }
        }

        /// <summary>
        /// Gets the name of the job.
        /// </summary>
        /// <value>The name of the job.</value>
        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public Action<string> ProgressAction
        {
            get
            {
                return this._updateStatus;
            }
        }
    }
}

