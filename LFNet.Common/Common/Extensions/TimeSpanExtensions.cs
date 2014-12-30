using System;

namespace LFNet.Common.Extensions
{
    public static class TimeSpanExtensions
    {
        public const double AvgDaysInAMonth = 30.436875;
        public const double AvgDaysInAYear = 365.2425;

        public static int GetDays(this TimeSpan timespan)
        {
            return (int) (timespan.TotalDays % 7.0);
        }

        public static double GetMicroseconds(this TimeSpan span)
        {
            return (((double) span.Ticks) / 10.0);
        }

        public static int GetMonths(this TimeSpan timespan)
        {
            return (int) ((timespan.TotalDays % 365.2425) / 30.436875);
        }

        public static double GetNanoseconds(this TimeSpan span)
        {
            return (((double) span.Ticks) / 100.0);
        }

        public static double GetTotalMonths(this TimeSpan timespan)
        {
            return (timespan.TotalDays / 30.436875);
        }

        public static double GetTotalWeeks(this TimeSpan timespan)
        {
            return (timespan.TotalDays / 7.0);
        }

        public static double GetTotalYears(this TimeSpan timespan)
        {
            return (timespan.TotalDays / 365.2425);
        }

        public static int GetWeeks(this TimeSpan timespan)
        {
            return (int) (((timespan.TotalDays % 365.2425) % 30.436875) / 7.0);
        }

        public static int GetYears(this TimeSpan timespan)
        {
            return (int) (timespan.TotalDays / 365.2425);
        }

        public static AgeSpan ToAgeSpan(this TimeSpan span)
        {
            return new AgeSpan(span);
        }

        public static string ToWords(this TimeSpan span)
        {
            return span.ToWords(false, -1);
        }

        public static string ToWords(this TimeSpan span, bool shortForm)
        {
            return span.ToWords(shortForm, -1);
        }

        public static string ToWords(this TimeSpan span, bool shortForm, int maxParts)
        {
            AgeSpan span2 = new AgeSpan(span);
            return span2.ToString(maxParts, shortForm, false);
        }
    }
}

