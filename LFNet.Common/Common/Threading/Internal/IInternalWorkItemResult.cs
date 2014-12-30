namespace LFNet.Common.Threading.Internal
{
    internal interface IInternalWorkItemResult
    {
        event WorkItemStateCallback OnWorkItemCompleted;

        event WorkItemStateCallback OnWorkItemStarted;
    }
}

