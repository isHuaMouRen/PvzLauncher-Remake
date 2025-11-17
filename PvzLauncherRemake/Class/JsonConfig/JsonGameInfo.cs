using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvzLauncherRemake.Class.JsonConfig
{
    public class JsonGameInfo
    {
        public partial class Index
        {
            [JsonProperty("tip")]
            public string Tip { get; set; } = AppInfo.Strings.GameConfigTip;

            [JsonProperty("game_info")]
            public GameInfo GameInfo { get; set; }

            [JsonProperty("record")]
            public Record Record { get; set; }
        }

        public partial class GameInfo
        {
            [JsonProperty("version_type")]
            public string VersionType { get; set; }

            [JsonProperty("version")]
            public string Version { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("dir_name")]
            public string DirName { get; set; }

            [JsonProperty("execute_name")]
            public string ExecuteName { get; set; }
        }

        public partial class Record
        {
            [JsonProperty("first_play")]
            public long FirstPlay { get; set; }

            [JsonProperty("play_time")]
            public long PlayTime { get; set; }

            [JsonProperty("play_count")]
            public long PlayCount { get; set; }
        }
    }
}
