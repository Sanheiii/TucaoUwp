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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Tucao.View
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Link : Page
    {
        public Link()
        {
            this.InitializeComponent();
            Content.Navigate(typeof(Index), null, new DrillInNavigationTransitionInfo());
        }
        /// <summary>
        /// 点击汉堡菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hamburger_Tapped(object sender, TappedRoutedEventArgs e)
        {
            HamburgerMenu.IsPaneOpen = !HamburgerMenu.IsPaneOpen;
        }
        /// <summary>
        /// 点击搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Search_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //打开搜索页面
            Frame root = Window.Current.Content as Frame;
            root.Navigate(typeof(Search), null, new DrillInNavigationTransitionInfo());
        }
        /// <summary>
        /// 点击下载队列
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadListTapped(object sender, TappedRoutedEventArgs e)
        {
            Frame root = Window.Current.Content as Frame;
            root.Navigate(typeof(DownloadList), null, new DrillInNavigationTransitionInfo());
        }
        /// <summary>
        /// 点击本地视频
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LocalVideoTapped(object sender, TappedRoutedEventArgs e)
        {
            Frame root = Window.Current.Content as Frame;
            root.Navigate(typeof(LocalVideo), null, new DrillInNavigationTransitionInfo());
        }

        private void Setting_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame root = Window.Current.Content as Frame;
            root.Navigate(typeof(About), null, new DrillInNavigationTransitionInfo());
        }
    }
}
