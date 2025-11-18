using HuaZi.Library.Downloader;
using HuaZi.Library.Json;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using PvzLauncherRemake.Controls;
using PvzLauncherRemake.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static PvzLauncherRemake.Class.AppLogger;

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageDownload.xaml 的交互逻辑
    /// </summary>
    public partial class PageDownload : ModernWpf.Controls.Page
    {
        private JsonDownloadIndex.Index DownloadIndex = null!;
        private Downloader downloader = null!;

        #region Load
        public void StartLoad()
        {
            tabControl_Main.IsEnabled = false;
            tabControl_Main.Effect = new BlurEffect { Radius = 10 };
            grid_Loading.Visibility = Visibility.Visible;
        }
        public void EndLoad()
        {
            tabControl_Main.IsEnabled = true;
            tabControl_Main.Effect = null;
            grid_Loading.Visibility = Visibility.Hidden;
        }
        #endregion

        #region Init
        public void Initialize() { }
        public async void InitializeLoaded()
        {
            try
            {
                logger.Info("PageDownload 开始初始化");
                StartLoad();

                using (var client = new HttpClient())
                    DownloadIndex = Json.ReadJson<JsonDownloadIndex.Index>(await client.GetStringAsync(AppInfo.DownloadIndexUrl));

                //中文原版
                listBox_zhOrigin.Items.Clear();
                foreach (var zhOriginGame in DownloadIndex.ZhOrigin)
                {
                    var card = new UserDownloadCard
                    {
                        Title = zhOriginGame.Name,
                        Icon = "origin",
                        Version = zhOriginGame.Version,
                        isNew = zhOriginGame.IsNew,
                        isRecommend = zhOriginGame.IsRecommend,
                        Tag = zhOriginGame
                    };
                    logger.Info($"添加游戏: {card.Title}");
                    listBox_zhOrigin.Items.Add(card);
                }

                //中文改版
                listBox_zhRevision.Items.Clear();
                foreach (var zhRevisionGame in DownloadIndex.ZhRevision)
                {
                    var card = new UserDownloadCard
                    {
                        Title = zhRevisionGame.Name,
                        Icon = zhRevisionGame.Version.StartsWith("β") ? "beta" : "origin",
                        Version = zhRevisionGame.Version,
                        isNew = zhRevisionGame.IsNew,
                        isRecommend = zhRevisionGame.IsRecommend,
                        Tag = zhRevisionGame
                    };
                    logger.Info($"添加游戏: {card.Title}");
                    listBox_zhRevision.Items.Add(card);
                }

                //英文原版
                listBox_enOrigin.Items.Clear();
                foreach (var enOriginGame in DownloadIndex.EnOrigin)
                {
                    var card = new UserDownloadCard
                    {
                        Title = enOriginGame.Name,
                        Icon = "origin",
                        Version = enOriginGame.Version,
                        isNew = enOriginGame.IsNew,
                        isRecommend = enOriginGame.IsRecommend,
                        Tag = enOriginGame
                    };
                    logger.Info($"添加游戏: {card.Title}");
                    listBox_enOrigin.Items.Add(card);
                }

                EndLoad();
                logger.Info("PageDownload 结束初始化");
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "加载后初始化 PageDownload 发生错误", ex);
            }
        }
        #endregion

        public PageDownload()
        {
            InitializeComponent();
            Initialize();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) { InitializeLoaded(); }
    }
}
