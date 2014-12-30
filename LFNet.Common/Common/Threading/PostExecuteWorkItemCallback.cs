namespace LFNet.Common.Threading
{
    /// <summary>
    /// A delegate to call after the WorkItemCallback completed
    /// </summary>
    /// <param name="wir">The work item result object</param>
    public delegate void PostExecuteWorkItemCallback(IWorkItemResult wir);
}

