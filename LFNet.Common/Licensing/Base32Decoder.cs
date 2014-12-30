using System;

namespace LFNet.Licensing
{
    public class Base32Decoder
    {
        public static byte[] Decode(string str)
        {
            return Base32Decoder.ConvertFromKey(str, 5);
        }
        private static byte[] ConvertFromKey(string key, byte base2)
        {
            if (Base32Decoder.IsEmpty(key))
            {
                return null;
            }
            uint num = (uint)Math.Floor((double)base2 / 8.0 * (double)key.Length);
            byte[] array = new byte[num];
            if (Base32Decoder.IsEmpty(array))
            {
                return null;
            }
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = 0;
            }
            int num2 = 0;
            int num3 = key.Length - 1;
            byte b = 0;
            while (num3 >= 0 && (long)num2 < (long)((ulong)(num * 8u)))
            {
                byte b2 = Base32Decoder.CharToVal(key[num3--]);
                int num4 = (int)array[num2 / 8] + ((int)b2 << num2 % 8) + (int)b;
                b = (byte)(num4 / 256);
                array[num2 / 8] = (byte)(num4 % 256);
                num2 += (int)base2;
            }
            return array;
        }
        private static bool IsEmpty(string value)
        {
            return value == null || value.Length == 0;
        }
        private static bool IsEmpty(Array value)
        {
            return value == null || value.Length == 0;
        }
        private static byte CharToVal(char c)
        {
            c = char.ToLower(c);
            if ('0' > c || c > '9')
            {
                return (byte)Base32Decoder.TrimNegative((int)((byte)(c - 'a' + '\n')));
            }
            return (byte)Base32Decoder.TrimNegative((int)((byte)(c - '0')));
        }
        private static int TrimNegative(int x)
        {
            if (x < 0)
            {
                return 0;
            }
            return x;
        }


    }
}