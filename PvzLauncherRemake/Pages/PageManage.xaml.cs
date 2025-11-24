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
using System.Drawing;
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
                listBox_Trainer.Items.Clear();
                //加载列表
                await GameManager.LoadGameList();
                await GameManager.LoadTrainerList();

                //游戏库里有东西才加
                if (AppInfo.GameList.Count > 0)
                {
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
                            Icon = game.GameInfo.Version.StartsWith("β") ? "beta" : "origin",
                            isCurrent = game.GameInfo.Name == AppInfo.Config.CurrentGame ? true : false,
                            Version = $"{version} {game.GameInfo.Version}", //拼接，示例:"英文原版 1.0.0.1051"
                            Background = System.Windows.Media.Brushes.Transparent,
                            Tag = game
                        };
                        card.PreviewMouseDoubleClick += SelectGame;
                        card.PreviewMouseRightButtonDown += SetGame;
                        listBox.Items.Add(card);//添加
                        logger.Info($"添加卡片: 标题: {card.Title} 版本: {card.Version}");
                    }
                }
                else
                {
                    AppInfo.Config.CurrentGame = null!;
                    if (AppInfo.Config.LauncherConfig.DownloadTip.ShowGameDownloadTip)
                        await DialogManager.ShowDialogAsync(new ContentDialog
                        {
                            Title = "提示",
                            Content = "您的游戏库内还没有游戏！快去下载页面下载或导入一个游戏吧！",
                            PrimaryButtonText = "去下载",
                            SecondaryButtonText = "去导入",
                            CloseButtonText = "稍后",
                            DefaultButton = ContentDialogButton.Primary
                        }, (() =>
                        {
                            NavigationController.Navigate(this, "Download");
                        }), (() =>
                        {
                            button_Load_Click(button_Load, null!);
                        }));
                }

                //添加修改器
                //游戏库里有东西才加
                if (AppInfo.TrainerList.Count > 0)
                {
                    //添加卡片
                    foreach (var trainer in AppInfo.TrainerList)
                    {
                        //定义卡片
                        var card = new UserTrainerCard
                        {
                            Title = trainer.Name,
                            Icon = Icon.ExtractAssociatedIcon(System.IO.Path.Combine(AppInfo.TrainerDirectory,trainer.Name,trainer.ExecuteName))!,
                            isCurrent = trainer.Name == AppInfo.Config.CurrentTrainer ? true : false,
                            Version = $"{trainer.Version}", //拼接，示例:"英文原版 1.0.0.1051"
                            Background = System.Windows.Media.Brushes.Transparent,
                            Tag = trainer
                        };
                        card.PreviewMouseDoubleClick += SelectTrainer;
                        //card.PreviewMouseRightButtonDown += SetGame;
                        listBox_Trainer.Items.Add(card);//添加
                        logger.Info($"添加修改器卡片: 标题: {card.Title} 版本: {card.Version}");
                    }
                }
                else
                {
                    AppInfo.Config.CurrentTrainer = null!;
                    if (AppInfo.Config.LauncherConfig.DownloadTip.ShowTrainerDownloadTip)
                        await DialogManager.ShowDialogAsync(new ContentDialog
                        {
                            Title = "提示",
                            Content = "您的修改器库内还没有修改器！快去下载页面下载吧！",
                            PrimaryButtonText = "去下载",
                            CloseButtonText = "稍后",
                            DefaultButton = ContentDialogButton.Primary
                        }, (() =>
                        {
                            NavigationController.Navigate(this, "Download");
                        }));
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

        public PageManage() { InitializeComponent(); Initialize(); }

        private void Page_Loaded(object sender, RoutedEventArgs e) { InitializeLoaded(); }

        //选择游戏
        private void SelectGame(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (listBox.SelectedItem != null)
                {
                    notificationManager.Show(new NotificationContent
                    {
                        Title = "选择游戏",
                        Message = $"已选择 \"{((UserGameCard)sender).Title}\" 作为启动游戏",
                        Type = NotificationType.Information
                    });

                    //更新控件
                    foreach (var card in listBox.Items)
                    {
                        ((UserGameCard)card).isCurrent = (card == sender);
                        ((UserGameCard)card).SetLabels();
                    }

                    logger.Info($"用户选择游戏: {((UserGameCard)sender).Title}");
                    AppInfo.Config.CurrentGame = $"{((UserGameCard)sender).Title}";
                    ConfigManager.SaveAllConfig();
                }
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "处理选择事件发生错误", ex);
            }
        }

        //选择修改器
        private void SelectTrainer(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (listBox_Trainer.SelectedItem != null)
                {
                    notificationManager.Show(new NotificationContent
                    {
                        Title = "选择修改器",
                        Message = $"已选择 \"{((UserTrainerCard)sender).Title}\" 作为当前修改器",
                        Type = NotificationType.Information
                    });

                    //更新控件
                    foreach (var card in listBox_Trainer.Items)
                    {
                        ((UserTrainerCard)card).isCurrent = (card == sender);
                        ((UserTrainerCard)card).SetLabels();
                    }

                    logger.Info($"用户选择修改器: {((UserTrainerCard)sender).Title}");
                    AppInfo.Config.CurrentTrainer = $"{((UserTrainerCard)sender).Title}";
                    ConfigManager.SaveAllConfig();
                }
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "处理选择事件发生错误", ex);
            }
        }


        //设置游戏
        private void SetGame(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.NavigationService.Navigate(new PageManageSet((JsonGameInfo.Index)((UserGameCard)sender).Tag));
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "在处理选择事件时发生错误", ex);
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
