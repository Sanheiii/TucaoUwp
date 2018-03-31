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
        SubmissionInfo info = new SubmissionInfo();

        /// <summary>
        /// 加载页面时调用
        /// </summary>
        /// <param name="e"></param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            string hid = (string)e.Parameter;
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
                FocusManager.TryMoveFocus(FocusNavigationDirection.Next);
            }
        }
        /// <summary>
        /// 播放器大小被改变时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MediaPlayer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (View.MediaPlayer.isfullwindow)
            {
                Details.Visibility = Visibility.Collapsed;
            }
            else
            {
                Details.Visibility = Visibility.Visible;
                try
                {
                    if (WindowState.CurrentState.Name == "WideState")
                    {
                        Row0.Height = new GridLength(1, GridUnitType.Star);
                    }
                    else
                    {
                        Row0.Height = new GridLength(PlayerGrid.ActualWidth / 16 * 9);
                    }
                }
                catch { return; }
            }
        }
        /// <summary>
        /// 点击分享
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Share_Tapped(object sender, TappedRoutedEventArgs e)
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            DataTransferManager.ShowShareUI();
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            
            DataRequest request = args.Request;
            Uri uri = new Uri("http://www.tucao.tv/play/h" + info.Hid + "/");
            request.Data.SetWebLink(uri);
            request.Data.Properties.Title = info.Title;
            request.Data.Properties.Description = "分享自吐槽UWP";
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

