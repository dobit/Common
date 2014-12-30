namespace LFNet.Common.Extensions
{
    public static class ValidatorExtensions
    {
        public static Validation<T> Require<T>(this T item)
        {
            return new Validation<T>(item, "value");
        }

        public static Validation<T> Require<T>(this T item, string argName)
        {
            return new Validation<T>(item, argName);
        }
    }
}

