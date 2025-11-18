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
                }

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

                EndLoad();
                logger.Info($"PageManage 结束初始化");
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "在加载后初始化 PageManage 发生错误", ex);
            }
        }
        #endregion

        public PageManage() { InitializeComponent(); Initialize(); }

        private void Page_Loaded(object sender, RoutedEventArgs e) { InitializeLoaded(); }

        //选择游戏
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

        //导入游戏
        private async void button_Load_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                logger.Info($"用户点击: {((Button)sender).Content} 按钮");

                //导入游戏总逻辑================================================
                string originPath = null!;
                string targetPath = null!;
                string gameName = null!;
                string[] gameFiles = null!;
                List<string> gameExes = new List<string>();
                string gameExeName = null!;
                JsonGameInfo.Index jsonContent = null!;

                bool isGameNameInputDone = false;
                bool isExeSelectDone = false;

                void cancelLoad() { logger.Info("用户取消游戏导入操作"); isGameNameInputDone = true; isExeSelectDone = true; }
                var openFolderDialog = new OpenFolderDialog
                {
                    Title = "请选择游戏文件夹",
                    Multiselect = false
                };
                var textBox = new TextBox();//游戏名输入框
                var listBox = new ListBox();//选择exe






                if (openFolderDialog.ShowDialog() == true)//用户完成选择
                {
                    originPath = openFolderDialog.FolderName;

                    //输入游戏名
                    while (!isGameNameInputDone)
                    {
                        await DialogManager.ShowDialogAsync(new ContentDialog
                        {
                            Title = "输入游戏名",
                            Content = textBox,
                            PrimaryButtonText = "确定",
                            CloseButtonText = "取消导入",
                            DefaultButton = ContentDialogButton.Primary
                        }, (async () =>
                        {
                            //确定逻辑
                            if (Directory.Exists(System.IO.Path.Combine(AppInfo.GameDirectory, textBox.Text)))
                            {
                                //目标名已存在
                                await DialogManager.ShowDialogAsync(new ContentDialog
                                {
                                    Title = "已有同名游戏!",
                                    Content = $"游戏目录下已有 \"{textBox.Text}\"同名文件夹!",
                                    PrimaryButtonText = "重新输入",
                                    DefaultButton = ContentDialogButton.Primary
                                });
                                //继续执行，重复输入游戏名
                            }
                            else
                            {
                                //不存在，继续保存
                                isGameNameInputDone = true;
                                gameName = textBox.Text;
                                targetPath = System.IO.Path.Combine(AppInfo.GameDirectory, gameName);

                                //复制文件夹
                                StartLoad();
                                await DirectoryManager.CopyDirectoryAsync(originPath, targetPath, ((f) => textBlock_Loading.Text = $"复制文件: {f}"));
                                EndLoad();

                                logger.Info("文件夹复制完成");

                                //检测exe
                                gameFiles = Directory.GetFiles(targetPath);
                                foreach (var file in gameFiles)//过滤.exe文件
                                {
                                    if (file.EndsWith(".exe"))
                                    {
                                        logger.Info("发现.exe文件");
                                        gameExes.Add(System.IO.Path.GetFileName(file));
                                    }
                                }

                                //选择exe
                                if (gameExes.Count == 0)//无exe
                                {
                                    logger.Info("未发现任何exe");
                                    await DialogManager.ShowDialogAsync(new ContentDialog
                                    {
                                        Title = "导入失败",
                                        Content = "游戏目录下没有任何可执行文件",
                                        CloseButtonText = "取消导入"
                                    });
                                    notificationManager.Show(new NotificationContent
                                    {
                                        Title = "提示",
                                        Message = "导入取消，清理文件中...",
                                        Type = NotificationType.Information
                                    });
                                    //清理文件
                                    StartLoad();
                                    await Task.Run(() => Directory.Delete(targetPath, true));
                                    EndLoad();

                                    cancelLoad();
                                }
                                else if (gameExes.Count == 1)//只有一个exe
                                {
                                    gameExeName = gameExes[0];
                                }
                                else//多个exe
                                {
                                    //添加进listBox
                                    listBox.Items.Clear();
                                    foreach (var exe in gameExes)
                                        listBox.Items.Add(exe);

                                    while (!isExeSelectDone)
                                    {
                                        await DialogManager.ShowDialogAsync(new ContentDialog
                                        {
                                            Title = "帮助我们解决问题！",
                                            Content = new StackPanel
                                            {
                                                Children =
                                                {
                                                    new TextBlock
                                                    {
                                                        Text="我们在您的游戏文件夹内发现了多个可执行文件, 请您选择正确的可执行文件!",
                                                        Margin=new Thickness(0,0,0,10)
                                                    },
                                                    listBox
                                                }
                                            },
                                            PrimaryButtonText = "确定",
                                            DefaultButton = ContentDialogButton.Primary
                                        });
                                        if (listBox.SelectedItem != null)
                                        {
                                            isExeSelectDone = true;
                                            gameExeName = (string)listBox.SelectedItem;
                                        }
                                    }

                                }

                                if (gameExes.Count > 0)
                                {
                                    //保存配置文件
                                    jsonContent = new JsonGameInfo.Index
                                    {
                                        GameInfo = new JsonGameInfo.GameInfo
                                        {
                                            ExecuteName = gameExeName,
                                            Name = gameName,
                                            Version = "unknown",
                                            VersionType = "unknown"
                                        },
                                        Record = new JsonGameInfo.Record
                                        {
                                            FirstPlay = DateTimeOffset.Now.ToUnixTimeSeconds(),
                                            PlayCount = 0,
                                            PlayTime = 0
                                        }
                                    };
                                    Json.WriteJson(System.IO.Path.Combine(targetPath, ".pvzl.json"), jsonContent);

                                    notificationManager.Show(new NotificationContent
                                    {
                                        Title = "导入成功",
                                        Message = $"{gameName} 已成功导入您的游戏库!",
                                        Type = NotificationType.Success
                                    });
                                    logger.Info($"{gameName} 已成功导入您的游戏库!");

                                    //当前选择:
                                    AppInfo.Config.CurrentGame = gameName;
                                    ConfigManager.SaveAllConfig();

                                    //刷新页面
                                    this.NavigationService.Refresh();
                                }
                            }


                        }), null, (() =>
                        {
                            //取消逻辑
                            cancelLoad();
                        }));
                    }
                }
                else //用户取消选择
                {

                }

            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "导入游戏时发生错误", ex);
            }
        }
    }
}
