using System;
using System.Collections.Generic;
using System.Linq;

namespace LFNet.Common
{
    /// <summary>
    /// A class representing a business week.
    /// </summary>
    public class BusinessWeek
    {
        private Dictionary<DayOfWeek, IList<BusinessDay>> _dayTree;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.BusinessWeek" /> class.
        /// </summary>
        public BusinessWeek()
        {
            this.BusinessDays = new List<BusinessDay>();
        }

        /// <summary>
        /// Gets the business end date using the specified time.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="businessTime">The business time.</param>
        /// <returns></returns>
        public DateTime GetBusinessEndDate(DateTime startDate, TimeSpan businessTime)
        {
            this.Validate(true);
            DateTime time = startDate;
            TimeSpan span = businessTime;
            while (span > TimeSpan.Zero)
            {
                DateTime time2;
                BusinessDay day;
                if (!this.NextBusinessDay(time, out time2, out day))
                {
                    return time;
                }
                TimeSpan ts = day.EndTime.Subtract(time2.TimeOfDay);
                if (span <= ts)
                {
                    return time2.Add(span);
                }
                span = span.Subtract(ts);
                time = time2.Add(ts);
            }
            return time;
        }

        /// <summary>
        /// Gets the business time between the start date and end date.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns>
        /// A TimeSpan of the amount of business time between the start and end date.
        /// </returns>
        /// <remarks>
        /// Business time is calculated by adding only the time that falls inside the business day range.
        /// If all the time between the start and end date fall outside the business day, the time will be zero.
        /// </remarks>
        public TimeSpan GetBusinessTime(DateTime startDate, DateTime endDate)
        {
            DateTime time3;
            this.Validate(true);
            TimeSpan zero = TimeSpan.Zero;
            for (DateTime time = startDate; time < endDate; time = time3)
            {
                DateTime time2;
                BusinessDay day;
                if ((!this.NextBusinessDay(time, out time2, out day) || (time2 > endDate)) || (day == null))
                {
                    return zero;
                }
                TimeSpan span2 = day.EndTime.Subtract(time2.TimeOfDay);
                time3 = time2.Add(span2);
                if (endDate <= time3)
                {
                    span2 = endDate.TimeOfDay.Subtract(time2.TimeOfDay);
                    return zero.Add(span2);
                }
                zero = zero.Add(span2);
            }
            return zero;
        }

        private Dictionary<DayOfWeek, IList<BusinessDay>> GetDayTree()
        {
            if (this._dayTree == null)
            {
                this._dayTree = new Dictionary<DayOfWeek, IList<BusinessDay>>();
                foreach (BusinessDay day in (from b in this.BusinessDays
                    orderby b.DayOfWeek, b.StartTime
                    select b).ToList<BusinessDay>())
                {
                    if (!this._dayTree.ContainsKey(day.DayOfWeek))
                    {
                        this._dayTree.Add(day.DayOfWeek, new List<BusinessDay>());
                    }
                    this._dayTree[day.DayOfWeek].Add(day);
                }
            }
            return this._dayTree;
        }

        /// <summary>
        /// Determines whether the specified date falls on a business day.
        /// </summary>
        /// <param name="date">The date to check.</param>
        /// <returns>
        /// <c>true</c> if the specified date falls on a business day; otherwise, <c>false</c>.
        /// </returns>
        public bool IsBusinessDay(DateTime date)
        {
            return this.BusinessDays.Any<BusinessDay>(day => day.IsBusinessDay(date));
        }

        internal bool NextBusinessDay(DateTime startDate, out DateTime nextDate, out BusinessDay businessDay)
        {
            nextDate = startDate;
            businessDay = null;
            Dictionary<DayOfWeek, IList<BusinessDay>> dayTree = this.GetDayTree();
            for (int i = 0; i < 7; i++)
            {
                DayOfWeek dayOfWeek = nextDate.DayOfWeek;
                if (!dayTree.ContainsKey(dayOfWeek))
                {
                    nextDate = nextDate.AddDays(1.0).Date;
                }
                else
                {
                    IList<BusinessDay> list = dayTree[dayOfWeek];
                    if (list != null)
                    {
                        foreach (BusinessDay day in list)
                        {
                            if (day != null)
                            {
                                TimeSpan timeOfDay = nextDate.TimeOfDay;
                                if ((timeOfDay >= day.StartTime) && (timeOfDay < day.EndTime))
                                {
                                    businessDay = day;
                                    return true;
                                }
                                if (timeOfDay < day.StartTime)
                                {
                                    businessDay = day;
                                    nextDate = nextDate.Date.Add(day.StartTime);
                                    return true;
                                }
                            }
                        }
                        nextDate = nextDate.AddDays(1.0).Date;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Validates the business week.
        /// </summary>
        /// <param name="throwExcption">if set to <c>true</c> throw excption if invalid.</param>
        /// <returns><c>true</c> if valid; otherwise <c>false</c>.</returns>
        protected virtual bool Validate(bool throwExcption)
        {
            if (this.BusinessDays.Any<BusinessDay>())
            {
                return true;
            }
            if (throwExcption)
            {
                throw new InvalidOperationException("The BusinessDays property must have at least one BusinessDay.");
            }
            return false;
        }

        /// <summary>
        /// Gets the business days for the week.
        /// </summary>
        /// <value>The business days for the week.</value>
        public IList<BusinessDay> BusinessDays { get; private set; }

        /// <summary>
        /// Gets the default BusinessWeek.
        /// </summary>
        public static BusinessWeek DefaultWeek
        {
            get
            {
                return Nested.Current;
            }
        }

        /// <summary>
        /// Nested class to lazy-load singleton.
        /// </summary>
        private class Nested
        {
            /// <summary>
            /// Current singleton instance.
            /// </summary>
            internal static readonly BusinessWeek Current = new BusinessWeek();

            /// <summary>
            /// Initializes the Nested class.
            /// </summary>
            /// <remarks>
            /// Explicit static constructor to tell C# compiler not to mark type as beforefieldinit.
            /// </remarks>
            static Nested()
            {
                Current.BusinessDays.Add(new BusinessDay(DayOfWeek.Monday));
                Current.BusinessDays.Add(new BusinessDay(DayOfWeek.Tuesday));
                Current.BusinessDays.Add(new BusinessDay(DayOfWeek.Wednesday));
                Current.BusinessDays.Add(new BusinessDay(DayOfWeek.Thursday));
                Current.BusinessDays.Add(new BusinessDay(DayOfWeek.Friday));
            }
        }
    }
}

