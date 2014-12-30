namespace LFNet.Common.Extensions
{
    public class Validation<T>
    {
        public Validation(T value, string argName)
        {
            this.Value = value;
            this.ArgName = argName;
        }

        public static implicit operator T(Validation<T> item)
        {
            return item.Value;
        }

        public string ArgName { get; set; }

        public T Value { get; set; }
    }
}

