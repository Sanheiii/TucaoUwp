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
using Windows.Web.Http;
namespace Tucao.Helpers
{
    class DownloadHelper
    {
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="url">下载地址</param>
        /// <param name="filename">文件名</param>
        /// <param name="folder">下载到的文件夹</param>
        static public async Task<DownloadOperation> Download(string url, string filename, StorageFolder folder)
        {
            try
            {
                BackgroundDownloader downloader = new BackgroundDownloader();
                // 获取文件夹权限
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(folder);
                //创建文件
                StorageFile file = await folder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
                DownloadOperation download = downloader.CreateDownload(new Uri(url), file);
                //设置流量限制(现在允许
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
}
