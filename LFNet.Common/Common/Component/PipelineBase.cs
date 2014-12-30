using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace LFNet.Common.Component
{
    /// <summary>
    /// The base class for a pipeline service.
    /// </summary>
    /// <typeparam name="TContext">The type used as the context for the pipeline.</typeparam>
    /// <typeparam name="TModule">The base type of the pipeline module to run in this pipeline.</typeparam>
    /// <remarks>
    /// The pipeline works by collection modules (classes) that have a common base class to run in a series.
    /// To setup a pipeline, you have to have a context class that will hold all the common data for the pipeline.
    /// You also have to have a common base class that inherits <see cref="T:LFNet.Common.Component.IPipelineContext" /> for all your modules.
    /// The pipeline looks for all types that inherit that common base class to run.
    /// </remarks>
    public abstract class PipelineBase<TContext, TModule> where TContext: IPipelineContext where TModule: IPipelineAction<TContext>
    {
        private static readonly ConcurrentDictionary<Type, List<TModule>> _moduleCache;

        static PipelineBase()
        {
            PipelineBase<TContext, TModule>._moduleCache = new ConcurrentDictionary<Type, List<TModule>>();
        }

        protected PipelineBase()
        {
        }

        /// <summary>
        /// Gets the modules that are subclasses of <typeparamref name="TModule" />.
        /// </summary>
        /// <returns>An enumerable list of modules to run for the pipeline.</returns>
        protected static IList<TModule> GetModules()
        {
            return PipelineBase<TContext, TModule>._moduleCache.GetOrAdd(typeof(TModule), t => (from r in (from type in t.Assembly.GetTypes()
                where ((type.IsClass && !type.IsNotPublic) && !type.IsAbstract) && type.IsSubclassOf(t)
                select type).Select<Type, object>(((Func<Type, object>) (type => Activator.CreateInstance(type)))).OfType<TModule>()
                orderby r.Priority
                select r).ToList<TModule>());
        }

        /// <summary>
        /// Called after all pipeline modules have run.
        /// </summary>
        /// <param name="context">The context the modules ran with.</param>
        protected virtual void PipelineCompleted(TContext context)
        {
        }

        /// <summary>
        /// Called before any pipeline modules are run.
        /// </summary>
        /// <param name="context">The context the modules will run with.</param>
        protected virtual void PipelineRunning(TContext context)
        {
        }

        /// <summary>
        /// Runs all the modules of pipeline with the specified context list.
        /// </summary>
        /// <param name="contexts">The context list to run the modules with.</param>
        public virtual void Run(IEnumerable<TContext> contexts)
        {
            IList<TModule> modules = PipelineBase<TContext, TModule>.GetModules();
            foreach (TContext local in contexts)
            {
                this.Run(local, modules);
            }
        }

        /// <summary>
        /// Runs all the modules of pipeline with the specified context.
        /// </summary>
        /// <param name="context">The context to run the modules with.</param>
        public virtual void Run(TContext context)
        {
            IList<TModule> modules = PipelineBase<TContext, TModule>.GetModules();
            this.Run(context, modules);
        }

        /// <summary>
        /// Runs all the specified modules of pipeline with the specified context.
        /// </summary>
        /// <param name="context">The context to run the modules with.</param>
        /// <param name="modules">The list modules to run.</param>
        protected virtual void Run(TContext context, IEnumerable<TModule> modules)
        {
            this.PipelineRunning(context);
            foreach (TModule local in modules)
            {
                local.Process(context);
                if (context.IsCancelled)
                {
                    break;
                }
            }
            this.PipelineCompleted(context);
        }
    }
}

