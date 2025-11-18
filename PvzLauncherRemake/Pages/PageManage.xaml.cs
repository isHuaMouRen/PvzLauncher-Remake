using HuaZi.Library.Json;
using HuaZi.Library.Logger;
using Microsoft.Win32;
using ModernWpf.Controls;
using Newtonsoft.Json;
using Notifications.Wpf;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using PvzLauncherRemake.Controls;
using PvzLauncherRemake.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
        private NotificationManager notificationManager = new NotificationManager();

        #region Copy
        public static async Task CopyDirectoryAsync(string sourceDir, string destDir)
        {

            // 复制当前目录的所有文件（异步）
            foreach (string filePath in Directory.GetFiles(sourceDir))
            {

                string fileName = System.IO.Path.GetFileName(filePath);
                string destFilePath = System.IO.Path.Combine(destDir, fileName);

                await CopyFileAsync(filePath, destFilePath).ConfigureAwait(false);
            }

            // 递归复制子文件夹
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string dirName = System.IO.Path.GetFileName(subDir);
                string destSubDir = System.IO.Path.Combine(destDir, dirName);

                await CopyDirectoryAsync(subDir, destSubDir).ConfigureAwait(false);
            }
        }

        private static async Task CopyFileAsync(string sourcePath, string destinationPath)
        {
            const int bufferSize = 81920;

            using var sourceStream = new FileStream(
                sourcePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize,
                useAsync: true);

            using var destinationStream = new FileStream(
                destinationPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize,
                useAsync: true);

            var fileInfo = new FileInfo(sourcePath);
            await sourceStream.CopyToAsync(destinationStream, bufferSize).ConfigureAwait(false);

            destinationStream.Flush(); // 确保写入完成
        }
        #endregion

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
                    };
                    listBox.Items.Add(card);//添加
                    logger.Info($"添加卡片: 标题: {card.Title} 版本: {card.Version}");

                    //选择卡片
                    bool isChecked = false;
                    if (AppInfo.Config.CurrentGame != null)
                    {
                        logger.Info("当前选择不为空，开始检查项");
                        foreach (var item in listBox.Items)
                        {
                            if ($"{((UserGameCard)item).Title}" == AppInfo.Config.CurrentGame)
                            {
                                isChecked = true;
                                listBox.SelectedItem = item;
                            }
                        }
                    }
                    if (!isChecked)
                        await DialogManager.ShowDialogAsync(new ContentDialog
                        {
                            Title = "发现无效的配置",
                            Content = $"发现无效的配置: \"Index -> CurrentGame\": \"{AppInfo.Config.CurrentGame}\"\n\n找不到目标游戏",
                            PrimaryButtonText = "确定",
                            DefaultButton = ContentDialogButton.Primary
                        });
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

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (listBox.SelectedItem != null)
                {
                    logger.Info($"用户选择游戏: {((UserGameCard)listBox.SelectedItem).Title}");
                    AppInfo.Config.CurrentGame = $"{((UserGameCard)listBox.SelectedItem).Title}";
                    ConfigManager.SaveAllConfig();
                }
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "处理选择事件发生错误", ex);
            }
        }

        private async void button_Load_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                logger.Info($"用户点击: {((Button)sender).Content} 按钮");
                    
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "导入游戏时发生错误", ex);
            }
        }
    }
}
