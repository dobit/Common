using System;
using System.Threading;

namespace System.Threading
{
    /// <summary>Runs an action when the CountdownEvent reaches zero.</summary>
    public class ActionCountdownEvent : IDisposable
    {
        private readonly Action _action;
        private readonly ExecutionContext _context;
        private readonly CountdownEvent _event;

        /// <summary>Initializes the ActionCountdownEvent.</summary>
        /// <param name="initialCount">The number of signals required to set the CountdownEvent.</param>
        /// <param name="action">The delegate to be invoked when the count reaches zero.</param>
        public ActionCountdownEvent(int initialCount, Action action)
        {
            if (initialCount < 0)
            {
                throw new ArgumentOutOfRangeException("initialCount");
            }
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            this._action = action;
            this._event = new CountdownEvent(initialCount);
            if (initialCount == 0)
            {
                action();
            }
            else
            {
                this._context = ExecutionContext.Capture();
            }
        }

        /// <summary>Increments the current count by one.</summary>
        public void AddCount()
        {
            this._event.AddCount();
        }

        /// <summary>Releases all resources used by the current instance.</summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>Releases all resources used by the current instance.</summary>
        /// <param name="disposing">
        /// true if called because the object is being disposed; otherwise, false.
        /// </param>
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._event.Dispose();
            }
        }

        /// <summary>Registers a signal with the event, decrementing its count.</summary>
        public void Signal()
        {
            ContextCallback callback = null;
            if (this._event.Signal())
            {
                if (this._context != null)
                {
                    if (callback == null)
                    {
                        callback = _ => this._action();
                    }
                    ExecutionContext.Run(this._context, callback, null);
                }
                else
                {
                    this._action();
                }
            }
        }
    }
}

