using System;
using System.Collections.Generic;
using System.Configuration;

namespace LFNet.Common.Scheduler
{
    /// <summary>
    /// The job configuration element
    /// </summary>
    public class JobElement : ConfigurationElement, IJobConfiguration
    {
        private readonly IDictionary<string, object> _arguments = new Dictionary<string, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Scheduler.JobElement" /> class.
        /// </summary>
        public JobElement()
        {
            this.Interval = TimeSpan.FromSeconds(30.0);
            this.KeepAlive = true;
            this.IsTimeOfDay = false;
            this.Description = string.Empty;
        }

        /// <summary>
        /// Gets a value indicating whether an unknown attribute is encountered during deserialization.
        /// </summary>
        /// <param name="name">The name of the unrecognized attribute.</param>
        /// <param name="value">The value of the unrecognized attribute.</param>
        /// <returns>
        /// true when an unknown attribute is encountered while deserializing; otherwise, false.
        /// </returns>
        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            this._arguments.Add(name, value);
            return true;
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
        /// Gets or sets the description for the job.
        /// </summary>
        /// <value>The description for the job.</value>
        [ConfigurationProperty("description", DefaultValue="")]
        public string Description
        {
            get
            {
                return (string) base["description"];
            }
            set
            {
                base["description"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the group for the job.
        /// </summary>
        /// <value>The group for the job.</value>
        [ConfigurationProperty("group", DefaultValue="")]
        public string Group
        {
            get
            {
                return (string) base["group"];
            }
            set
            {
                base["group"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the timer interval.
        /// </summary>
        [ConfigurationProperty("interval", DefaultValue="0:0:30")]
        public TimeSpan Interval
        {
            get
            {
                return (TimeSpan) base["interval"];
            }
            set
            {
                base["interval"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Interval is a time of day.
        /// </summary>
        /// <value><c>true</c> if Interval is time of day; otherwise, <c>false</c>.</value>
        [ConfigurationProperty("isTimeOfDay", DefaultValue=false)]
        public bool IsTimeOfDay
        {
            get
            {
                return (bool) base["isTimeOfDay"];
            }
            set
            {
                base["isTimeOfDay"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the job history provider.
        /// </summary>
        /// <value>The job history provider.</value>
        [ConfigurationProperty("jobHistoryProvider")]
        public string JobHistoryProvider
        {
            get
            {
                return (string) base["jobHistoryProvider"];
            }
            set
            {
                base["jobHistoryProvider"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the provider that is used to lock the job when running.
        /// </summary>
        /// <value>The type to use to lock the job.</value>
        [ConfigurationProperty("jobLockProvider")]
        public string JobLockProvider
        {
            get
            {
                return (string) base["jobLockProvider"];
            }
            set
            {
                base["jobLockProvider"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to keep alive the instance between job runs.
        /// </summary>
        /// <value><c>true</c> to keep alive instance; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Setting this to true, the default value, will keep the <see cref="T:LFNet.Common.Scheduler.IJob" /> instance alive between runs.
        /// </remarks>
        [ConfigurationProperty("keepAlive", DefaultValue=true)]
        public bool KeepAlive
        {
            get
            {
                return (bool) base["keepAlive"];
            }
            set
            {
                base["keepAlive"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the job.
        /// </summary>
        /// <value>The name of the job.</value>
        [ConfigurationProperty("name", IsKey=true, IsRequired=true)]
        public string Name
        {
            get
            {
                return (string) base["name"];
            }
            set
            {
                base["name"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the assembly type that contains the job to run.
        /// </summary>
        [ConfigurationProperty("type", IsRequired=true)]
        public string Type
        {
            get
            {
                return (string) base["type"];
            }
            set
            {
                base["type"] = value;
            }
        }
    }
}

