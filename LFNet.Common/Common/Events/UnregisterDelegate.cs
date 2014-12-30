namespace LFNet.Common.Events
{
    /// <summary>
    /// Delegate of an unsubscribe delegate
    /// </summary>
    public delegate void UnregisterDelegate<H>(H eventHandler) where H: class;
}

