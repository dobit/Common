using System;

namespace LFNet.Common.Component
{
    public class AnonymousDisposable : IDisposable
    {
        void IDisposable.Dispose()
        {
            this.Action();
        }

        public global::System.Action Action { get; set; }
    }
}

