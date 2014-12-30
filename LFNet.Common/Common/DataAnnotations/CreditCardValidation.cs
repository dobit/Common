using System;
using System.Text;

namespace LFNet.Common.DataAnnotations
{
    public class CreditCardValidation
    {
        /// <summary>
        /// Cleans the credit card number.
        /// </summary>
        /// <param name="cardNumber">The card number.</param>
        /// <returns>The card number with only the valid digits.</returns>
        public static string CleanCardNumber(string cardNumber)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < cardNumber.Length; i++)
            {
                if (char.IsDigit(cardNumber, i))
                {
                    builder.Append(cardNumber[i]);
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Determines whether the credit card is exired.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        /// <returns>
        /// <c>true</c> if credit card is exired; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCardExired(int year, int month)
        {
            DateTime time2 = new DateTime(year, month, 1);
            return (time2.AddMonths(1).Subtract(TimeSpan.FromDays(1.0)) < DateTime.Today);
        }

        /// <summary>
        /// Validates a credit card number using the standard Luhn/mod10 validation algorithm.
        /// </summary>
        /// <param name="cardNumber">Card number, with or without punctuation</param>
        /// <returns>
        /// <c>true</c> if card number appears valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCardNumberValid(string cardNumber)
        {
            int num;
            StringBuilder builder = new StringBuilder();
            for (num = 0; num < cardNumber.Length; num++)
            {
                if (char.IsDigit(cardNumber, num))
                {
                    builder.Append(cardNumber[num]);
                }
            }
            if ((builder.Length < 13) || (builder.Length > 0x10))
            {
                return false;
            }
            num = builder.Length + 1;
            while (num <= 0x10)
            {
                builder.Insert(0, "0");
                num++;
            }
            string str = builder.ToString();
            int num2 = 0;
            for (num = 1; num <= 0x10; num++)
            {
                int num3 = 1 + (num % 2);
                int num5 = int.Parse(str.Substring(num - 1, 1)) * num3;
                if (num5 > 9)
                {
                    num5 -= 9;
                }
                num2 += num5;
            }
            return ((num2 % 10) == 0);
        }
    }
}

