using System.IO;
using System.Reflection;

namespace PvzLauncherRemake.Class
{
    //全局类
    public static class AppInfo
    {
        public static readonly string ExecutePath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}";//执行目录
        public static readonly string Version = $"1.0.0-alpha.2";//版本
        public static readonly string[] Arguments = Environment.GetCommandLineArgs();//启动参数
    }
}