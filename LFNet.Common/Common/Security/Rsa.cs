using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace LFNet.Common.Security
{
    /// <summary>
    /// RSA加密
    /// </summary>
    public static class Rsa
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlPublicKey">xml 格式的公钥字符串</param>
        /// <param name="plainText">明文字符串</param>
        /// <returns></returns>
        public static string Encode(string xmlPublicKey, string plainText)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(Encode(xmlPublicKey, bytes));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlPrivateKey">xml 格式的私钥字符串</param>
        /// <param name="encryptedText">先加密然后经过Base64编码的字符串</param>
        /// <returns></returns>
        public static string Decode(string xmlPrivateKey, string encryptedText)
        {
            byte[] rgb = Convert.FromBase64String(encryptedText);
            return System.Text.Encoding.UTF8.GetString(Decode(xmlPrivateKey, rgb));
        }


        public static byte[] Decode(string xmlPrivateKey, byte[] encryptedData)
        {
            RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
            
            provider.FromXmlString(xmlPrivateKey);
            return provider.Decrypt(encryptedData, false);
        }

        public static byte[] Encode(string xmlPublicKey, byte[] data)
        {
            RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
            
            provider.FromXmlString(xmlPublicKey);
            return provider.Decrypt(data, false);
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="data"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static byte[] Encode(string filename,byte[] data,string password="")
        {
            X509Certificate2 x509 = new X509Certificate2(filename, password, X509KeyStorageFlags.Exportable);
            //if (x509.PublicKey.Key is DSACryptoServiceProvider)
            //{
            // DSACryptoServiceProvider crypto=   (DSACryptoServiceProvider) x509.PublicKey.Key;
            //    crypto.CreateSignature()
            //}
            if (x509.PublicKey.Key is RSACryptoServiceProvider)
            {
                var crypto = (RSACryptoServiceProvider)x509.PublicKey.Key;
              return  crypto.Encrypt(data, false);
            }
            return Encode(x509.PublicKey.Key.ToXmlString(), data);
        }
    }
}
