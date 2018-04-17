using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Tucao.Content;
using Tucao.Helpers;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

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
            var i = new Danmaku("<d p=\"34.2,5,25,16777215,1522827434\">我C站牛逼！</d>");
            this.InitializeComponent();
            VideoList.ItemsSource = new ObservableCollection<VideoPanel>();
            Task.Run(() =>
            {
                if (p == 0) LoadItems(++p);
            });
        }
        /// <summary>
        /// 点击汉堡菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void TucaoLogo_Tapped(object sender,TappedRoutedEventArgs e)
        //{
        //   LeftSplitView.IsPaneOpen = !LeftSplitView.IsPaneOpen;
        //}
        /// <summary>
        /// 点击搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            //打开搜索页面
            Frame root = Window.Current.Content as Frame;
            root.Navigate(typeof(Search), null, new DrillInNavigationTransitionInfo());
        }
        /// <summary>
        /// 加载内容
        /// </summary>
        /// <param name="page"></param>
        private async void LoadItems(int page)
        {
            //显示正在加载
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                BottomText.Visibility = Visibility.Collapsed;
                LoadingProgress.Visibility = Visibility.Visible;
            });
            var r = await GetList(page);
            //把信息添加到items
            for (int i = 0; i < r.Count; i++)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                ((ObservableCollection<VideoPanel>)VideoList.ItemsSource).Add(r[i]);
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
        private async Task<List<VideoPanel>> GetList(int page)
        {
            List<VideoPanel> r = new List<VideoPanel>();
            //获取信息
            try
            {
                r = await Tucao.Content.Content.GetSubclassiFication(11, page);
            }
            catch (Exception e)
            {
                ErrorHelper.PopUp(e.Message);
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
            var v=e.ClickedItem as VideoPanel;
            var id = v.link.Replace("http://www.tucao.tv/play/", "").Replace("/", "");
            //打开视频页面
            Frame root = Window.Current.Content as Frame;
            if (id.First() == 'h')
                root.Navigate(typeof(MainPage), id.Remove(0, 1), new DrillInNavigationTransitionInfo());
        }
        /// <summary>
        /// 点击加载下一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BottomText_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RefreshButton.IsEnabled = false;
            Task.Run(()=>LoadItems(++p));
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
            VideoList.ItemsSource = new ObservableCollection<VideoPanel>();
            Task.Run(() => LoadItems(p));
        }

        private void About_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame root = Window.Current.Content as Frame;
            root.Navigate(typeof(About), null, new DrillInNavigationTransitionInfo());
        }

        private void DownloadList_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame root = Window.Current.Content as Frame;
            root.Navigate(typeof(DownloadList), null, new DrillInNavigationTransitionInfo());
        }
    }
}


