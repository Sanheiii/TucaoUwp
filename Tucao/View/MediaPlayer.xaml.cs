using System;
using System.Collections.Generic;
using Tucao.Helpers;
using Windows.Graphics.Display;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Tucao.View
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MediaPlayer : Page
    {
        public static bool isfullwindow = false;
        MediaPlayerSource param = new MediaPlayerSource();
        public MediaPlayer()
        {
            this.InitializeComponent();
            Media.Stop();
            Media.Tapped -= Media_Tapped;
            ControlPanelGrid.Visibility = Visibility.Collapsed;
            StatusText.Text = "";
            Status.Visibility = Visibility.Visible;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            param = e.Parameter as MediaPlayerSource;
            PlayerTitle.Text = param.title;
            Media.LoadDanmaku(param.hid, SkylarkWsp.DanmakuEngine.DanmakuSource.Tucao);
            Play(param.play_list);
        }
        /// <summary>
        /// 播放视频
        /// </summary>
        /// <param name="part">分P号</param>
        private async void Play(List<string> play_list)
        {

            //设置资源
            StatusText.Text += Environment.NewLine + "正在初始化播放器...";


            //载入播放引擎
            SYEngine.Core.Initialize();
            var playlist = new SYEngine.Playlist(param.islocalfile ? SYEngine.PlaylistTypes.LocalFile : SYEngine.PlaylistTypes.NetworkHttp);
            //playlist=new SYEngine.Playlist(SYEngine.PlaylistTypes.LocalFile);
            //将分段添加到playlist
            foreach (var url in play_list)
            {
                playlist.Append(url, 0, 0);
            }
            //配置引擎
            SYEngine.PlaylistNetworkConfigs cfgs = default(SYEngine.PlaylistNetworkConfigs);
            cfgs.HttpUserAgent = string.Empty;
            cfgs.HttpReferer = string.Empty;
            cfgs.HttpCookie = string.Empty;
            cfgs.UniqueId = string.Empty;
            cfgs.DownloadRetryOnFail = true;
            cfgs.DetectDurationForParts = true;
            playlist.NetworkConfigs = cfgs;
            try
            {
                Media.Source = await playlist.SaveAndGetFileUriAsync();
            }
            catch (Exception ex)
            {
                ErrorHelper.PopUp(ex.Message);
            }

            StatusText.Text += "    [成功]";
            //开始播放
            StatusText.Text += Environment.NewLine + "开始缓冲视频...";
        }

        /// <summary>
        /// 载入媒体时的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Media_MediaOpened(object sender, RoutedEventArgs e)
        {
            //将时长绑定到进度条上
            var duration = Media.NaturalDuration.TimeSpan.TotalSeconds;
            ProgressSlider.Maximum = duration;
            TimeRemainingElement.Text = ((int)duration / 60).ToString("d2") + ":" + ((int)duration % 60).ToString("d2");
            //显示控制栏
            ControlPanelGrid.Visibility = Visibility.Visible;
            //使点击显示控制栏
            Media.Tapped += Media_Tapped;
            //双击可以开始/暂停播放
            Media.DoubleTapped += PlayPauseButton_Click;
            Player.Focus(FocusState.Programmatic);
        }
        /// <summary>
        /// 播放器失去焦点后立即重新获取焦点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Player_LostFocus(object sender, RoutedEventArgs e)
        {
            Player.Focus(FocusState.Programmatic);
        }

        /// <summary>
        /// 视频错误时候发生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Media_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            //回退打开视频进行的操作
            ErrorHelper.PopUp("加载视频失败,暂时无法播放该视频", "");
            ControlPanelGrid.Visibility = Visibility.Collapsed;
            Media.Tapped -= Media_Tapped;
            Media.DoubleTapped -= PlayPauseButton_Click;
        }

        /// <summary>
        /// 开始/暂停按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            var i = Media.Media.CurrentState;
            if (i == MediaElementState.Playing)
            {
                //暂停
                Media.Pause();
            }
            else
            {
                Media.Play();
            }
        }
        /// <summary>
        /// 全屏按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FullWindowButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode)
            {
                //做个标记以便mainpage判断是否需要隐藏其他控件
                isfullwindow = false;
                view.ExitFullScreenMode();
                //和下面都是反着的
                FullWindowSymbol.Symbol = Symbol.FullScreen;
                InvertScreen.Visibility = Visibility.Collapsed;
                FullWindowBackButton.Visibility = Visibility.Collapsed;
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
                //返回恢复原来的功能
                SystemNavigationManager.GetForCurrentView().BackRequested -= MediaPlayer_BackRequested;
                SystemNavigationManager.GetForCurrentView().BackRequested += App.OnBackrequested;
            }
            else
            {
                isfullwindow = true;
                view.TryEnterFullScreenMode();
                //修改按钮图标
                FullWindowSymbol.Symbol = Symbol.BackToWindow;
                //显示翻转屏幕按钮
                if (DeviceHelper.IsMobile) InvertScreen.Visibility = Visibility.Visible;
                //显示返回键
                FullWindowBackButton.Visibility = Visibility.Visible;
                //横过来
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
                //使得返回变成退出全屏
                SystemNavigationManager.GetForCurrentView().BackRequested -= App.OnBackrequested;
                SystemNavigationManager.GetForCurrentView().BackRequested += MediaPlayer_BackRequested;
            }
        }
        private void MediaPlayer_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            FullWindowButton_Click(this, new RoutedEventArgs());
        }

        /// <summary>
        /// 隐藏/显示控制栏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Media_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ControlPanelGrid.Visibility == Visibility.Collapsed)
                ControlPanelGrid.Visibility = Visibility.Visible;
            else
                ControlPanelGrid.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// 转屏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InvertScreen_Click(object sender, RoutedEventArgs e)
        {
            //获取当前方向
            var vo = DisplayInformation.GetForCurrentView().CurrentOrientation;
            if (vo == DisplayOrientations.Landscape)
            {
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.LandscapeFlipped;
            }
            else
            {
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            }
        }
        /// <summary>
        /// 播放完成后
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Media_MediaEnded(object sender, RoutedEventArgs e)
        {
            Media.Stop();
            PlayPauseSymbol.Symbol = Symbol.Play;
        }
        private void Media_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            var i = Media.Media.CurrentState;
            switch (i)
            {
                case MediaElementState.Playing:
                    {
                        StatusText.Visibility = Visibility.Collapsed;
                        Status.Visibility = Visibility.Collapsed;
                        PlayPauseSymbol.Symbol = Symbol.Pause;
                        SettingHelper.IsScreenAlwaysOn = true;
                        break;
                    }
                case MediaElementState.Paused:
                    {
                        goto case MediaElementState.Closed;
                    }
                case MediaElementState.Stopped:
                    {
                        goto case MediaElementState.Closed;
                    }
                case MediaElementState.Closed:
                    {
                        //修改按钮图标
                        PlayPauseSymbol.Symbol = Symbol.Play;
                        //取消屏幕常亮
                        SettingHelper.IsScreenAlwaysOn = false;
                        break;
                    }
            }

        }
        /// <summary>
        /// 缓冲进度发生变化时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Media_BufferingProgressChanged(object sender, RoutedEventArgs e)
        {
            if (Media.Media.BufferingProgress < 1 && Media.Media.BufferingProgress > 0)
            {
                Status.Visibility = Visibility.Visible;
                BufferingProgress.Visibility = Visibility.Visible;
                BufferingProgress.Text = ((int)(Media.Media.BufferingProgress * 100)).ToString() + "%";
            }
            else
            {
                Status.Visibility = Visibility.Collapsed;
                BufferingProgress.Visibility = Visibility.Collapsed;
                BufferingProgress.Text = "";
            }
        }
        public class MediaPlayerSource
        {
            public string title { get; set; }
            public string hid { get; set; }
            public bool islocalfile { get; set; }
            public List<string> play_list { get; set; }
        }

        private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            e.Handled = true;
            switch (e.Key)
            {
                case VirtualKey.Space:
                    {
                        PlayPauseButton_Click(PlayPauseButton, null);
                    }
                    break;
                case VirtualKey.Escape:
                    {
                        FullWindowButton_Click(FullWindowButton, null);
                    }
                    break;
                case VirtualKey.Enter:
                    {
                        Media_Tapped(Media, null);
                    }
                    break;
                case VirtualKey.Left:
                    {
                        Media.Position -= new TimeSpan(30000000);
                    }
                    break;
                case VirtualKey.Right:
                    {
                        Media.Position += new TimeSpan(30000000);
                    }
                    break;
            }
        }
    }
}
