using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
    public sealed partial class History : Page
    {
        public History()
        {
            this.InitializeComponent();
            LoadHistory();
        }
        async void LoadHistory()
        {
            var i=new ObservableCollection<ViewModel.History>();
            //打开文件
            var folder = ApplicationData.Current.LocalCacheFolder;
            StorageFile file = await folder.CreateFileAsync("history.json", CreationCollisionOption.OpenIfExists);
            Stream stream = await file.OpenStreamForReadAsync();
            //读取json
            StreamReader reader = new StreamReader(stream);
            string str = reader.ReadToEnd();
            reader.Dispose();
            stream.Dispose();
            JsonArray jsons = new JsonArray();
            JsonArray.TryParse(str, out jsons);
            foreach (var j in jsons)
            {
                var json = j.GetObject();
                var history = new ViewModel.History()
                {
                    Hid = json["hid"].GetString(),
                    Title = json["title"].GetString(),
                    Part = (int)json["part"].GetNumber(),
                    Position = json["position"].GetNumber()
                };
                i.Add(history);
            }
        }
    }
}
