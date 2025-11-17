using PvzLauncherRemake.Class.JsonConfigs;
using System.IO;
using System.Reflection;

namespace PvzLauncherRemake.Class
{
    //全局类
    public static class AppInfo
    {
        public static readonly string ExecutePath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}";//执行目录
        public static readonly string Version = $"1.0.0-alpha.3";//版本
        public static readonly string RootPath = $"{Path.GetDirectoryName(ExecutePath)}";//顶级目录
        public static readonly string GameDirectory = $"{Path.Combine(ExecutePath, "Games")}";//游戏目录
        public static List<JsonGameInfo.Index> GameList = new List<JsonGameInfo.Index>();//游戏列表
        public static JsonConfig.Index Config = null!;//配置
        
        //启动参数配置
        public static class Arguments
        {
            public static bool isShell = false;
        }

        //字符串常量
        public static class Strings
        {
            public static readonly string GameConfigTip = "此文件为PvzLauncher版本标志文件，请勿移除！";//游戏下.pvzl.json的tip
        }
    }
}