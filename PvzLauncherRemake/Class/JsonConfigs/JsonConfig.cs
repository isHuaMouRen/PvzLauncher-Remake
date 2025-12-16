using Newtonsoft.Json;

namespace PvzLauncherRemake.Class.JsonConfigs
{
    public class JsonConfig
    {
        public class Index
        {
            [JsonProperty("current_game")]
            public string CurrentGame { get; set; } = null!;

            [JsonProperty("current_trainer")]
            public string CurrentTrainer { get; set; } = null!;

            [JsonProperty("launcher_config")]
            public LauncherConfig LauncherConfig { get; set; } = new LauncherConfig();

            [JsonProperty("save_config")]
            public SaveConfig SaveConfig { get; set; } = new SaveConfig();

            [JsonProperty("record")]
            public Record Record { get; set; } = new Record();
        }

        public class LauncherConfig
        {
            [JsonProperty("window_size")]
            public WindowSize WindowSize { get; set; } = new WindowSize();

            [JsonProperty("launched_operate")]
            public string LaunchedOperate { get; set; } = "None";

            [JsonProperty("theme")]
            public string Theme { get; set; } = "Light";

            [JsonProperty("window_title")]
            public string WindowTitle { get; set; } = "Plants Vs. Zombies Launcher";

            [JsonProperty("title_image")]
            public string TitleImage { get; set; } = "EN";

            [JsonProperty("background")]
            public string Background { get; set; } = null!;

            [JsonProperty("navigation_view_align")]
            public string NavigationViewAlign { get; set; } = "Top";

            [JsonProperty("update_channel")]
            public string UpdateChannel { get; set; } = "Stable";

            [JsonProperty("start_up_check_update")]
            public bool StartUpCheckUpdate { get; set; } = true;

            [JsonProperty("download_tip")]
            public DownloadTip DownloadTip { get; set; } = new DownloadTip();
        }

        public class SaveConfig
        {
            [JsonProperty("enable_save_isolation")]
            public bool EnableSaveIsolation { get; set; } = false;
        }

        public class WindowSize
        {
            [JsonProperty("width")]
            public double Width { get; set; } = 800;

            [JsonProperty("height")]
            public double Height { get; set; } = 450;
        }

        public class DownloadTip
        {
            [JsonProperty("show_game_download_tip")]
            public bool ShowGameDownloadTip { get; set; } = true;

            [JsonProperty("show_trainer_download_tip")]
            public bool ShowTrainerDownloadTip { get; set; } = false;
        }

        public class Record
        {
            [JsonProperty("launch_count")]
            public int LaunchCount { get; set; } = 0;
        }
    }
}
