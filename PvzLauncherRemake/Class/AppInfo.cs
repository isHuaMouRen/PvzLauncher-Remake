using PvzLauncherRemake.Class.JsonConfigs;
using System.IO;
using System.Reflection;

namespace PvzLauncherRemake.Class
{
    //全局类
    public static class AppInfo
    {
        //路径
        public static readonly string ExecuteDirectory = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}";//执行目录
        public static readonly string RootDirectory = $"{Path.GetDirectoryName(ExecuteDirectory)}";//顶级目录
        public static readonly string GameDirectory = $"{Path.Combine(ExecuteDirectory, "Games")}";//游戏目录
        public static readonly string TrainerDirectory = $"{Path.Combine(ExecuteDirectory, "Trainer")}";//修改器目录
        public static readonly string TempDiectory = Path.GetTempPath();//临时文件夹
        public static readonly string SaveDirectory = @"C:\ProgramData\PopCap Games\PlantsVsZombies\userdata";//存档文件夹

        //特殊
        public static List<JsonGameInfo.Index> GameList = new List<JsonGameInfo.Index>();//游戏列表
        public static List<JsonTrainerInfo.Index> TrainerList = new List<JsonTrainerInfo.Index>();//修改器
        public static JsonConfig.Index Config = null!;//配置
        
        //字符串
        public static readonly string Version = $"1.0.0-beta.5";//版本
        public static readonly string DownloadIndexUrl = "https://gitee.com/huamouren110/UpdateService/raw/main/PvzLauncherRemake/download.json";//下载索引
        public static readonly string UpdateIndexUrl = "https://gitee.com/huamouren110/UpdateService/raw/main/PvzLauncherRemake/update.json";//更新索引

        //启动参数配置
        public static class Arguments
        {
            public static bool isShell = false;
            public static bool isUpdate = false;
        }
    }
}