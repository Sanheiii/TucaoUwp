using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
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
    public sealed partial class LocalVideo : Page
    {
        public LocalVideo()
        {
            this.InitializeComponent();
        }
        //导航到该页时调用
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Videos.ItemsSource = new ObservableCollection<Video>();
            LoadItems();
        }
        private async Task LoadItems()
        {
            ObservableCollection<Video> items = new ObservableCollection<Video>();
            //下载目录
            StorageFolder downloadfolder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("Download", CreationCollisionOption.OpenIfExists);
            //获取目录下
            var folders = await downloadfolder.GetFoldersAsync();
            int count = folders.Count;
            foreach (var folder in folders)
            {
                if (await folder.TryGetItemAsync("info.json") != null)
                {
                    Video v = new Video();
                    StorageFile file = await folder.GetFileAsync("info.json");
                    Stream stream = await file.OpenStreamForReadAsync();
                    StreamReader reader = new StreamReader(stream);
                    string str = await reader.ReadToEndAsync();
                    JsonObject json = JsonObject.Parse(str);
                    v.hid = json["hid"].GetString();
                    v.title = json["title"].GetString();
                    v.parts = await LoadParts(folder);
                    //没有下载完成的分p就不显示标题
                    if (v.parts.Count == 0) continue;
                    items.Add(v);
                }
            }
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                    {
                        Videos.ItemsSource = items;
                        LoadProgress.Visibility = Visibility.Collapsed;
                        Delete.IsEnabled = true;
                    });
        }
        private async Task<ObservableCollection<Part>> LoadParts(StorageFolder storage)
        {
            ObservableCollection<Part> parts = new ObservableCollection<Part>();
            var folders = await storage.GetFoldersAsync();
            foreach (StorageFolder folder in folders)
            {
                if (await folder.TryGetItemAsync("part.json") != null)
                {
                    int i;
                    Part p = new Part();
                    StorageFile jsonfile = await folder.GetFileAsync("part.json");
                    Stream stream = await jsonfile.OpenStreamForReadAsync();
                    StreamReader reader = new StreamReader(stream);
                    string str = await reader.ReadToEndAsync();
                    reader.Dispose();
                    stream.Dispose();
                    JsonObject json = JsonObject.Parse(str);
                    p.hid = storage.Name;
                    p.partNum = int.Parse(folder.Name) - 1;
                    p.partTitle = json["title"].GetString();
                    ulong size = 0;
                    List<string> playlist = new List<string>();
                    for (i = 0; i < json["filecount"].GetNumber(); i++)
                    {
                        if (await folder.TryGetItemAsync(i.ToString()) != null)
                        {
                            StorageFile file = await folder.GetFileAsync(i.ToString());
                            var filesize = (await file.GetBasicPropertiesAsync()).Size;
                            //文件大小为0时不继续判断
                            if (filesize == 0) break;
                            size += filesize;
                            playlist.Add(file.Path);
                        }
                    }
                    //有分段视频没有下载完不添加这个分p
                    if (i < json["filecount"].GetNumber()) continue;
                    p.size = $"{(((double)size / 1024 / 1024)).ToString("0.0")}M";
                    p.play_list = playlist;
                    parts.Add(p);
                }
            }
            return parts;
        }
        //点击一个视频
        private void Videos_ItemClick(object sender, ItemClickEventArgs e)
        {
            App.OpenVideo((e.ClickedItem as Video).hid);
        }
        //点击一个分P
        private void Parts_ItemClick(object sender, ItemClickEventArgs e)
        {
            var part = e.ClickedItem as Part;
            var param = new MediaPlayer.MediaPlayerSource();
            param.Title = (string)(sender as ListView).Tag;
            param.PartTitle = part.partTitle;
            param.PlayList = part.play_list;
            param.IsLocalFile = true;
            param.Hid = part.hid;
            param.Part = part.partNum;
            Frame root = Window.Current.Content as Frame;
            App.Link.Navigate(typeof(MediaPlayer), param, new DrillInNavigationTransitionInfo());
        }
        //打开缓存文件夹
        private async void OpenDownloadFolder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchFolderAsync(ApplicationData.Current.LocalCacheFolder);
        }
        public class Video
        {
            public string hid { get; set; }
            public string title { get; set; }
            public ObservableCollection<Part> parts { get; set; }
        }
        public class Part
        {
            public string hid { get; set; }
            public int partNum { get; set; }
            public string partTitle { get; set; }
            public string size { get; set; }
            public List<string> play_list { get; set; }
        }

        private void Delete_Tapped(object sender, TappedRoutedEventArgs e)
        {
            foreach (ListViewItem v in Videos.ItemsPanelRoot.Children)
            {
                ListView parts = ((v.ContentTemplateRoot as Grid).Children[0] as Grid).Children[1] as ListView;
                parts.SelectionMode = ListViewSelectionMode.Multiple;
                parts.IsItemClickEnabled = false;
            }
            Delete.Visibility = Visibility.Collapsed;
            OpenDownloadFolder.Visibility = Visibility.Collapsed;
            OK.Visibility = Visibility.Visible;
            Cancel.Visibility = Visibility.Visible;
        }

        private void Cancel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            foreach (ListViewItem v in Videos.ItemsPanelRoot.Children)
            {
                ListView parts = ((v.ContentTemplateRoot as Grid).Children[0] as Grid).Children[1] as ListView;
                parts.SelectionMode = ListViewSelectionMode.None;
                parts.IsItemClickEnabled = true;
            }
            Delete.Visibility = Visibility.Visible;
            OpenDownloadFolder.Visibility = Visibility.Visible;
            OK.Visibility = Visibility.Collapsed;
            Cancel.Visibility = Visibility.Collapsed;
        }

        private async void OK_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //倒着删,正着删会使\影响列表其他项目导致出错
            for (int c = Videos.ItemsPanelRoot.Children.Count - 1; c >= 0; c--)
            {
                ListViewItem v = Videos.ItemsPanelRoot.Children[c] as ListViewItem;
                ListView parts = ((v.ContentTemplateRoot as Grid).Children[0] as Grid).Children[1] as ListView;
                for (int i = parts.SelectedItems.Count - 1; i >= 0; i--)
                {
                    Part part = parts.SelectedItems[i] as Part;
                    var folderpath = part.play_list[0].Remove(part.play_list[0].LastIndexOf("\\"));
                    var folder = await StorageFolder.GetFolderFromPathAsync(folderpath);
                    var parent = await folder.GetParentAsync();
                    //删除文件夹
                    await folder.DeleteAsync();
                    var items = (parts.ItemsSource as ObservableCollection<Part>);
                    items.Remove(part);
                    if (items.Count == 0)
                    {
                        (Videos.ItemsSource as ObservableCollection<Video>).Remove(parts.DataContext as Video);
                    }
                    //父文件夹没有文件夹时删除它
                    int count = (await parent.GetFoldersAsync()).Count;
                    if (count == 0)
                    {
                        await parent.DeleteAsync();
                    }
                }
            }
            //完成操作后关闭删除模式
            Cancel_Tapped(Cancel, new TappedRoutedEventArgs());
        }
    }
}
