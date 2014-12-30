namespace LFNet.Common.Component
{
    /// <summary>
    /// The base class for pipeline modules
    /// </summary>
    /// <typeparam name="TContext">The type of the pipeline context.</typeparam>
    public abstract class PipelineActionBase<TContext> : IPipelineAction<TContext> where TContext: IPipelineContext
    {
        protected PipelineActionBase()
        {
        }

        /// <summary>
        /// Processes this module using the specified pipeline context.
        /// </summary>
        /// <param name="context">The pipeline context.</param>
        public abstract void Process(TContext context);

        /// <summary>
        /// Gets the priority of this pipeline module.
        /// </summary>
        /// <value>The pipeline module priority.</value>
        public virtual int Priority
        {
            get
            {
                return 10;
            }
        }
    }
}

