using HuaZi.Library.Downloader;
using HuaZi.Library.Json;
using Microsoft.Win32;
using ModernWpf.Controls;
using Notifications.Wpf;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using PvzLauncherRemake.Controls.Icons;
using System.Diagnostics;
using System.IO;
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

            foreach (string dir in Directory.EnumerateDirectories(AppGlobals.GameDirectory))
            {
                string configPath = Path.Combine(dir, ".pvzl.json");
                if (!File.Exists(configPath)) continue;

                try
                {
                    var config = Json.ReadJson<JsonGameInfo.Index>(configPath);
                    if (config != null)
                    {
                        if (AppGlobals.Config.SaveConfig.EnableSaveIsolation)
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

            AppGlobals.GameList = validGames;
            logger.Info($"[游戏管理器] 加载游戏版本完成，共 {AppGlobals.GameList.Count} 个有效版本");
        }

        /// <summary>
        /// 加载修改器列表
        /// </summary>
        /// <returns>无</returns>
        public static async Task LoadTrainerListAsync()
        {
            logger.Info("[游戏管理器] 开始加载修改器版本列表");

            var validTrainers = new List<JsonTrainerInfo.Index>();

            foreach (string dir in Directory.EnumerateDirectories(AppGlobals.TrainerDirectory))
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

            AppGlobals.TrainerList = validTrainers;
            logger.Info($"[游戏管理器] 加载游戏版本完成，共 {AppGlobals.TrainerList.Count} 个有效版本");
        }

        #endregion

        #region 下载

        public static async Task StartDownloadAsync(dynamic info, string savePath, bool isTrainer)
        {
            string tempPath = Path.Combine(AppGlobals.TempDiectory, $"PVZLAUNCHER.DOWNLOAD.CACHE.{AppGlobals.Random.Next(Int32.MinValue, Int32.MaxValue) + AppGlobals.Random.Next(Int32.MinValue, Int32.MaxValue)}");

            logger.Info($"[下载] 生成随机临时名: {tempPath}");

            try
            {
                //清除残留
                if (File.Exists(tempPath))
                    await Task.Run(() => File.Delete(tempPath));

                //定义下载器
                TaskManager.AddTask(new DownloadTaskInfo
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
                    SavePath = savePath,
                    TaskIcon = GameManager.ParseToGameIcons(info.Icon)
                });
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", null!, ex);
            }
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
            string gameExePath = System.IO.Path.Combine(AppGlobals.GameDirectory, gameInfo.GameInfo.Name, gameInfo.GameInfo.ExecuteName);
            logger.Info($"[游戏管理器] 设置游戏可执行文件路径: {gameExePath}");
            //定义Process
            AppProcess.Process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = gameExePath,
                    UseShellExecute = true,
                    WorkingDirectory = System.IO.Path.Combine(AppGlobals.GameDirectory, gameInfo.GameInfo.Name)
                }
            };
            logger.Info($"[游戏管理器] 启动进程");
            //启动
            AppProcess.Process.Start();

            LatestGameLaunchTime = DateTimeOffset.Now;

            //启动后操作
            logger.Info($"[启动] 启动后操作为: {AppGlobals.Config.LauncherConfig.LaunchedOperate}");
            switch (AppGlobals.Config.LauncherConfig.LaunchedOperate)
            {
                case "Close":
                    Environment.Exit(0); break;
                case "HideAndDisplay":
                    Application.Current.MainWindow.Visibility = Visibility.Hidden; break;
            }
            SetGameFullScreen();
            SetGameLocation();

            //启动次数
            gameInfo.Record.PlayCount++;
            logger.Info($"[启动] 启动次数+1, 现在为： {gameInfo.Record.PlayCount}");
            Json.WriteJson(System.IO.Path.Combine(AppGlobals.GameDirectory, gameInfo.GameInfo.Name, ".pvzl.json"), gameInfo);

            //启动器整体次数
            AppGlobals.Config.Record.LaunchCount++;
            ConfigManager.SaveConfig();
            logger.Info($"[启动] 启动器总体启动数: {AppGlobals.Config.Record.LaunchCount}");

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

            logger.Info($"[游戏管理器] 启动后操作为: {AppGlobals.Config.LauncherConfig.LaunchedOperate}");
            switch (AppGlobals.Config.LauncherConfig.LaunchedOperate)
            {
                case "HideAndDisplay":
                    Application.Current.MainWindow.Visibility = Visibility.Visible; break;
            }


            //保存游玩时间
            gameInfo.Record.PlayTime = gameInfo.Record.PlayTime + ((long)(DateTimeOffset.Now - LatestGameLaunchTime!).Value.TotalSeconds);
            Json.WriteJson(Path.Combine(AppGlobals.GameDirectory, gameInfo.GameInfo.Name, ".pvzl.json"), gameInfo);
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
            if (Directory.Exists(AppGlobals.SaveDirectory))
                Directory.Delete(AppGlobals.SaveDirectory, true);
            await DirectoryManager.CopyDirectoryAsync(Path.Combine(AppGlobals.GameDirectory, gamInfo.GameInfo.Name, ".save"), AppGlobals.SaveDirectory);
        }

        /// <summary>
        /// 保存当前存档至独立游戏存档
        /// </summary>
        /// <param name="gamInfo"></param>
        /// <returns></returns>
        public static async Task SaveGameSave(JsonGameInfo.Index gamInfo)
        {
            if (Directory.Exists(Path.Combine(AppGlobals.GameDirectory, gamInfo.GameInfo.Name, ".save")))
                Directory.Delete(Path.Combine(AppGlobals.GameDirectory, gamInfo.GameInfo.Name, ".save"), true);
            await DirectoryManager.CopyDirectoryAsync(AppGlobals.SaveDirectory, Path.Combine(AppGlobals.GameDirectory, gamInfo.GameInfo.Name, ".save"));
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

        /// <summary>
        /// 将图标字符串转换为枚举类型
        /// </summary>
        /// <param name="iconName">图标字符串，一般是下载索引获取的</param>
        /// <returns></returns>
        public static GameIcons ParseToGameIcons(string iconName)
        {
            switch (iconName)
            {
                case "origin": return GameIcons.Origin;
                case "goty": return GameIcons.GOTY;
                case "steam": return GameIcons.Steam;
                case "test": return GameIcons.Test;
                case "beta": return GameIcons.Beta;
                case "ghtr": return GameIcons.Ghtr;
                case "dream": return GameIcons.Dream;
                case "ninefive": return GameIcons.NineFive;
                case "he": return GameIcons.Hybrid;
                case "fe": return GameIcons.Fusion;
                case "tat": return GameIcons.Tat;
                case "eagrace": return GameIcons.Eagrace;
                case "unnamed": return GameIcons.Unnamed;

                case "pvztoolkit": return GameIcons.PvzToolkit;
                case "ce": return GameIcons.CheatEngine;

                case "unknown": return GameIcons.Unknown;


                default: return GameIcons.Unknown;
            }
        }

        /// <summary>
        /// 将图标类型转换为UserControl
        /// </summary>
        /// <param name="gameIcons"></param>
        /// <returns></returns>
        public static UserControl ParseToUserControl(GameIcons gameIcons)
        {
            switch (gameIcons)
            {
                case GameIcons.Unknown: return new GameIconUnknown();

                case GameIcons.Origin: return new GameIconOrigin();
                case GameIcons.GOTY: return new GameIconGoty();
                case GameIcons.Steam: return new GameIconSteam();
                case GameIcons.Test: return new GameIconTest();
                case GameIcons.Beta: return new GameIconBeta();
                case GameIcons.Ghtr: return new GameIconGhtr();
                case GameIcons.Dream: return new GameIconDream();
                case GameIcons.NineFive: return new GameIconNineFive();
                case GameIcons.Hybrid: return new GameIconHybrid();
                case GameIcons.Fusion: return new GameIconFusion();
                case GameIcons.Tat: return new GameIconTat();
                case GameIcons.Eagrace: return new GameIconEagrace();
                case GameIcons.Unnamed: return new GameIconUnnamed();

                case GameIcons.PvzToolkit: return new GameIconPvzToolkit();
                case GameIcons.CheatEngine: return new GameIconCheatEngine();

                default: return new GameIconUnknown();
            }
        }

        /// <summary>
        /// 设置游戏屏幕模式
        /// </summary>
        public static void SetGameFullScreen()
        {
            string registyPath = @"SOFTWARE\PopCap\PlantsVsZombies";
            string valueName = "ScreenMode";

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(registyPath))
            {
                int? valueData;
                switch (AppGlobals.Config.GameConfig.FullScreen)
                {
                    case "FullScreen": valueData = 1; break;
                    case "Windowed": valueData = 0; break;
                    default: valueData = null; break;
                }
                if (valueData != null)
                    key.SetValue(valueName, valueData, RegistryValueKind.DWord);
            }
        }

        /// <summary>
        /// 设置游戏窗口位置
        /// </summary>
        public static void SetGameLocation()
        {
            string registyPath = @"SOFTWARE\PopCap\PlantsVsZombies";
            string valueXName = "PreferredX";
            string valueYName = "PreferredY";

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(registyPath))
            {
                int? gameWindowX;
                int? gameWindowY;

                switch (AppGlobals.Config.GameConfig.StartUpLocation)
                {
                    case "Center":
                        gameWindowX = (int)((SystemParameters.WorkArea.Width / 2) - (800 / 2));
                        gameWindowY = (int)((SystemParameters.WorkArea.Height / 2) - (600 / 2));
                        break;
                    case "LeftTop":
                        gameWindowX = 0;
                        gameWindowY = 0;
                        break;

                    default:
                        gameWindowX = null; gameWindowY = null; break;
                }

                if (gameWindowX != null && gameWindowY != null)
                {
                    key.SetValue(valueXName, gameWindowX, RegistryValueKind.DWord);
                    key.SetValue(valueYName, gameWindowY, RegistryValueKind.DWord);
                }
            }
        }

        #endregion


    }
}