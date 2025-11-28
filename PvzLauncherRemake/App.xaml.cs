using PvzLauncherRemake.Class;
using System.Configuration;
using System.Data;
using System.Windows;

namespace PvzLauncherRemake
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            if (e.ApplicationExitCode == 0)
                AppLogger.logger.Info($"[应用程序] 应用程序正常退出(ExitCode: {e.ApplicationExitCode})");
            else
                AppLogger.logger.Warn($"[应用程序] 应用程序非正常退出(ExitCode: {e.ApplicationExitCode})");
        }
    }

}
