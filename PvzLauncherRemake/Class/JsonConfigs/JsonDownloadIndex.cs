using Newtonsoft.Json;

namespace PvzLauncherRemake.Class.JsonConfigs
{
    public class JsonDownloadIndex
    {
        public partial class Index
        {
            [JsonProperty("en_origin")]
            public GameInfo[] EnOrigin { get; set; }

            [JsonProperty("zh_origin")]
            public GameInfo[] ZhOrigin { get; set; }

            [JsonProperty("zh_revision")]
            public GameInfo[] ZhRevision { get; set; }

            [JsonProperty("trainer")]
            public TrainerInfo[] Trainer { get; set; }
        }

        public partial class GameInfo
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("screenshot")]
            public string Screenshot { get; set; }

            [JsonProperty("icon")]
            public string Icon { get; set; }

            [JsonProperty("version_type")]
            public string VersionType { get; set; }

            [JsonProperty("version")]
            public string Version { get; set; }

            [JsonProperty("size")]
            public double Size { get; set; } = 0;//MB

            [JsonProperty("execute_name")]
            public string ExecuteName { get; set; }

            [JsonProperty("is_recommend")]
            public bool IsRecommend { get; set; }

            [JsonProperty("is_new")]
            public bool IsNew { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }
        }

        public partial class TrainerInfo : GameInfo
        {
            [JsonProperty("support_version")]
            public string SupportVersion { get; set; }
        }
    }
}
