using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Web.Configuration;

namespace LFNet.Common.Scheduler
{
    /// <summary>
    /// A class to manage the jobs for the Scheduler.
    /// </summary>
    public class JobManager
    {
        private readonly JobLockProvider _defaultJobLockProvider = new DefaultJobLockProvider();
        private readonly string _id = Guid.NewGuid().ToString("N").Substring(0, 10).ToLower();
        private static readonly object _initLock = new object();
        private bool _isInitialized;
        private readonly JobLockProviderCollection _jobLockProviders = new JobLockProviderCollection();
        private readonly JobProviderCollection _jobProviders = new JobProviderCollection();
        private readonly Timer _jobProviderTimer;
        private readonly JobCollection _jobs = new JobCollection();
        private DateTime _lastInitilize;
        private readonly Dictionary<JobProvider, JobCollection> _providerJobs = new Dictionary<JobProvider, JobCollection>();
        internal static int JobsRunning;

        /// <summary>
        /// Occurs when the Job run is completed.
        /// </summary>
        /// <seealso cref="M:LFNet.Common.Scheduler.Job.Run" />
        public event EventHandler<JobCompletedEventArgs> JobCompleted;

        /// <summary>
        /// Occurs when the JobManager starts.
        /// </summary>
        /// <seealso cref="M:LFNet.Common.Scheduler.JobManager.Start" />
        public event EventHandler<JobEventArgs> JobMangerStarting;

        /// <summary>
        /// Occurs when the JobManager stops.
        /// </summary>
        /// <seealso cref="M:LFNet.Common.Scheduler.JobManager.Stop" />
        public event EventHandler<JobEventArgs> JobMangerStopping;

        /// <summary>
        /// Occurs when the Job is running.
        /// </summary>
        /// <seealso cref="M:LFNet.Common.Scheduler.Job.Run" />
        public event EventHandler<JobEventArgs> JobRunning;

        /// <summary>
        /// Occurs when the Job is starting.
        /// </summary>
        /// <seealso cref="M:LFNet.Common.Scheduler.Job.Start" />
        public event EventHandler<JobEventArgs> JobStarting;

        /// <summary>
        /// Occurs when the Job is stopping.
        /// </summary>
        /// <seealso cref="M:LFNet.Common.Scheduler.Job.Stop" />
        public event EventHandler<JobEventArgs> JobStopping;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Scheduler.JobManager" /> class.
        /// </summary>
        private JobManager()
        {
            this._jobProviderTimer = new Timer(new TimerCallback(this.OnJobProviderCallback));
        }

        private void AddJobs(IEnumerable<IJobConfiguration> jobs, JobProvider provider)
        {
            if (jobs != null)
            {
                foreach (IJobConfiguration configuration in jobs)
                {
                    Type jobType = Type.GetType(configuration.Type, false, true);
                    if (jobType == null)
                    {
                        throw new ConfigurationErrorsException(string.Format("Could not load type '{0}' for job '{1}'.", configuration.Type, configuration.Name));
                    }
                    JobLockProvider jobLockProvider = this._defaultJobLockProvider;
                    if (!string.IsNullOrEmpty(configuration.JobLockProvider))
                    {
                        jobLockProvider = this._jobLockProviders[configuration.JobLockProvider];
                        if (jobLockProvider == null)
                        {
                            Type type = Type.GetType(configuration.JobLockProvider, false, true);
                            if (type == null)
                            {
                                throw new ConfigurationErrorsException(string.Format("Could not load job lock type '{0}' for job '{1}'.", configuration.JobLockProvider, configuration.Name));
                            }
                            jobLockProvider = Activator.CreateInstance(type) as JobLockProvider;
                        }
                        if (jobLockProvider == null)
                        {
                            throw new ConfigurationErrorsException(string.Format("Could not find job lock provider '{0}' for job '{1}'.", configuration.JobLockProvider, configuration.Name));
                        }
                    }
                    JobHistoryProvider jobHistoryProvider = null;
                    if (!string.IsNullOrEmpty(configuration.JobHistoryProvider))
                    {
                        Type type3 = Type.GetType(configuration.JobHistoryProvider, false, true);
                        if (type3 == null)
                        {
                            throw new ConfigurationErrorsException(string.Format("Could not load job history type '{0}' for job '{1}'.", configuration.JobHistoryProvider, configuration.Name));
                        }
                        jobHistoryProvider = Activator.CreateInstance(type3) as JobHistoryProvider;
                    }
                    Job item = new Job(configuration, jobType, jobLockProvider, jobHistoryProvider);
                    this._jobs.Add(item);
                    if (provider != null)
                    {
                        JobCollection jobs2;
                        if (!this._providerJobs.TryGetValue(provider, out jobs2))
                        {
                            jobs2 = new JobCollection();
                            this._providerJobs.Add(provider, jobs2);
                        }
                        jobs2.Add(item);
                    }
                }
            }
        }

