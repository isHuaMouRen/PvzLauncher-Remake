using HuaZi.Library.Json;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using System.IO;
using static PvzLauncherRemake.Class.AppLogger;

namespace PvzLauncherRemake.Utils
{
    public static class GameManager
    {
        /// <summary>
        /// 加载游戏列表
        /// </summary>
        public static async Task LoadGameList()
        {
            await Task.Run(() =>
            {
                logger.Info($"[游戏管理器] 开始加载游戏列表");
                //清理
                AppInfo.GameList.Clear();

                //游戏文件夹
                string[] games = Directory.GetDirectories(AppInfo.GameDirectory);

                foreach (var game in games)
                {
                    string configPath = Path.Combine(game, ".pvzl.json");
                    if (File.Exists(configPath))
                    {
                        logger.Info($"[游戏管理器] 发现有效配置文件: {configPath}");
                        //创建独立存档
                        if (AppInfo.Config.SaveConfig.EnableSaveIsolation && !Directory.Exists(Path.Combine(game, ".save")))
                        {
                            Directory.CreateDirectory(Path.Combine(game, ".save"));
                        }

                        JsonGameInfo.Index configContent = Json.ReadJson<JsonGameInfo.Index>(configPath);
                        
                        AppInfo.GameList.Add(configContent);
                    }
                }

                
            });
            
        }

        /// <summary>
        /// 加载修改器列表
        /// </summary>
        public static async Task LoadTrainerList()
        {
            await Task.Run(() =>
            {
                logger.Info($"[游戏管理器] 开始加载修改器列表");

                //清理
                AppInfo.TrainerList.Clear();

                //游戏文件夹
                string[] trainers = Directory.GetDirectories(AppInfo.TrainerDirectory);

                foreach (var trainer in trainers)
                {
                    string configPath = Path.Combine(trainer, ".pvzl.json");
                    if (File.Exists(configPath))
                    {
                        logger.Info($"[游戏管理器] 发现有效配置文件: {configPath}");
                        JsonTrainerInfo.Index configContent = Json.ReadJson<JsonTrainerInfo.Index>(configPath);
                        
                        AppInfo.TrainerList.Add(configContent);
                    }
                }

                
            });
        }
    }
}
