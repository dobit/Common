using System;
using System.IO;
using System.Security.Cryptography;

namespace LFNet.Common
{
   public static class HashFile
    {
       public static string HashSHA1(string path)
        {
            using (HashAlgorithm hashSHA1 = new SHA1Managed())
            {
                using (Stream file = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    byte[] hash = hashSHA1.ComputeHash(file);
                    return BitConverter.ToString(hash).Replace("-", string.Empty);
                }
            }
        }

       public static string HashSHA224(string path)
        {
            
            using (HashAlgorithm hashSHA224 = new SHA224Managed())
            {
                using (Stream file = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    byte[] hash = hashSHA224.ComputeHash(file);
                    return BitConverter.ToString(hash).Replace("-", string.Empty);
                }
            }
        }

       public static string HashSHA256(string path)
        {
            using (HashAlgorithm hashSHA256 = new SHA256Managed())
            {
                using (Stream file = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    byte[] hash = hashSHA256.ComputeHash(file);
                    return BitConverter.ToString(hash).Replace("-", string.Empty);
                }
            }
        }

       public static string HashSHA384(string path)
        {
            using (HashAlgorithm hashSHA384 = new SHA384Managed())
            {
                using (Stream file = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    byte[] hash = hashSHA384.ComputeHash(file);
                    return BitConverter.ToString(hash).Replace("-", string.Empty);
                }
            }
        }

       public static string HashSHA512(string path)
        {
            using (HashAlgorithm hashSHA512 = new SHA512Managed())
            {
                using (Stream file = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    byte[] hash = hashSHA512.ComputeHash(file);
                    return BitConverter.ToString(hash).Replace("-", string.Empty);
                }
            }
        }

       public static string HashMD5(string path)
        {
            using (HashAlgorithm hashMD5 = new MD5CryptoServiceProvider())
            {
                using (Stream file = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    byte[] hash = hashMD5.ComputeHash(file);
                    return BitConverter.ToString(hash).Replace("-", string.Empty);
                }
            }
        }

       public static string HashRipeMD160(string path)
        {
            using (HashAlgorithm hashRIPEMD160 = new RIPEMD160Managed())
            {
                using (Stream file = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    byte[] hash = hashRIPEMD160.ComputeHash(file);
                    return BitConverter.ToString(hash).Replace("-", string.Empty);
                }
            }
        }
    }
}
