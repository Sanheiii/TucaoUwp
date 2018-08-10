using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Tucao
{
    /// <summary>
    /// 这里瞎jb写代码用于测试
    /// </summary>
    class Test
    {
        public static async void ldhrbq()
        {
            //获取含有首页内容的字符串
            string html = await Content.GetIndex();
            //解析上面的字符串
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            //找到一个符合条件的标签 括号内参数的格式是//标签名[@属性名='值']
            var bangumi = doc.DocumentNode.SelectSingleNode("//td");
            //获得所有的子节点
            var list = bangumi.ChildNodes[0];
            //list可以用foreach遍历得到每个子节点
            //一个节点的结构:<标签名 属性名1="属性值1" 属性名2="属性值2">内容</标签名>
            //               <ldh class="rbq" price="1">真舒服</ldh>
            //    节点.Attitudes["属性名"].Value可以得到属性值
            //    节点.InnerText可以得到内容的值
            //如果内容是其他节点的话可以和上面一样用  节点.SelectSingleNode(string xpath)
            //                          或者用  节点.Child[索引(0开始)]   来得到子节点
        }
    }
}
