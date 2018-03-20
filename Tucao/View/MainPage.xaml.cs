using System;
using System.Collections.Generic;
using Tucao.Content;
using Tucao.Helpers;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.Media.Playback;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI;
using Windows.Media.Core;
using Windows.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.ApplicationModel.Core;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Web.Http;
using Windows.Storage;
using System.IO;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage.Streams;
using Windows.Data.Json;
// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Tucao.View
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public VisualState windowstate
        {
            get
            {
                return WindowState.CurrentState;
            }
        }
        Content.SubmissionInfo info = new Content.SubmissionInfo();

        /// <summary>
        /// 加载页面时调用
        /// </summary>
        /// <param name="e"></param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string hid = (string)e.Parameter;
            ////修改标题栏文本
            //var c = ApplicationView.GetForCurrentView();
            //c.Title = "h" + Hid;
            if (!DeviceHelper.IsMobile) TopControls.SecondaryCommands.Clear();
            try
            {
                info = await Tucao.Content.Content.GetSubmissionInfo(hid);
            }
            catch
            {
                ErrorHelper.PopUp("无法打开这个投稿");
                return;
            }
            //获取视频信息
            Task.Run(() => ViewInfo());

        }
        public MainPage()
        {
            this.InitializeComponent();
        }
        /// <summary>
        /// 显示视频信息
        /// </summary>
        /// <param name="hid">Hid</param>
        private async void ViewInfo()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
            {
                Title.Text = info.Title;//标题
                PAD.Text = "播放数:" + info.Play + "  弹幕数:" + info.Mukio;//播放,弹幕
                Description.NavigateToString(info.Description.Replace("<img", "<img style=\"width: 100%; \""));//详情
                User.Text = info.User;//发布者
                Create.Text = info.Create;//创建时间
                UserIcon.Source = new BitmapImage(new Uri(info.UserIcon));
                Media.PosterSource = new BitmapImage(new Uri(info.Thumb));

                //添加分p列表
                var i = 0;
                foreach (var p in info.Video)
                {

                    Button btn = new Button();
                    btn.Click += Part_Click;
                    btn.Margin = new Thickness(3, 3, 3, 3);
                    btn.Content = p["title"];
                    btn.Foreground = new SolidColorBrush(Colors.White);
                    //btn.Width = 150;
                    btn.Name = (i + 1).ToString();
                    Parts.Children.Add(btn);
                    try
                    {
                        btn.Tag = await info.GetPlayUrl(i + 1);
                        if (((List<string>)btn.Tag).Count == 0) throw new Exception();
                        btn.Background = new SolidColorBrush(Color.FromArgb(255, 255, 51, 102));
                    }
                    catch
                    {
                        btn.Background = new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
                    }
                    i++;
                }
                PartNum.Text = "分集(" + i.ToString() + ')';
                Introduction.Visibility = Visibility.Visible;
                LoadingControl.IsLoading = false;
            });
        }
        /// <summary>
        /// 分p选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Part_Click(object sender, RoutedEventArgs e)
        {
            var folder = ApplicationData.Current.LocalCacheFolder;
            JsonArray jsons = new JsonArray();
            Button button = (Button)sender;
            List<string> play_list = (List<string>)button.Tag;
            int part = int.Parse(button.Name);
            //是否选择了缓存
            if (Download.IsChecked == true)
            {
                DownloadHelper.Download(info, part);
            }
            else
            {
                Media.Stop();
                Media.Tapped -= Media_Tapped;
                ControlPanelGrid.Visibility = Visibility.Collapsed;
                StatusText.Text = "";

                Status.Visibility = Visibility.Visible;
                //如果之前获取播放地址失败再次尝试获取播放地址
                if (play_list == null || play_list.Count == 0)
                {
                    StatusText.Text += Environment.NewLine + "再次尝试获取播放地址...";
                    try
                    {
                        play_list = await info.GetPlayUrl(part);
                        if (play_list.Count == 0) throw new Exception();
                        button.Tag = play_list;
                        button.Background = new SolidColorBrush(Color.FromArgb(255, 255, 51, 102));
                        StatusText.Text += "    [成功]";
                    }
                    catch
                    {
                        StatusText.Text += "    [失败]";
                        return;
                    }
                }
                PlayerTitle.Text = "P" + part + ":" + info.Video[part - 1]["title"].ToString();
                Play(play_list);
            }
        }
        /// <summary>
        /// 播放视频
        /// </summary>
        /// <param name="part">分P号</param>
        private async void Play(List<string> play_list)
        {

            //设置资源
            StatusText.Text += Environment.NewLine + "正在初始化播放器...";
            //设置实时通信
            Media.RealTimePlayback = true;


            //载入播放引擎
            SYEngine.Core.Initialize();
            var playlist = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);
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
            Media.Source = await playlist.SaveAndGetFileUriAsync();

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
                //和下面都是反着的
                Details.Visibility = Visibility.Visible;
                FullWindowSymbol.Symbol = Symbol.FullScreen;
                FullWindowBackButton.Visibility = Visibility.Collapsed;
                view.ExitFullScreenMode();
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
            }
            else
            {
                //隐藏详情
                Details.Visibility = Visibility.Collapsed;
                //修改按钮图标
                FullWindowSymbol.Symbol = Symbol.BackToWindow;
                //显示返回键
                FullWindowBackButton.Visibility = Visibility.Visible;
                //全屏
                view.TryEnterFullScreenMode();
                //横过来
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            }
        }
        private void BackRequested(object sender, BackRequestedEventArgs e)
        {
            FullWindowButton_Click(null, null);
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

        private void Chevron_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //这里看起来是白的其实是"&#xE70D;"ChevronDown
            if (Chevron.Glyph == "")
            {
                Chevron.Glyph = "";//ChevronUp
                Parts.Height = 38;
            }
            else
            {
                Chevron.Glyph = "";//ChevronDown
                //设置为Double.NaN即可设为Auto
                Parts.Height = Double.NaN;
            }


        }
        private void MediaPlayer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Row0.Height = new GridLength(ActualWidth / 16 * 9);
            return;
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
            if (Media.BufferingProgress < 1)
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
    //进度条-显示的时间
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
    //媒体播放器高度
    class MediaHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (new MainPage().windowstate.Name == "WideState")
                return new GridLength(1, GridUnitType.Star);
            return new GridLength((double)value / 16 * 9);

        }
        //下面的用不到,瞎jb写了个
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
    //监控某控件属性变化
    //<c:ActualSizePropertyProxy Element="{x:Bind xxxxx}" 
    //                           x:Name="Pproxy" />
    public class ActualSizePropertyProxy : FrameworkElement, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public FrameworkElement Element
        {
            get { return (FrameworkElement)GetValue(ElementProperty); }
            set { SetValue(ElementProperty, value); }
        }

        public double ActualHeightValue
        {
            get { return Element == null ? 0 : Element.ActualHeight; }
        }

        public double ActualWidthValue
        {
            get { return Element == null ? 0 : Element.ActualWidth; }
        }

        public static readonly DependencyProperty ElementProperty =
            DependencyProperty.Register("Element", typeof(FrameworkElement), typeof(ActualSizePropertyProxy),
                                        new PropertyMetadata(null, OnElementPropertyChanged));

        private static void OnElementPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ActualSizePropertyProxy)d).OnElementChanged(e);
        }

        private void OnElementChanged(DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement oldElement = (FrameworkElement)e.OldValue;
            FrameworkElement newElement = (FrameworkElement)e.NewValue;

            newElement.SizeChanged += new SizeChangedEventHandler(Element_SizeChanged);
            if (oldElement != null)
            {
                oldElement.SizeChanged -= new SizeChangedEventHandler(Element_SizeChanged);
            }
            NotifyPropChange();
        }

        private void Element_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            NotifyPropChange();
        }

        private void NotifyPropChange()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ActualWidthValue"));
                PropertyChanged(this, new PropertyChangedEventArgs("ActualHeightValue"));
            }
        }
    }
}


