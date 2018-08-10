using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
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
            List.ItemsSource = new ObservableCollection<ViewModel.History>();
            //打开文件
            var folder = ApplicationData.Current.LocalCacheFolder;
            StorageFile file = await folder.CreateFileAsync("history.xml", CreationCollisionOption.OpenIfExists);
            Stream stream = await file.OpenStreamForReadAsync();
            StreamReader reader = new StreamReader(stream);
            string str = reader.ReadToEnd();
            reader.Dispose();
            stream.Dispose();
            //读取xml
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
                var position = TimeSpan.FromSeconds(double.Parse(element.Attribute("position").Value));
                var history = new ViewModel.History()
                {
                    Hid = element.Attribute("hid").Value,
                    Title = element.Value,
                    Part = int.Parse(element.Attribute("part").Value)+1,
                    Position = (position.Hours * 60 + position.Minutes).ToString("D2") + ':' + position.Seconds.ToString("D2"),
                    Time = long.Parse(element.Attribute("time").Value)
                };
                (List.ItemsSource as ObservableCollection<ViewModel.History>).Add(history);
            }
        }

        private void List_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (e.ClickedItem as ViewModel.History);
            App.OpenVideo(item.Hid);
        }
    }
}
