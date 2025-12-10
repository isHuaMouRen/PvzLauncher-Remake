using HuaZi.Library.Json;
using ModernWpf.Controls;
using Notifications.Wpf;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static PvzLauncherRemake.Class.AppLogger;

namespace PvzLauncherRemake.Utils
{
    public static class GameManager
    {
        public static DateTimeOffset? LatestGameLaunchTime = null;
        public static bool IsGameRuning = false;

        #region 加载列表

        /// <summary>
        /// 加载游戏列表
        /// </summary>
        /// <returns>无</returns>
        public static async Task LoadGameListAsync()
        {
            logger.Info("[游戏管理器] 开始加载游戏版本列表");

            var validGames = new List<JsonGameInfo.Index>();

            foreach (string dir in Directory.EnumerateDirectories(AppInfo.GameDirectory))
            {
                string configPath = Path.Combine(dir, ".pvzl.json");
                if (!File.Exists(configPath)) continue;

                try
                {
                    var config = Json.ReadJson<JsonGameInfo.Index>(configPath);
                    if (config != null)
                    {
                        if (AppInfo.Config.SaveConfig.EnableSaveIsolation)
                        {
                            string saveDir = Path.Combine(dir, ".save");
                            if (!Directory.Exists(saveDir))
                                Directory.CreateDirectory(saveDir);
                        }

                        validGames.Add(config);
                    }
                    else
                    {
                        logger.Warn($"[游戏管理器] 配置文件为空 {configPath}");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"[游戏管理器] 读取游戏配置文件失败，已跳过: {configPath}\n{ex.Message}");
                }
            }

            AppInfo.GameList = validGames;
            logger.Info($"[游戏管理器] 加载游戏版本完成，共 {AppInfo.GameList.Count} 个有效版本");
        }

        /// <summary>
        /// 加载修改器列表
        /// </summary>
        /// <returns>无</returns>
        public static async Task LoadTrainerListAsync()
        {
            logger.Info("[游戏管理器] 开始加载修改器版本列表");

            var validTrainers = new List<JsonTrainerInfo.Index>();

            foreach (string dir in Directory.EnumerateDirectories(AppInfo.TrainerDirectory))
            {
                string configPath = Path.Combine(dir, ".pvzl.json");
                if (!File.Exists(configPath)) continue;

                try
                {
                    var config = Json.ReadJson<JsonTrainerInfo.Index>(configPath);
                    if (config != null)
                    {
                        validTrainers.Add(config);
                    }
                    else
                    {
                        logger.Warn($"[游戏管理器] 配置文件为空 {configPath}");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"[游戏管理器] 读取游戏配置文件失败，已跳过: {configPath}\n{ex.Message}");
                }
            }

            AppInfo.TrainerList = validTrainers;
            logger.Info($"[游戏管理器] 加载游戏版本完成，共 {AppInfo.TrainerList.Count} 个有效版本");
        }

        #endregion

        #region 启动/等待/结束游戏

        /// <summary>
        /// 启动游戏
        /// </summary>
        /// <param name="gameInfo">要启动的游戏信息</param>
        public static async void LaunchGame(JsonGameInfo.Index gameInfo, Action? exitCallback = null)
        {
            //游戏exe路径
            string gameExePath = System.IO.Path.Combine(AppInfo.GameDirectory, gameInfo.GameInfo.Name, gameInfo.GameInfo.ExecuteName);
            logger.Info($"[游戏管理器] 设置游戏可执行文件路径: {gameExePath}");
            //定义Process
            AppProcess.Process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = gameExePath,
                    UseShellExecute = true,
                    WorkingDirectory = System.IO.Path.Combine(AppInfo.GameDirectory, gameInfo.GameInfo.Name)
                }
            };
            logger.Info($"[游戏管理器] 启动进程");
            //启动
            AppProcess.Process.Start();

            LatestGameLaunchTime = DateTimeOffset.Now;

            //启动后操作
            logger.Info($"[启动] 启动后操作为: {AppInfo.Config.LauncherConfig.LaunchedOperate}");
            switch (AppInfo.Config.LauncherConfig.LaunchedOperate)
            {
                case "Close":
                    Environment.Exit(0); break;
                case "HideAndDisplay":
                    Application.Current.MainWindow.Visibility = Visibility.Hidden; break;
            }

            //启动次数
            gameInfo.Record.PlayCount++;
            logger.Info($"[启动] 启动次数+1, 现在为： {gameInfo.Record.PlayCount}");
            Json.WriteJson(System.IO.Path.Combine(AppInfo.GameDirectory, gameInfo.GameInfo.Name, ".pvzl.json"), gameInfo);

            IsGameRuning = true;

            logger.Info($"[启动] 启动操作完毕，等待游戏结束...");
            await WaitGameExit(gameInfo);

