using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using LFNet.Common.Threading;

namespace LFNet.Common.Scheduler
{
    /// <summary>
    /// A class representing a scheduled job.
    /// </summary>
    public class Job
    {
        private readonly IDictionary<string, object> _arguments;
        private readonly string _description;
        private readonly string _group;
        private readonly string _id;
        private IJob _instance;
        private readonly TimeSpan _interval;
        private readonly Synchronized<bool> _isBusy;
        private readonly bool _isTimeOfDay;
        private readonly JobHistoryProvider _jobHistoryProvider;
        private readonly JobLockProvider _jobLockProvider;
        private readonly Type _jobType;
        private readonly bool _keepAlive;
        private readonly Synchronized<string> _lastResult;
        private readonly Synchronized<DateTime> _lastRunFinishTime;
        private readonly Synchronized<DateTime> _lastRunStartTime;
        private readonly Synchronized<JobStatus> _lastStatus;
        private readonly string _name;
        private readonly Synchronized<DateTime> _nextRunTime;
        private readonly object _runLock;
        private readonly Synchronized<JobStatus> _status;
        private readonly Timer _timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Scheduler.Job" /> class.
        /// </summary>
        public Job(IJobConfiguration configuration, Type jobType, JobLockProvider jobLockProvider) : this(configuration, jobType, jobLockProvider, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.Scheduler.Job" /> class.
        /// </summary>
        public Job(IJobConfiguration configuration, Type jobType, JobLockProvider jobLockProvider, JobHistoryProvider jobHistoryProvider)
        {
            this._id = Guid.NewGuid().ToString("N").Substring(0, 10).ToLower();
            this._isBusy = new Synchronized<bool>();
            this._lastResult = new Synchronized<string>();
            this._lastRunStartTime = new Synchronized<DateTime>();
            this._lastRunFinishTime = new Synchronized<DateTime>();
            this._lastStatus = new Synchronized<JobStatus>();
            this._nextRunTime = new Synchronized<DateTime>();
            this._status = new Synchronized<JobStatus>();
            this._runLock = new object();
            this._name = configuration.Name;
            this._description = configuration.Description;
            this._group = configuration.Group;
            this._interval = configuration.Interval;
            this._isTimeOfDay = configuration.IsTimeOfDay;
            this._keepAlive = configuration.KeepAlive;
            this._arguments = configuration.Arguments;
            this._jobType = jobType;
            this._jobLockProvider = jobLockProvider ?? new DefaultJobLockProvider();
            this._jobHistoryProvider = jobHistoryProvider;
            this._instance = null;
            this._timer = new Timer(new TimerCallback(this.OnTimerCallback));
            if (this._jobHistoryProvider != null)
            {
                this._jobHistoryProvider.RestoreHistory(this);
            }
        }

        private void CreateInstance()
        {
            if (this._instance == null)
            {
                this._instance = Activator.CreateInstance(this._jobType) as IJob;
            }
            if (this._instance == null)
            {
                throw new InvalidOperationException(string.Format("Could not create an instance of '{0}'.", this._jobType.Name));
            }
        }

        private void OnTimerCallback(object state)
        {
            try
            {
                this.StopTimer();
                this.Run();
            }
            finally
            {
                if (!this.IsBusy)
                {
                    this.StartTimer();
                    this.Status = JobStatus.Waiting;
                }
            }
        }

        /// <summary>
        /// Runs this job.
        /// </summary>
        public void Run()
        {
            if (!this.IsBusy)
            {
                lock (this._runLock)
                {
                    this.RunInternal();
                }
            }
        }

        /// <summary>
        /// Runs this job at the specified time.
        /// </summary>
        /// <param name="runTime">The run time.</param>
        /// <remarks>Can be used to speed up the job when an event occurs.</remarks>
        public void Run(TimeSpan runTime)
        {
            if (!this.IsBusy)
            {
                this.NextRunTime = DateTime.Now.Add(runTime);
                this._timer.Change(runTime, TimeSpan.Zero);
                this.Status = JobStatus.Waiting;
            }
        }

        /// <summary>
        /// Runs this job asynchronous.
        /// </summary>
        /// <remarks>Can be used to speed up the job when an event occurs.</remarks>
        public void RunAsync()
        {
            this.Run(TimeSpan.FromMilliseconds(30.0));
        }

        private void RunInternal()
        {
            if (!this.IsBusy)
            {
                this.IsBusy = true;
                using (JobLock @lock = this._jobLockProvider.Acquire(this.Name))
                {
                    if (!@lock.LockAcquired)
                    {
                        this.LastResult = "Could not acquire a job lock.";
                        this.LastStatus = JobStatus.Canceled;
                        this.Status = JobStatus.Waiting;
                        this.IsBusy = false;
                    }
                    else
                    {
                        DateTime now = DateTime.Now;
                        this.LastRunStartTime = now;
                        JobManager.Current.OnJobRunning(new JobEventArgs(this.Name, JobAction.Running, this._id));
                        Trace.TraceInformation("Run job '{0}' at {1}.", new object[] { this.Name, now });
                        this.Status = JobStatus.Running;
                        Interlocked.Increment(ref JobManager.JobsRunning);
                        try
                        {
                            this.CreateInstance();
                            JobContext context = new JobContext(this.Name, this.Description, this.LastRunStartTime, this.LastStatus, this.Arguments, new Action<string>(this.UpdateStatus));
                            JobResult result = this._instance.Run(context);
                            if (result == null)
                            {
                                if (string.IsNullOrEmpty(this.LastResult))
                                {
                                    this.LastResult = "Completed";
                                }
                                this.LastStatus = JobStatus.Completed;
                            }
                            else if (result.Error != null)
                            {
                                this.LastResult = result.Error.Message;
                                this.LastStatus = JobStatus.Error;
                                Trace.TraceError(result.Error.ToString());
                            }
                            else
                            {
                                if (result.Result != null)
                                {
                                    this.LastResult = result.Result.ToString();
                                }
                                this.LastStatus = JobStatus.Completed;
                            }
                        }
                        catch (Exception exception)
                        {
                            this.LastResult = exception.Message;
                            this.LastStatus = JobStatus.Error;
                            Trace.TraceError(exception.ToString());
                        }
                        finally
                        {
                            Interlocked.Decrement(ref JobManager.JobsRunning);
                            this.LastRunFinishTime = DateTime.Now;
                            if (!this._keepAlive)
                            {
                                this._instance = null;
                            }
                            try
                            {
                                if (this._jobHistoryProvider != null)
                                {
                                    this._jobHistoryProvider.SaveHistory(this);
                                }
                            }
                            catch (Exception exception2)
                            {
                                Trace.TraceError("Error saving job history: " + exception2.Message);
                            }
                            JobManager.Current.OnJobCompleted(new JobCompletedEventArgs(this.Name, JobAction.Completed, this._id, now, this.LastRunFinishTime, this.LastResult, this.LastStatus));
                            Trace.TraceInformation("Job '{0}' completed with status '{1}'.", new object[] { this.Name, this.LastStatus });
                            this.Status = JobStatus.Waiting;
                            this.IsBusy = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Starts this job timer.
        /// </summary>
        public void Start()
        {
            if (!this.IsBusy)
            {
                JobManager.Current.OnJobStarting(new JobEventArgs(this.Name, JobAction.Starting, this._id));
                this.StartTimer();
                this.Status = JobStatus.Waiting;
            }
        }

        private void StartTimer()
        {
            if (!this.IsTimeOfDay)
            {
                this.NextRunTime = DateTime.Now.Add(this._interval);
                this._timer.Change(this._interval, TimeSpan.Zero);
            }
            else
            {
                DateTime now = DateTime.Now;
                DateTime time2 = new DateTime(now.Year, now.Month, now.Day, this._interval.Hours, this._interval.Minutes, this._interval.Seconds);
                if ((this.LastRunStartTime != DateTime.MinValue) && (now.Subtract(this.LastRunStartTime).TotalDays > 1.0))
                {
                    time2 = DateTime.Now.AddSeconds(10.0);
                }
                else if (time2 < now)
                {
                    now = now.AddDays(1.0);
                    time2 = new DateTime(now.Year, now.Month, now.Day, this._interval.Hours, this._interval.Minutes, this._interval.Seconds);
                }
                this.NextRunTime = time2;
                this._timer.Change(time2.Subtract(DateTime.Now), TimeSpan.Zero);
            }
        }

        /// <summary>
        /// Stops this job timer.
        /// </summary>
        public void Stop()
        {
            this.Stop(false);
        }

        /// <summary>
        /// Stops this job timer.
        /// </summary>
        /// <param name="cancel">if set to <c>true</c> cancel running job.</param>
        public void Stop(bool cancel)
        {
            JobManager.Current.OnJobStopping(new JobEventArgs(this.Name, JobAction.Stopping, this._id));
            this.StopTimer();
            if ((cancel && this.IsBusy) && (this._instance != null))
            {
                this._instance.Cancel();
            }
            this.Status = JobStatus.Stopped;
        }

        private void StopTimer()
        {
            this._timer.Change(-1, -1);
        }

        private void UpdateStatus(string message)
        {
            this.LastResult = message;
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
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description
        {
            get
            {
                return this._description;
            }
        }

        /// <summary>
        /// Gets the group name.
        /// </summary>
        /// <value>The group name.</value>
        public string Group
        {
            get
            {
                return this._group;
            }
        }

        public IJob Instance
        {
            get
            {
                this.CreateInstance();
                return this._instance;
            }
        }

        /// <summary>
        /// Gets the job interval.
        /// </summary>
        /// <value>The job interval.</value>
        public TimeSpan Interval
        {
            get
            {
                return this._interval;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this job is busy.
        /// </summary>
        /// <value><c>true</c> if this job is busy; otherwise, <c>false</c>.</value>
        /// <remarks>This property is thread safe.</remarks>
        public bool IsBusy
        {
            get
            {
                return this._isBusy.Value;
            }
            private set
            {
                this._isBusy.Value = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether Interval is a time of day.
        /// </summary>
        /// <value>
        /// <c>true</c> if Interval is time of day; otherwise, <c>false</c>.
        /// </value>
        public bool IsTimeOfDay
        {
            get
            {
                return this._isTimeOfDay;
            }
        }

        /// <summary>
        /// Gets a value indicating whether to keep alive the job instance.
        /// </summary>
        /// <value><c>true</c> to keep alive; otherwise, <c>false</c>.</value>
        public bool KeepAlive
        {
            get
            {
                return this._keepAlive;
            }
        }

        /// <summary>
        /// Gets the last result.
        /// </summary>
        /// <value>The last result.</value>
        /// <remarks>This property is thread safe.</remarks>
        public string LastResult
        {
            get
            {
                return this._lastResult.Value;
            }
            set
            {
                this._lastResult.Value = value;
            }
        }

        /// <summary>
        /// Gets the last run duration.
        /// </summary>
        /// <value>The last run duration.</value>
        /// <remarks>This property is thread safe.</remarks>
        public TimeSpan LastRunDuration
        {
            get
            {
                return this._lastRunFinishTime.Value.Subtract(this._lastRunStartTime.Value);
            }
        }

        /// <summary>
        /// Gets the last run finish time.
        /// </summary>
        /// <value>The last run finish time.</value>
        /// <remarks>This property is thread safe.</remarks>
        public DateTime LastRunFinishTime
        {
            get
            {
                return this._lastRunFinishTime.Value;
            }
            set
            {
                this._lastRunFinishTime.Value = value;
            }
        }

        /// <summary>
        /// Gets the last run start time.
        /// </summary>
        /// <value>The last run start time.</value>
        /// <remarks>This property is thread safe.</remarks>
        public DateTime LastRunStartTime
        {
            get
            {
                return this._lastRunStartTime.Value;
            }
            set
            {
                this._lastRunStartTime.Value = value;
            }
        }

        /// <summary>
        /// Gets the last status.
        /// </summary>
        /// <value>The last status.</value>
        /// <remarks>This property is thread safe.</remarks>
        public JobStatus LastStatus
        {
            get
            {
                return this._lastStatus.Value;
            }
            set
            {
                this._lastStatus.Value = value;
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get
            {
                return this._name;
            }
        }

        /// <summary>
        /// Gets the next run time.
        /// </summary>
        /// <value>The next run time.</value>
        /// <remarks>This property is thread safe.</remarks>
        public DateTime NextRunTime
        {
            get
            {
                return this._nextRunTime.Value;
            }
            private set
            {
                this._nextRunTime.Value = value;
            }
        }

        /// <summary>
        /// Gets the status.
        /// </summary>
        /// <value>The status.</value>
        /// <remarks>This property is thread safe.</remarks>
        public JobStatus Status
        {
            get
            {
                return this._status.Value;
            }
            private set
            {
                this._status.Value = value;
            }
        }
    }
}

