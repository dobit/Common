namespace LFNet.Common.Rules
{
    public abstract class RuleAction<T>
    {
        protected RuleAction()
        {
        }

        public abstract void Run(T context);

        public string Description { get; set; }

        public string Name { get; set; }
    }
}

