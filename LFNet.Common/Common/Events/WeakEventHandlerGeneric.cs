using System;

namespace LFNet.Common.Events
{
    /// <summary>
    /// A handler for an event that doesn't store a reference to the source
    /// handler must be a instance method
    /// </summary>
    /// <typeparam name="T">type of calling object</typeparam>
    /// <typeparam name="E">type of event args</typeparam>
    /// <typeparam name="H">type of event handler</typeparam>
    public class WeakEventHandlerGeneric<T, E, H>
        where T : class
        where E : EventArgs
        where H : class
    {
        private delegate void OpenEventHandler(T @this, object sender, E e);
        private delegate void LocalHandler(object sender, E e);
        private WeakReference _targetRef;
        private WeakEventHandlerGeneric<T, E, H>.OpenEventHandler _openHandler;
        private H _handler;
        private UnregisterDelegate<H> _unregister;
        /// <summary>
        /// Gets the handler.
        /// </summary>
        public H Handler
        {
            get
            {
                return this._handler;
            }
        }
        public WeakEventHandlerGeneric(H eventHandler, UnregisterDelegate<H> unregister)
        {
            this._targetRef = new WeakReference((eventHandler as Delegate).Target);
            this._openHandler = (WeakEventHandlerGeneric<T, E, H>.OpenEventHandler)Delegate.CreateDelegate(typeof(WeakEventHandlerGeneric<T, E, H>.OpenEventHandler), null, (eventHandler as Delegate).Method);
            this._handler = WeakEventHandlerGeneric<T, E, H>.CastDelegate(new WeakEventHandlerGeneric<T, E, H>.LocalHandler(this.Invoke));
            this._unregister = unregister;
        }
        private void Invoke(object sender, E e)
        {
            T t = (T)((object)this._targetRef.Target);
            if (t != null)
            {
                this._openHandler(t, sender, e);
                return;
            }
            if (this._unregister != null)
            {
                this._unregister(this._handler);
                this._unregister = null;
            }
        }
        /// <summary>
        /// Performs an implicit conversion from <see cref="!:PR.utils.WeakEventHandler&lt;T,E&gt;" /> to <see cref="T:System.EventHandler`1" />.
        /// </summary>
        /// <param name="weh">The weh.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator H(WeakEventHandlerGeneric<T, E, H> weh)
        {
            return weh.Handler;
        }
        /// <summary>
        /// Casts the delegate.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static H CastDelegate(Delegate source)
        {
            if (source == null)
            {
                return default(H);
            }
            Delegate[] invocationList = source.GetInvocationList();
            if (invocationList.Length == 1)
            {
                return Delegate.CreateDelegate(typeof(H), invocationList[0].Target, invocationList[0].Method) as H;
            }
            for (int i = 0; i < invocationList.Length; i++)
            {
                invocationList[i] = Delegate.CreateDelegate(typeof(H), invocationList[i].Target, invocationList[i].Method);
            }
            return Delegate.Combine(invocationList) as H;
        }
    }
}
