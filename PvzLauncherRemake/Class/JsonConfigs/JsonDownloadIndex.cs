using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }

        public partial class GameInfo
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("version_type")]
            public string VersionType { get; set; }

            [JsonProperty("version")]
            public string Version { get; set; }

            [JsonProperty("execute_name")]
            public string ExecuteName { get; set; }

            [JsonProperty("is_recommend")]
            public bool IsRecommend { get; set; }

            [JsonProperty("is_new")]
            public bool IsNew { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }
        }
    }
}
