using System;

namespace LFNet.Common.Extensions
{
    public static class DateTimeExtensions
    {
        private const int KindShift = 0x3e;
        private const ulong LocalMask = 9223372036854775808L;
        private const long TicksCeiling = 0x4000000000000000L;

        public static AgeSpan GetAge(this DateTime fromDate)
        {
            return fromDate.GetAge(DateTime.Now);
        }

        public static AgeSpan GetAge(this DateTime fromDate, DateTime toDate)
        {
            return new AgeSpan((TimeSpan) (toDate - fromDate));
        }

        public static DateTime SubtractDays(this DateTime date, double value)
        {
            if (value < 0.0)
            {
                throw new ArgumentException("Value cannot be less than 0.", "value");
            }
            return date.AddDays(value * -1.0);
        }

        public static DateTime SubtractHours(this DateTime date, double value)
        {
            if (value < 0.0)
            {
                throw new ArgumentException("Value cannot be less than 0.", "value");
            }
            return date.AddHours(value * -1.0);
        }

        public static DateTime SubtractMilliseconds(this DateTime date, double value)
        {
            if (value < 0.0)
            {
                throw new ArgumentException("Value cannot be less than 0.", "value");
            }
            return date.AddMilliseconds(value * -1.0);
        }

        public static DateTime SubtractMinutes(this DateTime date, double value)
        {
            if (value < 0.0)
            {
                throw new ArgumentException("Value cannot be less than 0.", "value");
            }
            return date.AddMinutes(value * -1.0);
        }

        public static DateTime SubtractMonths(this DateTime date, int months)
        {
            if (months < 0)
            {
                throw new ArgumentException("Months cannot be less than 0.", "months");
            }
            return date.AddMonths(months * -1);
        }

        public static DateTime SubtractSeconds(this DateTime date, double value)
        {
            if (value < 0.0)
            {
                throw new ArgumentException("Value cannot be less than 0.", "value");
            }
            return date.AddSeconds(value * -1.0);
        }

        public static DateTime SubtractTicks(this DateTime date, long value)
        {
            if (value < 0L)
            {
                throw new ArgumentException("Value cannot be less than 0.", "value");
            }
            return date.AddTicks(value * -1L);
        }

        public static DateTime SubtractYears(this DateTime date, int value)
        {
            if (value < 0)
            {
                throw new ArgumentException("Value cannot be less than 0.", "value");
            }
            return date.AddYears(value * -1);
        }

        public static string ToAgeString(this DateTime fromDate)
        {
            return fromDate.ToAgeString(DateTime.Now, 0);
        }

        public static string ToAgeString(this DateTime fromDate, int maxSpans)
        {
            return fromDate.ToAgeString(DateTime.Now, maxSpans);
        }

        public static string ToAgeString(this DateTime fromDate, DateTime toDate, int maxSpans)
        {
            return fromDate.GetAge(toDate).ToString(maxSpans, false, false);
        }

        public static string ToAgeString(this DateTime fromDate, int maxSpans, bool shortForm)
        {
            return fromDate.ToAgeString(DateTime.Now, maxSpans, shortForm);
        }

        public static string ToAgeString(this DateTime fromDate, DateTime toDate, int maxSpans, bool shortForm)
        {
            return fromDate.GetAge(toDate).ToString(maxSpans, shortForm, false);
        }

        public static string ToApproximateAgeString(this DateTime fromDate)
        {
            AgeSpan age = fromDate.GetAge();
            if (Math.Abs(age.TotalMinutes) <= 1.0)
            {
                if (age.TotalSeconds <= 0.0)
                {
                    return "Right now";
                }
                return "Just now";
            }
            if (age.TotalSeconds > 0.0)
            {
                return (age.ToString(1, false, false) + " ago");
            }
            return (age.ToString(1, false, false) + " from now");
        }

        /// <summary>
        /// Serializes the current DateTime object to a 64-bit binary value that subsequently can be used to recreate the DateTime object.
        /// </summary>
        /// <param name="self">The DateTime to serialize.</param>
        /// <returns>A 64-bit signed integer that encodes the Kind and Ticks properties.</returns>
        /// <remarks>
        /// This method exists to add missing funtionality in Silverlight.
        /// </remarks>
        public static long ToBinary(this DateTime self)
        {
            if (self.Kind != DateTimeKind.Local)
            {
                return (self.Ticks | (((long) self.Kind) << 0x3e));
            }
            TimeSpan utcOffset = TimeZoneInfo.Local.GetUtcOffset(self);
            long num2 = self.Ticks - utcOffset.Ticks;
            if (num2 < 0L)
            {
                num2 = 0x4000000000000000L + num2;
            }
            return (num2 | -9223372036854775808L);
        }

        /// <summary>
        /// Adjust the DateTime so the time is 1 millisecond before the next day.
        /// </summary>
        /// <param name="dateTime">The DateTime to adjust.</param>
        /// <returns>A DateTime that is 1 millisecond before the next day.</returns>
        public static DateTime ToEndOfDay(this DateTime dateTime)
        {
            return dateTime.Date.AddDays(1.0).Subtract(TimeSpan.FromMilliseconds(1.0));
        }

        public static int ToEpoch(this DateTime fromDate)
        {
            long num = (fromDate.ToUniversalTime().Ticks - 0x89f7ff5f7b58000L) / 0x989680L;
            return Convert.ToInt32(num);
        }

        public static int ToEpoch(this DateTime date, int offset)
        {
            return (offset + date.ToEpoch());
        }

        public static int ToEpochOffset(this DateTime date, int timestamp)
        {
            return (timestamp - date.ToEpoch());
        }
    }
}

