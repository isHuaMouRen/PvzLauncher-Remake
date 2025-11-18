using HuaZi.Library.Json;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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
                logger.Info("开始加载游戏列表");
                //清理
                AppInfo.GameList.Clear();

                //游戏文件夹
                string[] games = Directory.GetDirectories(AppInfo.GameDirectory);

                foreach (var game in games)
                {
                    string configPath = Path.Combine(game, ".pvzl.json");
                    if (File.Exists(configPath))
                    {
                        JsonGameInfo.Index configContent = Json.ReadJson<JsonGameInfo.Index>(configPath);
                        logger.Info($"找到游戏配置: {Path.GetFileName(game)}");
                        AppInfo.GameList.Add(configContent);
                    }
                }

                logger.Info("加载游戏列表结束");
            });
            
        }
    }
}
