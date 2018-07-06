using System;
namespace Tucao
{
    class Method
    {
        /// <summary>
        /// 获取unix时间戳
        /// </summary>
        /// <returns></returns>
        public static long GetUnixTimestamp()
        {
            DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
            var unixtimestamp = (long)(DateTime.Now - Jan1st1970).TotalMilliseconds;
            return unixtimestamp;
        }

        /// <summary>
        /// unix时间戳转时间字符串
        /// </summary>
        /// <returns></returns>
        public static string LongDateTimeToDateTimeString(string unixtimestamp)
        {
            long unixDate;
            DateTime start;
            DateTime date;
            unixDate = long.Parse(unixtimestamp);
            start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            date = start.AddSeconds(unixDate).ToLocalTime();
            return date.ToString("yyyy-MM-dd HH:mm");
        }
        ///// <summary>
        ///// 时间字符串转xxx前
        ///// </summary>
        ///// <param name="time"></param>
        ///// <returns></returns>
        //public static string TimeToAgo(string timeString)
        //{
        //    DateTime datetime = Convert.ToDateTime(timeString.Replace("UP:", ""));
        //    TimeSpan timespan = DateTime.Now.Subtract(datetime);
        //    DateTime time = new DateTime(timespan.Ticks);
        //    if (time.Year >= 2) return (time.Year - 1).ToString() + "年前";
        //    if (time.Month >= 2)   return (time.Month-1).ToString() + "月前";
        //    if (time.Day >= 2) return (time.Day - 1).ToString() + "天前";
        //    if (time.Hour>= 1) return time.Hour + "小时前";
        //    if (time.Minute >= 1) return time.Minute.ToString() + "分钟前";
        //    return "刚刚";

        //}
    }

}