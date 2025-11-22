using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvzLauncherRemake.Class.JsonConfigs
{
    public class JsonUpdateIndex
    {
        public class Index
        {
            [JsonProperty("stable")]   
            public ChannelInfo Stable { get; set; }

            [JsonProperty("development")]
            public ChannelInfo Development { get; set; }
        }

        public class ChannelInfo
        {
            [JsonProperty("latest_version")]
            public string LatestVersion { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }
        }
    }
}
