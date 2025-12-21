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
            AppLogger.logger.Warn($"[应用程序] 应用程序{(e.ApplicationExitCode != 0 ? "非" : null)}正常退出(ExitCode: {e.ApplicationExitCode})");
        }
    }

}
