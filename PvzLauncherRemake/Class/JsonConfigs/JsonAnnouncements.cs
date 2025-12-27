using Newtonsoft.Json;

namespace PvzLauncherRemake.Class.JsonConfigs
{
    public class JsonAnnouncements
    {
        public class Index
        {
            [JsonProperty("announcements")]
            public AnnouncementInfo[] Announcements { get; set; }
        }

        public class AnnouncementInfo
        {
            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("date")]
            public string Date { get; set; }

            [JsonProperty("content")]
            public string Content { get; set; }
        }
    }
}
