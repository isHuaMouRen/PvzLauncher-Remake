using System.IO;
using HuaZi.Library.Logger;

namespace PvzLauncherRemake.Class
{
    public static class AppLogger
    {
        //日志记录器
        public static Logger logger = new Logger
        {
            LogDirectory = Path.Combine(AppInfo.ExecuteDirectory, "Logs"),
            ShowCallerInfo = false,
            ShowDate = false
        };
    }
}