        public JobCollection GetJobsByGroup(string group)
        {
            JobCollection jobs = new JobCollection();
            lock (_initLock)
            {
                foreach (Job job in this._jobs)
                {
                    if (job.Group == group)
                    {
                        jobs.Add(job);
                    }
                }
            }
            return jobs;
        }

        /// <summary>
        /// Initializes the jobs for this manager.
        /// </summary>
        public void Initialize()
        {
            if (!this._isInitialized)
            {
                lock (_initLock)
                {
                    if (!this._isInitialized)
                    {
                        JobManagerSection section = ConfigurationManager.GetSection("jobManager") as JobManagerSection;
                        if (section == null)
                        {
                            throw new ConfigurationErrorsException("Could not find 'jobManager' section in app.config or web.config file.");
                        }
                        ProvidersHelper.InstantiateProviders(section.JobLockProviders, this._jobLockProviders, typeof(JobLockProvider));
                        this.AddJobs(section.Jobs, null);
                        ProvidersHelper.InstantiateProviders(section.JobProviders, this._jobProviders, typeof(JobProvider));
                        foreach (JobProvider provider in this._jobProviders)
                        {
                            this.AddJobs(provider.GetJobs(), provider);
                        }
                        this._jobProviderTimer.Change(section.JobProviderPoll, section.JobProviderPoll);
                        this._lastInitilize = DateTime.Now;
                        this._isInitialized = true;
                    }
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:LFNet.Common.Scheduler.JobManager.JobCompleted" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:LFNet.Common.Scheduler.JobCompletedEventArgs" /> instance containing the event data.</param>
        internal void OnJobCompleted(JobCompletedEventArgs e)
        {
            if (this.JobCompleted != null)
            {
                this.JobCompleted(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:LFNet.Common.Scheduler.JobManager.JobMangerStarting" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:LFNet.Common.Scheduler.JobEventArgs" /> instance containing the event data.</param>
        private void OnJobMangerStarting(JobEventArgs e)
        {
            if (this.JobMangerStarting != null)
            {
                this.JobMangerStarting(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:LFNet.Common.Scheduler.JobManager.JobMangerStopping" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:LFNet.Common.Scheduler.JobEventArgs" /> instance containing the event data.</param>
        private void OnJobMangerStopping(JobEventArgs e)
        {
            if (this.JobMangerStopping != null)
            {
                this.JobMangerStopping(this, e);
            }
        }

        private void OnJobProviderCallback(object state)
        {
            bool flag = false;
            lock (_initLock)
            {
                foreach (JobProvider provider in this._jobProviders)
                {
                    if (provider.IsReloadRequired(this._lastInitilize))
                    {
                        JobCollection jobs;
                        Trace.TraceInformation("Reload jobs for provider {0}.", new object[] { provider.ToString() });
                        if (!this._providerJobs.TryGetValue(provider, out jobs))
                        {
                            jobs = new JobCollection();
                            this._providerJobs.Add(provider, jobs);
                        }
                        foreach (Job job in jobs)
                        {
                            job.Stop(true);
                            this._jobs.Remove(job);
                        }
                        jobs.Clear();
                        this.AddJobs(provider.GetJobs(), provider);
                        flag = true;
                        foreach (Job job2 in jobs)
                        {
                            job2.Start();
                        }
                    }
                }
            }
            if (flag)
            {
                this._lastInitilize = DateTime.Now;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:LFNet.Common.Scheduler.JobManager.JobRunning" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:LFNet.Common.Scheduler.JobEventArgs" /> instance containing the event data.</param>
        internal void OnJobRunning(JobEventArgs e)
        {
            if (this.JobRunning != null)
            {
                this.JobRunning(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:LFNet.Common.Scheduler.JobManager.JobStarting" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:LFNet.Common.Scheduler.JobEventArgs" /> instance containing the event data.</param>
        internal void OnJobStarting(JobEventArgs e)
        {
            if (this.JobStarting != null)
            {
                this.JobStarting(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:LFNet.Common.Scheduler.JobManager.JobStopping" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:LFNet.Common.Scheduler.JobEventArgs" /> instance containing the event data.</param>
        internal void OnJobStopping(JobEventArgs e)
        {
            if (this.JobStopping != null)
            {
                this.JobStopping(this, e);
            }
        }

        /// <summary>
        /// Reload by stopping all jobs and reloading configuration. All the jobs will be restarted after reload.
        /// </summary>
        public void Reload()
        {
            this.Reload(true);
        }

        /// <summary>
        /// Reload by stopping all jobs and reloading configuration.
        /// </summary>
        /// <param name="startAfter">if set to <c>true</c> start the jobs after reload.</param>
        public void Reload(bool startAfter)
        {
            this.Stop();
            lock (_initLock)
            {
                this._jobLockProviders.Clear();
                this._jobProviders.Clear();
                this._jobs.Clear();
                this._lastInitilize = DateTime.MinValue;
                this._isInitialized = false;
            }
            if (startAfter)
            {
                this.Start();
            }
        }

        /// <summary>
        /// Starts all jobs in this manager.
        /// </summary>
        public void Start()
        {
            Trace.TraceInformation("JobManager.Start called at {0} on Thread {1}.", new object[] { DateTime.Now, Thread.CurrentThread.ManagedThreadId });
            this.OnJobMangerStarting(new JobEventArgs("{JobManager}", JobAction.Starting, this._id));
            this.Initialize();
            lock (_initLock)
            {
                foreach (Job job in this._jobs)
                {
                    job.Start();
                }
            }
        }

        /// <summary>
        /// Stops all jobs in this manager.
        /// </summary>
        public void Stop()
        {
            Trace.TraceInformation("JobManager.Stop called at {0} on Thread {1}.", new object[] { DateTime.Now, Thread.CurrentThread.ManagedThreadId });
            this.OnJobMangerStopping(new JobEventArgs("{JobManager}", JobAction.Stopping, this._id));
            this.Initialize();
            lock (_initLock)
            {
                foreach (Job job in this._jobs)
                {
                    job.Stop(true);
                }
            }
            DateTime time = DateTime.Now.AddSeconds(30.0);
            while (JobsRunning > 0)
            {
                Thread.Sleep(300);
                if (time < DateTime.Now)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Gets the number of active jobs.
        /// </summary>
        /// <value>The number of active jobs.</value>
        public int ActiveJobs
        {
            get
            {
                return JobsRunning;
            }
        }

        /// <summary>
        /// Gets the current instance of <see cref="T:LFNet.Common.Scheduler.JobManager" />.
        /// </summary>
        /// <value>The current instance.</value>
        public static JobManager Current
        {
            get
            {
                return Nested.Current;
            }
        }

        /// <summary>
        /// Gets the collection of jobs.
        /// </summary>
        /// <value>The collection of jobs.</value>
        public JobCollection Jobs
        {
            get
            {
                return this._jobs;
            }
        }

        private class Nested
        {
            internal static readonly JobManager Current = new JobManager();
        }
    }
}

