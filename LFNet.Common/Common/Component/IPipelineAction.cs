namespace LFNet.Common.Component
{
    public interface IPipelineAction<in TContext> where TContext: IPipelineContext
    {
        /// <summary>
        /// Processes this module using the specified pipeline context.
        /// </summary>
        /// <param name="context">The pipeline context.</param>
        void Process(TContext context);

        /// <summary>
        /// Gets the priority of this pipeline module.
        /// </summary>
        /// <value>The pipeline module priority.</value>
        int Priority { get; }
    }
}

