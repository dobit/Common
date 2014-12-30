namespace LFNet.Common.Threading
{
    /// <summary>
    /// A delegate that represents the method to run as the work item
    /// </summary>
    /// <param name="state">A state object for the method to run</param>
    public delegate object WorkItemCallback(object state);
}

