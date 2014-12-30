using System;
using System.Diagnostics;

namespace LFNet.Common.Extensions
{
    public static class StringValidationExtensions
    {
        [DebuggerHidden]
        public static Validation<string> ExactLenght(this Validation<string> item, int length)
        {
            if (item.Value.Length != length)
            {
                throw new ArgumentOutOfRangeException(item.ArgName, item.Value, "Parameter {0} has to be {1} characters long.".FormatWith(new object[] { item.ArgName, length }));
            }
            return item;
        }

        [DebuggerHidden]
        public static Validation<string> LongerThan(this Validation<string> item, int limit)
        {
            if (item.Value.Length <= limit)
            {
                throw new ArgumentException("Parameter {0} must be longer than {1} chars".FormatWith(new object[] { item.ArgName, limit }));
            }
            return item;
        }

        [DebuggerHidden]
        public static Validation<string> NotEmpty(this Validation<string> item)
        {
            if (item == "")
            {
                throw new ArgumentException("Parameter {0} may not be empty".FormatWith(new object[] { item.ArgName }), item.ArgName);
            }
            return item;
        }

        [DebuggerHidden]
        public static Validation<string> NotNullOrEmpty(this Validation<string> item)
        {
            item.NotNull<string>();
            item.NotEmpty();
            return item;
        }

        [DebuggerHidden]
        public static Validation<string> ShorterThan(this Validation<string> item, int limit)
        {
            if (item.Value.Length >= limit)
            {
                throw new ArgumentException("Parameter {0} must be shorter than {1} chars".FormatWith(new object[] { item.ArgName, limit }));
            }
            return item;
        }

        [DebuggerHidden]
        public static Validation<string> StartsWith(this Validation<string> item, string pattern)
        {
            if (!item.Value.StartsWith(pattern))
            {
                throw new ArgumentException("Parameter {0} must start with {1}".FormatWith(new object[] { item.ArgName, pattern }));
            }
            return item;
        }
    }
}

