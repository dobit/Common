using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace LFNet.Common
{
    /// <summary>
    /// 提供一些加密函数方法
    /// </summary>
    public static class SecurityUtil
    {
        #region Sercurit

        /// <summary>
        /// MD5函数
        /// </summary>
        /// <param name="str">原始字符串</param>
        /// <returns>MD5结果</returns>
        public static string Md5(string str)
        {
            byte[] b = Encoding.Default.GetBytes(str);
            b = new MD5CryptoServiceProvider().ComputeHash(b);
            string ret = "";
            for (int i = 0; i < b.Length; i++)
                ret += b[i].ToString("x").PadLeft(2, '0');

            return ret;
        }

        /// <summary>
        /// 计算流的MD5值 可用于文件校验
        /// </summary>
        /// <param name="inputStream">流</param>
        /// <returns></returns>
        public static string Md5(Stream inputStream)
        {
            byte[] b = new MD5CryptoServiceProvider().ComputeHash(inputStream);
            string ret = "";
            for (int i = 0; i < b.Length; i++)
                ret += b[i].ToString("x").PadLeft(2, '0');

            return ret;
        }

        /// <summary>
        /// 按字节计算MD5
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Md5(byte[] data)
        {
            byte[] b = new MD5CryptoServiceProvider().ComputeHash(data);
            string ret = "";
            for (int i = 0; i < b.Length; i++)
                ret += b[i].ToString("x").PadLeft(2, '0');
            return ret;
        }

        /// <summary>
        /// SHA256函数
        /// </summary>
        /// /// <param name="str">原始字符串</param>
        /// <returns>SHA256结果</returns>
        public static string SHA256(string str)
        {
            byte[] sha256Data = Encoding.UTF8.GetBytes(str);
            var sha256 = new SHA256Managed();
            byte[] result = sha256.ComputeHash(sha256Data);
            return Convert.ToBase64String(result); //返回长度为44字节的字符串
        }

        /// <summary>
        /// 改正sql语句中的转义字符
        /// </summary>
        public static string MashSql(string str)
        {
            return (str == null) ? "" : str.Replace("\'", "'");
        }

        /// <summary>
        /// 替换sql语句中的有问题符号
        /// </summary>
        public static string ChkSql(string str)
        {
            return (str == null) ? "" : str.Replace("'", "''");
        }

        #endregion
    }
}