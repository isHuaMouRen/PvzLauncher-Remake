using HuaZi.Library.Logger;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Controls;
using PvzLauncherRemake.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// PageManage.xaml 的交互逻辑
    /// </summary>
    public partial class PageManage : ModernWpf.Controls.Page
    {
        #region Loads
        public void StartLoad()
        {
            grid_Loading.Visibility = Visibility.Visible;
            grid_Main.IsEnabled = false;
            grid_Main.Effect = new BlurEffect { Radius = 10 };
        }

        public void EndLoad()
        {
            grid_Loading.Visibility = Visibility.Hidden;
            grid_Main.IsEnabled = true;
            grid_Main.Effect = null;
        }
        #endregion

        #region Initialize
        public void Initialize() { }
        public async void InitializeLoaded()
        {
            try
            {
                logger.Info($"PageManage 开始初始化");
                StartLoad();

                //清理
                listBox.Items.Clear();
                //加载游戏列表
                await GameManager.LoadGameList();

                //添加卡片
                foreach (var game in AppInfo.GameList)
                {
                    //判断版本
                    string version = 
                        game.GameInfo.VersionType == "en_origin" ? "英文原版" : 
                        game.GameInfo.VersionType == "zh_origin" ? "中文原版" : 
                        game.GameInfo.VersionType == "zh_revision" ? "中文改版" : "未知";
                    //定义卡片
                    var card = new UserGameCard
                    {
                        Title = game.GameInfo.Name,
                        Version = $"{version} {game.GameInfo.Version}", //拼接，示例:"英文原版 1.0.0.1051"
                        Tag = game
                    };
                    listBox.Items.Add(card);//添加
                    logger.Info($"添加卡片: 标题: {card.Title} 版本: {card.Version}");
                }

                EndLoad();
                logger.Info($"PageManage 结束初始化");
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "在加载后初始化 PageManage 发生错误", ex);
            }
        }
        #endregion

        public PageManage()
        {
            InitializeComponent();
            Initialize();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) { InitializeLoaded(); }
    }
}
