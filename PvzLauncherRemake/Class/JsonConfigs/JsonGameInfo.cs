using Newtonsoft.Json;

namespace PvzLauncherRemake.Class.JsonConfigs
{
    public class JsonGameInfo
    {
        public partial class Index
        {
            [JsonProperty("tip")]
            public string Tip { get; set; } = "此文件为PvzLauncher版本标志文件，请勿移除！";

            [JsonProperty("game_info")]
            public GameInfo GameInfo { get; set; }

            [JsonProperty("record")]
            public Record Record { get; set; }
        }

        public partial class GameInfo
        {
            [JsonProperty("version")]
            public string Version { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("icon")]
            public string Icon { get; set; }

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
