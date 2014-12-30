namespace LFNet.Common.Scheduler
{
    /// <summary>
    /// The status state for a job
    /// </summary>
    public enum JobStatus
    {
        None,
        Stopped,
        Running,
        Waiting,
        Error,
        Completed,
        Canceled
    }
}

