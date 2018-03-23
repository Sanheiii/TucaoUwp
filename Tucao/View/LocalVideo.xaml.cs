using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Animation;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;

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
            Task.Run(LoadItems);
        }
        private async Task LoadItems()
        {
            //下载目录
            StorageFolder downloadfolder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("Download", CreationCollisionOption.OpenIfExists);
            //获取目录下
            var folders = await downloadfolder.GetFoldersAsync();
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
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                    {
                        (Videos.ItemsSource as ObservableCollection<Video>).Add(v);
                    });
                }
            }
        }
        private async Task<ObservableCollection<Part>> LoadParts(StorageFolder storage)
        {
            ObservableCollection<Part> parts = new ObservableCollection<Part>();
            var folders = await storage.GetFoldersAsync();
            int c = 1;
            foreach (StorageFolder folder in folders)
            {
                if (await folder.TryGetItemAsync("part.json") != null)
                {
                    Part p = new Part();
                    StorageFile jsonfile = await folder.GetFileAsync("part.json");
                    Stream stream = await jsonfile.OpenStreamForReadAsync();
                    StreamReader reader = new StreamReader(stream);
                    string str = await reader.ReadToEndAsync();
                    reader.Dispose();
                    stream.Dispose();
                    JsonObject json = JsonObject.Parse(str);
                    p.partTitle =c+":"+ json["title"].GetString();
                    ulong size = 0;
                    List<string> uri = new List<string>();
                    for (int i = 0; i < json["filecount"].GetNumber(); i++)
                    {
                        if (await folder.TryGetItemAsync(i.ToString()) != null)
                        {
                            StorageFile file = await folder.GetFileAsync(i.ToString());
                            size += (await file.GetBasicPropertiesAsync()).Size;
                            uri.Add(file.Path);
                        }
                    }
                    foreach (StorageFile file in await folder.GetFilesAsync())
                    {

                    }
                    p.size = $"{(((double)size / 1024 / 1024)).ToString("0.0")}M";
                    p.uri = uri;
                    parts.Add(p);
                }
                c++;
            }
            return parts;
        }
        //点击一个视频
        private void Videos_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame root = Window.Current.Content as Frame;
            root.Navigate(typeof(MainPage), (e.ClickedItem as Video).hid, new DrillInNavigationTransitionInfo());
        }
        //点击一个分P
        private void Parts_ItemClick(object sender, ItemClickEventArgs e)
        {

        }
        //打开缓存文件夹
        private void OpenDownloadFolder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Windows.System.Launcher.LaunchFolderAsync(ApplicationData.Current.LocalCacheFolder);
        }
        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            Frame root = Window.Current.Content as Frame;
            root.Navigate(typeof(DownloadList), null, new DrillInNavigationTransitionInfo());
        }
        public class Video
        {
            public string hid { get; set; }
            public string title { get; set; }
            public ObservableCollection<Part> parts { get; set; }
        }
        public class Part
        {
            public string partTitle { get; set; }
            public string size { get; set; }
            public List<string> uri { get; set; }
        }

    }
}
