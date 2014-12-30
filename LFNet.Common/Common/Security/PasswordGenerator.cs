using System;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;

namespace LFNet.Common.Security
{
    public class PasswordGenerator
    {
        private static readonly char[] _consonants = new char[] { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'r', 's', 't', 'v' };
        private static readonly char[] _doubleConsonants = new char[] { 'c', 'd', 'f', 'g', 'l', 'm', 'n', 'p', 'r', 's', 't' };
        private static readonly Random _random = new Random();
        private static readonly char[] _vowels = new char[] { 'a', 'e', 'i', 'o', 'u' };
        private const string DEFAULT_ALPHANUMERIC_CHARS = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        private const int DEFAULT_LENGTH = 10;
        private const int DEFAULT_REQUIRED_SPECIAL_CHARS = 2;
        private const string DEFAULT_SPECIAL_CHARS = "!@$?_-";

        public PasswordGenerator()
            : this(DEFAULT_LENGTH, DEFAULT_REQUIRED_SPECIAL_CHARS)
        {
        }

        public PasswordGenerator(int length, int requiredSpecialCharacters)
            : this(length, requiredSpecialCharacters, DEFAULT_ALPHANUMERIC_CHARS, DEFAULT_SPECIAL_CHARS)
        {
        }

        public PasswordGenerator(int length, int requiredSpecialCharacters, string alphaNumericChars, string specialChars)
        {
            if ((length < 1) || (length > 0x80))
            {
                throw new ArgumentException("The specified password length is invalid.", "length");
            }
            if ((requiredSpecialCharacters > length) || (requiredSpecialCharacters < 0))
            {
                throw new ArgumentException("The specified number of required non-alphanumeric characters is invalid.", "requiredSpecialCharacters");
            }
            this.Length = length;
            this.RequiredSpecialCharacters = requiredSpecialCharacters;
            this.AllowedAlphaNumericCharacters = alphaNumericChars;
            this.AllowedSpecialCharacters = specialChars;
        }

        public static string Generate()
        {
            return Generate(10, 2);
        }

        public static string Generate(int length, int requiredSpecialChars)
        {
            PasswordGenerator generator = new PasswordGenerator(length, requiredSpecialChars);
            return generator.Next();
        }

        public static string GeneratePassword()
        {
            return GeneratePassword(Membership.MinRequiredPasswordLength);
        }

        public static string GeneratePassword(int passwordLength)
        {
            bool flag = false;
            int num = 0;
            StringBuilder builder = new StringBuilder();
            for (num = 0; num <= passwordLength; num++)
            {
                if (((builder.Length > 0) & !flag) & (_random.Next(100) < 10))
                {
                    builder.Append(_doubleConsonants[_random.Next(_doubleConsonants.Length)], 2);
                    num++;
                    flag = true;
                }
                else if (!flag & (_random.Next(100) < 90))
                {
                    builder.Append(_consonants[_random.Next(_consonants.Length)]);
                    flag = true;
                }
                else
                {
                    builder.Append(_vowels[_random.Next(_vowels.Length)]);
                    flag = false;
                }
            }
            builder.Length = passwordLength;
            return builder.ToString();
        }

        public string Next()
        {
            if ((this.Length < 1) || (this.Length > 0x80))
            {
                throw new InvalidOperationException("The specified password length is invalid.");
            }
            if ((this.RequiredSpecialCharacters > this.Length) || (this.RequiredSpecialCharacters < 0))
            {
                throw new InvalidOperationException("The specified number of required non-alphanumeric characters is invalid.");
            }
            string str = this.AllowedAlphaNumericCharacters + this.AllowedSpecialCharacters;
            byte[] data = new byte[this.Length];
            char[] chArray = new char[this.Length];
            new RNGCryptoServiceProvider().GetBytes(data);
            int num = 0;
            for (int i = 0; i < this.Length; i++)
            {
                int num3 = data[i] % str.Length;
                if (num3 > this.AllowedAlphaNumericCharacters.Length)
                {
                    num++;
                }
                chArray[i] = str[num3];
            }
            if (num < this.RequiredSpecialCharacters)
            {
                Random random = new Random();
                for (int j = 0; j < (this.RequiredSpecialCharacters - num); j++)
                {
                    int num5;
                    do
                    {
                        num5 = random.Next(0, this.Length);
                    }
                    while (!char.IsLetterOrDigit(chArray[num5]));
                    int num6 = random.Next(0, this.AllowedSpecialCharacters.Length);
                    chArray[num5] = this.AllowedSpecialCharacters[num6];
                }
            }
            return new string(chArray);
        }

        public string AllowedAlphaNumericCharacters { get; set; }

        public string AllowedSpecialCharacters { get; set; }

        public int Length { get; set; }

        public int RequiredSpecialCharacters { get; set; }
    }
}

