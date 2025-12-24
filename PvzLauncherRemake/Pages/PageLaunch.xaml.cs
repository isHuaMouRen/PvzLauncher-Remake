using Notifications.Wpf;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using PvzLauncherRemake.Utils;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using static PvzLauncherRemake.Class.AppLogger;

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageLaunch.xaml 的交互逻辑
    /// </summary>
    public partial class PageLaunch : ModernWpf.Controls.Page
    {
        private JsonGameInfo.Index currentGameInfo = null!;
        private JsonTrainerInfo.Index currentTrainerInfo = null!;
        private NotificationManager notifi = new NotificationManager();

        #region Animation
        public void StartAnimation()
        {
            var animation = new ThicknessAnimation
            {
                To = new Thickness(0),
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new BackEase { Amplitude = 0.2, EasingMode = EasingMode.EaseOut }
            };
            grid_Title.BeginAnimation(MarginProperty, animation);
            stackPanel_LaunchButtons.BeginAnimation(MarginProperty, animation);
        }
        #endregion

        #region Init
        public async void Initialize()
        {
            try
            {
                logger.Info($"[启动] 开始初始化...");

                if (!string.IsNullOrEmpty(AppGlobals.Config.CurrentGame))
                {
                    logger.Info($"[启动] 当前选中游戏: {AppGlobals.Config.CurrentGame}");

                    //查找选择游戏信息
                    foreach (var game in AppGlobals.GameList)
                        if (game.GameInfo.Name == AppGlobals.Config.CurrentGame)
                            currentGameInfo = game;

                    //设置按钮文本
                    textBlock_LaunchVersion.Text = AppGlobals.Config.CurrentGame;

                }
                else
                {
                    button_Launch.IsEnabled = false;
                    textBlock_LaunchVersion.Text = "请选择一个游戏";
                }

                if (!string.IsNullOrEmpty(AppGlobals.Config.CurrentTrainer))
                {
                    logger.Info($"[启动] 当前选中修改器: {AppGlobals.Config.CurrentTrainer}");
                    foreach (var trainer in AppGlobals.TrainerList)
                        if (trainer.Name == AppGlobals.Config.CurrentTrainer)
                            currentTrainerInfo = trainer;
                }
                else
                {
                    button_LaunchTrainer.IsEnabled = false;
                }

                //判断游戏是否运行
                try
                {
                    if (AppProcess.Process != null && AppProcess.Process.Id != 0 && !AppProcess.Process.HasExited)
                    {
                        logger.Info($"[启动] 检测到游戏仍在运行切换为结束状态");
                        textBlock_LaunchText.Text = "结束进程";
                    }
                }
                catch (InvalidOperationException) { }


                //播放动画
                viewBox_Title_EN.Visibility = Visibility.Hidden;
                viewBox_Title_ZH.Visibility = Visibility.Hidden;
                logger.Info($"[启动] 标题语言: {AppGlobals.Config.LauncherConfig.TitleImage}");
                switch (AppGlobals.Config.LauncherConfig.TitleImage)//切换语言
                {
                    case "EN":
                        viewBox_Title_EN.Visibility = Visibility.Visible; break;
                    case "ZH":
                        viewBox_Title_ZH.Visibility = Visibility.Visible; break;
                }
                grid_Title.Margin = new Thickness(0, -10 - grid_Title.Height, 0, 0);
                stackPanel_LaunchButtons.Margin = new Thickness(0, 0, -50 - button_Launch.Width, 0);

                await Task.Delay(200);//等待Frame动画播放完毕
                StartAnimation();

                //设置背景
                if (!string.IsNullOrEmpty(AppGlobals.Config.LauncherConfig.Background))
                    image.Source = new BitmapImage(new Uri(AppGlobals.Config.LauncherConfig.Background));
                logger.Info($"[启动] 完成初始化");

            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "加载后初始化 PageLaunch 发生错误", ex);
            }
        }
        #endregion

        public PageLaunch()
        {
            InitializeComponent();
            Loaded += ((sender, e) => Initialize());
        }

        private async void button_Launch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //没运行就启动
                if (GameManager.IsGameRuning == false)
                {
                    logger.Info($"[启动] 开始启动游戏");

                    textBlock_LaunchText.Text = "结束进程";

                    //切换存档
                    if (AppGlobals.Config.SaveConfig.EnableSaveIsolation && Directory.Exists(Path.Combine(AppGlobals.GameDirectory, AppGlobals.Config.CurrentGame, ".save")))
                    {
                        logger.Info($"[启动] 已启用存档隔离，开始切换存档");
                        await GameManager.SwitchGameSave(currentGameInfo);
                        logger.Info($"[启动] 存档切换成功！");
                    }

                    //启动游戏
                    GameManager.LaunchGame(currentGameInfo, (async () =>
                    {
                        notifi.Show(new NotificationContent
                        {
                            Title = "提示",
                            Message = $"游戏进程退出, 退出代码: {AppProcess.Process.ExitCode}",
                            Type = NotificationType.Warning
                        });

                        textBlock_LaunchText.Text = "启动游戏";

                        //保存存档
                        if (AppGlobals.Config.SaveConfig.EnableSaveIsolation && Directory.Exists(AppGlobals.SaveDirectory))
                        {
                            logger.Info($"[启动] 存档隔离已开启，开始保存存档");
                            await GameManager.SaveGameSave(currentGameInfo);
                            logger.Info($"[启动] 存档保存成功");
                        }
                    }));

                    //启动提示
                    notifi.Show(new NotificationContent
                    {
                        Title = "提示",
                        Message = $"{AppGlobals.Config.CurrentGame} 启动成功!",
                        Type = NotificationType.Success
                    });
                }
                //运行就结束
                else if (GameManager.IsGameRuning == true) 
                {
                    logger.Info($"[启动] 正在结束游戏...");
                    textBlock_LaunchText.Text = "启动游戏";

                    await GameManager.KillGame((() =>
                    {
                        new NotificationManager().Show(new NotificationContent
                        {
                            Title = "结束游戏",
                            Message = "成功结束游戏",
                            Type = NotificationType.Success
                        });
                    }), (() =>
                    {
                        new NotificationManager().Show(new NotificationContent
                        {
                            Title = "结束游戏",
                            Message = "无法结束游戏，请手动关闭游戏",
                            Type = NotificationType.Error
                        });
                    }));
                    
                }
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "启动游戏时发生错误", ex);
            }
        }

        private void button_LaunchTrainer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                logger.Info($"[启动] 开始启动修改器...");

                Process.Start(new ProcessStartInfo
                {
                    FileName = System.IO.Path.Combine(AppGlobals.TrainerDirectory, currentTrainerInfo.Name, currentTrainerInfo.ExecuteName),
                    UseShellExecute = true
                });
                notifi.Show(new NotificationContent
                {
                    Title = "提示",
                    Message = $"{AppGlobals.Config.CurrentTrainer} 启动成功!",
                    Type = NotificationType.Success
                });
                logger.Info($"[启动] 修改器启动成功");
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", null!, ex);
            }
        }
    }
}
