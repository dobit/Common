using System;

namespace LFNet.Common.Threading.Internal
{
    public class WorkItemFactory
    {
        /// <summary>
        /// Create a new work item
        /// </summary>
        /// <param name="workItemsGroup">The work items group.</param>
        /// <param name="wigStartInfo">Work item group start information</param>
        /// <param name="callback">A callback to execute</param>
        /// <returns>Returns a work item</returns>
        public static WorkItem CreateWorkItem(IWorkItemsGroup workItemsGroup, WIGStartInfo wigStartInfo, WorkItemCallback callback)
        {
            return CreateWorkItem(workItemsGroup, wigStartInfo, callback, null);
        }

        /// <summary>
        /// Create a new work item
        /// </summary>
        /// <param name="workItemsGroup"></param>
        /// <param name="wigStartInfo">Work item group start information</param>
        /// <param name="callback">A callback to execute</param>
        /// <param name="workItemPriority">The priority of the work item</param>
        /// <returns>Returns a work item</returns>
        public static WorkItem CreateWorkItem(IWorkItemsGroup workItemsGroup, WIGStartInfo wigStartInfo, WorkItemCallback callback, WorkItemPriority workItemPriority)
        {
            return CreateWorkItem(workItemsGroup, wigStartInfo, callback, null, workItemPriority);
        }

        /// <summary>
        /// Create a new work item
        /// </summary>
        /// <param name="workItemsGroup"></param>
        /// <param name="wigStartInfo">Work item group start information</param>
        /// <param name="callback">A callback to execute</param>
        /// <param name="state">
        /// The context object of the work item. Used for passing arguments to the work item. 
        /// </param>
        /// <returns>Returns a work item</returns>
        public static WorkItem CreateWorkItem(IWorkItemsGroup workItemsGroup, WIGStartInfo wigStartInfo, WorkItemCallback callback, object state)
        {
            ValidateCallback(callback);
            return new WorkItem(workItemsGroup, new WorkItemInfo { UseCallerCallContext = wigStartInfo.UseCallerCallContext, UseCallerHttpContext = wigStartInfo.UseCallerHttpContext, PostExecuteWorkItemCallback = wigStartInfo.PostExecuteWorkItemCallback, CallToPostExecute = wigStartInfo.CallToPostExecute, DisposeOfStateObjects = wigStartInfo.DisposeOfStateObjects }, callback, state);
        }

        /// <summary>
        /// Create a new work item
        /// </summary>
        /// <param name="workItemsGroup"></param>
        /// <param name="wigStartInfo">Work item group start information</param>
        /// <param name="workItemInfo">Work item info</param>
        /// <param name="callback">A callback to execute</param>
        /// <returns>Returns a work item</returns>
        public static WorkItem CreateWorkItem(IWorkItemsGroup workItemsGroup, WIGStartInfo wigStartInfo, WorkItemInfo workItemInfo, WorkItemCallback callback)
        {
            return CreateWorkItem(workItemsGroup, wigStartInfo, workItemInfo, callback, null);
        }

        /// <summary>
        /// Create a new work item
        /// </summary>
        /// <param name="workItemsGroup"></param>
        /// <param name="wigStartInfo">Work item group start information</param>
        /// <param name="callback">A callback to execute</param>
        /// <param name="state">
        /// The context object of the work item. Used for passing arguments to the work item. 
        /// </param>
        /// <param name="postExecuteWorkItemCallback">
        /// A delegate to call after the callback completion
        /// </param>
        /// <returns>Returns a work item</returns>
        public static WorkItem CreateWorkItem(IWorkItemsGroup workItemsGroup, WIGStartInfo wigStartInfo, WorkItemCallback callback, object state, PostExecuteWorkItemCallback postExecuteWorkItemCallback)
        {
            ValidateCallback(callback);
            ValidateCallback(postExecuteWorkItemCallback);
            return new WorkItem(workItemsGroup, new WorkItemInfo { UseCallerCallContext = wigStartInfo.UseCallerCallContext, UseCallerHttpContext = wigStartInfo.UseCallerHttpContext, PostExecuteWorkItemCallback = postExecuteWorkItemCallback, CallToPostExecute = wigStartInfo.CallToPostExecute, DisposeOfStateObjects = wigStartInfo.DisposeOfStateObjects }, callback, state);
        }

        /// <summary>
        /// Create a new work item
        /// </summary>
        /// <param name="workItemsGroup"></param>
        /// <param name="wigStartInfo">Work item group start information</param>
        /// <param name="callback">A callback to execute</param>
        /// <param name="state">
        /// The context object of the work item. Used for passing arguments to the work item. 
        /// </param>
        /// <param name="workItemPriority">The work item priority</param>
        /// <returns>Returns a work item</returns>
        public static WorkItem CreateWorkItem(IWorkItemsGroup workItemsGroup, WIGStartInfo wigStartInfo, WorkItemCallback callback, object state, WorkItemPriority workItemPriority)
        {
            ValidateCallback(callback);
            return new WorkItem(workItemsGroup, new WorkItemInfo { UseCallerCallContext = wigStartInfo.UseCallerCallContext, UseCallerHttpContext = wigStartInfo.UseCallerHttpContext, PostExecuteWorkItemCallback = wigStartInfo.PostExecuteWorkItemCallback, CallToPostExecute = wigStartInfo.CallToPostExecute, DisposeOfStateObjects = wigStartInfo.DisposeOfStateObjects, WorkItemPriority = workItemPriority }, callback, state);
        }

