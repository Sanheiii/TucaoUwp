using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    public sealed partial class Index : Page
    {
        int p = 0;
        public Index()
        {
            this.InitializeComponent();
            VideoList.ItemsSource = new ObservableCollection<Introduction>();
            Task.Run(() =>
            {
                if (p == 0) LoadItems(++p);
            });
        }
        /// <summary>
        /// 加载内容
        /// </summary>
        /// <param name="page"></param>
        private async void LoadItems(int page)
        {
            //显示正在加载
            List<Introduction> r;
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                ExceptionMessage.Visibility = Visibility.Collapsed;
                BottomText.Visibility = Visibility.Collapsed;
                LoadingProgress.Visibility = Visibility.Visible;
            });
            try
            {
                r = await GetList(page);
            }
            catch
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    if (page == 1)
                    {
                        ExceptionMessage.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        Link.ShowToast("加载失败");
                        BottomText.Visibility = Visibility.Visible;
                    }
                    RefreshButton.IsEnabled = true;
                    LoadingProgress.Visibility = Visibility.Collapsed;
                });
                p--;
                return;
            }
            //把信息添加到items
            for (int i = 0; i < r.Count; i++)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    ((ObservableCollection<Introduction>)VideoList.ItemsSource).Add(r[i]);
                });
                await Task.Delay(10);
            }
            //显示加载下一页
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                RefreshButton.IsEnabled = true;
                BottomText.Visibility = Visibility.Visible;
                LoadingProgress.Visibility = Visibility.Collapsed;
            });

        }
        private async Task<List<Introduction>> GetList(int page)
        {
            List<Introduction> r = new List<Introduction>();
            //获取信息
            try
            {
                r = await Tucao.Content.GetSubclassiFication(11, page);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return r;
        }
        /// <summary>
        /// 点击打开视频
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VideoList_ItemClick(object sender, ItemClickEventArgs e)
        {
            //提取出h号
            var v = e.ClickedItem as Introduction;
            var id = v.Link.Replace("http://www.tucao.tv/play/", "").Replace("/", "");
            //打开视频页面
            if (id.First() == 'h')
            {
                App.OpenVideo(id.Remove(0, 1));
            }
        }
        /// <summary>
        /// 点击加载下一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BottomText_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RefreshButton.IsEnabled = false;
            Task.Run(() => LoadItems(++p));
        }
        /// <summary>
        /// 点击刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Refresh_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RefreshButton.IsEnabled = false;
            p = 1;
            VideoList.ItemsSource = new ObservableCollection<Introduction>();
            Task.Run(() => LoadItems(p));
        }
        /// <summary>
        /// 滚到底加载下一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var scrollViewer = (sender as ScrollViewer);
            if(scrollViewer.VerticalOffset==scrollViewer.ScrollableHeight)
            {
                BottomText_Tapped(BottomText, new TappedRoutedEventArgs());
            }
        }
    }
}


