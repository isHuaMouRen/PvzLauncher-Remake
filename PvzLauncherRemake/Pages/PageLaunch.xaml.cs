using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using PvzLauncherRemake.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
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

        #region Init
        public void Initialize() { }
        public void InitializeLoaded()
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
            Initialize();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) { InitializeLoaded(); }

        private async void button_Launch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (textBlock_LaunchText.Text == "启动游戏")
                {
                    textBlock_LaunchText.Text = "结束进程";

                    logger.Info("游戏开始启动...");
                    logger.Info($"当前游戏: {AppInfo.Config.CurrentGame}");
                    //游戏exe路径
                    string gameExePath = System.IO.Path.Combine(AppInfo.GameDirectory, currentGameInfo.GameInfo.DirName, currentGameInfo.GameInfo.ExecuteName);

                    logger.Info($"游戏exe路径: {gameExePath}");

                    //定义Process
                    AppProcess.Process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = gameExePath,
                            UseShellExecute = true,
                            WorkingDirectory = System.IO.Path.Combine(AppInfo.GameDirectory, currentGameInfo.GameInfo.DirName)
                        }
                    };

                    //启动
                    AppProcess.Process.Start();
                    logger.Info($"进程启动完毕");

                    await DialogManager.ShowDialogAsync(new ModernWpf.Controls.ContentDialog
                    {
                        Title = "提示",
                        Content = "游戏启动成功!",
                        PrimaryButtonText = "确定",
                        DefaultButton = ModernWpf.Controls.ContentDialogButton.Primary
                    });

                    //等待结束
                    logger.Info("等待进程退出...");

                    await AppProcess.Process.WaitForExitAsync();
                    logger.Info($"进程退出, ExitCode: {AppProcess.Process.ExitCode}");

                    textBlock_LaunchText.Text = "启动游戏";
                }
                else if(textBlock_LaunchText.Text=="结束进程")
                {
                    logger.Info($"用户手动结束进程中...");
                    textBlock_LaunchText.Text = "启动游戏";
                    //结束进程
                    AppProcess.Process.Close();
                }
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "启动游戏时发生错误", ex);
            }
        }
    }
}
