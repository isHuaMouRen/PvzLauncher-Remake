using HuaZi.Library.Downloader;
using HuaZi.Library.Json;
using ModernWpf.Controls;
using Newtonsoft.Json;
using Notifications.Wpf;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using PvzLauncherRemake.Controls;
using PvzLauncherRemake.Utils;
using SharpCompress.Archives;
using SharpCompress.Common;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using static PvzLauncherRemake.Class.AppLogger;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageDownload.xaml 的交互逻辑
    /// </summary>
    public partial class PageDownload : ModernWpf.Controls.Page
    {
        private JsonDownloadIndex.Index DownloadIndex = null!;

        #region AddCard
        private void AddGameCard(StackPanel stackPanel, JsonDownloadIndex.GameInfo[] gameInfos)
        {
            foreach (var gameInfo in gameInfos)
            {
                var card = new UserCard
                {
                    Title = gameInfo.Name,
                    Description = gameInfo.Description,
                    Icon =
                    gameInfo.Version.StartsWith("β") ? "Beta" :
                    gameInfo.Version.StartsWith("TAT", StringComparison.OrdinalIgnoreCase) ? "Tat" : "Origin",
                    Version = gameInfo.Version,
                    Size = gameInfo.Size.ToString(),
                    isNew = gameInfo.IsNew,
                    isRecommend = gameInfo.IsRecommend,
                    Tag = gameInfo,
                    Margin = new Thickness(0, 0, 0, 5)
                };
                card.MouseUp += UserCard_Click;
                logger.Info($"[下载] 添加游戏卡片: Title: {card.Title} | Icon: {card.Icon} | Version: {card.Version}");
                stackPanel.Children.Add(card);
            }
        }
        private void AddTrainerCard(StackPanel stackPanel, JsonDownloadIndex.TrainerInfo[] trainerInfos)
        {
            foreach (var trainerInfo in trainerInfos)
            {
                var card = new UserCard
                {
                    Title = trainerInfo.Name,
                    Description = trainerInfo.Description,
                    Icon = "Origin",
                    Version = trainerInfo.Version,
                    Size = trainerInfo.Size.ToString(),
                    SupportVersion = trainerInfo.SupportVersion,
                    isNew = trainerInfo.IsNew,
                    isRecommend = trainerInfo.IsRecommend,
                    Tag = trainerInfo,
                    Margin = new Thickness(0, 0, 0, 5)
                };
                card.MouseUp += UserCard_Click;
                logger.Info($"[下载] 添加修改器卡片: Title: {card.Title} | Icon: {card.Icon} | Version: {card.Version}");
                stackPanel.Children.Add(card);
            }
        }
        #endregion

        #region Load
        public void StartLoad(bool showProgressBar = false)
        {
            tabControl_Main.IsEnabled = false;
            tabControl_Main.Effect = new BlurEffect { Radius = 10 };
            grid_Loading.Visibility = Visibility.Visible;
            if (showProgressBar)
                progressBar_Loading.Visibility = Visibility.Visible;
        }
        public void EndLoad()
        {
            tabControl_Main.IsEnabled = true;
            tabControl_Main.Effect = null;
            grid_Loading.Visibility = Visibility.Hidden;
            progressBar_Loading.Visibility = Visibility.Hidden;
        }
        #endregion

        #region Init
        public async void Initialize()
        {
            try
            {
                logger.Info($"[下载] 开始初始化...");
                StartLoad();

                using (var client = new HttpClient())
                {
                    string indexString = await client.GetStringAsync(AppInfo.DownloadIndexUrl);
                    logger.Info($"[下载] 获取下载索引: {indexString}");
                    DownloadIndex = Json.ReadJson<JsonDownloadIndex.Index>(indexString);
                }

                //中文原版
                logger.Info($"[下载] 开始加载中文原版游戏列表");
                stackPanel_zhOrigin.Children.Clear();
                stackPanel_zhRevision.Children.Clear();
                stackPanel_enOrigin.Children.Clear();
                stackPanel_trainer.Children.Clear();

                AddGameCard(stackPanel_zhOrigin, DownloadIndex.ZhOrigin);
                AddGameCard(stackPanel_zhRevision, DownloadIndex.ZhRevision);
                AddGameCard(stackPanel_enOrigin, DownloadIndex.EnOrigin);
                AddTrainerCard(stackPanel_trainer, DownloadIndex.Trainer);

                EndLoad();
                logger.Info($"[下载] 结束初始化");
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "加载后初始化 PageDownload 发生错误", ex);
            }
        }
        #endregion


        //Tab动画
        private void tabControl_Main_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsInitialized)
            {
                if (e.OriginalSource != sender)
                    return;

                var selectItem = ((TabControl)sender).SelectedContent;
                ListBox animControl = null!;

                if (selectItem is ListBox)
                {
                    animControl = (ListBox)selectItem;
                }
                else if (selectItem is TabControl tabcontrol && tabcontrol.SelectedContent is ListBox)
                {
                    animControl = (ListBox)tabcontrol.SelectedContent;
                }
                else
                {
                    return;
                }

                animControl.BeginAnimation(MarginProperty, null);
                animControl.BeginAnimation(OpacityProperty, null);

                animControl.Margin = new Thickness(0, 25, 0, 0);
                animControl.Opacity = 0;

                var margniAnim = new ThicknessAnimation
                {
                    To = new Thickness(0),
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
                };
                var opacAnim = new DoubleAnimation
                {
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
                };
                animControl.BeginAnimation(MarginProperty, margniAnim);
                animControl.BeginAnimation(OpacityProperty, opacAnim);
            }
        }

        public PageDownload() => InitializeComponent();

        private void Page_Loaded(object sender, RoutedEventArgs e) => Initialize();

        private async void UserCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not UserCard userCard) return;

            bool isTrainer = userCard.Tag?.ToString() == "trainer";
            var info = isTrainer ? (JsonDownloadIndex.TrainerInfo)userCard.Tag! : (JsonDownloadIndex.GameInfo)userCard.Tag!;
            string baseDirectory =
                isTrainer ? AppInfo.TrainerDirectory :
                AppInfo.GameDirectory;

            //确认下载
            bool confirm = false;
            await DialogManager.ShowDialogAsync(new ContentDialog
            {
                Title = "下载确认",
                Content = $"是否下载 \"{info.Name}\"",
                PrimaryButtonText = "确定",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary
            }, (() => confirm = true));
            if (!confirm) return;

            //处理同名
            string savePath = await GameManager.ResolveSameName(info.Name, baseDirectory);

            //开始下载
            await StartDownloadAsync(info, savePath, isTrainer);
        }

        private async Task StartDownloadAsync(dynamic info, string savePath, bool isTrainer)
        {
            StartLoad(true);

            string tempPath = Path.Combine(AppInfo.TempDiectory, $"PVZLAUNCHER.DOWNLOAD.CACHE.{AppInfo.Random.Next(Int32.MinValue, Int32.MaxValue) + AppInfo.Random.Next(Int32.MinValue, Int32.MaxValue)}");

            logger.Info($"[下载] 生成随机临时名: {tempPath}");

            try
            {
                //清除残留
                if (File.Exists(tempPath))
                    await Task.Run(() => File.Delete(tempPath));

                //定义下载器
                AppDownloader.AddTask(new DownloadTaskInfo
                {
                    Downloader = new Downloader
                    {
                        Url = info.Url,
                        SavePath = tempPath
                    },
                    GameInfo = isTrainer ? null : info,
                    TrainerInfo = isTrainer ? info : null,
                    TaskName = $"下载 {Path.GetFileName(savePath)}",
                    TaskType = isTrainer ? TaskType.Trainer : TaskType.Game,
                    SavePath = savePath
                });

                new NotificationManager().Show(new NotificationContent
                {
                    Title = "下载已开始",
                    Message = "您的下载任务已被添加进任务列表",
                    Type = NotificationType.Information
                });
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", null!, ex);
            }

            EndLoad();
        }
    }
}
