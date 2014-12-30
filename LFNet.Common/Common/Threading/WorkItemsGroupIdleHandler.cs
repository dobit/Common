namespace LFNet.Common.Threading
{
    /// <summary>
    /// A delegate to call when a WorkItemsGroup becomes idle
    /// </summary>
    /// <param name="workItemsGroup">A reference to the WorkItemsGroup that became idle</param>
    public delegate void WorkItemsGroupIdleHandler(IWorkItemsGroup workItemsGroup);
}

