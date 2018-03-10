using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using System.Collections;

namespace Tucao.Http
{
    public class HttpService
    {
        static HttpClient client = new HttpClient();
        static string apikey = "25tids8f1ew1821ed";
        /// <summary>
        /// 获取首页
        /// </summary>
        /// <returns></returns>
        static public async Task<string> _getIndex()
        {
            Hashtable param = new Hashtable();
            {
                param.Add("m", "member");
                param.Add("c", "index");
                param.Add("a", "mini");
                param.Add("forward", "http%3A%2F%2Ftucao.tv%2F");
                param.Add("siteid", "1");
            }
            var result = await HttpGet("http://www.tucao.tv/index.php", param);
            return await result.Content.ReadAsStringAsync();
        }
        /// <summary>
        /// 获取子分类的视频
        /// </summary>
        /// <returns></returns>
        static public async Task<string> _getsubclassification(int tid,int page)
        {
            var result = await HttpGet("http://www.tucao.tv/list/" + tid + "/index_" + page + ".html");
            return await result.Content.ReadAsStringAsync();
        }



        /// <summary>
        /// 获取播放地址
        /// </summary>
        /// <param name="part">分P信息</param>
        /// <returns></returns>
        static public async Task<string> _getPlayUrl(Hashtable part)
        {
            var unixtimestamp = Method.GetUnixTimestamp();
            Hashtable param = new Hashtable();
            {
                param.Add("type", part["type"]);
                param.Add("vid", part["vid"]);
                param.Add("r", (unixtimestamp / 1000).ToString());
            }
            var result = await HttpGet("http://api.tucao.tv/api/playurl", param);
            return await result.Content.ReadAsStringAsync();
        }
        /// <summary>
        /// 获取投稿详情
        /// </summary>
        /// <param name="hid">Hid</param>
        /// <returns></returns>
        static public async Task<string> _getSubmissionInfo(string hid)
        {
            Hashtable param = new Hashtable();
            {
                param.Add("hid", hid);
                param.Add("apikey", apikey);
                param.Add("type", "json");
            }
            var result = await HttpGet("http://www.tucao.tv/api_v2/view.php", param);
            return await result.Content.ReadAsStringAsync();
        }
        /// <summary>
        /// 搜索视频
        /// </summary>
        /// <param name="tid">分类ID(可选)</param>
        /// <param name="page">分页选择(默认第一页)</param>
        /// <param name="pagesize">返回记录数(默认10)</param>
        /// <param name="order">排序方法（date:发布时间,mukio:弹幕数,views:播放数）</param>
        /// <param name="keywords">关键词</param>
        /// <returns></returns>
        static public async Task<string> _getsearchresult(int tid, int page, string keywords)
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
            var result = await HttpGet("http://www.tucao.tv/index.php", param);
            return await result.Content.ReadAsStringAsync();
        }



        /// <summary>
        /// 整合参数并发出get请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="param">参数</param>
        /// <returns></returns>
        static public async Task<HttpResponseMessage> HttpGet(string url, Hashtable param)
        {
            if (param != null)
            {
                url += '?';
                foreach (DictionaryEntry kv in param)
                {
                    url += (kv.Key + "=" + kv.Value + "&");
                }
            url = url.Remove(url.Length - 1, 1);
            }
            var response = await client.GetAsync(new Uri(url));
            return response;
        }
        static public async Task<HttpResponseMessage> HttpGet(string url)
        {
            return await HttpGet(url, null);
        }
    }
}