            exitCallback?.Invoke();
        }

        /// <summary>
        /// 等待游戏退出
        /// </summary>
        public static async Task WaitGameExit(JsonGameInfo.Index gameInfo)
        {
            await AppProcess.Process.WaitForExitAsync();

            IsGameRuning = false;

            logger.Info($"[游戏管理器] 启动后操作为: {AppInfo.Config.LauncherConfig.LaunchedOperate}");
            switch (AppInfo.Config.LauncherConfig.LaunchedOperate)
            {
                case "HideAndDisplay":
                    Application.Current.MainWindow.Visibility = Visibility.Visible; break;
            }


            //保存游玩时间
            gameInfo.Record.PlayTime = gameInfo.Record.PlayTime + ((long)(DateTimeOffset.Now - LatestGameLaunchTime!).Value.TotalSeconds);
            Json.WriteJson(Path.Combine(AppInfo.GameDirectory, gameInfo.GameInfo.Name, ".pvzl.json"), gameInfo);
        }

        /// <summary>
        /// 结束游戏
        /// </summary>
        /// <returns></returns>
        public static async Task KillGame(Action? completeCallback = null, Action? failCallback = null)
        {
            if (!AppProcess.Process.HasExited)
            {
                logger.Info($"[游戏管理器] 尝试使游戏进程自行退出");
                AppProcess.Process.CloseMainWindow();
                //等待自己关闭
                await Task.Delay(1000);

                //强制关
                if (!AppProcess.Process.HasExited)
                {
                    logger.Warn($"[游戏管理器] 游戏进程仍然运行，开始强制结束");
                    AppProcess.Process.Kill();
                    //等待完全关闭
                    await Task.Delay(1000);
                }

                if (!AppProcess.Process.HasExited)
                {
                    //都Kill()了不能再关不上吧
                    logger.Error($"[游戏管理器] 无法终止游戏进程!");
                    failCallback?.Invoke();
                    return;
                }
                else
                {
                    completeCallback?.Invoke();
                    return;
                }
            }
        }

        #endregion

        #region 存档控制

        /// <summary>
        /// 切换当前存档为当前游戏的独立存档
        /// </summary>
        /// <returns></returns>
        public static async Task SwitchGameSave(JsonGameInfo.Index gamInfo)
        {
            if (Directory.Exists(AppInfo.SaveDirectory))
                Directory.Delete(AppInfo.SaveDirectory, true);
            await DirectoryManager.CopyDirectoryAsync(Path.Combine(AppInfo.GameDirectory, gamInfo.GameInfo.Name, ".save"), AppInfo.SaveDirectory);
        }

        /// <summary>
        /// 保存当前存档至独立游戏存档
        /// </summary>
        /// <param name="gamInfo"></param>
        /// <returns></returns>
        public static async Task SaveGameSave(JsonGameInfo.Index gamInfo)
        {
            if (Directory.Exists(Path.Combine(AppInfo.GameDirectory, gamInfo.GameInfo.Name, ".save")))
                Directory.Delete(Path.Combine(AppInfo.GameDirectory, gamInfo.GameInfo.Name, ".save"), true);
            await DirectoryManager.CopyDirectoryAsync(AppInfo.SaveDirectory, Path.Combine(AppInfo.GameDirectory, gamInfo.GameInfo.Name, ".save"));
        }

        #endregion

        #region 其他

        /// <summary>
        /// 解决重名
        /// </summary>
        /// <param name="name">旧名</param>
        /// <param name="baseDir">基础文件夹</param>
        /// <returns>新名</returns>
        public static async Task<string> ResolveSameName(string name, string baseDir)
        {
            string path = Path.Combine(baseDir, name);
            if (!Directory.Exists(path)) return path;

            while (true)
            {
                var textBox = new TextBox { Text = name };
                await DialogManager.ShowDialogAsync(new ContentDialog
                {
                    Title = "发现重名",
                    Content = new StackPanel
                    {
                        Children =
                        {
                            new TextBlock
                            {
                                Text=$"在您的库内发现与 \"{name}\" 重名的文件夹, 请输入一个新名称!",
                                Margin=new Thickness(0,0,0,5)
                            },
                            textBox
                        }
                    },
                    PrimaryButtonText = "确定",
                    DefaultButton = ContentDialogButton.Primary
                });

                if (!Directory.Exists(Path.Combine(baseDir, textBox.Text)))
                    return Path.Combine(baseDir, textBox.Text);
                else
                    new NotificationManager().Show(new NotificationContent
                    {
                        Title = "无法解决",
                        Message = $"库内仍然有与 \"{textBox.Text}\" 同名的文件夹，请继续解决",
                        Type = NotificationType.Warning
                    });
            }
        }

        #endregion


    }
}