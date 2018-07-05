using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
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
        public static TextBlock FrameTitle=new TextBlock();
        public Link()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackrequested;
            Rua.Navigated += Content_Navigated;
            Rua.Navigated += App.OnNavigated;
            Rua.NavigationFailed += App.OnNavigationFailed;
            Rua.Navigate(typeof(Index), null, new DrillInNavigationTransitionInfo());
            App.Link = Rua;
            FrameTitle = Title;
        }

        private void Content_Navigated(object sender, NavigationEventArgs e)
        {
            switch (e.SourcePageType.Name)
            {
                case "Index": { Title.Text = "首页"; break; }
                case "DownloadList": { Title.Text = "下载队列"; break; }
                case "LocalVideo": { Title.Text = "本地视频"; break; }
                case "About": { Title.Text = "关于"; break; }
                case "Search": { Title.Text = "搜索"; break; }
            }
        }

        /// <summary>
        /// 点击汉堡菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hamburger_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //打开或关闭汉堡菜单
            HamburgerMenu.IsPaneOpen = !HamburgerMenu.IsPaneOpen;
            //模糊效果
            PageContent.Blur(value: 10, duration: 200, delay: 0).StartAsync();
        }
        /// <summary>
        /// 关闭汉堡菜单取消模糊
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void HamburgerMenu_PaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args)
        {
            PageContent.Blur(value: 0, duration: 200, delay: 0).StartAsync();
        }
        /// <summary>
        /// 点击搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Search_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //打开搜索页面
            if (Rua.CurrentSourcePageType != typeof(Search))
                Rua.Navigate(typeof(Search), null, new DrillInNavigationTransitionInfo());
        }
        private void Index_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Rua.CurrentSourcePageType != typeof(Index))
            {
                App.Link.BackStack.Clear();
                Rua.Navigate(typeof(Index), null, new DrillInNavigationTransitionInfo());
            }
        }
        /// <summary>
        /// 点击下载队列
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadListTapped(object sender, TappedRoutedEventArgs e)
        {
            if (Rua.CurrentSourcePageType != typeof(DownloadList))
                Rua.Navigate(typeof(DownloadList), null, new DrillInNavigationTransitionInfo());
        }
        /// <summary>
        /// 点击本地视频
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LocalVideoTapped(object sender, TappedRoutedEventArgs e)
        {
            if (Rua.CurrentSourcePageType != typeof(LocalVideo))
                Rua.Navigate(typeof(LocalVideo), null, new DrillInNavigationTransitionInfo());
        }
        /// <summary>
        /// 点击关于，以后可能做成设置按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Setting_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Rua.CurrentSourcePageType != typeof(About))
                HamburgerMenu.IsPaneOpen = !HamburgerMenu.IsPaneOpen;
            Rua.Navigate(typeof(About), null, new DrillInNavigationTransitionInfo());
        }
        /// <summary>
        /// 点击菜单项目后收起汉堡菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NavLinksList_ItemClick(object sender, ItemClickEventArgs e)
        {
            HamburgerMenu.IsPaneOpen = !HamburgerMenu.IsPaneOpen;
        }
        public static void OnBackrequested(object sender, BackRequestedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null && rootFrame.CurrentSourcePageType.Name != "Link" && rootFrame.CanGoBack)
            {
                rootFrame.GoBack();
            }
            else if (App.Link != null && App.Link.CurrentSourcePageType.Name != "Index")
            {
                App.Link.GoBack();
            }
            else
            {
                Application.Current.Exit();
            }
            e.Handled = true;
        }

    }
}
