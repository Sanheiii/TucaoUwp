using System;
using System.Collections.Generic;
using System.ComponentModel;
using Tucao.Content;
using Tucao.Helpers;
using Windows.ApplicationModel.DataTransfer;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Tucao.View
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ApplicationView view = ApplicationView.GetForCurrentView();
        public VisualState windowstate
        {
            get
            {
                return WindowState.CurrentState;
            }
        }
        SubmissionInfo info = new SubmissionInfo();

        /// <summary>
        /// 加载页面时调用
        /// </summary>
        /// <param name="e"></param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            string hid = (string)e.Parameter;
            ////修改标题栏文本
            //var c = ApplicationView.GetForCurrentView();
            //c.Title = "h" + Hid;
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
            ViewInfo();

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

            Thumb.UriSource = new Uri(info.Thumb);
            Title.Text = info.Title;//标题
            PAD.Text = "播放数:" + info.Play + "  弹幕数:" + info.Mukio;//播放,弹幕
            Description.NavigateToString(info.Description.Replace("<img", "<img style=\"width: 100%; \""));//详情
            User.Text = info.User;//发布者
            Create.Text = info.Create;//创建时间
            UserIcon.Source = new BitmapImage(new Uri(info.UserIcon));
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
                var param = new MediaPlayer.MediaPlayerSource();
                //如果之前获取播放地址失败再次尝试获取播放地址
                if (play_list == null || play_list.Count == 0)
                {
                    try
                    {
                        play_list = await info.GetPlayUrl(part);
                        if (play_list.Count == 0) throw new Exception();
                        button.Tag = play_list;
                        button.Background = new SolidColorBrush(Color.FromArgb(255, 255, 51, 102));
                    }
                    catch
                    {
                        return;
                    }
                }
                param.title = "P" + part + ":" + info.Video[part - 1]["title"].ToString();
                param.play_list = play_list;
                param.islocalfile = false;
                MediaPlayer.Navigate(typeof(MediaPlayer), param);
                //Media.Stop();
                //Media.Tapped -= Media_Tapped;
                //ControlPanelGrid.Visibility = Visibility.Collapsed;
                //StatusText.Text = "";

                //Status.Visibility = Visibility.Visible;
                ////如果之前获取播放地址失败再次尝试获取播放地址
                //if (play_list == null || play_list.Count == 0)
                //{
                //    StatusText.Text += Environment.NewLine + "再次尝试获取播放地址...";
                //    try
                //    {
                //        play_list = await info.GetPlayUrl(part);
                //        if (play_list.Count == 0) throw new Exception();
                //        button.Tag = play_list;
                //        button.Background = new SolidColorBrush(Color.FromArgb(255, 255, 51, 102));
                //        StatusText.Text += "    [成功]";
                //    }
                //    catch
                //    {
                //        StatusText.Text += "    [失败]";
                //        return;
                //    }
                //}
            }
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
            if (View.MediaPlayer.isfullwindow)
            {
                Details.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (windowstate.Name == "NarrowState")
                    Row0.Height = new GridLength(ActualWidth / 16 * 9);
                Details.Visibility = Visibility.Visible;
            }
        }

        private void Share_Tapped(object sender, TappedRoutedEventArgs e)
        {
            DataPackage dataPackage = new DataPackage();
            // copy 
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText("http://www.tucao.tv/play/h" + info.Hid+"/");
            Clipboard.SetContent(dataPackage);
            ErrorHelper.PopUp("分享链接已复制到剪切板");
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

