using System;
using System.Collections.Generic;
using Tucao.Helpers;
using Windows.Graphics.Display;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
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
        bool isTapped = false;
        MediaPlayerSource param = new MediaPlayerSource();
        public MediaPlayer()
        {
            this.InitializeComponent();
            //这两个东西有最小值,加载会出发ValueChanged事件,这里等它加载完再添加委托
            DanmakuSizeSlider.Loaded += ((sender, e) => DanmakuSizeSlider.ValueChanged += DanmakuSizeSlider_ValueChanged);
            DanmakuSpeedSlider.Loaded += ((sender, e) => DanmakuSpeedSlider.ValueChanged += DanmakuSpeedSlider_ValueChanged);
            Media.Stop();
            Media.Tapped -= Media_Tapped;
            ControlPanelGrid.Visibility = Visibility.Collapsed;
            StatusText.Text = "";
            Status.Visibility = Visibility.Visible;
            SetValues();
        }
        /// <summary>
        /// 设置一些控件的状态
        /// </summary>
        private void SetValues()
        {
            IsShowDanmaku.IsChecked = (bool?)SettingHelper.GetValue("IsShowDanmaku") ?? true;
            IsShowScrollableDanmaku.IsChecked = (bool?)SettingHelper.GetValue("IsShowScrollableDanmaku") ?? true;
            IsShowTopDanmaku.IsChecked = (bool?)SettingHelper.GetValue("IsShowTopDanmaku") ?? true;
            IsShowBottomDanmaku.IsChecked = (bool?)SettingHelper.GetValue("IsShowBottomDanmaku") ?? true;
            DanmakuSizeSlider.Value = (double?)SettingHelper.GetValue("DanmakuSize") ?? 0.7;
            DanmakuSpeedSlider.Value = (double?)SettingHelper.GetValue("DanmakuSpeed") ?? 0.6;
            DanmakuOpacitySlider.Value = (double?)SettingHelper.GetValue("DanmakuOpacity") ?? 1;
        }

        private void IsShowDanmaku_Checked(object sender, RoutedEventArgs e)
        {
            SettingHelper.SetValue("IsShowDanmaku", true);
        }
        private void IsShowDanmaku_Unchecked(object sender, RoutedEventArgs e)
        {
            SettingHelper.SetValue("IsShowDanmaku", false);
        }
        private void IsShowScrollableDanmaku_Checked(object sender, RoutedEventArgs e)
        {
            SettingHelper.SetValue("IsShowScrollableDanmaku", true);
            HideScrollableDanmaku.Opacity = 1;
        }
        private void IsShowScrollableDanmaku_Unchecked(object sender, RoutedEventArgs e)
        {
            SettingHelper.SetValue("IsShowScrollableDanmaku", false);
            HideScrollableDanmaku.Opacity = 0.3;
        }
        private void HideScrollableDanmaku_Click(object sender, RoutedEventArgs e)
        {
            IsShowScrollableDanmaku.IsChecked = !IsShowScrollableDanmaku.IsChecked;
        }
        private void IsShowTopDanmaku_Checked(object sender, RoutedEventArgs e)
        {
            SettingHelper.SetValue("IsShowTopDanmaku", true);
            HideTopDanmaku.Opacity = 1;
        }
        private void IsShowTopDanmaku_Unchecked(object sender, RoutedEventArgs e)
        {
            SettingHelper.SetValue("IsShowTopDanmaku", false);
            HideTopDanmaku.Opacity = 0.3;
        }
        private void HideTopDanmaku_Click(object sender, RoutedEventArgs e)
        {
            IsShowTopDanmaku.IsChecked = !IsShowTopDanmaku.IsChecked;
        }
        private void IsShowBottomDanmaku_Checked(object sender, RoutedEventArgs e)
        {
            SettingHelper.SetValue("IsShowBottomDanmaku", true);
            HideBottomDanmaku.Opacity = 1;
        }
        private void IsShowBottomDanmaku_Unchecked(object sender, RoutedEventArgs e)
        {
            SettingHelper.SetValue("IsShowBottomDanmaku", false);
            HideBottomDanmaku.Opacity = 0.3;
        }
        private void HideBottomDanmaku_Click(object sender, RoutedEventArgs e)
        {
            IsShowBottomDanmaku.IsChecked = !IsShowBottomDanmaku.IsChecked;
        }
        private void DanmakuSizeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            SettingHelper.SetValue("DanmakuSize", e.NewValue);
        }
        private void DanmakuSpeedSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            SettingHelper.SetValue("DanmakuSpeed", e.NewValue);
        }
        private void DanmakuOpacitySlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            SettingHelper.SetValue("DanmakuOpacity", e.NewValue);
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            param = e.Parameter as MediaPlayerSource;
            PlayerTitle.Text = param.title;
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
                var msgDialog = new Windows.UI.Popups.MessageDialog(ex.Message);
                msgDialog.ShowAsync();
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
            Media.DoubleTapped += Media_DoubleTapped;
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
            var msgDialog = new Windows.UI.Popups.MessageDialog("加载视频失败,暂时无法播放该视频");
            msgDialog.ShowAsync();
            ControlPanelGrid.Visibility = Visibility.Collapsed;
            Media.Tapped -= Media_Tapped;
            Media.DoubleTapped -= Media_DoubleTapped;
        }

        /// <summary>
        /// 开始/暂停按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            var i = Media.CurrentState;
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
                view.ExitFullScreenMode();
                //和下面都是反着的
                FullWindowSymbol.Symbol = Symbol.FullScreen;
                InvertScreen.Visibility = Visibility.Collapsed;
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
                //返回恢复原来的功能
                SystemNavigationManager.GetForCurrentView().BackRequested -= MediaPlayer_BackRequested;
                SystemNavigationManager.GetForCurrentView().BackRequested += Link.OnBackrequested;
            }
            else
            {
                view.TryEnterFullScreenMode();
                //修改按钮图标
                FullWindowSymbol.Symbol = Symbol.BackToWindow;
                //显示翻转屏幕按钮
                if (DeviceHelper.IsMobile) InvertScreen.Visibility = Visibility.Visible;
                //显示返回键
                //横过来
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
                //使得返回变成退出全屏
                SystemNavigationManager.GetForCurrentView().BackRequested -= Link.OnBackrequested;
                SystemNavigationManager.GetForCurrentView().BackRequested += MediaPlayer_BackRequested;
            }
        }
        private void MediaPlayer_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            FullWindowButton_Click(this, new RoutedEventArgs());
        }

        /// <summary>
        /// 点击视频画面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Media_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(200);
            timer.Tick += ((s, ea) =>
            {
                if (isTapped)
                {
                    isTapped = false;
                    if (ControlPanelGrid.Visibility == Visibility.Collapsed)
                        ControlPanelGrid.Visibility = Visibility.Visible;
                    else
                        ControlPanelGrid.Visibility = Visibility.Collapsed;
                }
                timer.Stop();
            });
            isTapped = true;
            timer.Start();
        }
        /// <summary>
        /// 连续点击视频时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Media_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (isTapped)
            {
                isTapped = false;
                PlayPauseButton_Click(PlayPauseButton, new RoutedEventArgs());
            }
            else Media_Tapped(Media, new TappedRoutedEventArgs());
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
            var i = Media.CurrentState;
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
            if (Media.BufferingProgress < 1 && Media.BufferingProgress > 0)
            {
                Status.Visibility = Visibility.Visible;
                BufferingProgress.Visibility = Visibility.Visible;
                BufferingProgress.Text = ((int)(Media.BufferingProgress * 100)).ToString() + "%";
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
                        ProgressSlider.Value -= 3;
                    }
                    break;
                case VirtualKey.Right:
                    {

                        ProgressSlider.Value += 3;
                    }
                    break;
            }
            e.Handled = true;
        }
        /// <summary>
        /// 弹幕颜色改变时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RGB_ValueChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void RGB_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            try
            {
                int value = int.Parse(sender.Text);
                if (value >= 0 && value <= 255)
                {
                    ColorPreview.Fill = new SolidColorBrush(Color.FromArgb(255, byte.Parse(R.Text), byte.Parse(G.Text), byte.Parse(B.Text)));
                    sender.Tag = sender.Text;
                    return;
                }
                throw new Exception();
            }
            catch
            {
                sender.Text = (string)sender.Tag;
                sender.SelectionStart = ((string)sender.Tag).Length;
            }
        }

    }




    /// <summary>
    /// 播放时间-进度条
    /// </summary>
    class MediaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((TimeSpan)value).TotalSeconds;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return TimeSpan.FromSeconds((double)value);
        }
    }
    /// <summary>
    ///进度条-显示的时间
    /// </summary>
    class TimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((int)(double)value / 60).ToString("d2") + ":" + ((int)(double)value % 60).ToString("d2");
        }
        //下面的用不到,瞎jb写了个
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
