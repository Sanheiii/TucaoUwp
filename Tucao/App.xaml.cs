using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
namespace Tucao
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application
    {
        public static Frame Link = new Frame();
        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            //Test.ldhrbq();
#endif
            //设置桌面模式窗口的最小宽高
            //设置快捷键
            //Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
            //隐藏原来的标题栏
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                //状态栏颜色
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.BackgroundColor = Color.FromArgb(0xFF, 0xBF, 0xBF, 0xBF);
                statusBar.BackgroundOpacity = 60d/255;
                statusBar.ForegroundColor = RequestedTheme == ApplicationTheme.Dark ? Colors.White : Colors.Black;
            }

            Frame rootFrame = Window.Current.Content as Frame;
            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (rootFrame == null)
            {
                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: 从之前挂起的应用程序加载状态
                }

                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;

            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // 当导航堆栈尚未还原时，导航到第一页，
                    // 并通过将所需信息作为导航参数传入来配置
                    // 参数
                    rootFrame.Navigate(typeof(View.Link), e.Arguments);
                }
                // 确保当前窗口处于活动状态
                Window.Current.Activate();
            }
        }
        /// <summary>
        /// 按下快捷键的处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Dispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
        {
            //if(args.EventType.ToString()=="KeyDown")
            //switch (args.VirtualKey)
            //{
            //    case Windows.System.VirtualKey.Escape: ; break;
            //}
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        public static void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: 保存应用程序状态并停止任何后台活动
            deferral.Complete();
        }
        public static bool CanGoBack()
        {
            if (Link != null && Link.CurrentSourcePageType != typeof(View.Index) && Link.CanGoBack)
                return true;
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null && rootFrame.CurrentSourcePageType != typeof(View.Link) && rootFrame.CanGoBack)
                return true;
            return false;
        }
        /// <summary>
        /// 重设缓存以清除某些页面
        /// </summary>
        public static void ResetPageCache()
        {
            var cacheSize = Link.CacheSize;
            Link.CacheSize = 0;
            Link.CacheSize = cacheSize;
        }
        /// <summary>
        /// 打开视频详情页
        /// </summary>
        /// <param name="hid"></param>
        public static void OpenVideo(string hid)
        {
            App.ResetPageCache();
            Link.Navigate(typeof(View.Details), hid, new DrillInNavigationTransitionInfo());
        }
    }
}
