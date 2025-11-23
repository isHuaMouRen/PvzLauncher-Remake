using HuaZi.Library.Downloader;
using HuaZi.Library.Json;
using ModernWpf.Controls;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static PvzLauncherRemake.Class.AppLogger;

namespace PvzLauncherRemake.Utils
{
    public static class Updater
    {
        public static JsonUpdateIndex.Index UpdateIndex = null!;
        public static string LatestVersion = null!;
        public static string Url = null!;
        public static string SavePath = Path.Combine(AppInfo.TempDiectory, "PVZLAUNCHERUPDATECACHE.zip");
        public static bool isUpdate = false;
        public static HttpClient Client = new HttpClient();

        public static async Task CheckUpdate(Action<double,double> progressCallback = null!)
        {
            logger.Info("开始检测更新...");
            //获取主索引
            UpdateIndex = Json.ReadJson<JsonUpdateIndex.Index>(await Client.GetStringAsync(AppInfo.UpdateIndexUrl));

            //判断更新通道
            switch (AppInfo.Config.LauncherConfig.UpdateChannel)
            {
                case "Stable":
                    LatestVersion = UpdateIndex.Stable.LatestVersion; Url = UpdateIndex.Stable.Url; break;
                case "Development":
                    LatestVersion = UpdateIndex.Development.LatestVersion; Url = UpdateIndex.Development.Url; break;
            }
            logger.Info($"当前更新通道: {AppInfo.Config.LauncherConfig.UpdateChannel}\n最新版本: {LatestVersion}\nURL: {Url}");

            //判断版本
            if (AppInfo.Version != LatestVersion)
            {
                await DialogManager.ShowDialogAsync(new ContentDialog
                {
                    Title = "发现可用更新",
                    Content = $"当前版本: {AppInfo.Version}\n最新版本: {LatestVersion}\n\n您现在可升级到最新的 {LatestVersion} 版本",
                    PrimaryButtonText = "立即更新",
                    CloseButtonText = "取消更新",
                    DefaultButton = ContentDialogButton.Primary
                }, (() => isUpdate = true));
            }
            else
            {
                await DialogManager.ShowDialogAsync(new ContentDialog
                {
                    Title = "无可用更新",
                    Content = $"您使用的已经是最新版本 {AppInfo.Version} , 无需更新!",
                    PrimaryButtonText = "确定",
                    DefaultButton = ContentDialogButton.Primary
                });
            }


            //不更新直接return
            if (!isUpdate)
                return;

            //开始更新
            bool? done = null;
            string errorMessage = null!;

            var downloader = new Downloader
            {
                Url = Url,
                SavePath = SavePath,
                Completed = ((s, e) =>
                {
                    if (s)
                        done = true;
                    else
                    {
                        done = false;
                        errorMessage = e!;
                    }
                }),
                Progress = progressCallback
            };

            downloader.StartDownload();

            //等待下载完毕
            while (done == null)
                await Task.Delay(1000);
            //如下载失败抛错误
            if (done == false)
                throw new Exception(errorMessage);
            //下载成功↓
            //运行更新服务
            Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(AppInfo.ExecuteDirectory, "UpdateService"),
                UseShellExecute = true
            });

            Environment.Exit(0);
        }
    }
}
