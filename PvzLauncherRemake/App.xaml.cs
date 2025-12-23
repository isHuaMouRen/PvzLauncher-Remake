using PvzLauncherRemake.Class;
using PvzLauncherRemake.Windows;
using System.Windows;

namespace PvzLauncherRemake
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private WindowSplash? _splash;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _splash = new WindowSplash();
            _splash.ShowAndFadeIn();

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
