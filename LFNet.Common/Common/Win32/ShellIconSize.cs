namespace LFNet.Common.Win32
{
    /// <summary>
    /// Represents the different icon sizes that can be extracted using the ExtractAssociatedIcon method.
    /// </summary>
    public enum ShellIconSize : uint
    {
        /// <summary>
        /// Specifies a large (32x32) icon.
        /// </summary>
        LargeIcon = 0x100,
        /// <summary>
        /// Specifies a small (16x16) icon.
        /// </summary>
        SmallIcon = 0x101
    }
}

