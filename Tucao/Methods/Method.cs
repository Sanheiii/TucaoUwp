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
        //public static string MillisecondToMinute()

    }

}