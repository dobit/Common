namespace LFNet.Common.Component
{
    /// <summary>
    /// The interface for pipeline context data
    /// </summary>
    public interface IPipelineContext
    {
        /// <summary>
        /// Gets or sets a value indicating whether this pipeline is cancelled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this pipeline is cancelled; otherwise, <c>false</c>.
        /// </value>
        bool IsCancelled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this pipeline context is processed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this pipeline context is processed; otherwise, <c>false</c>.
        /// </value>
        bool IsProcessed { get; set; }
    }
}

