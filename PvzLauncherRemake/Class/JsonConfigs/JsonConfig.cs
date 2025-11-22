using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
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
            public string CurrentGame { get; set; } = null!;

            [JsonProperty("launcher_config")]
            public LauncherConfig LauncherConfig { get; set; } = new LauncherConfig();
        }

        public class LauncherConfig
        {
            [JsonProperty("window_size")]
            public WindowSize WindowSize { get; set; } = new WindowSize();

            [JsonProperty("launched_operate")]
            public string LaunchedOperate { get; set; } = "None";

            [JsonProperty("window_title")]
            public string WindowTitle { get; set; } = "Plants Vs. Zombies Launcher";

            [JsonProperty("title_image")]
            public string TitleImage { get; set; } = "EN";

            [JsonProperty("background")]
            public string Background { get; set; } = null!;

            [JsonProperty("navigation_view_align")]
            public string NavigationViewAlign { get; set; } = "Left";
        }

        public class WindowSize
        {
            [JsonProperty("width")]
            public double Width { get; set; } = 800;

            [JsonProperty("height")]
            public double Height { get; set; } = 450;
        }
    }
}
