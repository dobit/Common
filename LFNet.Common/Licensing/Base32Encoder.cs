using System;

namespace LFNet.Licensing
{
    public class Base32Encoder
    {
        public static string Encode(byte[] data)
        {
            return Base32Encoder.ConvertToNumber(data, 5);
        }
        private static string ConvertToNumber(byte[] longNumber, byte base2)
        {
            string text = string.Empty;
            for (int i = 0; i < longNumber.Length * 8; i += (int)base2)
            {
                uint num = (uint)longNumber[i / 8];
                if (i % 8 > (int)(8 - base2) && i / 8 < longNumber.Length - 1)
                {
                    num += (uint)((uint)longNumber[i / 8 + 1] << 8);
                }
                byte b = (byte)Base32Encoder.Bits16(num, i % 8 + (int)base2, i % 8);
                text = Base32Encoder.ValToDigit(b) + text;
            }
            return text;
        }
        private static char ValToDigit(byte b)
        {
            if (b <= 9)
            {
                return (char)(b + 48);
            }
            return (char)(b - 10 + 97);
        }
        private static uint Bits16(uint number16, int from, int to)
        {
            return number16 >> to & (uint)Math.Pow(2.0, (double)(from - to)) - 1u;
        }
    }
}