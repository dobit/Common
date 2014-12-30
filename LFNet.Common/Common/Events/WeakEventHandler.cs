using System;

namespace LFNet.Common.Events
{
    /// <summary>
    /// A handler for an event that doesn't store a reference to the source
    /// handler must be a instance method
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="E"></typeparam>
    public class WeakEventHandler<T, E> : WeakEventHandlerGeneric<T, E, EventHandler<E>>, IWeakEventHandler<E> where T: class where E: EventArgs
    {
        public WeakEventHandler(EventHandler<E> eventHandler, UnregisterDelegate<EventHandler<E>> unregister) : base(eventHandler, unregister)
        {
        }
    }
}

