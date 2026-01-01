using ModernWpf;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Utils.Configuration;
using PvzLauncherRemake.Utils.Services;
using System.IO;
using System.Windows;

namespace PvzLauncherRemake
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
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

        private async void InitializeLoaded()
        {

        }
        #endregion

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Initialize();

            var mainWindow = new WindowMain();
            this.MainWindow = mainWindow;

            InitializeLoaded();

            mainWindow.Show();
            mainWindow.Activate();
            mainWindow.Focus();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            AppLogger.logger.Warn($"[应用程序] 应用程序{(e.ApplicationExitCode != 0 ? "非" : null)}正常退出(ExitCode: {e.ApplicationExitCode})");
        }
    }

}
