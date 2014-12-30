using System.ComponentModel;

namespace LFNet.Common.Events
{
    /// <summary>
    /// An interface for a weak event handler
    /// </summary>
    /// <typeparam name="E"></typeparam>
    public interface IWeakPropertyChangedEventHandler
    {
        PropertyChangedEventHandler Handler { get; }
    }
}

