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
                    Stream stream =await file.OpenStreamForReadAsync();
                    BinaryReader reader = new BinaryReader(stream);
                    string str=reader.ReadString();
                    JsonObject json = JsonObject.Parse(str);
                    v.hid = json["hid"].ToString();
                    v.title = json["title"].ToString();
                    v.parts = await LoadParts(folder);
                    (Videos.ItemsSource as ObservableCollection<Video>).Add(v);
                }
            }
        }
        private async Task<ObservableCollection<Part>> LoadParts(StorageFolder storage)
        {
            ObservableCollection<Part> parts = new ObservableCollection<Part>();
            var folders = await storage.GetFoldersAsync();
            foreach(var folder in folders)
            {
                if (await folder.TryGetItemAsync("part.json") != null)
                {
                    Part p = new Part();
                    StorageFile file = await folder.GetFileAsync("part.json");
                    Stream stream = await file.OpenStreamForReadAsync();
                    BinaryReader reader = new BinaryReader(stream);
                    string str = reader.ReadString();
                    JsonObject json = JsonObject.Parse(str);
                    p.partTitle = json["title"].ToString();
                    ulong size;
                    List<string> uri = new List<string>();
                    for(int i=0;i<json["filecount"].GetNumber();i++)
                    {

                    }
                    p.uri=
                    parts.Add(p);
                }
            }
            return parts;
        }
        //点击一个视频
        private void Videos_ItemClick(object sender, ItemClickEventArgs e)
        {

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
        public class Video
        {
            public string hid { get; set; }
            public string title { get; set; }
            public ObservableCollection<Part> parts { get; set; }
        }
        public class Part
        {
            public string partTitle { get; set; }
            public ulong size { get; set; }
            public List<string> uri { get; set; }
        }

    }
}
