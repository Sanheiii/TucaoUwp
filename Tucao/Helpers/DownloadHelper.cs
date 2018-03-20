using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tucao.Http;
using Windows.Data.Json;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Collections.Generic;
using Windows.Networking.BackgroundTransfer;
using Windows.Web.Http;
using Tucao.Content;
namespace Tucao.Helpers
{
    class DownloadHelper
    {
        //下载hid和分P
        static public async void Download(SubmissionInfo info, int part)
        {
            try
            {
                //软件的临时文件夹\LocalCache\Download\视频的HID
                StorageFolder localfolder = await (await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("Download", CreationCollisionOption.OpenIfExists)).CreateFolderAsync(info.Hid, CreationCollisionOption.OpenIfExists);
                //保存视频信息
                {
                    //上面那个文件夹\info.json   用于存储视频信息
                    if (await localfolder.TryGetItemAsync("info.json") == null)
                    {
                        StorageFile file = await localfolder.CreateFileAsync("info.json", CreationCollisionOption.OpenIfExists);
                        //创建json
                        JsonObject json = new JsonObject
                        {
                            { "hid", JsonValue.CreateStringValue(info.Hid) },
                            { "title", JsonValue.CreateStringValue(info.Title) },
                        };
                        //打开文件
                        Stream file1 = await file.OpenStreamForWriteAsync();
                        StreamWriter writer = new StreamWriter(file1);
                        //写入
                        await writer.WriteAsync(json.ToString());
                        //关闭文件
                        writer.Dispose();
                        file1.Dispose();
                    }
                }
                //上面那个文件夹\分p号
                StorageFolder folder = await localfolder.CreateFolderAsync(part.ToString(), CreationCollisionOption.OpenIfExists);
                //保存分P信息
                {
                    //如果有part.json说明下载过了
                    if (await folder.TryGetItemAsync("part.json") == null)
                    {
                        int i = 0;
                        //获取视频地址
                        var url_list = await info.GetPlayUrl(part);
                        //下载
                        if (url_list.Count == 0)
                        {
                            var parent = await folder.GetParentAsync();
                            //删除文件夹
                            await folder.DeleteAsync();
                            //父文件夹没有文件夹时删除它
                            int count = (await parent.GetFoldersAsync()).Count;
                            if (count == 0)
                            {
                                await parent.DeleteAsync();
                            }
                            return;
                        }
                        foreach (var url in url_list)
                        {
                            DownloadOperation d = await DownloadHelper.DownloadFile(url, i.ToString(), folder);
                            //开始下载
                            d.StartAsync();
                            i++;
                        }
                        StorageFile file = await folder.CreateFileAsync("part.json", CreationCollisionOption.OpenIfExists);
                        JsonObject json = new JsonObject
                        {
                            { "title",JsonValue.CreateStringValue(info.Video[part-1]["title"].ToString())},
                            { "filecount",JsonValue.CreateNumberValue(url_list.Count) }
                        };
                        //打开文件
                        Stream file1 = await file.OpenStreamForWriteAsync();
                        StreamWriter writer = new StreamWriter(file1);
                        //写入
                        await writer.WriteAsync(json.ToString());
                        //关闭文件
                        writer.Dispose();
                        file1.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHelper.PopUp(ex.Message);
            };
        }
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="url">下载地址</param>
        /// <param name="filename">文件名</param>
        /// <param name="folder">下载到的文件夹</param>
        static public async Task<DownloadOperation> DownloadFile(string url, string filename, StorageFolder folder)
        {
            try
            {
                BackgroundDownloader downloader = new BackgroundDownloader();
                // 获取文件夹权限
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(folder);
                //创建文件
                StorageFile file = await folder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
                DownloadOperation download = downloader.CreateDownload(new Uri(url), file);
                //设置网络限制(现在允许付费网络下载
                download.CostPolicy = BackgroundTransferCostPolicy.Default;
                return download;
            }
            catch (Exception ex)
            {
                ErrorHelper.PopUp(ex.Message);
                return null;
            }
        }
    }
    /// <summary>
    /// Handler
    /// </summary>
    public class Transfer : INotifyPropertyChanged
    {
        // 操作对象
        public CancellationTokenSource CTS = new CancellationTokenSource();
        DownloadOperation down;
        public DownloadOperation DownOpration
        {
            get { return down; }
            set { down = value; }
        }

        // 进度
        double process;
        public double Process
        {
            get { return double.Parse(process.ToString("0.0")); }
            set
            {
                process = value;
                OnPropertyChanged(nameof(Process));
            }
        }

        // 总大小
        string totalSize;
        public string TotalSize
        {
            get
            {
                return $"{(((double)(ulong.Parse(totalSize)) / 1024 / 1024)).ToString("0.0")}M";
            }
            set
            {
                totalSize = value;
                OnPropertyChanged(nameof(TotalSize));
            }
        }

        // GUID
        public string Guid { get { return down.Guid.ToString(); } }

        // 状态
        string status;
        public string Status
        {
            get { return status; }
            set
            {
                switch (down.Progress.Status)
                {
                    case BackgroundTransferStatus.Idle:
                        status = "空闲中";
                        break;
                    case BackgroundTransferStatus.Running:
                        status = "下载中";
                        break;
                    case BackgroundTransferStatus.PausedByApplication:
                        status = "已暂停";
                        break;
                    case BackgroundTransferStatus.PausedCostedNetwork:
                        status = "因网络暂停";
                        break;
                    case BackgroundTransferStatus.PausedNoNetwork:
                        status = "没有连接至网络";
                        break;
                    case BackgroundTransferStatus.Completed:
                        status = "完成";
                        break;
                    case BackgroundTransferStatus.Canceled:
                        status = "取消";
                        break;
                    case BackgroundTransferStatus.Error:
                        status = "下载错误";
                        break;
                    case BackgroundTransferStatus.PausedSystemPolicy:
                        status = "因系统问题暂停";
                        break;
                    default:
                        status = "未知";
                        break;
                }
                OnPropertyChanged(nameof(Status));
            }
        }

        // 已下载大小
        string size;
        public string Size
        {
            get
            {
                return $"{(((double)(ulong.Parse(size)) / 1024 / 1024)).ToString("0.0")}M";
            }
            set
            {
                size = value;
                OnPropertyChanged(nameof(Size));
            }
        }
        // 名称
        public string Name { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
