using System;

namespace LFNet.Common.Extensions
{
    public static class NumberExtensions
    {
        public static bool Between(this byte value, byte start, byte end)
        {
            return ((start <= value) && (value <= end));
        }

        public static bool Between(this decimal value, decimal start, decimal end)
        {
            return ((start <= value) && (value <= end));
        }

        public static bool Between(this double value, double start, double end)
        {
            return ((start <= value) && (value <= end));
        }

        public static bool Between(this short value, short start, short end)
        {
            return ((start <= value) && (value <= end));
        }

        public static bool Between(this int value, int start, int end)
        {
            return ((start <= value) && (value <= end));
        }

        public static bool Between(this long value, long start, long end)
        {
            return ((start <= value) && (value <= end));
        }

        public static bool Between(this float value, float start, float end)
        {
            return ((start <= value) && (value <= end));
        }

        public static string ToFileSizeDisplay(this int i)
        {
            return ((long) i).ToFileSizeDisplay(2);
        }

        public static string ToFileSizeDisplay(this long i)
        {
            return i.ToFileSizeDisplay(2);
        }

        public static string ToFileSizeDisplay(this int i, int decimals)
        {
            return ((long) i).ToFileSizeDisplay(decimals);
        }

        public static string ToFileSizeDisplay(this long i, int decimals)
        {
            if (i < 0x40000000L)
            {
                string str = Math.Round((decimal) ((i / 1024M) / 1024M), decimals).ToString("N" + decimals);
                if ((decimals > 0) && str.EndsWith(new string('0', decimals)))
                {
                    str = str.Substring(0, (str.Length - decimals) - 1);
                }
                return (str + " MB");
            }
            string str2 = Math.Round((decimal) (((i / 1024M) / 1024M) / 1024M), decimals).ToString("N" + decimals);
            if ((decimals > 0) && str2.EndsWith(new string('0', decimals)))
            {
                str2 = str2.Substring(0, (str2.Length - decimals) - 1);
            }
            return (str2 + " GB");
        }
    }
}

