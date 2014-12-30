using System;

namespace LFNet.Common
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks></remarks>
    public static class DateTimeUtil
    {
        // Fields
        private const long FileTimeOffset = 0x701ce1722770000L;
        private static readonly DateTime MaxValueMinusOneDay = DateTime.MaxValue.AddDays(-1.0);
        private static readonly DateTime MinValuePlusOneDay = DateTime.MinValue.AddDays(1.0);

        // Methods

        /// <summary>
        /// Converts to local time.
        /// </summary>
        /// <param name="utcTime">The UTC time.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static DateTime ConvertToLocalTime(this DateTime utcTime)
        {
            if (utcTime < MinValuePlusOneDay)
            {
                return DateTime.MinValue;
            }
            if (utcTime > MaxValueMinusOneDay)
            {
                return DateTime.MaxValue;
            }
            return utcTime.ToLocalTime();
        }

        /// <summary>
        /// Converts to universal time.
        /// </summary>
        /// <param name="localTime">The local time.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static DateTime ConvertToUniversalTime(this DateTime localTime)
        {
            if (localTime < MinValuePlusOneDay)
            {
                return DateTime.MinValue;
            }
            if (localTime > MaxValueMinusOneDay)
            {
                return DateTime.MaxValue;
            }
            return localTime.ToUniversalTime();
        }

        /// <summary>
        /// Froms the file time to UTC.
        /// </summary>
        /// <param name="filetime">The filetime.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static DateTime FromFileTimeToUtc(long filetime)
        {
            return new DateTime(filetime + FileTimeOffset);
        }

        /// <summary>
        /// 将指定的分钟数加到当前时间
        /// </summary>
        /// <param name="times">分钟数</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string AdDeTime(int times)
        {
            return (DateTime.Now).AddMinutes(times).ToString();
        }

        #region Date

        /// <summary>
        /// 根据阿拉伯数字返回月份的名称(可更改为某种语言)
        /// </summary>	
        public static string[] Monthes
        {
            get
            {
                return new[]
                           {
                               "January", "February", "March", "April", "May", "June", "July", "August", "September",
                               "October", "November", "December"
                           };
            }
        }

        /// <summary>
        /// 根据阿拉伯数字返回月份的名称(中文)
        /// </summary>	
        public static string[] MonthesZh
        {
            get
            {
                return new[]
                           {
                               "一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月",
                               "十月", "十一月", "十二月"
                           };
            }
        }

        /// <summary>
        /// 返回标准日期格式string
        /// </summary>
        public static string GetDate()
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// 返回指定日期格式
        /// </summary>
        public static string GetDate(string datetimestr, string replacestr)
        {
            if (datetimestr == null)
                return replacestr;

            if (datetimestr.Equals(""))
                return replacestr;

            try
            {
                datetimestr = Convert.ToDateTime(datetimestr).ToString("yyyy-MM-dd").Replace("1900-01-01", replacestr);
            }
            catch
            {
                return replacestr;
            }
            return datetimestr;
        }


        /// <summary>
        /// 返回标准时间格式string
        /// </summary>
        public static string GetTime()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }

        /// <summary>
        /// 返回标准时间格式string
        /// </summary>
        public static string GetDateTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 返回相对于当前时间的相对天数
        /// </summary>
        public static string GetDateTime(int relativeday)
        {
            return DateTime.Now.AddDays(relativeday).ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 返回标准时间格式string
        /// </summary>
        public static string GetDateTimeF()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fffffff");
        }

        /// <summary>
        /// 返回标准时间
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="formatStr">The format STR.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string GetStandardDateTime(string dateTime, string formatStr)
        {
            if (dateTime == "0000-0-0 0:00:00")
                return dateTime;

            return Convert.ToDateTime(dateTime).ToString(formatStr);
        }

        /// <summary>
        /// 返回标准时间 yyyy-MM-dd HH:mm:ss
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string GetStandardDateTime(string dateTime)
        {
            return GetStandardDateTime(dateTime, "yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 返回标准时间 yyyy-MM-dd
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string GetStandardDate(string date)
        {
            return GetStandardDateTime(date, "yyyy-MM-dd");
        }


        /// <summary>
        /// 转换时间为unix时间戳
        /// </summary>
        /// <param name="date">需要传递UTC时间,避免时区误差,例:DataTime.UTCNow</param>
        /// <returns></returns>
        public static double ConvertToUnixTimestamp(this DateTime date)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date - origin;
            return Math.Floor(diff.TotalSeconds);
        }

        #endregion

        #region date

        /// <summary>
        /// 返回时间+秒后与当前时间相差的秒数
        /// 返回相差的秒数
        /// </summary>
        /// <param name="time">时间 datetime</param>
        /// <param name="sec">秒</param>
        /// <returns></returns>
        public static int StrDateDiffSeconds(string time, int sec)
        {
            TimeSpan ts = DateTime.Now - DateTime.Parse(time).AddSeconds(sec);
            if (ts.TotalSeconds > int.MaxValue)
                return int.MaxValue;

            if (ts.TotalSeconds < int.MinValue)
                return int.MinValue;

            return (int) ts.TotalSeconds;
        }

        /// <summary>
        /// 返回相差的分钟数
        /// </summary>
        /// <param name="time"></param>
        /// <param name="minutes"></param>
        /// <returns></returns>
        public static int StrDateDiffMinutes(string time, int minutes)
        {
            if (Utils.StrIsNullOrEmpty(time))
                return 1;

            TimeSpan ts = DateTime.Now - DateTime.Parse(time).AddMinutes(minutes);
            if (ts.TotalMinutes > int.MaxValue)
                return int.MaxValue;
            if (ts.TotalMinutes < int.MinValue)
                return int.MinValue;

            return (int) ts.TotalMinutes;
        }

        /// <summary>
        /// 返回相差的小时数
        /// </summary>
        /// <param name="time"></param>
        /// <param name="hours"></param>
        /// <returns></returns>
        public static int StrDateDiffHours(string time, int hours)
        {
            if (Utils.StrIsNullOrEmpty(time))
                return 1;

            TimeSpan ts = DateTime.Now - DateTime.Parse(time).AddHours(hours);
            if (ts.TotalHours > int.MaxValue)
                return int.MaxValue;
            if (ts.TotalHours < int.MinValue)
                return int.MinValue;

            return (int) ts.TotalHours;
        }

        #endregion
    }
}