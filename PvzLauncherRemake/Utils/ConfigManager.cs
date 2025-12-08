using HuaZi.Library.Json;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using System.IO;
using static PvzLauncherRemake.Class.AppLogger;

namespace PvzLauncherRemake.Utils
{
    public static class ConfigManager
    {
        public static string ConfigPath = Path.Combine(AppInfo.ExecuteDirectory, "config.json");

        public static void CreateDefaultConfig()
        {
            AppInfo.Config = new JsonConfig.Index();
            SaveConfig();
        }

        public static void SaveConfig()
        {
            try
            {
                Json.WriteJson(ConfigPath, AppInfo.Config);
                logger.Info("[配置管理器] 配置保存成功");
            }
            catch (Exception ex)
            {
                logger.Error($"[配置管理器] 保存配置失败！异常: {ex.Message}");
            }
        }

        public static void LoadConfig()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                {
                    CreateDefaultConfig();
                    logger.Info("[配置管理器] 未找到配置文件，已创建默认配置");
                    return;
                }

                var config = Json.ReadJson<JsonConfig.Index>(ConfigPath);
                if (config == null)
                {
                    logger.Warn("[配置管理器] 配置文件读取结果为 null，使用默认配置");
                    CreateDefaultConfig();
                    return;
                }
                AppInfo.Config = config;
                logger.Info("[配置管理器] 配置加载成功");
            }
            catch (Exception ex)
            {
                logger.Error($"[配置管理器] 读取配置文件失败，使用默认配置。异常: {ex.Message}");
                CreateDefaultConfig();
            }
        }
    }
}
