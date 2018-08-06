using Microsoft.Toolkit.Uwp.UI.Animations;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Tucao.View
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Link : Page
    {

        public static TextBlock frameTitle = new TextBlock();
        public static Link currentObject;
        static Grid toastArea = new Grid();
        public Link()
        {
            currentObject = this;
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
            frameTitle = PageName;
            toastArea = ToastArea;
            //如果第一次打开这个版本则弹出提示
            string appVersion = Methods.GetAppVersion();
            if ((string)Helpers.SettingHelper.GetValue("Version") != appVersion)
            {
                Controls.UpdateLog dialog = new Controls.UpdateLog();
                var task = dialog.ShowAsync();
                Helpers.SettingHelper.SetValue("Version", appVersion);
            }
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
            if (e.Handled) return;
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
                case "Search": { PageName.Text = "搜索"; ShowTopBar = true; break; }
                case "Setting": { PageName.Text = "设置"; ShowTopBar = true; break; }
                case "Details": { ShowTopBar = true; break; }
                case "MediaPlayer": { ShowTopBar = false; break; }
                default: { ShowTopBar = true; break; }
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
        private void XFListTapped(object sender, TappedRoutedEventArgs e)
        {
            ShowToast("正在施工");
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
        /// 点击设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Setting_Tapped(object sender, TappedRoutedEventArgs e)
        {

            HamburgerMenu.IsPaneOpen = !HamburgerMenu.IsPaneOpen;
            if (Rua.CurrentSourcePageType != typeof(Setting))
            {
                Rua.Navigate(typeof(Setting), null, new DrillInNavigationTransitionInfo());
            }
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
        /// <summary>
        /// 显示提示
        /// </summary>
        /// <param name="message"></param>
        public static void ShowToast(string message)
        {
            //创建要显示的控件
            Grid item = new Grid() { Opacity = 0, MaxWidth = App.Link.ActualWidth, MinWidth = 200 };
            Rectangle rectangle = new Rectangle() { Fill = new SolidColorBrush(Color.FromArgb(0xCC, 0x66, 0x66, 0x66)), RadiusX = 4, RadiusY = 4 };
            TextBlock textBlock = new TextBlock { TextWrapping = TextWrapping.WrapWholeWords, Foreground = new SolidColorBrush(Color.FromArgb(0xCC, 0xFF, 0xFF, 0xFF)), Margin = new Thickness(20, 10, 20, 10), HorizontalAlignment = HorizontalAlignment.Center, Text = message };
            item.Children.Add(rectangle);
            item.Children.Add(textBlock);
            //到ui线程中添加控件
            var task = currentObject.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
              {
                  toastArea.Children.Add(item);
                  var s = item.Fade(1, 300, 0, EasingType.Default, EasingMode.EaseOut);
                  s.Completed += (ssender, se) =>
                  {
                      var h = item.Fade(0, 300, 3000, EasingType.Default, EasingMode.EaseIn);
                      h.Completed += (hsender, he) =>
                      {
                          toastArea.Children.Remove(item);
                      };
                      h.Start();
                  };
                  s.Start();
              });
        }


    }
}
