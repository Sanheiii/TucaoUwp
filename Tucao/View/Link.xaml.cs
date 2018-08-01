using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
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
        public static TextBlock FrameTitle = new TextBlock();
        public Link()
        {
            this.InitializeComponent();
            //设置标题栏
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            if (!coreTitleBar.IsVisible) AppTitleBar.Visibility = Visibility.Collapsed;
            coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;
            Window.Current.SetTitleBar(DraggableArea);
            //设置导航
            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackrequested;
            Rua.Navigated += Content_Navigated;
            Rua.NavigationFailed += App.OnNavigationFailed;
            Rua.Navigate(typeof(Index), null, new DrillInNavigationTransitionInfo());
            App.Link = Rua;
            FrameTitle = PageName;
        }
        /// <summary>
        /// 点击标题栏的返回
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            GoBack();
        }
        /// <summary>
        /// 点击导航栏的返回
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void OnBackrequested(object sender, BackRequestedEventArgs e)
        {
            GoBack();
            e.Handled = true;
        }
        /// <summary>
        /// 返回的方法
        /// </summary>
        public static void GoBack()
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
        }
        /// <summary>
        /// 在需要时隐藏标题栏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
            if (sender.IsVisible)
            {
                AppTitleBar.Visibility = Visibility.Visible;
            }
            else
            {
                AppTitleBar.Visibility = Visibility.Collapsed;
            }
        }
        private void Content_Navigated(object sender, NavigationEventArgs e)
        {
            BackButton.Visibility = App.CanGoBack() ? Visibility.Visible : Visibility.Collapsed;
            switch (e.SourcePageType.Name)
            {
                case "Index": { PageName.Text = "首页"; ShowTopBar = true; break; }
                case "DownloadList": { PageName.Text = "下载队列"; ShowTopBar = true; break; }
                case "LocalVideo": { PageName.Text = "本地视频"; ShowTopBar = true; break; }
                case "About": { PageName.Text = "关于"; ShowTopBar = true; break; }
                case "Search": { PageName.Text = "搜索"; ShowTopBar = true; break; }
                case "MediaPlayer": { ShowTopBar=false; break; }
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
            HamburgerMenu.IsPaneOpen = !HamburgerMenu.IsPaneOpen; ;
        }
        /// <summary>
        /// 关闭汉堡菜单取消模糊
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void HamburgerMenu_PaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args)
        {
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
        bool ShowTopBar
        {
            set
            {
                TopBar.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
