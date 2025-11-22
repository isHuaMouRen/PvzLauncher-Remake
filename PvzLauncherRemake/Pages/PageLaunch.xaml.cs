using HuaZi.Library.Json;
using ModernWpf;
using Notifications.Wpf;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using PvzLauncherRemake.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
        private bool MainCycleEnable = false;

        #region Animation
        public void StartTitleAnimation(double gridHeight, double timeMs = 500)
        {
            var animation = new ThicknessAnimationUsingKeyFrames();
            animation.Duration = TimeSpan.FromMilliseconds(timeMs);
            animation.KeyFrames.Add(new DiscreteThicknessKeyFrame(
                new Thickness(0, -10 - gridHeight, 0, 0),
                KeyTime.FromTimeSpan(TimeSpan.Zero)));

            var easingKeyFrame = new EasingThicknessKeyFrame(
                new Thickness(0, 0, 0, 0),
                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(timeMs)))
            {

                // 1. 快速滑入并轻微回弹
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.35 }

                // 2. 平滑强减速
                // EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut }

                // 3. 弹性弹跳效果
                // EasingFunction = new ElasticEase { EasingMode = EasingMode.EaseOut, Oscillations = 2, Springiness = 8 }

                // 4. 先慢后快再减速
                // EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }

                // 5. 经典 Power 缓动
                // EasingFunction = new PowerEase { EasingMode = EasingMode.EaseOut, Power = 4 }
            };

            animation.KeyFrames.Add(easingKeyFrame);
            animation.FillBehavior = FillBehavior.HoldEnd;
            grid_Title.BeginAnimation(FrameworkElement.MarginProperty, animation);
        }
        #endregion

        #region Init
        public void Initialize() { MainCycle(); }
        public async void InitializeLoaded()
        {
            try
            {
                if (!string.IsNullOrEmpty(AppInfo.Config.CurrentGame))
                {
                    logger.Info($"当前选择游戏: {AppInfo.Config.CurrentGame}");
                    //查找选择游戏信息
                    foreach (var game in AppInfo.GameList)
                        if (game.GameInfo.Name == AppInfo.Config.CurrentGame)
                            currentGameInfo = game;

                    //设置按钮文本
                    textBlock_LaunchVersion.Text = AppInfo.Config.CurrentGame;

                }
                else
                {
                    logger.Info("没有检测到选择游戏，禁用按钮");
                    button_Launch.IsEnabled = false;
                    textBlock_LaunchVersion.Text = "请选择一个游戏";
                }

                if (!string.IsNullOrEmpty(AppInfo.Config.CurrentTrainer))
                {
                    logger.Info($"当前选择修改器: {AppInfo.Config.CurrentTrainer}");
                    foreach (var trainer in AppInfo.TrainerList)
                        if (trainer.Name == AppInfo.Config.CurrentTrainer)
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
                        logger.Info("检测到游戏进程仍在运行...");
                        textBlock_LaunchText.Text = "结束进程";

                    }
                }
                catch (InvalidOperationException) { }


                //播放动画
                viewBox_Title_EN.Visibility = Visibility.Hidden;
                viewBox_Title_ZH.Visibility = Visibility.Hidden;
                switch (AppInfo.Config.LauncherConfig.TitleImage)//切换语言
                {
                    case "EN":
                        viewBox_Title_EN.Visibility = Visibility.Visible; break;
                    case "ZH":
                        viewBox_Title_ZH.Visibility = Visibility.Visible; break;
                }
                grid_Title.Margin = new Thickness(0, -10 - grid_Title.Height, 0, 0);
                await Task.Delay(200);//等待Frame动画播放完毕
                StartTitleAnimation(grid_Title.Height, 500);

                //设置背景
                if (!string.IsNullOrEmpty(AppInfo.Config.LauncherConfig.Background))
                    image.Source = new BitmapImage(new Uri(AppInfo.Config.LauncherConfig.Background));

            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "加载后初始化 PageLaunch 发生错误", ex);
            }
        }
        #endregion

        #region Cycle
        public async void MainCycle()
        {
            while (true)
            {
                await Task.Delay(1000);
                if (MainCycleEnable)
                {
                    currentGameInfo.Record.PlayTime++;
                    Json.WriteJson(System.IO.Path.Combine(AppInfo.GameDirectory, currentGameInfo.GameInfo.Name, ".pvzl.json"), currentGameInfo);
                }
            }
        }
        #endregion

        public PageLaunch()
        {
            InitializeComponent();
            Initialize();
            Loaded += ((sender, e) => InitializeLoaded());
        }

        private async void button_Launch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //没运行就启动
                if (textBlock_LaunchText.Text == "启动游戏")
                {
                    textBlock_LaunchText.Text = "结束进程";

                    logger.Info("游戏开始启动...");
                    logger.Info($"当前游戏: {AppInfo.Config.CurrentGame}");
                    //游戏exe路径
                    string gameExePath = System.IO.Path.Combine(AppInfo.GameDirectory, currentGameInfo.GameInfo.Name, currentGameInfo.GameInfo.ExecuteName);

                    logger.Info($"游戏exe路径: {gameExePath}");

                    //定义Process
                    AppProcess.Process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = gameExePath,
                            UseShellExecute = true,
                            WorkingDirectory = System.IO.Path.Combine(AppInfo.GameDirectory, currentGameInfo.GameInfo.Name)
                        }
                    };

                    //启动
                    AppProcess.Process.Start();
                    logger.Info($"进程启动完毕");

                    //启动后操作
                    switch (AppInfo.Config.LauncherConfig.LaunchedOperate)
                    {
                        case "Close":
                            Environment.Exit(0); break;
                        case "HideAndDisplay":
                            ((MainWindow)Window.GetWindow(this)).Visibility = Visibility.Hidden; break;
                    }

                    //记录启动次数
                    currentGameInfo.Record.PlayCount++;
                    Json.WriteJson(System.IO.Path.Combine(AppInfo.GameDirectory, currentGameInfo.GameInfo.Name, ".pvzl.json"), currentGameInfo);

                    //开始计时
                    MainCycleEnable = true;

                    //启动提示
                    notifi.Show(new NotificationContent
                    {
                        Title = "提示",
                        Message = $"{AppInfo.Config.CurrentGame} 启动成功!",
                        Type = NotificationType.Success
                    });

                    //等待结束
                    logger.Info("等待进程退出...");

                    await AppProcess.Process.WaitForExitAsync();

                    //停止计时
                    MainCycleEnable = false;

                    logger.Info($"进程退出, ExitCode: {AppProcess.Process.ExitCode}");
                    notifi.Show(new NotificationContent
                    {
                        Title = "提示",
                        Message = $"游戏进程退出, 退出代码: {AppProcess.Process.ExitCode}",
                        Type = NotificationType.Warning
                    });

                    textBlock_LaunchText.Text = "启动游戏";

                    //启动后操作
                    switch (AppInfo.Config.LauncherConfig.LaunchedOperate)
                    {
                        case "HideAndDisplay":
                            ((MainWindow)Window.GetWindow(this)).Visibility = Visibility.Visible; break;
                    }
                }
                //运行就结束
                else if (textBlock_LaunchText.Text == "结束进程")
                {
                    logger.Info($"用户手动结束进程中...");
                    textBlock_LaunchText.Text = "启动游戏";

                    //尝试使程序自行退出
                    if (!AppProcess.Process.HasExited)
                    {
                        notifi.Show(new NotificationContent
                        {
                            Title = "提示",
                            Message = "正在尝试关闭游戏...",
                            Type = NotificationType.Information
                        });
                        AppProcess.Process.CloseMainWindow();
                        //等待自己关闭
                        await Task.Delay(1000);

                        //强制关
                        if (!AppProcess.Process.HasExited)
                        {
                            AppProcess.Process.Kill();
                            //等待完全关闭
                            await Task.Delay(1000);
                        }

                        if (!AppProcess.Process.HasExited)
                        {
                            //都Kill()了不能再关不上吧
                            notifi.Show(new NotificationContent
                            {
                                Title = "失败",
                                Message = "我们无法终止您的游戏，请您自行退出",
                                Type = NotificationType.Error
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MainCycleEnable = false;
                ErrorReportDialog.Show("发生错误", "启动游戏时发生错误", ex);
            }
        }

        private void button_LaunchTrainer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                logger.Info("开始启动修改器...");

                Process.Start(new ProcessStartInfo
                {
                    FileName = System.IO.Path.Combine(AppInfo.TrainerDirectory, currentTrainerInfo.Name, currentTrainerInfo.ExecuteName),
                    UseShellExecute = true
                });
                notifi.Show(new NotificationContent
                {
                    Title = "提示",
                    Message = $"{AppInfo.Config.CurrentTrainer} 启动成功!",
                    Type = NotificationType.Success
                });
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", null!, ex);
            }
        }
    }
}
