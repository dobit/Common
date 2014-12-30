using System;
using System.Diagnostics;

namespace LFNet.Common
{
    /// <summary>
    /// A class defining a business day.
    /// </summary>
    [DebuggerDisplay("DayOfWeek={DayOfWeek}, StartTime={StartTime}, EndTime={EndTime}")]
    public class BusinessDay
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.BusinessDay" /> class.
        /// </summary>
        /// <param name="dayOfWeek">The day of week this business day represents.</param>
        public BusinessDay(global::System.DayOfWeek dayOfWeek)
        {
            this.StartTime = TimeSpan.FromHours(9.0);
            this.EndTime = TimeSpan.FromHours(17.0);
            this.DayOfWeek = dayOfWeek;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LFNet.Common.BusinessDay" /> class.
        /// </summary>
        /// <param name="dayOfWeek">The day of week this business day represents.</param>
        /// <param name="startTime">The start time of the business day.</param>
        /// <param name="endTime">The end time of the business day.</param>
        public BusinessDay(global::System.DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime)
        {
            if (startTime.TotalDays >= 1.0)
            {
                throw new ArgumentOutOfRangeException("startTime", startTime, "The startTime argument must be less then one day.");
            }
            if (endTime.TotalDays > 1.0)
            {
                throw new ArgumentOutOfRangeException("endTime", endTime, "The endTime argument must be less then one day.");
            }
            if (endTime <= startTime)
            {
                throw new ArgumentOutOfRangeException("endTime", endTime, "The endTime argument must be greater then startTime.");
            }
            this.DayOfWeek = dayOfWeek;
            this.StartTime = startTime;
            this.EndTime = endTime;
        }

        /// <summary>
        /// Determines whether the specified date falls in the business day.
        /// </summary>
        /// <param name="date">The date to check.</param>
        /// <returns>
        /// <c>true</c> if the specified date falls in the business day; otherwise, <c>false</c>.
        /// </returns>
        public bool IsBusinessDay(DateTime date)
        {
            if (date.DayOfWeek != this.DayOfWeek)
            {
                return false;
            }
            if (date.TimeOfDay < this.StartTime)
            {
                return false;
            }
            if (date.TimeOfDay > this.EndTime)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the day of week this business day represents..
        /// </summary>
        /// <value>The day of week.</value>
        public global::System.DayOfWeek DayOfWeek { get; private set; }

        /// <summary>
        /// Gets the end time of the business day.
        /// </summary>
        /// <value>The end time of the business day.</value>
        public TimeSpan EndTime { get; private set; }

        /// <summary>
        /// Gets the start time of the business day.
        /// </summary>
        /// <value>The start time of the business day.</value>
        public TimeSpan StartTime { get; private set; }
    }
}

