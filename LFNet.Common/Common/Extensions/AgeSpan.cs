using System;
using System.Runtime.InteropServices;
using System.Text;

namespace LFNet.Common.Extensions
{
    [StructLayout(LayoutKind.Sequential)]
    public struct AgeSpan
    {
        public AgeSpan(global::System.TimeSpan span)
        {
            this = new AgeSpan();
            this.TotalYears = span.GetTotalYears();
            this.Years = span.GetYears();
            this.TotalMonths = span.GetTotalMonths();
            this.Months = span.GetMonths();
            this.TotalWeeks = span.GetTotalWeeks();
            this.Weeks = span.GetWeeks();
            this.Days = span.GetDays();
            this.TimeSpan = span;
        }

        public double TotalYears { get; private set; }
        public int Years { get; private set; }
        public double TotalMonths { get; private set; }
        public int Months { get; private set; }
        public double TotalWeeks { get; private set; }
        public int Weeks { get; private set; }
        public double TotalDays
        {
            get
            {
                return this.TimeSpan.TotalDays;
            }
        }
        public int Days { get; private set; }
        public double TotalHours
        {
            get
            {
                return this.TimeSpan.TotalHours;
            }
        }
        public int Hours
        {
            get
            {
                return this.TimeSpan.Hours;
            }
        }
        public double TotalMinutes
        {
            get
            {
                return this.TimeSpan.TotalMinutes;
            }
        }
        public int Minutes
        {
            get
            {
                return this.TimeSpan.Minutes;
            }
        }
        public double TotalSeconds
        {
            get
            {
                return this.TimeSpan.TotalSeconds;
            }
        }
        public int Seconds
        {
            get
            {
                return this.TimeSpan.Seconds;
            }
        }
        public double TotalMilliseconds
        {
            get
            {
                return this.TimeSpan.TotalMilliseconds;
            }
        }
        public int Milliseconds
        {
            get
            {
                return this.TimeSpan.Milliseconds;
            }
        }
        public global::System.TimeSpan TimeSpan { get; private set; }
        public override string ToString()
        {
            return this.ToString(0x7fffffff, false, false);
        }

        public string ToString(int maxParts, bool shortForm = false, bool includeMilliseconds = false)
        {
            int partCount = 0;
            if (maxParts <= 0)
            {
                maxParts = 0x7fffffff;
            }
            StringBuilder builder = new StringBuilder();
            if ((!AppendPart(builder, "year", (double) this.Years, shortForm, ref partCount) || (maxParts <= 0)) || (partCount < maxParts))
            {
                if ((AppendPart(builder, "month", (double) this.Months, shortForm, ref partCount) && (maxParts > 0)) && (partCount >= maxParts))
                {
                    return builder.ToString();
                }
                if ((AppendPart(builder, "week", (double) this.Weeks, shortForm, ref partCount) && (maxParts > 0)) && (partCount >= maxParts))
                {
                    return builder.ToString();
                }
                if ((AppendPart(builder, "day", (double) this.Days, shortForm, ref partCount) && (maxParts > 0)) && (partCount >= maxParts))
                {
                    return builder.ToString();
                }
                if ((AppendPart(builder, "hour", (double) this.Hours, shortForm, ref partCount) && (maxParts > 0)) && (partCount >= maxParts))
                {
                    return builder.ToString();
                }
                if ((AppendPart(builder, "minute", (double) this.Minutes, shortForm, ref partCount) && (maxParts > 0)) && (partCount >= maxParts))
                {
                    return builder.ToString();
                }
                double a = (includeMilliseconds || (Math.Abs(this.TotalMinutes) > 1.0)) ? ((double) this.Seconds) : Math.Round(this.TotalSeconds, 2);
                if (a > 10.0)
                {
                    a = Math.Round(a);
                }
                if ((AppendPart(builder, "second", a, shortForm, ref partCount) && (maxParts > 0)) && (partCount >= maxParts))
                {
                    return builder.ToString();
                }
                if ((includeMilliseconds && AppendPart(builder, "millisecond", (double) this.Milliseconds, shortForm, ref partCount)) && ((maxParts > 0) && (partCount >= maxParts)))
                {
                    return builder.ToString();
                }
            }
            return builder.ToString();
        }

        private static bool AppendPart(StringBuilder builder, string partName, double partValue, bool shortForm, ref int partCount)
        {
            if (Math.Abs(partValue) == 0.0)
            {
                return false;
            }
            if (builder.Length > 0)
            {
                builder.Append(" ");
            }
            string str = (partCount > 0) ? Math.Abs(partValue).ToString("0.##") : partValue.ToString("0.##");
            if (shortForm && (partName == "millisecond"))
            {
                partName = "ms";
            }
            else if (shortForm)
            {
                partName = partName.Substring(0, 1);
            }
            if (shortForm)
            {
                builder.AppendFormat("{0}{1}", str, partName);
            }
            else
            {
                builder.AppendFormat("{0} {1}{2}", str, partName, GetTense(partValue));
            }
            partCount++;
            return true;
        }

        private static string GetTense(double value)
        {
            if (Math.Abs(value) <= 1.0)
            {
                return string.Empty;
            }
            return "s";
        }
    }
}

