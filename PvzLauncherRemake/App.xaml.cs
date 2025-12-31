using HuaZi.Library.Json;
using ModernWpf;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using PvzLauncherRemake.Utils.Configuration;
using PvzLauncherRemake.Utils.Services;
using PvzLauncherRemake.Windows;
using System.IO;
using System.Net.Http;
using System.Windows;

namespace PvzLauncherRemake
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private WindowSplash? _splash;

        #region init
        private async void Initialize()
        {
            //初始化配置文件
            if (!File.Exists(System.IO.Path.Combine(AppGlobals.ExecuteDirectory, "config.json")))
            {
                ConfigManager.CreateDefaultConfig();
            }
            //游戏目录
            if (!Directory.Exists(AppGlobals.GameDirectory))
            {
                Directory.CreateDirectory(AppGlobals.GameDirectory);
            }
            //修改器目录
            if (!Directory.Exists(AppGlobals.TrainerDirectory))
            {
                Directory.CreateDirectory(AppGlobals.TrainerDirectory);
            }

            //读配置
            ConfigManager.LoadConfig();

            //主题
            switch (AppGlobals.Config.LauncherConfig.Theme)
            {
                case "Light":
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light; break;
                case "Dark":
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark; break;
            }

            //切换语言
            LocalizeManager.SwitchLanguage(AppGlobals.Config.LauncherConfig.Language);

            //加载列表
            await GameManager.LoadGameListAsync();
            await GameManager.LoadTrainerListAsync();
        }
        #endregion

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _splash = new WindowSplash();
            _splash.ShowAndFadeIn();

            Initialize();

            Task.Run(async () =>
            {
                await Task.Delay(500);

                await this.Dispatcher.Invoke(async () =>
                {
                    var mainWindow = new WindowMain();
                    this.MainWindow = mainWindow;

                    mainWindow.Show();
                    mainWindow.Activate();
                    mainWindow.Focus();

                    await Task.Delay(500);

                    _splash?.FadeOutAndClose();
                    _splash = null;

                });
            });

        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            AppLogger.logger.Warn($"[应用程序] 应用程序{(e.ApplicationExitCode != 0 ? "非" : null)}正常退出(ExitCode: {e.ApplicationExitCode})");
        }
    }

}
