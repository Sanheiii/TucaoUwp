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
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using Windows.Storage;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage.Streams;
using Windows.Data.Json;
using Tucao.Helpers;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Tucao
{

    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DownloadList : Page
    {
        ObservableCollection<Transfer> transfers = new ObservableCollection<Transfer>();
        CancellationTokenSource cts = new CancellationTokenSource();
        public DownloadList()
        {
            this.InitializeComponent();
            this.List.ItemsSource = transfers;
            DiscoverDownloadsAsync();
        }
        /// <summary>
        /// 加载下载任务
        /// </summary>
        private async Task DiscoverDownloadsAsync()
        {
            IReadOnlyList<DownloadOperation> downloads = null;
            try
            {
                downloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
                if (downloads.Count > 0)
                {
                    List<Task> tasks = new List<Task>();
                    foreach (DownloadOperation download in downloads)
                    {
                        tasks.Add(HandleDownloadAsync(download));
                    }
                    await Task.WhenAll(tasks);
                }
                else
                {

                }
            }
            catch (Exception e)
            {
                Helpers.ErrorHelper.PopUp(e.Message);
            }
        }
        private async Task HandleDownloadAsync(DownloadOperation download)
        {
            try
            {
                string name = download.ResultFile.Name.Split('.')[0];
                Transfer transfer = new Transfer
                {
                    DownOpration = download,
                    Size = download.Progress.BytesReceived.ToString(),
                    TotalSize = download.Progress.TotalBytesToReceive.ToString(),
                    Process = 0,
                    Name = name
                };
                transfers.Add(transfer);
                DownloadProgress(download);
                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(DownloadProgress);
                await download.AttachAsync().AsTask(cts.Token, progressCallback);
            }
            catch (TaskCanceledException)
            {
                Helpers.ErrorHelper.PopUp("取消： " + download.Guid);
            }
        }

        /// <summary>
        /// 监视下载进度
        /// </summary>
        private void DownloadProgress(DownloadOperation download)
        {
            try
            {
                Transfer transfer = transfers.First(p => p.DownOpration == download);
                transfer.Size = download.Progress.BytesReceived.ToString();
                transfer.TotalSize = download.Progress.TotalBytesToReceive.ToString();
                transfer.Process = ((double)download.Progress.BytesReceived / download.Progress.TotalBytesToReceive) * 100;
                transfer.Status = download.Progress.Status.ToString();

                if (download.Progress.BytesReceived == download.Progress.TotalBytesToReceive && download.Progress.BytesReceived > 0)
                {
                    var a = this.List.Items.ToList();
                    var item = a.Find((p => ((Transfer)p).DownOpration == download)) as Transfer;
                    transfers.Remove(item);
                }
            }
            catch (Exception e)
            {
                Helpers.ErrorHelper.PopUp(e.Message);
            }
        }
        private void OpenDownloadFolder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Windows.System.Launcher.LaunchFolderAsync(ApplicationData.Current.LocalCacheFolder);
        }

        private async void List_ItemClick(object sender, ItemClickEventArgs e)
        {
            Transfer model = (Transfer)e.ClickedItem;
            if (Delete.IsChecked == true)
            {
                model.CTS.Cancel(false);
                model.CTS.Dispose();
                try
                {
                    // 删除目标文件
                    model.DownOpration.AttachAsync().Cancel();
                    await model.DownOpration.ResultFile.DeleteAsync();
                    model.CTS.Dispose();
                    transfers.Remove(model);
                    var folder = ApplicationData.Current.LocalCacheFolder;
                }
                catch (Exception ex)
                {
                    Helpers.ErrorHelper.PopUp(ex.Message);
                }
            }
            else
            {
                if (model.DownOpration.Progress.Status == BackgroundTransferStatus.Running)
                {
                    try
                    {
                        model.DownOpration.Pause();
                    }
                    catch { };
                }
                else
                {
                    try
                    {
                        model.DownOpration.Resume();
                    }
                    catch { };
                }
            }
        }
        public void Dispose()
        {
            if (cts != null)
            {
                cts.Dispose();
                cts = null;
            }
            GC.SuppressFinalize(this);
        }
    }
}
