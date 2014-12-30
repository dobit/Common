using System;

namespace LFNet.Common.Threading
{
    [Flags]
    public enum CallToPostExecute
    {
        Never,
        WhenWorkItemCanceled,
        WhenWorkItemNotCanceled,
        Always
    }
}

