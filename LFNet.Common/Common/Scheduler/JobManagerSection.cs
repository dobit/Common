using System;
using System.Configuration;

namespace LFNet.Common.Scheduler
{
    public class JobManagerSection : ConfigurationSection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Scheduler.JobManagerSection" /> class.
        /// </summary>
        public JobManagerSection()
        {
            this.JobProviderPoll = TimeSpan.Zero;
        }

        /// <summary>
        /// Gets the job lock providers.
        /// </summary>
        /// <value>The job lock providers.</value>
        [ConfigurationProperty("jobLockProviders")]
        public ProviderSettingsCollection JobLockProviders
        {
            get
            {
                return (base["jobLockProviders"] as ProviderSettingsCollection);
            }
        }

        /// <summary>
        /// Gets or sets the poll interval for calling <see cref="T:LFNet.Common.Scheduler.JobProvider" />.IsReloadRequired.
        /// </summary>
        /// <remarks>Set to <see cref="T:System.TimeSpan" />.Zero to disable reload checking.</remarks>
        [ConfigurationProperty("jobProviderPoll", DefaultValue="0:0:0")]
        public TimeSpan JobProviderPoll
        {
            get
            {
                return (TimeSpan) base["jobProviderPoll"];
            }
            set
            {
                base["jobProviderPoll"] = value;
            }
        }

        /// <summary>
        /// Gets the providers to configure jobs.
        /// </summary>
        /// <value>The job configuration providers.</value>
        [ConfigurationProperty("jobProviders")]
        public ProviderSettingsCollection JobProviders
        {
            get
            {
                return (base["jobProviders"] as ProviderSettingsCollection);
            }
        }

        /// <summary>
        /// The jobs to schedule.
        /// </summary>
        [ConfigurationProperty("jobs")]
        public JobElementCollection Jobs
        {
            get
            {
                return (base["jobs"] as JobElementCollection);
            }
        }
    }
}

