using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tucao.Http;
using Windows.Web.Http;
namespace Tucao.Content
{
    public class Content
    {
        /// <summary>
        /// 获取视频信息
        /// </summary>
        /// <param name="hid">Hid</param>
        /// <returns></returns>
        static public async Task<VideoInfo> GetVideoInfo(string hid)
        {
            VideoInfo info = new VideoInfo();
            //通过api获取绝大多数信息
            string message1 = await HttpService._getSubmissionInfo(hid);
            var information = Newtonsoft.Json.JsonConvert.DeserializeObject<Hashtable>(message1);
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<Hashtable>(information["result"].ToString());
            info.Parse(result);
            //解析html得到头像和介绍
            HttpResponseMessage message2 = await HttpService.HttpGet("http://www.tucao.tv/play/h" + hid);
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
        static public async Task<List<VideoPanel>> Search(int tid, int page, string keywords)
        {

            string webpage = await HttpService._getsearchresult(tid, page, keywords);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(webpage);
            var list = doc.DocumentNode.SelectNodes(".//div[@class='list']");
            List<VideoPanel> result = new List<VideoPanel>();
            if (list == null) return result;
            foreach (var item in list)
            {
                var v = new VideoPanel();
                v.img = item.SelectSingleNode(".//div[1]").FirstChild.FirstChild.Attributes["src"].Value;
                v.link = item.SelectSingleNode(".//div[1]").FirstChild.Attributes["href"].Value;
                v.title = item.SelectSingleNode(".//div[2]/div[1]").InnerText;
                v.up = item.SelectSingleNode(".//div[2]/div[2]/ul[1]/li[1]/a[1]").InnerText;
                v.time = item.SelectSingleNode(".//div[2]/div[2]/ul[1]/li[2]").InnerText.Replace("发布于：", "UP:");
                v.description = item.SelectSingleNode(".//div[2]/div[3]").InnerText;
                if (v.description.Trim() == "") v.description += "暂无简介";
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
        static public async Task<List<VideoPanel>> GetSubclassiFication(int tid, int pagenum)
        {
            var webpage = await HttpService._getsubclassification(tid, pagenum);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(webpage);
            var list = doc.DocumentNode.SelectNodes("//div[@class='main']/div[@class='left']/div[@class='box lists_box']/div[@class='list']/ul/li/div[@class='box']");
            List<VideoPanel> result = new List<VideoPanel>();
            foreach (var item in list)
            {
                var v = new VideoPanel();
                v.img = item.FirstChild.FirstChild.Attributes["src"].Value;
                v.link = item.FirstChild.Attributes["href"].Value;
                v.title = item.FirstChild.FirstChild.Attributes["alt"].Value;
                v.up = item.LastChild.FirstChild.InnerText.Trim();
                v.time = item.LastChild.LastChild.InnerText.Trim();
                v.description = item.SelectSingleNode(".//p[@class='description']").InnerText;
                if (v.description.Trim() == "") v.description += "暂无简介";
                v.play = item.SelectSingleNode(".//div[1]").FirstChild.InnerText;
                result.Add(v);
            }
            return result;
        }
    }
    public class VideoPanel
    {
        public string img { get; set; }
        public string link { get; set; }
        public string title { get; set; }
        public string time { get; set; }
        public string up { get; set; }
        public string description { get; set; }
        public string play { get; set; }
    }
}
