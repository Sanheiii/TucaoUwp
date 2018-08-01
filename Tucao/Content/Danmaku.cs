using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI;
using static Controls.DanmakuManager;

namespace Tucao.Content
{
    public class Danmaku:IComparable<Danmaku>
    {
        /*
         <i>
         <d p="对应时间,1滚动4底部5顶部,字号,颜色,发送时间(Unix时间戳)">内容</d>
         <d p="34.2,1,25,16777215,1522827434">我C站牛逼！</d>
         </i>*/
        private double position;
        /// <summary>
        /// 弹幕所在时间点
        /// </summary>
        public double Position
        {
            get { return position; }
            set { position = value; }
        }
        private string content;
        /// <summary>
        /// 弹幕内容
        /// </summary>
        public string Content
        {
            get { return content; }
            set { content = value; }
        }
        private DanmakuType type;
        /// <summary>
        /// 弹幕类型
        /// </summary>
        public DanmakuType Type
        {
            get { return type; }
            set { type = value; }
        }
        /// <summary>
        /// 用十进制表示的弹幕颜色
        /// </summary>
        public int DecimalColor { get; set; }
        /// <summary>
        /// 弹幕颜色
        /// </summary>
        public Color TextColor
        {
            get
            {
                return new Color()
                {
                    A =255,
                    R = Convert.ToByte((DecimalColor >> 16) & 255),
                    G = Convert.ToByte((DecimalColor >> 8) & 255),
                    B = Convert.ToByte((DecimalColor >> 0) & 255)
                };
            }
        }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="position"></param>
        /// <param name="type"></param>
        /// <param name="color"></param>
        /// <param name="content"></param>
        public Danmaku(double position,DanmakuType type,int color,string content)
        {
            this.Position = position;
            this.Type = type;
            this.DecimalColor = color;
            this.Content = content;
        }
        public Danmaku()
        {

        }
        /// <summary>
        /// 含有弹幕信息的一条xml标签
        /// </summary>
        /// <param name="XML"></param>
        /// <returns></returns>
        private static Danmaku Parse(string XML)
        {
            string p = Regex.Match(XML, @"<.+?>").Value;
            MatchCollection attitudes = Regex.Matches(p,@"\d+\.?\d*");
            double position = double.Parse(attitudes[0].Value);
            int type= int.Parse(attitudes[1].Value);
            int color = int.Parse(attitudes[3].Value);
            string content = XML.Replace(p, "");
            content = content.Remove(content.Length - 4);
            Danmaku danmaku = new Danmaku(position, (DanmakuType)(Enum.IsDefined(DanmakuType.Top.GetType(), type) ? type : 1),color,content);
            return danmaku;
        }
        //实现IComparable<T>接口对List<Danmaku>排序
        public int CompareTo(Danmaku other)
        {
            int result;
            CaseInsensitiveComparer ObjectCompare = new CaseInsensitiveComparer();
            result = ObjectCompare.Compare(this.Position, other.Position);
            return result;
        }
        // 将含有很多弹幕的xml文档转化为List<Danmaku>
        public static List<Danmaku> ParseDanmakus(string danmakus_xml)
        {
            List<Danmaku> danmakus = new List<Danmaku>();
            var maches = Regex.Matches(danmakus_xml, @"<d p=.+?</d>");
            foreach (Match danmaku_xml in maches)
            {
                danmakus.Add(Parse(danmaku_xml.Value));
            }
            danmakus.Sort();
            return danmakus;
        }
    }
}
