namespace LFNet.Common.Threading
{
    /// <summary>
    /// Summary description for WIGStartInfo.
    /// </summary>
    public class WIGStartInfo
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
        /// Indicate the WorkItemsGroup to suspend the handling of the work items
        /// until the Start() method is called.
        /// </summary>
        private bool _startSuspended;
        /// <summary>
        /// Use the caller's security context
        /// </summary>
        private bool _useCallerCallContext;
        /// <summary>
        /// Use the caller's HTTP context
        /// </summary>
        private bool _useCallerHttpContext;

        public WIGStartInfo()
        {
            this._useCallerCallContext = false;
            this._useCallerHttpContext = false;
            this._disposeOfStateObjects = false;
            this._callToPostExecute = CallToPostExecute.Always;
            this._postExecuteWorkItemCallback = SmartThreadPool.DefaultPostExecuteWorkItemCallback;
            this._startSuspended = false;
        }

        public WIGStartInfo(WIGStartInfo wigStartInfo)
        {
            this._useCallerCallContext = wigStartInfo._useCallerCallContext;
            this._useCallerHttpContext = wigStartInfo._useCallerHttpContext;
            this._disposeOfStateObjects = wigStartInfo._disposeOfStateObjects;
            this._callToPostExecute = wigStartInfo._callToPostExecute;
            this._postExecuteWorkItemCallback = wigStartInfo._postExecuteWorkItemCallback;
            this._startSuspended = wigStartInfo._startSuspended;
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

        public bool StartSuspended
        {
            get
            {
                return this._startSuspended;
            }
            set
            {
                this._startSuspended = value;
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
    }
}

