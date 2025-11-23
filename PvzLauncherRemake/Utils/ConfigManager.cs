using HuaZi.Library.Json;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvzLauncherRemake.Utils
{
    public static class ConfigManager
    {
        public static void SaveAllConfig()
        {
            Json.WriteJson(Path.Combine(AppInfo.ExecuteDirectory, "config.json"), AppInfo.Config);
        }

        public static void ReadAllConfig()
        {
            AppInfo.Config = Json.ReadJson<JsonConfig.Index>(Path.Combine(AppInfo.ExecuteDirectory, "config.json"));
        }
    }
}