        /// <summary>
        /// Create a new work item
        /// </summary>
        /// <param name="workItemsGroup"></param>
        /// <param name="wigStartInfo">Work item group start information</param>
        /// <param name="workItemInfo">Work item information</param>
        /// <param name="callback">A callback to execute</param>
        /// <param name="state">
        /// The context object of the work item. Used for passing arguments to the work item. 
        /// </param>
        /// <returns>Returns a work item</returns>
        public static WorkItem CreateWorkItem(IWorkItemsGroup workItemsGroup, WIGStartInfo wigStartInfo, WorkItemInfo workItemInfo, WorkItemCallback callback, object state)
        {
            ValidateCallback(callback);
            ValidateCallback(workItemInfo.PostExecuteWorkItemCallback);
            return new WorkItem(workItemsGroup, new WorkItemInfo(workItemInfo), callback, state);
        }

        /// <summary>
        /// Create a new work item
        /// </summary>
        /// <param name="workItemsGroup"></param>
        /// <param name="wigStartInfo">Work item group start information</param>
        /// <param name="callback">A callback to execute</param>
        /// <param name="state">
        /// The context object of the work item. Used for passing arguments to the work item. 
        /// </param>
        /// <param name="postExecuteWorkItemCallback">
        /// A delegate to call after the callback completion
        /// </param>
        /// <param name="callToPostExecute">Indicates on which cases to call to the post execute callback</param>
        /// <returns>Returns a work item</returns>
        public static WorkItem CreateWorkItem(IWorkItemsGroup workItemsGroup, WIGStartInfo wigStartInfo, WorkItemCallback callback, object state, PostExecuteWorkItemCallback postExecuteWorkItemCallback, CallToPostExecute callToPostExecute)
        {
            ValidateCallback(callback);
            ValidateCallback(postExecuteWorkItemCallback);
            return new WorkItem(workItemsGroup, new WorkItemInfo { UseCallerCallContext = wigStartInfo.UseCallerCallContext, UseCallerHttpContext = wigStartInfo.UseCallerHttpContext, PostExecuteWorkItemCallback = postExecuteWorkItemCallback, CallToPostExecute = callToPostExecute, DisposeOfStateObjects = wigStartInfo.DisposeOfStateObjects }, callback, state);
        }

        /// <summary>
        /// Create a new work item
        /// </summary>
        /// <param name="workItemsGroup"></param>
        /// <param name="wigStartInfo">Work item group start information</param>
        /// <param name="callback">A callback to execute</param>
        /// <param name="state">
        /// The context object of the work item. Used for passing arguments to the work item. 
        /// </param>
        /// <param name="postExecuteWorkItemCallback">
        /// A delegate to call after the callback completion
        /// </param>
        /// <param name="workItemPriority">The work item priority</param>
        /// <returns>Returns a work item</returns>
        public static WorkItem CreateWorkItem(IWorkItemsGroup workItemsGroup, WIGStartInfo wigStartInfo, WorkItemCallback callback, object state, PostExecuteWorkItemCallback postExecuteWorkItemCallback, WorkItemPriority workItemPriority)
        {
            ValidateCallback(callback);
            ValidateCallback(postExecuteWorkItemCallback);
            return new WorkItem(workItemsGroup, new WorkItemInfo { UseCallerCallContext = wigStartInfo.UseCallerCallContext, UseCallerHttpContext = wigStartInfo.UseCallerHttpContext, PostExecuteWorkItemCallback = postExecuteWorkItemCallback, CallToPostExecute = wigStartInfo.CallToPostExecute, DisposeOfStateObjects = wigStartInfo.DisposeOfStateObjects, WorkItemPriority = workItemPriority }, callback, state);
        }

        /// <summary>
        /// Create a new work item
        /// </summary>
        /// <param name="workItemsGroup"></param>
        /// <param name="wigStartInfo">Work item group start information</param>
        /// <param name="callback">A callback to execute</param>
        /// <param name="state">
        /// The context object of the work item. Used for passing arguments to the work item. 
        /// </param>
        /// <param name="postExecuteWorkItemCallback">
        /// A delegate to call after the callback completion
        /// </param>
        /// <param name="callToPostExecute">Indicates on which cases to call to the post execute callback</param>
        /// <param name="workItemPriority">The work item priority</param>
        /// <returns>Returns a work item</returns>
        public static WorkItem CreateWorkItem(IWorkItemsGroup workItemsGroup, WIGStartInfo wigStartInfo, WorkItemCallback callback, object state, PostExecuteWorkItemCallback postExecuteWorkItemCallback, CallToPostExecute callToPostExecute, WorkItemPriority workItemPriority)
        {
            ValidateCallback(callback);
            ValidateCallback(postExecuteWorkItemCallback);
            return new WorkItem(workItemsGroup, new WorkItemInfo { UseCallerCallContext = wigStartInfo.UseCallerCallContext, UseCallerHttpContext = wigStartInfo.UseCallerHttpContext, PostExecuteWorkItemCallback = postExecuteWorkItemCallback, CallToPostExecute = callToPostExecute, WorkItemPriority = workItemPriority, DisposeOfStateObjects = wigStartInfo.DisposeOfStateObjects }, callback, state);
        }

        private static void ValidateCallback(Delegate callback)
        {
            if (callback.GetInvocationList().Length > 1)
            {
                throw new NotSupportedException("SmartThreadPool doesn't support delegates chains");
            }
        }
    }
}

