using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvzLauncherRemake.Class.JsonConfigs
{
    public class JsonConfig
    {
        public class Index
        {
            [JsonProperty("current_game")]
            public string CurrentGame { get; set; }
        }
    }
}
