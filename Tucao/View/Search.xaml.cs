﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Tucao.View
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Search : Page
    {
        int p = 1;
        public Search()
        {
            this.InitializeComponent();
            VideoList.ItemsSource = new ObservableCollection<Introduction>();
        }
        /// <summary>
        /// 点击打开视频
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VideoList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var v=e.ClickedItem as Introduction;
            var id = v.Link.Replace("http://www.tucao.one/play/", "").Replace("/", "");
            Frame root = Window.Current.Content as Frame;
            if (id.First() == 'h')
            {
                App.OpenVideo(id.Remove(0, 1));
            }
        }
        /// <summary>
        /// 点击搜索按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            p = 1;
            VideoList.ItemsSource = new ObservableCollection<Introduction>();
            var q = args.QueryText;
            //如果搜索框的文本由数字组成(前面可以有h)就直接跳到视频页
            if (Regex.IsMatch(q, @"^h?\d+$"))
            {
                //去掉h
                q = q.Replace("h","");
                App.OpenVideo(q);
            }
            else
            {
                BottomText.Visibility = Visibility.Collapsed;
                LoadingProgress.Visibility = Visibility.Visible;
                Task.Run(() => LoadItems(p, q));
            }
        }
        /// <summary>
        /// 加载结果
        /// </summary>
        /// <param name="page">页</param>
        /// <param name="q">关键字</param>
        private async void LoadItems(int page, string q)
        {
            List<Introduction> r=new List<Introduction>();
            try
            {
                for (int i = 1; i >= 0; i--)
                {
                    //获取一页
                    r = await Tucao.Content.Search(0, page * 2 - i, q);
                    //添加到控件中
                    for (int j = 0; j < r.Count; j++)
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                        {
                            ((ObservableCollection<Introduction>)VideoList.ItemsSource).Add(r[j]);
                        });
                        await Task.Delay(10);
                    }
                    //不够12的话说明没有下一页
                    if (r.Count < 12 || r == null)
                        break;
                }
            }
            catch (Exception ex)
            {
                Link.ShowToast(ex.Message);
                return;
            }
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                if (((ObservableCollection<Introduction>)VideoList.ItemsSource).Count == 0)
                {
                    BottomText.Text = "没有找到结果";
                }
                else if (((ObservableCollection<Introduction>)VideoList.ItemsSource).Count % 24 == 0&&r.Count!=0)
                {
                    BottomText.Text = "加载下一页";
                    BottomText.Tapped += BottomText_Tapped;
                }
                else
                {
                    BottomText.Text = "到底了";
                }
                BottomText.Visibility = Visibility.Visible;
                LoadingProgress.Visibility = Visibility.Collapsed;
            });
        }
        /// <summary>
        /// 点击加载下一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BottomText_Tapped(object sender, TappedRoutedEventArgs e)
        {
            BottomText.Tapped -= BottomText_Tapped;
            BottomText.Visibility = Visibility.Collapsed;
            LoadingProgress.Visibility = Visibility.Visible;
            string q = SearchBox.Text;
            Task.Run(() => LoadItems(++p, q));
        }
        /// <summary>
        /// 搜索框加载完成后获得焦点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchBox_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as AutoSuggestBox).Focus(FocusState.Programmatic);
        }
        /// <summary>
        /// 滚到底加载下一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var scrollViewer = (sender as ScrollViewer);
            if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight&&BottomText.Text=="加载下一页")
            {
                BottomText_Tapped(BottomText, new TappedRoutedEventArgs());
            }
        }
    }
}
