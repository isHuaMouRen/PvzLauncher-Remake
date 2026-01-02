using HuaZi.Library.Json;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using PvzLauncherRemake.Controls;
using PvzLauncherRemake.Utils.Services;
using PvzLauncherRemake.Utils.UI;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using static PvzLauncherRemake.Class.AppLogger;

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageDownload.xaml 的交互逻辑
    /// </summary>
    public partial class PageDownload : ModernWpf.Controls.Page
    {
        #region AddCard
        private void AddGameCard(StackPanel stackPanel, JsonDownloadIndex.GameInfo[] gameInfos)
        {
            if (gameInfos == null || gameInfos.Length <= 0)
                return;


            foreach (var gameInfo in gameInfos)
            {
                var card = new UserCard
                {
                    Title = gameInfo.Name,
                    Icon = GameIconConverter.ParseToGameIcons(gameInfo.Icon),
                    Version = gameInfo.Version,
                    Size = gameInfo.Size.ToString(),
                    isNew = gameInfo.IsNew,
                    isRecommend = gameInfo.IsRecommend,
                    Tag = gameInfo,
                    AttachedProperty = "Game",
                    Margin = new Thickness(0, 0, 0, 5)
                };
                card.MouseUp += UserCard_Click;
                logger.Info($"[下载] 添加游戏卡片: Title: {card.Title} | Icon: {card.Icon} | Version: {card.Version}");
                stackPanel.Children.Add(card);
            }
        }
        private void AddTrainerCard(StackPanel stackPanel, JsonDownloadIndex.TrainerInfo[] trainerInfos)
        {
            if (trainerInfos == null || trainerInfos.Length <= 0)
                return;

            foreach (var trainerInfo in trainerInfos)
            {
                var card = new UserCard
                {
                    Title = trainerInfo.Name,
                    Icon = GameIconConverter.ParseToGameIcons(trainerInfo.Icon),
                    Version = trainerInfo.Version,
                    Size = trainerInfo.Size.ToString(),
                    SupportVersion = trainerInfo.SupportVersion,
                    isNew = trainerInfo.IsNew,
                    isRecommend = trainerInfo.IsRecommend,
                    Tag = trainerInfo,
                    AttachedProperty = "Trainer",
                    Margin = new Thickness(0, 0, 0, 5)
                };
                card.MouseUp += UserCard_Click;
                logger.Info($"[下载] 添加修改器卡片: Title: {card.Title} | Icon: {card.Icon} | Version: {card.Version}");
                stackPanel.Children.Add(card);
            }
        }
        #endregion

        #region Load
        public void StartLoad()
        {
            tabControl_Main.IsEnabled = false;
            tabControl_Main.Effect = new BlurEffect { Radius = 10 };
            grid_Loading.Visibility = Visibility.Visible;
        }
        public void EndLoad()
        {
            tabControl_Main.IsEnabled = true;
            tabControl_Main.Effect = null;
            grid_Loading.Visibility = Visibility.Hidden;
        }
        #endregion

        #region Init
        public async void Initialize()
        {
            try
            {
                logger.Info($"[下载] 开始初始化...");
                StartLoad();

                if (AppGlobals.DownloadIndex == null)
                {
                    using (var client = new HttpClient())
                    {
                        string indexString = await client.GetStringAsync(AppGlobals.DownloadIndexUrl);
                        logger.Info($"[下载] 获取下载索引: {indexString}");
                        AppGlobals.DownloadIndex = Json.ReadJson<JsonDownloadIndex.Index>(indexString);
                    }
                }

                //中文原版
                logger.Info($"[下载] 开始加载中文原版游戏列表");
                stackPanel_zhOrigin.Children.Clear();
                stackPanel_zhRevision.Children.Clear();
                stackPanel_enOrigin.Children.Clear();
                stackPanel_trainer.Children.Clear();

                AddGameCard(stackPanel_zhOrigin, AppGlobals.DownloadIndex.ZhOrigin);
                AddGameCard(stackPanel_zhRevision, AppGlobals.DownloadIndex.ZhRevision);
                AddGameCard(stackPanel_enOrigin, AppGlobals.DownloadIndex.EnOrigin);
                AddTrainerCard(stackPanel_trainer, AppGlobals.DownloadIndex.Trainer);

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
                UserScrollViewer animControl = null!;

                if (selectItem is UserScrollViewer)
                {
                    animControl = (UserScrollViewer)selectItem;
                }
                else if (selectItem is TabControl tabcontrol && tabcontrol.SelectedContent is UserScrollViewer)
                {
                    animControl = (UserScrollViewer)tabcontrol.SelectedContent;
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

            bool isTrainer = userCard.AttachedProperty.ToString() == "Trainer";
            var info = isTrainer ? (JsonDownloadIndex.TrainerInfo)userCard.Tag! : (JsonDownloadIndex.GameInfo)userCard.Tag!;
            string baseDirectory =
                isTrainer ? AppGlobals.TrainerDirectory :
                AppGlobals.GameDirectory;


            this.NavigationService.Navigate(new PageDownloadConfirm
            {
                Info = info,
                BaseDirectory = baseDirectory,
                IsTrainer = isTrainer
            });

            return;
        }
    }
}
