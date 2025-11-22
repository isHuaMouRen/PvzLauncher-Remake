using PvzLauncherRemake.Class.JsonConfigs;
using System.IO;
using System.Reflection;

namespace PvzLauncherRemake.Class
{
    //全局类
    public static class AppInfo
    {
        public static readonly string ExecutePath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}";//执行目录
        public static readonly string Version = $"1.0.0-alpha.12";//版本
        public static readonly string RootPath = $"{Path.GetDirectoryName(ExecutePath)}";//顶级目录
        public static readonly string GameDirectory = $"{Path.Combine(ExecutePath, "Games")}";//游戏目录
        public static List<JsonGameInfo.Index> GameList = new List<JsonGameInfo.Index>();//游戏列表
        public static JsonConfig.Index Config = null!;//配置
        public static readonly string DownloadIndexUrl = "https://gitee.com/huamouren110/UpdateService/raw/main/PvzLauncherRemake/download.json";//下载索引
        public static readonly string TempPath = Path.GetTempPath();//临时文件夹

        //启动参数配置
        public static class Arguments
        {
            public static bool isShell = false;
        }
    }
}