namespace LFNet.Common.Reflection
{
    public class ArraySpec
    {
        private bool bound;
        private int dimensions;

        internal ArraySpec(int dimensions, bool bound)
        {
            this.dimensions = dimensions;
            this.bound = bound;
        }

        public override string ToString()
        {
            if (this.bound)
            {
                return "[*]";
            }
            string str = "[";
            for (int i = 1; i < this.dimensions; i++)
            {
                str = str + ",";
            }
            return (str + "]");
        }
    }
}

