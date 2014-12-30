using System;

namespace LFNet.Common.Rules
{
    public class RuleContext<T> where T: class
    {
        public Exception Error { get; set; }

        public bool IsComplete { get; set; }

        public T State { get; set; }
    }
}

