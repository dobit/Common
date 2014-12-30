using System;
using System.Security.Cryptography;

namespace LFNet.Common
{
    /// <summary>
    /// 使用 RNGCryptoServiceProvider 產生由密碼編譯服務供應者 (CSP) 提供的亂數產生器。
    /// </summary>
    public static class RNG
    {
        private static readonly RNGCryptoServiceProvider rngp = new RNGCryptoServiceProvider();
        private static readonly byte[] rb = new byte[4];

        /// <summary>
        /// 產生一個非負數的亂數
        /// </summary>
        public static int Next()
        {
            rngp.GetBytes(rb);
            int value = BitConverter.ToInt32(rb, 0);
            if (value < 0) value = -value;
            return value;
        }

        /// <summary>
        /// 產生一個非負數且最大值 max 以下的亂數
        /// </summary>
        /// <param name="max">最大值</param>
        public static int Next(int max)
        {
            rngp.GetBytes(rb);
            int value = BitConverter.ToInt32(rb, 0);
            value = value%(max + 1);
            if (value < 0) value = -value;
            return value;
        }

        /// <summary>
        /// 產生一個非負數且最小值在 min 以上最大值在 max 以下的亂數
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        public static int Next(int min, int max)
        {
            int value = Next(max - min) + min;
            return value;
        }

        /// <summary>
        /// 产生定长度的随机密码
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomPassword(int length)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            char[] chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            for (int i = 0; i < length; i++)
            {
                sb.Append(chars[RNG.Next(chars.Length - 1)]);
            }
            return sb.ToString();
        }
    }
}