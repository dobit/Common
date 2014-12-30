namespace LFNet.Common.Reflection
{
    /// <summary>
    /// Delegate for getting a value.
    /// </summary>
    /// <param name="target">Target object.</param>
    /// <returns>The value from the target.</returns>
    public delegate object DynamicMemberGetter(object target);
}

