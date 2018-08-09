using HtmlAgilityPack;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tucao.Helpers;
using Windows.ApplicationModel.DataTransfer;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Tucao.View
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Details : Page
    {
        //视频信息
        public VideoInfo info;
        //评论页码
        int Commentpage;
        public Details()
        {
            this.InitializeComponent();
            info = new VideoInfo();
            PartList.ItemsSource = new ObservableCollection<PartInfo>();
            Comment.ItemsSource = new ObservableCollection<Comment>();
            Commentpage = 0;
        }
        /// <summary>
        /// 加载页面时调用
        /// </summary>
        /// <param name="e"></param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            int i = 0;
            string hid = (string)e.Parameter;
            Link.frameTitle.Text = 'h' + hid;
            if (hid == info.Hid) return;
            try
            {
                info = await Tucao.Content.GetVideoInfo(hid);
            }
            catch
            {
                Link.ShowToast("无法打开这个投稿");
                return;
            }
            SaveThumb.Tag = info.Thumb;
            Thumb.Source = new BitmapImage(new Uri(info.Thumb));
            Title.Text = info.Title;
            PAD.Text = "播放:" + info.Play + "\t弹幕:" + info.Mukio;
            UserIcon.Source = new BitmapImage(new Uri(info.UserIcon));
            UP.Text = info.User;
            Create.Text = "投稿于" + info.Create;
            Type.Text = info.MainTypeName + '/' + info.TypeName;
            HtmlDocument doc = new HtmlDocument();
            //把</br>转成\n;
            info.Description = Regex.Replace(info.Description, "</?br>", "\n");
            doc.LoadHtml(WebUtility.HtmlDecode(info.Description));
            //合并多余\n,删除\r\n
            DescriptionTextBlock.Text = Regex.Replace(doc.DocumentNode.InnerText.Replace("\r\n", ""), "[\n]+", "\n");
            var nodes = doc.DocumentNode.SelectNodes("//img");
            if (nodes != null)
                foreach (var node in nodes)
                {
                    //c站有一种特殊的图片用于加载一些代码,这里不显示它
                    var width = node.Attributes["width"];
                    if (width != null && width.Value == "0")
                        continue;
                    string url = node.Attributes["src"].Value;
                    if (url[0] == '/')
                        url = url.Insert(0, @"http://www.tucao.tv");
                    Image img = new Image();
                    img.MaxHeight = 160;
                    img.MaxWidth = 200;
                    img.Margin = new Thickness(10, 10, 10, 10);
                    img.Source = new BitmapImage(new Uri(url));
                    MenuFlyout flyoutmenu = new MenuFlyout();
                    MenuFlyoutItem item = new MenuFlyoutItem();
                    item.Text = "保存图片";
                    item.Tag = url;
                    item.Click += SaveImg_Click;
                    flyoutmenu.Items.Add(item);
                    img.ContextFlyout = flyoutmenu;
                    Description.Children.Add(img);
                }
            List<PartInfo> parts = new List<PartInfo>();
            foreach (var p in info.Video)
            {
                i++;
                parts.Add(new PartInfo() {PartNumber=i, PartTitle = p["title"].ToString(), SourceType = p["type"].ToString() });
            }
            for (i = 0; i < parts.Count; i++)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    ((ObservableCollection<PartInfo>)PartList.ItemsSource).Add(parts[i]);
                });
            }
            LinkTest();
        }
        /// <summary>
        /// 测试是否能够获取视频地址
        /// </summary>
        async void LinkTest()
        {
            List<string> url = new List<string>();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
            {

                var items = PartList.ItemsSource as ObservableCollection<PartInfo>;
                for (int partNum = 0; partNum < items.Count; partNum++)
                {
                    url = await info.GetPlayUrl(partNum);
                    if (url.Count == 0)
                        items[partNum].LinkDetectorColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xAA, 0xAA, 0xAA));
                    else
                    {
                        items[partNum].LinkDetectorColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x33, 0x66));
                        if (!url[0].Contains("/"))
                        {
                            items[partNum].SourceType = "已下载";
                        }
                    }
                }
            });
        }
        /// <summary>
        /// 离开页面时
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                App.ResetPageCache();
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
        /// <summary>
        /// 保存图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SaveImg_Click(object sender, RoutedEventArgs e)
        {
            string s = (sender as MenuFlyoutItem).Tag.ToString();
            var uri = new Uri(s);
            string filename = uri.Segments[uri.Segments.Length - 1];
            Windows.Web.Http.HttpClient http = new Windows.Web.Http.HttpClient();
            var stream = await http.GetInputStreamAsync(uri);
            StorageFile destinationFile = await KnownFolders.SavedPictures.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);

            using (var destinationStream = await destinationFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (var destinationOutputStream = destinationStream.GetOutputStreamAt(0))
                {
                    await RandomAccessStream.CopyAndCloseAsync(stream, destinationStream);
                }
            }
            Link.ShowToast("图片已保存到"+ KnownFolders.SavedPictures.Path);
        }
        /// <summary>
        /// 点击一个分p时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            List<string> url = new List<string>();
            //获取被点击的分p
            PartInfo clickedItem = e.ClickedItem as PartInfo;
            //获取视频地址;
            url = await info.GetPlayUrl(clickedItem.PartNumber - 1);
            if (url.Count < 1)
            {
                Link.ShowToast("获取视频播放地址失败,请稍后再试");
                return;
            }
            //打开播放器
            var param = new MediaPlayer.MediaPlayerSource();
            param.Hid = info.Hid;
            param.Title = info.Title;
            param.PartTitle = clickedItem.PartTitle;
            param.PlayList = url;
            //本地文件夹禁止有/字符.而网页url绝对有这个字符
            param.IsLocalFile = !url[0].Contains("/");
            param.Part = clickedItem.PartNumber - 1;
            param.Tid = info.TypeId;
            App.Link.Navigate(typeof(MediaPlayer), param, new DrillInNavigationTransitionInfo());
        }
        /// <summary>
        /// 选择下载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Download_Tapped(object sender, TappedRoutedEventArgs e)
        {
            PartList.IsItemClickEnabled = false;
            PartList.SelectionMode = ListViewSelectionMode.Multiple;
            DetailCommandBar.Visibility = Visibility.Collapsed;
            DownloadCommandBar.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// 关闭下载菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancel_Tapped(object sender, TappedRoutedEventArgs e)
        {

            PartList.IsItemClickEnabled = true;
            PartList.SelectionMode = ListViewSelectionMode.None;
            DetailCommandBar.Visibility = Visibility.Visible;
            DownloadCommandBar.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        ///选中分p后点确定开始下载视频
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OK_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var items = PartList.SelectedItems;
            int i=0;
            foreach (PartInfo item in items)
            {
                if (item.SourceType == "已下载") continue;
                await DownloadHelper.Download(info, item.PartNumber);
                i++;
            }
            if (i <1)
            {
                Link.ShowToast("未选择任何项");
                return;
            }
            Link.ShowToast("尝试下载" + i + "个视频");
            Cancel_Tapped(Cancel, new TappedRoutedEventArgs());
        }
        /// <summary>
        /// 评论列表没有的时候尝试获取
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void Pivot_PivotItemLoading(Pivot sender, PivotItemEventArgs args)
        {
            TextBlock header = args.Item.Header as TextBlock;
            if (header.Text == "评论")
            {
                await CommandBars.Offset(offsetY: 48).StartAsync();
                CommandBars.Visibility = Visibility.Collapsed;
                if (Commentpage == 0)
                {
                    Commentpage++;
                    LoadComment();
                }
            }
            else
            {
                CommandBars.Visibility = Visibility.Visible;
                await CommandBars.Offset(offsetY: 0).StartAsync();
            }
        }
        /// <summary>
        /// 加载一页评论
        /// </summary>
        async void LoadComment()
        {
            var comments = await Tucao.Content.GetComment(info.TypeId, info.Hid, Commentpage, Tucao.Content.Order.New);
            foreach (Comment comment in comments)
            {
                (Comment.ItemsSource as ObservableCollection<Comment>).Add(comment);
            }
            if ((Comment.ItemsSource as ObservableCollection<Comment>).Count == 0)
            {
                BottomText.Text = "还没有人评论过";
                BottomText.Tapped -= BottomText_Tapped;
            }
            else if ((Comment.ItemsSource as ObservableCollection<Comment>).Count % 20 == 0 && comments.Count != 0)
            {
                BottomText.Text = "加载下一页";
                BottomText.Tapped += BottomText_Tapped;
            }
            else
            {
                BottomText.Text = "下面没了";
                BottomText.Tapped -= BottomText_Tapped;
            }
            BottomText.Visibility = Visibility.Visible;
            LoadingProgress.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// 点击加载下一页评论
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BottomText_Tapped(object sender, TappedRoutedEventArgs e)
        {
            BottomText.Visibility = Visibility.Collapsed;
            LoadingProgress.Visibility = Visibility.Visible;
            Commentpage++;
            LoadComment();
        }
        /// <summary>
        /// 用户头像加载失败时换成错误的图标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BitmapImage_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var img = sender as BitmapImage;
            img.UriSource = new Uri("ms-appx:///Assets/nophoto.gif");
        }
        public class PartInfo : INotifyPropertyChanged
        {
            public PartInfo() { }
            public int PartNumber { get; set; }
            public string PartTitle { get; set; }
            string sourceType;
            public string SourceType {
                get
                {
                    return sourceType;
                }
                set
                {
                    sourceType = value;
                    OnPropertyChanged(nameof(SourceType));
                }
            }
            SolidColorBrush linkDetectorColor;
            public SolidColorBrush LinkDetectorColor
            {
                get
                {
                    return linkDetectorColor;
                }
                set
                {
                    linkDetectorColor = value;
                    OnPropertyChanged(nameof(LinkDetectorColor));
                }
            }
            public event PropertyChangedEventHandler PropertyChanged;
            void OnPropertyChanged(string name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
