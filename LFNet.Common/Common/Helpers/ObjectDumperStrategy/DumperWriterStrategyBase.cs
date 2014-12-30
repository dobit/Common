namespace LFNet.Common.Helpers.ObjectDumperStrategy
{
    public abstract class DumperWriterStrategyBase : IDumperWriterStrategy
    {
        private int _depth;
        private int _level;

        protected DumperWriterStrategyBase()
        {
        }

        public abstract void Write(object o);

        protected int Depth
        {
            get
            {
                return this._depth;
            }
            set
            {
                this._depth = value;
            }
        }

        protected int Level
        {
            get
            {
                return this._level;
            }
            set
            {
                this._level = value;
            }
        }
    }
}

