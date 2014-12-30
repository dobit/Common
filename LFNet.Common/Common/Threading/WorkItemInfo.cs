namespace LFNet.Common.Threading
{
    /// <summary>
    /// Summary description for WorkItemInfo.
    /// </summary>
    public class WorkItemInfo
    {
        /// <summary>
        /// The option to run the post execute
        /// </summary>
        private CallToPostExecute _callToPostExecute;
        /// <summary>
        /// Dispose of the state object of a work item
        /// </summary>
        private bool _disposeOfStateObjects;
        /// <summary>
        /// A post execute callback to call when none is provided in 
        /// the QueueWorkItem method.
        /// </summary>
        private PostExecuteWorkItemCallback _postExecuteWorkItemCallback;
        /// <summary>
        /// Use the caller's security context
        /// </summary>
        private bool _useCallerCallContext;
        /// <summary>
        /// Use the caller's security context
        /// </summary>
        private bool _useCallerHttpContext;
        /// <summary>
        /// The priority of the work item
        /// </summary>
        private WorkItemPriority _workItemPriority;

        public WorkItemInfo()
        {
            this._useCallerCallContext = false;
            this._useCallerHttpContext = false;
            this._disposeOfStateObjects = false;
            this._callToPostExecute = CallToPostExecute.Always;
            this._postExecuteWorkItemCallback = SmartThreadPool.DefaultPostExecuteWorkItemCallback;
            this._workItemPriority = WorkItemPriority.Normal;
        }

        public WorkItemInfo(WorkItemInfo workItemInfo)
        {
            this._useCallerCallContext = workItemInfo._useCallerCallContext;
            this._useCallerHttpContext = workItemInfo._useCallerHttpContext;
            this._disposeOfStateObjects = workItemInfo._disposeOfStateObjects;
            this._callToPostExecute = workItemInfo._callToPostExecute;
            this._postExecuteWorkItemCallback = workItemInfo._postExecuteWorkItemCallback;
            this._workItemPriority = workItemInfo._workItemPriority;
        }

        public CallToPostExecute CallToPostExecute
        {
            get
            {
                return this._callToPostExecute;
            }
            set
            {
                this._callToPostExecute = value;
            }
        }

        public bool DisposeOfStateObjects
        {
            get
            {
                return this._disposeOfStateObjects;
            }
            set
            {
                this._disposeOfStateObjects = value;
            }
        }

        public PostExecuteWorkItemCallback PostExecuteWorkItemCallback
        {
            get
            {
                return this._postExecuteWorkItemCallback;
            }
            set
            {
                this._postExecuteWorkItemCallback = value;
            }
        }

        public bool UseCallerCallContext
        {
            get
            {
                return this._useCallerCallContext;
            }
            set
            {
                this._useCallerCallContext = value;
            }
        }

        public bool UseCallerHttpContext
        {
            get
            {
                return this._useCallerHttpContext;
            }
            set
            {
                this._useCallerHttpContext = value;
            }
        }

        public WorkItemPriority WorkItemPriority
        {
            get
            {
                return this._workItemPriority;
            }
            set
            {
                this._workItemPriority = value;
            }
        }
    }
}

