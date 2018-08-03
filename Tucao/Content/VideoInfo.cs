using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Tucao
{
    //定义 投稿 类
    public class VideoInfo
    {
        public VideoInfo()
        {

        }
        public VideoInfo(Hashtable str)
        {
            Parse(str);
        }
        /// <summary>
        ///视频ID
        /// </summary>
        public string Hid { get; set; }
        /// <summary>
        ///分类ID
        /// </summary>
        public string TypeId { get; set; }
        /// <summary>
        ///发布时间
        /// </summary>
        public string Create { get; set; }
        /// <summary>
        ///弹幕数
        /// </summary>
        public string Mukio { get; set; }
        /// <summary>
        ///分类名
        /// </summary>
        public string TypeName { get; set; }
        /// <summary>
        ///视频名称
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        ///播放数
        /// </summary>
        public string Play { get; set; }
        /// <summary>
        ///介绍
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        ///关键词
        /// </summary>
        public string KeyWords { get; set; }
        /// <summary>
        /// 略缩图
        /// </summary>
        public string Thumb { get; set; }
        /// <summary>
        ///发布人
        /// </summary>
        public string User { get; set; }
        /// <summary>
        ///发布人的头像
        /// </summary>
        public string UserIcon { get; set; }
        /// <summary>
        ///发布人UID
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        ///分P数量
        /// </summary>
        public string Part { get; set; }
        /// <summary>
        ///分P视频信息
        /// </summary>
        public Hashtable[] Video { get; set; }
        public string MainTypeName
        {
            get
            {
                switch (TypeId)
                {
                    case "28": goto case "29";
                    case "6": goto case "29";
                    case "25": goto case "29";
                    case "29": return "动画";
                    case "7": goto case "52";
                    case "31": goto case "52";
                    case "37": goto case "52";
                    case "30": goto case "52";
                    case "40": goto case "52";
                    case "88": goto case "52";
                    case "52": return "音乐";
                    case "8": goto case "42";
                    case "34": goto case "42";
                    case "44": goto case "42";
                    case "33": goto case "42";
                    case "42": return "游戏";
                    case "9": goto case "15";
                    case "32": goto case "15";
                    case "57": goto case "15";
                    case "61": goto case "15";
                    case "65": goto case "15";
                    case "15": return "三次元";
                    case "39": goto case "27";
                    case "38": goto case "27";
                    case "16": goto case "27";
                    case "27": return "影剧";
                    case "11": goto case "10";
                    case "43": goto case "10";
                    case "26": goto case "10";
                    case "10": return "新番";

                    default: return "其他";
                }
            }
        }

        /// <summary>
        /// 获取视频地址
        /// </summary>
        /// <param name="vid">vid</param>
        /// <returns></returns>
        public static async Task<List<string>> GetPlayUrl(Hashtable part)
        {
            List<string> url = new List<string>();
            if (part["type"].ToString() == "video")
            {
                url.Add(part["file"].ToString());
            }
            else
            {
                var unixtimestamp = Methods.GetUnixTimestamp();
                Hashtable param = new Hashtable();
                {
                    param.Add("type", part["type"]);
                    param.Add("vid", part["vid"]);
                    param.Add("r", (unixtimestamp / 1000).ToString());
                }
                var result = await Methods.HttpGetAsync("http://api.tucao.tv/api/playurl", param);
                var xml =await result.Content.ReadAsStringAsync();
                XMLParser xmlParser = new XMLParser();
                XMLNode xn = xmlParser.Parse(xml);
                try
                {
                    int i = 0;
                    while (true)
                    {
                        var temp = xn.GetValue("video>0>durl>" + i.ToString() + ">url>0>_text");
                        if (temp.StartsWith("https://"))
                            url.Add(temp.Remove(4, 1));
                        else
                            url.Add(temp);
                        i++;
                    }
                }
                catch { }
            }
            return url;
        }
        /// <summary>
        /// 解析视频详情页面返回的信息
        /// </summary>
        /// <param name="message">带有视频信息的Hashtable</param>
        /// <returns></returns>
        public void Parse(Hashtable result)
        {
            Hid = result["hid"].ToString();
            TypeId = result["typeid"].ToString();
            Create = Methods.LongDateTimeToDateTimeString(result["create"].ToString());
            Mukio = result["mukio"].ToString();
            TypeName = result["typename"].ToString();
            Title = result["title"].ToString();
            Play = result["play"].ToString();
            Part = result["part"].ToString();
            KeyWords = result["keywords"].ToString();
            Thumb = result["thumb"].ToString();
            User = result["user"].ToString();
            UserId = result["userid"].ToString();
            Video = Newtonsoft.Json.JsonConvert.DeserializeObject<Hashtable[]>(result["video"].ToString());
        }
    }
}
