using Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Tucao.Helpers;
using Tucao.View.Dialogs;
using Windows.ApplicationModel.Core;
using Windows.Data.Json;
using Windows.Devices.Sensors;
using Windows.Graphics.Display;
using Windows.Storage;
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
        List<Danmaku> danmakuList = new List<Danmaku>();
        private SimpleOrientationSensor simpleorientation;
        public MediaPlayer()
        {
            this.InitializeComponent();
            Media.Stop();
        }
        //弹幕设置
        #region
        private void ShowDanmaku_Checked(object sender, RoutedEventArgs e)
        {
            SettingHelper.Values.IsShowDanmaku = true;
            DanmakuManager.IsShowDanmaku = true;
        }
        private void ShowDanmaku_Unchecked(object sender, RoutedEventArgs e)
        {
            SettingHelper.Values.IsShowDanmaku = false;
            DanmakuManager.IsShowDanmaku = false;
        }
        private void ShowScrollableDanmaku_Checked(object sender, RoutedEventArgs e)
        {
            SettingHelper.Values.IsShowScrollableDanmaku = true;
            HideScrollableDanmaku.Opacity = 1;
            DanmakuManager.IsShowScrollableDanmaku = true;
        }
        private void ShowScrollableDanmaku_Unchecked(object sender, RoutedEventArgs e)
        {
            SettingHelper.Values.IsShowScrollableDanmaku = false;
            HideScrollableDanmaku.Opacity = 0.3;
            DanmakuManager.IsShowScrollableDanmaku = false;
        }
        private void HideScrollableDanmaku_Click(object sender, RoutedEventArgs e)
        {
            ShowScrollableDanmaku.IsChecked = !ShowScrollableDanmaku.IsChecked;
        }
        private void ShowTopDanmaku_Checked(object sender, RoutedEventArgs e)
        {
            SettingHelper.Values.IsShowTopDanmaku = true;
            HideTopDanmaku.Opacity = 1;
            DanmakuManager.IsShowTopDanmaku = true;
        }
        private void ShowTopDanmaku_Unchecked(object sender, RoutedEventArgs e)
        {
            SettingHelper.Values.IsShowTopDanmaku = false;
            HideTopDanmaku.Opacity = 0.3;
            DanmakuManager.IsShowTopDanmaku = false;
        }
        private void HideTopDanmaku_Click(object sender, RoutedEventArgs e)
        {
            ShowTopDanmaku.IsChecked = !ShowTopDanmaku.IsChecked;
        }
        private void ShowBottomDanmaku_Checked(object sender, RoutedEventArgs e)
        {
            SettingHelper.Values.IsShowBottomDanmaku = true;
            HideBottomDanmaku.Opacity = 1;
            DanmakuManager.IsShowBottomDanmaku = true;
        }
        private void ShowBottomDanmaku_Unchecked(object sender, RoutedEventArgs e)
        {
            SettingHelper.Values.IsShowBottomDanmaku = false;
            HideBottomDanmaku.Opacity = 0.3;
            DanmakuManager.IsShowBottomDanmaku = false;
        }
        private void HideBottomDanmaku_Click(object sender, RoutedEventArgs e)
        {
            ShowBottomDanmaku.IsChecked = !ShowBottomDanmaku.IsChecked;
        }
        private void DanmakuSizeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            SettingHelper.Values.DanmakuSize = e.NewValue;
            DanmakuManager.SizeRatio = e.NewValue;
        }
        private void DanmakuSpeedSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            SettingHelper.Values.DanmakuSpeed = e.NewValue;
            DanmakuManager.SpeedRatio = e.NewValue;
        }
        private void DanmakuOpacitySlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            SettingHelper.Values.DanmakuOpacity = e.NewValue;
            DanmakuManager.Opacity = e.NewValue;
        }
        #endregion

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //获取参数打开视频
            param = e.Parameter as MediaPlayerSource;
            PlayerTitle.Text = param.Title + '-' + param.PartTitle;
            //加载弹幕
            LoadDanmaku();
            //播放视频
            Play(param.PlayList);
            //设置屏幕方向
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            //获取方向传感器
            simpleorientation = SimpleOrientationSensor.GetDefault();
            if (simpleorientation == null)
            {
                SettingsRow0.Visibility = Visibility.Collapsed;
            }
            //这两个东西有最小值,加载会出发ValueChanged事件,这里等它加载完再添加委托
            DanmakuSizeSlider.Loaded += ((sender, args) => DanmakuSizeSlider.ValueChanged += DanmakuSizeSlider_ValueChanged);
            DanmakuSpeedSlider.Loaded += ((sender, args) => DanmakuSpeedSlider.ValueChanged += DanmakuSpeedSlider_ValueChanged);
            //隐藏控制栏
            ControlPanel.Visibility = Visibility.Collapsed;
            StatusText.Text = "";
            Status.Visibility = Visibility.Visible;
            //设置可拖动区域
            Window.Current.SetTitleBar(DraggableArea);
            //设置一些控件的初始值
            SetValues();
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            //保存历史记录
            SaveHistory(param.Hid, param.Title, param.Part, ProgressSlider.Value);
            //屏幕转正
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
            //取消自动转屏
            if (simpleorientation != null)
            {
                simpleorientation.OrientationChanged -= Simpleorientation_OrientationChanged;
            }

        }
        /// <summary>
        /// 设置一些控件的状态
        /// </summary>
        private void SetValues()
        {
            ShowDanmaku.IsChecked = SettingHelper.Values.IsShowDanmaku;
            ShowScrollableDanmaku.IsChecked = SettingHelper.Values.IsShowScrollableDanmaku;
            ShowTopDanmaku.IsChecked = SettingHelper.Values.IsShowTopDanmaku;
            ShowBottomDanmaku.IsChecked = SettingHelper.Values.IsShowBottomDanmaku;
            DanmakuSizeSlider.Value = SettingHelper.Values.DanmakuSize;
            DanmakuSpeedSlider.Value = SettingHelper.Values.DanmakuSpeed;
            DanmakuOpacitySlider.Value = SettingHelper.Values.DanmakuOpacity;
            DanmakuManager.IsShowDanmaku = ShowDanmaku.IsChecked ?? false;
            DanmakuManager.IsShowScrollableDanmaku = ShowScrollableDanmaku.IsChecked ?? false;
            DanmakuManager.IsShowBottomDanmaku = ShowBottomDanmaku.IsChecked ?? false;
            DanmakuManager.IsShowTopDanmaku = ShowTopDanmaku.IsChecked ?? false;
            DanmakuManager.SizeRatio = DanmakuSizeSlider.Value;
            DanmakuManager.SpeedRatio = DanmakuSpeedSlider.Value;
            DanmakuManager.Opacity = DanmakuOpacitySlider.Value;

            AutoRotateSwitch.IsOn = SettingHelper.Values.IsAutoRotate;
            VolumeSlider.Value = SettingHelper.Values.Volume;
        }
        private void Simpleorientation_OrientationChanged(SimpleOrientationSensor sender, SimpleOrientationSensorOrientationChangedEventArgs args)
        {
            SimpleOrientation orientation = args.Orientation;
            switch (orientation)
            {
                case SimpleOrientation.Rotated90DegreesCounterclockwise: DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape; break;
                case SimpleOrientation.Rotated270DegreesCounterclockwise: DisplayInformation.AutoRotationPreferences = DisplayOrientations.LandscapeFlipped; break;
                default: return;
            }
        }

        /// <summary>
        /// 从外部导航到此界面传递的信息
        /// </summary>
        public class MediaPlayerSource
        {
            public string Title;
            public string Hid;
            public int Part;
            public string PartTitle;
            //分类id
            public string Tid;
            public bool IsLocalFile;
            public List<string> PlayList;
        }
        /// <summary>
        /// 加载弹幕
        /// </summary>
        private async void LoadDanmaku()
        {
            danmakuList = await Tucao.Content.GetDanmakus(param.Hid, param.Part, param.IsLocalFile);
        }

        private async void SaveHistory(string hid, string title, int part, double position)
        {
            //打开文件
            var folder = ApplicationData.Current.LocalCacheFolder;
            StorageFile file = await folder.CreateFileAsync("history.xml", CreationCollisionOption.OpenIfExists);
            Stream inputStream = await file.OpenStreamForReadAsync();
            StreamReader reader = new StreamReader(inputStream);
            string str = reader.ReadToEnd();
            reader.Dispose();
            inputStream.Dispose();
            await file.DeleteAsync();
            XDocument xDocument;
            try
            {
                xDocument = XDocument.Parse(str);
            }
            catch
            {
                xDocument = new XDocument();
                xDocument.Add(new XElement("history"));
            }
            //操作xml文档
            var xElement = xDocument.Element("history");
            foreach (var element in xElement.Elements())
            {
                if (element.Attribute("hid").Value == hid)
                {
                    element.Remove();
                    break;
                }
            }
            //<li hid="4077227" part="1" position="2" time="1533922742453">【7月】来玩游戏吧 04【喵萌茶会】</li>
            xElement.AddFirst(new XElement("li", new XAttribute("hid", hid), new XAttribute("part", part), new XAttribute("position", (int)position), new XAttribute("time", Methods.GetUnixTimestamp())) { Value = title });
            //写入文件
            file = await folder.CreateFileAsync("history.xml", CreationCollisionOption.OpenIfExists);
            Stream outputStream = await file.OpenStreamForWriteAsync();
            StreamWriter writer = new StreamWriter(outputStream);
            writer.Write(xDocument);
            writer.Dispose();
            outputStream.Dispose();
        }

        /// <summary>
        /// 播放视频
        /// </summary>
        /// <param name="part">分P号</param>
        private async void Play(List<string> play_list)
        {
            Uri source;
            if (play_list[0].Contains("gss3.baidu.com"))
            {
                source = new Uri(play_list[0]);
            }
            else
            {
                //载入播放引擎
                SYEngine.Core.Initialize();
                var playlist = new SYEngine.Playlist(param.IsLocalFile ? SYEngine.PlaylistTypes.LocalFile : SYEngine.PlaylistTypes.NetworkHttp);
                //将分段添加到playlist
                foreach (var url in play_list)
                {
                    playlist.Append(url, 0, 0);
                }
                //配置引擎
                SYEngine.PlaylistNetworkConfigs cfgs = default(SYEngine.PlaylistNetworkConfigs);
                cfgs.HttpUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.181 Safari/537.36";
                cfgs.HttpReferer = string.Empty;
                cfgs.HttpCookie = string.Empty;
                cfgs.UniqueId = string.Empty;
                cfgs.DownloadRetryOnFail = true;
                cfgs.DetectDurationForParts = true;
                playlist.NetworkConfigs = cfgs;
                source = await playlist.SaveAndGetFileUriAsync();
            }
            //开始播放
            StatusText.Text += Environment.NewLine + "开始缓冲视频...";
            try
            {
                Media.Source = source;
            }
            catch (Exception ex)
            {
                StatusText.Text += "    [失败]";
            }

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
            Count();
        }
        //播放数+1
        private void Count()
        {
            var t = Methods.HttpGetAsync("http://www.tucao.one/api.php?op=count&id=" + param.Hid + "&modelid=" + param.Tid);
        }

        /// <summary>
        /// 视频错误时候发生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Media_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            //回退打开视频进行的操作
            var dialog = new ErrorDialog("视频播放失败");
            var asyncOperation = dialog.ShowAsync();
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
                //DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
                //返回恢复原来的功能
                SystemNavigationManager.GetForCurrentView().BackRequested -= MediaPlayer_OnBackRequested;
                //SystemNavigationManager.GetForCurrentView().BackRequested += Link.OnBackrequested;
                view.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseVisible);
            }
            else
            {
                view.TryEnterFullScreenMode();
                //修改按钮图标
                FullWindowSymbol.Symbol = Symbol.BackToWindow;
                //显示返回键
                //横过来
                //DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
                //使得返回变成退出全屏
                //SystemNavigationManager.GetForCurrentView().BackRequested -= Link.OnBackrequested;
                SystemNavigationManager.GetForCurrentView().BackRequested += MediaPlayer_OnBackRequested;
                //使得导航栏透明
                view.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            }
        }
        private void MediaPlayer_OnBackRequested(object sender, BackRequestedEventArgs e)
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
                    if (ControlPanel.Visibility == Visibility.Collapsed)
                        ControlPanel.Visibility = Visibility.Visible;
                    else
                        ControlPanel.Visibility = Visibility.Collapsed;
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
                        DanmakuManager.Resume();
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
                        DanmakuManager.Pause();
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

        private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.Space:
                    {
                        PlayPauseButton_Click(PlayPauseButton, new RoutedEventArgs());
                    }
                    break;
                case VirtualKey.Escape:
                    {
                        FullWindowButton_Click(FullWindowButton, new RoutedEventArgs());
                    }
                    break;
                case VirtualKey.Enter:
                    {
                        Media_Tapped(Media, new TappedRoutedEventArgs());
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
        //划动屏幕跳转进度
        static double windowWidth;
        static double positionValue;
        static string positionText;
        private void Media_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            positionText = TimeElapsedElement.Text;
            positionValue = ProgressSlider.Value;
            windowWidth = this.RenderSize.Width;
        }
        //划动时根据距离更改提示的文本
        private void Media_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            int t = (int)(100 * e.Cumulative.Translation.X / windowWidth);
            if (t < 1 && t > -1)
            {
                SwipingMessage.Text = "取消跳转";
                return;
            }
            if (positionValue + t < 0) t = (int)-positionValue;
            SwipingMessage.Text = positionText + Convert.ToInt16(t).ToString("+#;-#;+0") + 's';
            SwipingPopup.Visibility = Visibility.Visible;
        }
        //松手时跳转进度
        private void Media_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            //松手0.5秒后使提示消失
            DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(500) };
            timer.Tick += ((s, ex) =>
            {
                SwipingPopup.Visibility = Visibility.Collapsed;
                timer.Stop();
            });
            timer.Start();
            int t = (int)(100 * e.Cumulative.Translation.X / windowWidth);
            if (t < 1 && t > -1)
            {
                SwipingPopup.Visibility = Visibility.Collapsed;
                return;
            }
            if (positionValue + t < 0) t = (int)-positionValue;
            ProgressSlider.Value = (int)positionValue + t;
        }
        /// <summary>
        /// 检测进度条变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProgressSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (e.OldValue > e.NewValue || e.NewValue - e.OldValue > 1)
            {
                DanmakuManager.Clear();
                return;
            }
            var danmakus = danmakuList.FindAll((d) => d.Position >= e.OldValue && d.Position < e.NewValue);
            foreach (var danmaku in danmakus)
            {
                DanmakuManager.AddDanmaku(danmaku.Content.Trim(), danmaku.TextColor, danmaku.Type);
            }
        }
        /// <summary>
        /// 发送弹幕
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void DanmakuSender_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string content = args.QueryText.Trim();
            if (content.Length == 0) return;
            string cid = param.Tid + '-' + param.Hid + "-1-" + param.Part;
            DanmakuManager.DanmakuType type;
            switch (Mode.SelectedIndex)
            {
                case 0: type = DanmakuManager.DanmakuType.Scrollable; break;
                case 1: type = DanmakuManager.DanmakuType.Top; break;
                case 2: type = DanmakuManager.DanmakuType.Bottom; break;
                default: type = DanmakuManager.DanmakuType.Scrollable; break;
            }
            DanmakuManager.SendDanmaku(content, ((SolidColorBrush)ColorPreview.Fill).Color, ProgressSlider.Value, cid, type);
            (sender as AutoSuggestBox).Text = "";
        }
        /// <summary>
        /// 如果窗口没有标题栏就自动全屏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Media_Loaded(object sender, RoutedEventArgs e)
        {
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            if (!coreTitleBar.IsVisible) FullWindowButton_Click(FullWindowButton, new RoutedEventArgs());
        }
        /// <summary>
        /// 点击控制栏的返回
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var view = ApplicationView.GetForCurrentView();
            view.ExitFullScreenMode();
            SystemNavigationManager.GetForCurrentView().BackRequested -= MediaPlayer_OnBackRequested;
            view.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseVisible);
            Link.GoBack();
        }
        /// <summary>
        /// 播放速率的值发生变化时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlaybackRateSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Media.PlaybackRate = e.NewValue;
        }
        /// <summary>
        /// 音量条的值变化时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Media.Volume = e.NewValue / 100;
            if (e.NewValue == 0)
            {
                VolumeIcon.Glyph = System.Net.WebUtility.HtmlDecode("&#xE74F;");
            }
            else
            {
                int d = (int)(e.NewValue) / 34;
                VolumeIcon.Glyph = System.Net.WebUtility.HtmlDecode("&#xE99" + (3 + d) + ";");
            }
            SettingHelper.Values.Volume = (int)e.NewValue;
        }
        /// <summary>
        /// 自动转屏按钮被开关
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoRotateSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (simpleorientation != null)
            {
                var t = sender as ToggleSwitch;
                bool isOn = t.IsOn;
                SettingHelper.Values.IsAutoRotate = isOn;
                if (isOn)
                {
                    simpleorientation.OrientationChanged += Simpleorientation_OrientationChanged;
                }
                else
                {
                    simpleorientation.OrientationChanged -= Simpleorientation_OrientationChanged;
                }
            }
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
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
