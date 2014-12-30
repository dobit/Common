using System;

namespace LFNet.Common.Events
{
    /// <summary>
    /// An interface for a weak event handler
    /// </summary>
    /// <typeparam name="E"></typeparam>
    public interface IWeakEventHandler<E> where E: EventArgs
    {
        EventHandler<E> Handler { get; }
    }
}

