using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Web.Http;

namespace Tucao
{
    static class Methods
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
        /// <summary>
        /// 颜色转int
        /// </summary>
        /// <param name="color"></param>
        /// <returns>十进制RGB颜色值(没有A)</returns>
        public static int ToInt(this Color color)
        {
            var hex = "0x" + color.ToString().Remove(0, 3);
            int i = Convert.ToInt32(hex, 16);
            return i;
        }


        static HttpClient client = new HttpClient();
        /// <summary>
        /// 整合参数并发出get请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="param">参数</param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> HttpGetAsync(string url, Hashtable queries = null)
        {
            if (queries != null)
            {
                url += '?';
                foreach (DictionaryEntry kv in queries)
                {
                    url += (kv.Key + "=" + kv.Value + "&");
                }
                url = url.Remove(url.Length - 1, 1);
            }
            var response = await client.GetAsync(new Uri(url));
            return response;
        }
        /// <summary>
        /// POST
        /// </summary>
        /// <param name="url"></param>
        /// <param name="body"></param>
        /// <param name="queries"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> HttpPostAsync(string url, List<KeyValuePair<string, string>> body, Hashtable queries = null)
        {
            if (queries != null)
            {
                url += '?';
                foreach (DictionaryEntry kv in queries)
                {
                    url += (kv.Key + "=" + kv.Value + "&");
                }
                url = url.Remove(url.Length - 1, 1);
            }
            var response = await client.PostAsync(new Uri(url), new HttpFormUrlEncodedContent(body));
            return response;
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