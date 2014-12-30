using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using LFNet.Common.Security;

namespace LFNet.Common.Extensions
{
    /// <summary>
    /// Hash Extension methods
    /// </summary>
    public static class HashExtensions
    {
        /// <summary>Compute hash on input stream</summary>
        /// <param name="input">The stream to compute hash on.</param>
        /// <returns>The hash as a hexadecimal string.</returns>
        public static string ComputeHash(this Stream input, HashAlgorithm algorithm)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            BufferedStream inputStream = new BufferedStream(input, 0x2000);
            return algorithm.ComputeHash(inputStream).ToHex();
        }

        /// <summary>Compute hash on input string</summary>
        /// <param name="input">The string to compute hash on.</param>
        /// <returns>The hash as a hexadecimal string.</returns>
        public static string ComputeHash(this string input, HashAlgorithm algorithm)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentNullException("input");
            }
            return algorithm.ComputeHash(Encoding.Unicode.GetBytes(input)).ToHex();
        }

        /// <summary>
        /// Compute hash on byte array
        /// </summary>
        /// <param name="input">The byte array to get hash from.</param>
        /// <returns>The hash as a hexadecimal string.</returns>
        public static string ComputeHash(this byte[] input, HashAlgorithm algorithm)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            return algorithm.ComputeHash(input).ToHex();
        }

        /// <summary>Compute hash on input string</summary>
        /// <param name="file">The file to get hash from.</param>
        /// <returns>The hash as a hexadecimal string.</returns>
        public static string ComputeHash(this FileInfo file, HashAlgorithm algorithm)
        {
            using (BufferedStream stream = new BufferedStream(file.OpenRead(), 0x2000))
            {
                return stream.ComputeHash(algorithm);
            }
        }

        /// <summary>Compute hash on input string</summary>
        /// <param name="buffer">The string to compute hash on.</param>
        /// <returns>The hash as a hexadecimal string.</returns>
        public static string ComputeHash(this StringBuilder buffer, HashAlgorithm algorithm)
        {
            return buffer.ToString().ComputeHash(algorithm);
        }

        /// <summary>
        /// Converts a hexadecimal string into a byte array.
        /// </summary>
        /// <param name="hex">The hex string.</param>
        /// <returns>A byte array.</returns>
        public static byte[] ToByteArray(this string hex)
        {
            return (from x in Enumerable.Range(0, hex.Length)
                where 0 == (x % 2)
                select Convert.ToByte(hex.Substring(x, 2), 0x10)).ToArray<byte>();
        }

        /// <summary>
        /// Compute CRC32 hash on input string
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The hash as a hexadecimal string.</returns>
        public static string ToCRC32(this byte[] buffer)
        {
            return buffer.ComputeHash(new Crc32());
        }

        /// <summary>Compute CRC32 hash on input string</summary>
        /// <param name="file">The file to get hash from.</param>
        /// <returns>The hash as a hexadecimal string.</returns>
        public static string ToCRC32(this FileInfo file)
        {
            return file.ComputeHash(new Crc32());
        }

        /// <summary>Compute CRC32 hash on input string</summary>
        /// <param name="input">The string to compute hash on.</param>
        /// <returns>The hash as a hexadecimal string.</returns>
        public static string ToCRC32(this Stream input)
        {
            return input.ComputeHash(new Crc32());
        }

        /// <summary>Compute CRC32 hash on input string</summary>
        /// <param name="input">The string to compute hash on.</param>
        /// <returns>The hash as a hexadecimal string.</returns>
        public static string ToCRC32(this string input)
        {
            return input.ComputeHash(new Crc32());
        }

        /// <summary>Compute CRC32 hash on input string</summary>
        /// <param name="buffer">The string to compute hash on.</param>
        /// <returns>The hash as a hexadecimal string.</returns>
        public static string ToCRC32(this StringBuilder buffer)
        {
            return buffer.ComputeHash(new Crc32());
        }

        /// <summary>
        /// Converts a byte array to Hexadecimal.
        /// </summary>
        /// <param name="bytes">The bytes to convert.</param>
        /// <returns>Hexadecimal string of the byte array.</returns>
        public static string ToHex(this IEnumerable<byte> bytes)
        {
            StringBuilder builder = new StringBuilder();
            foreach (byte num in bytes)
            {
                builder.Append(num.ToString("x2"));
            }
            return builder.ToString();
        }

        /// <summary>Compute MD5 hash on input string</summary>
        /// <param name="input">The string to compute hash on.</param>
        /// <returns>The hash as a hexadecimal string.</returns>
        public static string ToMD5(this Stream input)
        {
            return input.ComputeHash(MD5.Create());
        }

        /// <summary>Compute MD5 hash on input string</summary>
        /// <param name="input">The string to compute hash on.</param>
        /// <returns>The hash as a hexadecimal string.</returns>
        public static string ToMD5(this string input)
        {
            return input.ComputeHash(MD5.Create());
        }

        /// <summary>
        /// Compute MD5 hash on input string
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The hash as a hexadecimal string.</returns>
        public static string ToMD5(this byte[] buffer)
        {
            return buffer.ComputeHash(MD5.Create());
        }

        /// <summary>Compute MD5 hash on input string</summary>
        /// <param name="file">The file to get hash from.</param>
        /// <returns>The hash as a hexadecimal string.</returns>
        public static string ToMD5(this FileInfo file)
        {
            return file.ComputeHash(MD5.Create());
        }

        /// <summary>Compute MD5 hash on input string</summary>
        /// <param name="buffer">The string to compute hash on.</param>
        /// <returns>The hash as a hexadecimal string.</returns>
        public static string ToMD5(this StringBuilder buffer)
        {
            return buffer.ComputeHash(MD5.Create());
        }

        /// <summary>Compute SHA1 hash on input string</summary>
        /// <param name="file">The file to get hash from.</param>
        /// <returns>The hash as a hexadecimal string.</returns>
        public static string ToSHA1(this FileInfo file)
        {
            return file.ComputeHash(new SHA1Managed());
        }

        /// <summary>Compute SHA1 hash on input string</summary>
        /// <param name="input">The string to compute hash on.</param>
        /// <returns>The hash as a hexadecimal string.</returns>
        public static string ToSHA1(this Stream input)
        {
            return input.ComputeHash(new SHA1Managed());
        }

        /// <summary>Compute SHA1 hash on input string</summary>
        /// <param name="input">The string to compute hash on.</param>
        /// <returns>The hash as a hexadecimal string.</returns>
        public static string ToSHA1(this string input)
        {
            return input.ComputeHash(new SHA1Managed());
        }

        /// <summary>
        /// Compute SHA1 hash on input string
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The hash as a hexadecimal string.</returns>
        public static string ToSHA1(this byte[] buffer)
        {
            return buffer.ComputeHash(new SHA1Managed());
        }

        /// <summary>Compute SHA1 hash on input string</summary>
        /// <param name="buffer">The string to compute hash on.</param>
        /// <returns>The hash as a hexadecimal string.</returns>
        public static string ToSHA1(this StringBuilder buffer)
        {
            return buffer.ComputeHash(new SHA1Managed());
        }

        /// <summary>
        /// Compute SHA256 hash on input string
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The hash as a hexadecimal string.</returns>
        public static string ToSHA256(this byte[] buffer)
        {
            return buffer.ComputeHash(new SHA256Managed());
        }

        /// <summary>Compute SHA256 hash on input string</summary>
        /// <param name="file">The file to get hash from.</param>
        /// <returns>The hash as a hexadecimal string.</returns>
        public static string ToSHA256(this FileInfo file)
        {
            return file.ComputeHash(new SHA256Managed());
        }

        /// <summary>Compute SHA256 hash on input string</summary>
        /// <param name="input">The string to compute hash on.</param>
        /// <returns>The hash as a hexadecimal string.</returns>
        public static string ToSHA256(this Stream input)
        {
            return input.ComputeHash(new SHA256Managed());
        }

        /// <summary>Compute SHA256 hash on input string</summary>
        /// <param name="input">The string to compute hash on.</param>
        /// <returns>The hash as a hexadecimal string.</returns>
        public static string ToSHA256(this string input)
        {
            return input.ComputeHash(new SHA256Managed());
        }

        /// <summary>Compute SHA256 hash on input string</summary>
        /// <param name="buffer">The string to compute hash on.</param>
        /// <returns>The hash as a hexadecimal string.</returns>
        public static string ToSHA256(this StringBuilder buffer)
        {
            return buffer.ComputeHash(new SHA256Managed());
        }

        /// <summary>
        /// Compute SHA512 hash on input string
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The hash as a hexadecimal string.</returns>
        public static string ToSHA512(this byte[] buffer)
        {
            return buffer.ComputeHash(new SHA512Managed());
        }

        /// <summary>Compute SHA512 hash on input string</summary>
        /// <param name="file">The file to get hash from.</param>
        /// <returns>The hash as a hexadecimal string.</returns>
        public static string ToSHA512(this FileInfo file)
        {
            return file.ComputeHash(new SHA512Managed());
        }

        /// <summary>Compute SHA512 hash on input string</summary>
        /// <param name="input">The string to compute hash on.</param>
        /// <returns>The hash as a hexadecimal string.</returns>
        public static string ToSHA512(this Stream input)
        {
            return input.ComputeHash(new SHA512Managed());
        }

        /// <summary>Compute SHA512 hash on input string</summary>
        /// <param name="input">The string to compute hash on.</param>
        /// <returns>The hash as a hexadecimal string.</returns>
        public static string ToSHA512(this string input)
        {
            return input.ComputeHash(new SHA512Managed());
        }

        /// <summary>Compute SHA512 hash on input string</summary>
        /// <param name="buffer">The string to compute hash on.</param>
        /// <returns>The hash as a hexadecimal string.</returns>
        public static string ToSHA512(this StringBuilder buffer)
        {
            return buffer.ComputeHash(new SHA512Managed());
        }
    }
}

