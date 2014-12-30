namespace LFNet.Common.Threading
{
    /// <summary>
    /// Specifies how a Lazy instance synchronizes access among multiple threads.
    /// </summary>
    public enum LazyThreadSafetyMode
    {
        None,
        PublicationOnly,
        ExecutionAndPublication
    }
}

