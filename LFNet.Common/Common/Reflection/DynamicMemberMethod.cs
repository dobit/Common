namespace LFNet.Common.Reflection
{
    /// <summary>
    /// Delegate for a dynamic method.
    /// </summary>
    /// <param name="target">
    /// Object containg method to invoke.
    /// </param>
    /// <param name="args">
    /// Parameters passed to method.
    /// </param>
    public delegate object DynamicMemberMethod(object target, object[] args);
}

