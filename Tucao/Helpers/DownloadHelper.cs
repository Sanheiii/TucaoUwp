﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tucao.View;
using Windows.Data.Json;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
namespace Tucao.Helpers
{
    class DownloadHelper
    {
        //下载hid和分P
        static public async Task Download(VideoInfo info, int partNumber)
        {
            try
            {
                //获取视频地址
                List<string> url_list = new List<string>();
                Hashtable part = info.Video[partNumber - 1];
                if (part["type"].ToString() == "video")
                    url_list.Add(part["file"].ToString());
                else
                {
                    url_list = await info.GetPlayUrl(partNumber - 1);
                }
                if (url_list.Count == 0)
                {
                    return;
                }
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
                StorageFolder folder = await localfolder.CreateFolderAsync(partNumber.ToString(), CreationCollisionOption.OpenIfExists);
                //保存分P信息
                {
                    int fileCount=0;
                    while (fileCount < url_list.Count)
                    {
                        var file = await folder.TryGetItemAsync(fileCount.ToString());
                        if (file == null) break;
                        var properties = await file.GetBasicPropertiesAsync();
                        if (properties.Size == 0)
                        {
                            break;
                        }
                        fileCount++;
                    }
                    //如果文件数正确说明下载过了
                    if (fileCount<url_list.Count)
                    {
                        //下载
                        while(fileCount<url_list.Count)
                        {
                            DownloadOperation d = await DownloadFile(url_list[fileCount], fileCount.ToString(), folder);
                            //开始下载
                            d.StartAsync();
                            fileCount++;
                        }
                        StorageFile file = await folder.CreateFileAsync("part.json", CreationCollisionOption.OpenIfExists);
                        JsonObject json = new JsonObject
                        {
                            { "title",JsonValue.CreateStringValue(info.Video[partNumber-1]["title"].ToString())},
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
                        //下载弹幕文件
                        StorageFile danmakuFile = await folder.CreateFileAsync("danmakus.xml", CreationCollisionOption.OpenIfExists);
                        Hashtable param = new Hashtable();
                        {
                            param.Add("m", "mukio");
                            param.Add("c", "index");
                            param.Add("a", "init");
                            param.Add("playerID", "11-" + info.Hid + "-1-" + (partNumber-1) + "");
                        }
                        var result = await Methods.HttpGetAsync("http://www.tucao.one/index.php", param);
                        var danmakus =await result.Content.ReadAsStringAsync();
                        //打开文件
                        Stream danmakuFileStream = await danmakuFile.OpenStreamForWriteAsync();
                        StreamWriter danmakuFileWriter = new StreamWriter(danmakuFileStream);
                        //写入
                        await danmakuFileWriter.WriteAsync(danmakus);
                        //关闭文件
                        danmakuFileWriter.Dispose();
                        danmakuFileStream.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Link.ShowToast(ex.Message);
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
                Link.ShowToast(ex.Message);
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
                        status = "检测到计费网络";
                        break;
                    case BackgroundTransferStatus.PausedNoNetwork:
                        status = "来到了没有网络的世界";
                        break;
                    case BackgroundTransferStatus.Completed:
                        status = "已完成";
                        break;
                    case BackgroundTransferStatus.Canceled:
                        status = "已取消";
                        break;
                    case BackgroundTransferStatus.Error:
                        status = "下载错误";
                        break;
                    case BackgroundTransferStatus.PausedSystemPolicy:
                        status = "受到系统限制";
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
