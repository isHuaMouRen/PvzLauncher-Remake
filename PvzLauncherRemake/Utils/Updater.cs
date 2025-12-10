using HuaZi.Library.Downloader;
using HuaZi.Library.Json;
using MdXaml;
using ModernWpf.Controls;
using Notifications.Wpf;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows.Controls;
using static PvzLauncherRemake.Class.AppLogger;

namespace PvzLauncherRemake.Utils
{
    public static class Updater
    {
        public static JsonUpdateIndex.Index UpdateIndex = null!;
        public static HttpClient Client = new HttpClient();

        public static string LatestVersion = null!;
        public static string ChangeLog = null!;
        public static string Url = null!;
        public static string SavePath = Path.Combine(AppInfo.TempDiectory, "PVZLAUNCHERUPDATECACHE");

        public static bool isUpdate = false;

        public static async Task CheckUpdate(Action<double, double> progressCallback = null!, bool isStartUp = false)
        {
            logger.Info($"[更新器] 开始检测更新");
            //获取主索引
            string indexString = await Client.GetStringAsync(AppInfo.UpdateIndexUrl);
            logger.Info($"[更新器] 获取更新索引: {indexString}");
            UpdateIndex = Json.ReadJson<JsonUpdateIndex.Index>(indexString);

            //判断更新通道
            logger.Info($"[更新器] 当前更新通道: {AppInfo.Config.LauncherConfig.UpdateChannel}");
            switch (AppInfo.Config.LauncherConfig.UpdateChannel)
            {
                case "Stable":
                    LatestVersion = UpdateIndex.Stable.LatestVersion;
                    ChangeLog = await Client.GetStringAsync(UpdateIndex.Stable.ChangeLog);
                    Url = UpdateIndex.Stable.Url;
                    break;

                case "Development":
                    LatestVersion = UpdateIndex.Development.LatestVersion;
                    ChangeLog = await Client.GetStringAsync(UpdateIndex.Development.ChangeLog);
                    Url = UpdateIndex.Development.Url;
                    break;
            }
            logger.Info($"[更新器] 最新版本: {LatestVersion}  更新文件Url: {Url}");

            //判断版本
            logger.Info($"[更新器] 当前版本: {AppInfo.Version}");
            if (AppInfo.Version != LatestVersion)
            {
                logger.Info($"[更新器] 检测到更新，开始更新");
                await DialogManager.ShowDialogAsync(new ContentDialog
                {
                    Title = $"发现可用更新 - {LatestVersion}",
                    Content = new ScrollViewer
                    {
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                        Content = new MarkdownScrollViewer
                        {
                            Markdown = ChangeLog,
                            MarkdownStyleName = "GithubLike"
                        }
                    },
                    PrimaryButtonText = "立即更新",
                    CloseButtonText = "取消更新",
                    DefaultButton = ContentDialogButton.Primary
                }, (() => { isUpdate = true; logger.Info($"[更新器] 用户取消更新"); }));
            }
            else
            {
                logger.Info($"[更新器] 无可用更新");
                if (!isStartUp)
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
            {
                logger.Info($"[更新器] 更新结束");
                return;
            }

            logger.Info($"[更新器] 准备更新...");

            if (AppDownloader.downloader != null)
            {
                new NotificationManager().Show(new NotificationContent
                {
                    Title = "启动下载任务失败",
                    Message = "已经有一个下载任务在执行，请等待任务完成",
                    Type = NotificationType.Error
                });
                return;
            }

            //开始更新
            bool? done = null;
            string errorMessage = null!;

            AppDownloader.downloader = new Downloader
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
                Progress = ((p, s) =>
                {
                    progressCallback?.Invoke(p, s);
                    logger.Info($"[更新器] 下载更新文件: {Math.Round(p, 2)}  ({Math.Round(s / 1024, 2)}MB/s)");
                })
            };
            logger.Info($"[更新器] 开始下载更新文件");

            AppDownloader.downloader.StartDownload();

            //等待下载完毕
            while (done == null)
                await Task.Delay(1000);

            //清空
            AppDownloader.downloader = null;

            logger.Info($"[更新器] 下载完成");
            //如下载失败抛错误
            if (done == false)
                throw new Exception(errorMessage);
            //下载成功↓
            //运行更新服务
            logger.Info($"[更新器] 下载完成，运行更新服务");
            if (File.Exists(Path.Combine(AppInfo.ExecuteDirectory, "UpdateService.exe")))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = Path.Combine(AppInfo.ExecuteDirectory, "UpdateService.exe"),
                    UseShellExecute = true
                });
                Environment.Exit(0);
            }
            else
            {
                await DialogManager.ShowDialogAsync(new ContentDialog
                {
                    Title = "失败",
                    Content = $"无法在 \"{Path.Combine(AppInfo.ExecuteDirectory, "UpdateService.exe")}\" 找到更新服务",
                    PrimaryButtonText = "确定",
                    DefaultButton = ContentDialogButton.Primary
                });
                Environment.Exit(1);
            }


        }
    }
}
