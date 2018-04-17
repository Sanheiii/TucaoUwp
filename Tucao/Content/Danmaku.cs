using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Windows.UI;
using System.Threading.Tasks;

namespace Tucao.Content
{
    /// <summary>
    /// 弹幕
    /// </summary>
    public class Danmaku
    {
        /// <summary>
        /// 弹幕出现的时间
        /// </summary>
        public TimeSpan Timing;
        /// <summary>
        /// 弹幕类型
        /// </summary>
        public DanmakuType Type;
        /// <summary>
        /// 字体大小
        /// </summary>
        public int Fontsize;
        /// <summary>
        /// 字体颜色
        /// </summary>
        public Color FontColor;
        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime SendTime;
        /// <summary>
        /// 弹幕内容
        /// </summary>
        public string Content;
        /// <summary>
        /// 弹幕类型的枚举值
        /// </summary>
        public enum DanmakuType
        {
            normal = 1,
            bottom = 4,
            top = 5
        }
        public Danmaku(string message)
        {
            //解析弹幕的属性
            string[] attribute = Regex.Match(message, "(?<=p=\").*?(?=\">)").Value.Split(',');
            Timing = new TimeSpan((long)(float.Parse(attribute[0]) * 10000000));
            Type = (DanmakuType)byte.Parse(attribute[1]);
            Fontsize = int.Parse(attribute[2]);
            //将十进制rgb转化为16进制,不足6位左边补0
            string HexRGB = Convert.ToString(long.Parse(attribute[3]), 16).PadLeft(6, '0');
            var r = Convert.ToByte(HexRGB.Substring(0, 2), 16);
            var g = Convert.ToByte(HexRGB.Substring(2, 2), 16);
            var b = Convert.ToByte(HexRGB.Substring(4, 2), 16);
            FontColor = Color.FromArgb(0, r, g, b);
            SendTime = DateTime.Parse(Method.LongDateTimeToDateTimeString(attribute[4]));
            Content = Regex.Match(message, "(?<=\">).*?(?=</d>)").Value.Trim();
        }
    }
}
