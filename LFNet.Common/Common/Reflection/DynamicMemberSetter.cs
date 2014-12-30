namespace LFNet.Common.Reflection
{
    /// <summary>
    /// Delegate for setting a value.
    /// </summary>
    /// <param name="target">Target object.</param>
    /// <param name="arg">Argument value.</param>
    public delegate void DynamicMemberSetter(object target, object arg);
}

