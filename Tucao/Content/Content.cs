using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace Tucao
{
    public class Content
    {
        static string apikey = "25tids8f1ew1821ed";
        /// <summary>
        /// 获取视频信息
        /// </summary>
        /// <param name="hid">Hid</param>
        /// <returns></returns>
        static public async Task<VideoInfo> GetVideoInfo(string hid)
        {
            VideoInfo info = new VideoInfo();
            //通过api获取绝大多数信息
            Hashtable param = new Hashtable();
            {
                param.Add("hid", hid);
                param.Add("apikey", apikey);
                param.Add("type", "json");
            }
            var str = await Methods.HttpGetAsync("http://www.tucao.tv/api_v2/view.php", param);
            string message1 = await str.Content.ReadAsStringAsync();
            var information = Newtonsoft.Json.JsonConvert.DeserializeObject<Hashtable>(message1);
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<Hashtable>(information["result"].ToString());
            info.Parse(result);
            //解析html得到头像和介绍
            HttpResponseMessage message2 = await Methods.HttpGetAsync("http://www.tucao.tv/play/h" + hid);
            string webpage = await message2.Content.ReadAsStringAsync();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(webpage);
            info.Description = doc.DocumentNode.SelectSingleNode("//div[@class='show_content']").InnerHtml;
            info.UserIcon = doc.DocumentNode.SelectSingleNode("//img[@alt='" + info.User + "']").Attributes["src"].Value;
            return info;
        }

        /// <summary>
        /// 搜索,返回搜索结果的对象
        /// </summary>
        /// <param name="tid">分类ID(可选)</param>
        /// <param name="page">分页选择(默认第一页)</param>
        /// <param name="keywords">关键词</param>
        /// <returns></returns>
        static public async Task<List<Introduction>> Search(int tid, int page, string keywords)
        {
            Hashtable param = new Hashtable();
            {
                param.Add("m", "search");
                param.Add("c", "index");
                param.Add("a", "init2");
                param.Add("catid", tid);
                param.Add("time", "all");
                param.Add("order", "inputtime");
                param.Add("username", "");
                param.Add("tag", "");
                param.Add("q", keywords);
                param.Add("page", page);
            }
            var webpage = await Methods.HttpGetAsync("http://www.tucao.tv/index.php", param);
            string htmlstring = await webpage.Content.ReadAsStringAsync();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlstring);
            var list = doc.DocumentNode.SelectNodes(".//div[@class='list']");
            List<Introduction> result = new List<Introduction>();
            if (list == null) return result;
            foreach (var item in list)
            {
                var v = new Introduction();
                v.Imgurl = item.SelectSingleNode(".//div[1]").FirstChild.FirstChild.Attributes["src"].Value;
                v.Link = item.SelectSingleNode(".//div[1]").FirstChild.Attributes["href"].Value;
                v.Title = item.SelectSingleNode(".//div[2]/div[1]").InnerText;
                v.Up = item.SelectSingleNode(".//div[2]/div[2]/ul[1]/li[1]/a[1]").InnerText;
                v.Time = item.SelectSingleNode(".//div[2]/div[2]/ul[1]/li[2]").InnerText.Replace("发布于：", "");
                result.Add(v);
            }
            return result;
        }
        /// <summary>
        /// 获取分类里的视频
        /// </summary>
        /// <param name="tid">分类id</param>
        /// <param name="pagenum">页码</param>
        /// <returns>含有视频信息的列表</returns>
        static public async Task<List<Introduction>> GetSubclassiFication(int tid, int pagenum)
        {
            var webpage = await Methods.HttpGetAsync("http://www.tucao.tv/list/" + tid + "/index_" + pagenum + ".html");
            var htmlstring = await webpage.Content.ReadAsStringAsync();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlstring);
            var list = doc.DocumentNode.SelectNodes("//div[@class='main']/div[@class='left']/div[@class='box lists_box']/div[@class='list']/ul/li/div[@class='box']");
            List<Introduction> result = new List<Introduction>();
            foreach (var item in list)
            {
                var v = new Introduction();
                v.Imgurl = item.FirstChild.FirstChild.Attributes["src"].Value;
                v.Link = item.FirstChild.Attributes["href"].Value;
                v.Title = item.FirstChild.FirstChild.Attributes["alt"].Value;
                v.Up = item.LastChild.FirstChild.InnerText.Trim();
                v.Time = item.LastChild.LastChild.InnerText.Trim().Replace("UP:", "");
                v.Play = item.SelectSingleNode(".//div[1]").FirstChild.InnerText;
                result.Add(v);
            }
            return result;
        }
        /// <summary>
        /// 获取评论的排序方式
        /// </summary>
        public enum Order
        {
            New = 0, Hot = 1
        }
        /// <summary>
        /// 获取评论区
        /// </summary>
        /// <param name="typeid">分类ID</param>
        /// <param name="hid">HID</param>
        /// <param name="page">页数</param>
        /// <param name="hot">Hot:最热 New:最新</param>
        public static async Task<List<Comment>> GetComment(string typeid, string hid, int page, Order hot)
        {
            //获取评论区的html
            Hashtable param = new Hashtable();
            {
                param.Add("m", "comment");
                param.Add("c", "index");
                param.Add("a", "init");
                param.Add("commentid", "content_" + typeid + '-' + hid + "-1");
                param.Add("hot", hot);
                param.Add("iframe", "1");
                param.Add("page", page);
            }
            var webpage = await Methods.HttpGetAsync("http://www.tucao.tv/index.php", param);
            var htmlstring = await webpage.Content.ReadAsStringAsync();
            //解析html
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlstring);
            var list = doc.DocumentNode.SelectNodes("//tr");
            var result = new List<Comment>();
            if (list == null)
                return result;
            for (int i = 0; i < list.Count; i++)
            {
                var c = new Comment();
                c.ProfilePhoto = list[i].SelectSingleNode(".//img[1]").Attributes["src"].Value;
                c.UserId = list[i].SelectSingleNode(".//a[2]").Attributes["href"].Value.Replace("/", "").Replace("http:www.tucao.tvplayu", "");
                c.UserName = list[i].SelectSingleNode(".//a[2]").InnerText;
                c.Level = int.Parse(list[i].SelectSingleNode(".//div[1]").Attributes["class"].Value.Replace("lv lv", "")) - 1;
                if (c.Level == 27) c.Level -= 7;
                c.Content = list[i].SelectSingleNode(".//td[2]").InnerText;
                c.Content = WebUtility.HtmlDecode(c.Content);
                i++;
                c.Lch = list[i].SelectSingleNode(".//em[@class='lch']").InnerText.Replace('楼', '\0');
                c.Time = list[i].SelectSingleNode(".//em[@class='time']").InnerText;
                c.DiggCount = int.Parse(list[i].SelectSingleNode(".//a[@class='digg']").SelectSingleNode(".//em").InnerText);
                result.Add(c);
            }
            return result;
        }
        /// <summary>
        /// 获取首页
        /// </summary>
        /// <returns></returns>
        static public async Task<string> GetIndex()
        {
            Hashtable param = new Hashtable();
            {
                param.Add("m", "member");
                param.Add("c", "index");
                param.Add("a", "mini");
                param.Add("forward", "http%3A%2F%2Ftucao.tv%2F");
                param.Add("siteid", "1");
            }
            var result = await Methods.HttpGetAsync("http://www.tucao.tv/index.php", param);
            return await result.Content.ReadAsStringAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hid">HID</param>
        /// <param name="part">分p号(从0开始)</param>
        /// <returns></returns>
        public static async Task<List<Danmaku>> GetDanmakus(string hid, int part)
        {
            //http://www.tucao.tv/index.php?m=mukio&c=index&a=init&playerID=11-<hid>-1-<part>
            Hashtable param = new Hashtable();
            {
                param.Add("m", "mukio");
                param.Add("c", "index");
                param.Add("a", "init");
                param.Add("playerID", "11-"+hid+"-1-"+part+"");
            }
            var result = await Methods.HttpGetAsync("http://www.tucao.tv/index.php", param);
            string danmakus_xml= await result.Content.ReadAsStringAsync();
            return Danmaku.ParseDanmakus(danmakus_xml);
        }
    }
    /// <summary>
    /// 分区和搜索里的每一块视频的必要信息
    /// </summary>
    public class Introduction
    {
        /// <summary>
        /// 封面地址
        /// </summary>
        public string Imgurl { get; set; }
        /// <summary>
        /// 视频链接
        /// </summary>
        public string Link { get; set; }
        /// <summary>
        /// 视频标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 投稿时间
        /// </summary>
        public string Time { get; set; }
        /// <summary>
        /// up主
        /// </summary>
        public string Up { get; set; }
        /// <summary>
        /// 介绍
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 播放数
        /// </summary>
        public string Play { get; set; }
    }
    /// <summary>
    /// 一个评论的必要信息
    /// </summary>
    public class Comment
    {
        /// <summary>
        /// 头像
        /// </summary>
        public string ProfilePhoto { get; set; }
        /// <summary>
        /// 用户id
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 等级
        /// </summary>
        public int Level { get; set; }
        /// <summary>
        /// 评论内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 第几楼
        /// </summary>
        public string Lch { get; set; }
        /// <summary>
        /// 发布时间
        /// </summary>
        public string Time { get; set; }
        /// <summary>
        /// 赞数
        /// </summary>
        public int DiggCount { get; set; }
    }
}
