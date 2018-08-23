using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Tucao;
using HtmlAgilityPack;
using System.Collections.ObjectModel;
using Windows.Storage;
using System.Xml.Linq;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Tucao.View
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Bangumi : Page
    {
        public Bangumi()
        {
            this.InitializeComponent();
            Getlist(); 
            
            



        }
        public  async void Getlist()
        {
            var Listfanzu1 = await Tucao.Content.GetBangumi();//获取返回的对象数组 list番组信息

            List1.ItemsSource = Listfanzu1; //控件里面的数据来源

            var list0 = Listfanzu1.FindAll((d) => d.Week==0);
            List0.ItemsSource = list0;
            var list1 = Listfanzu1.FindAll((d) => d.Week == 1);
            List1.ItemsSource = list1;
            var list2 = Listfanzu1.FindAll((d) => d.Week == 2);
            List2.ItemsSource = list2;
            var list3 = Listfanzu1.FindAll((d) => d.Week == 3);
            List3.ItemsSource = list3;
            var list4 = Listfanzu1.FindAll((d) => d.Week == 4);
            List4.ItemsSource = list4;
            var list5 = Listfanzu1.FindAll((d) => d.Week == 5);
            List5.ItemsSource = list5;
            var list6 = Listfanzu1.FindAll((d) => d.Week == 3);
            List6.ItemsSource = list6;
            //var list7 = Listfanzu1.FindAll((d) => d.Count >= 39 && d.Count <= 47);
            //List7.ItemsSource = list7;

            //    4 4 6 6zxczxcasdasdasd



        }

        private void List0_ItemClick(object sender, ItemClickEventArgs e)
        {
            var i = e.ClickedItem;
        }






















    }
    





}
