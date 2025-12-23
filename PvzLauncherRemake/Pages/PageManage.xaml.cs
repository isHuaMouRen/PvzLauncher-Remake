using HuaZi.Library.Json;
using Microsoft.Win32;
using ModernWpf.Controls;
using Newtonsoft.Json;
using Notifications.Wpf;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using PvzLauncherRemake.Controls;
using PvzLauncherRemake.Utils;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
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
        public async void Initialize()
        {
            try
            {
                logger.Info($"[管理] 开始初始化...");
                StartLoad();

                //清理
                stackPanel_Game.Children.Clear();
                stackPanel_Trainer.Children.Clear();
                //加载列表
                logger.Info($"[管理] 开始加载游戏列表");
                await GameManager.LoadGameListAsync();
                await GameManager.LoadTrainerListAsync();
                logger.Info($"[管理] 游戏列表加载完毕");

                //游戏库里有东西才加
                if (AppGlobals.GameList.Count > 0)
                {
                    logger.Info($"[管理] 开始添加卡片");
                    //添加卡片
                    foreach (var game in AppGlobals.GameList)
                    {
                        //判断版本
                        string version =
                            game.GameInfo.VersionType == "en_origin" ? "英文原版" :
                            game.GameInfo.VersionType == "zh_origin" ? "中文原版" :
                            game.GameInfo.VersionType == "zh_revision" ? "中文改版" : "未知";
                        //定义卡片
                        var card = new UserCard
                        {
                            Title = game.GameInfo.Name,
                            Icon = GameManager.ParseToGameIcons(game.GameInfo.Icon),
                            isActive = game.GameInfo.Name == AppGlobals.Config.CurrentGame ? true : false,
                            Version = $"{version} {game.GameInfo.Version}", //拼接，示例:"英文原版 1.0.0.1051"
                            Background = System.Windows.Media.Brushes.Transparent,
                            Tag = game,
                            Margin = new Thickness(0, 0, 0, 5)
                        };
                        card.MouseDoubleClick += SelectGame;
                        card.MouseRightButtonUp += SetGame;
                        logger.Info($"[管理] 添加游戏: Title=\"{card.Title}\"  Icon=\"{card.Icon}\"  isCurrent=\"{card.isActive}\"  Version=\"{card.Version}\"");
                        stackPanel_Game.Children.Add(card);//添加

                    }
                }
                else
                {
                    logger.Warn($"[管理] 在游戏库内未发现任何游戏");
                    AppGlobals.Config.CurrentGame = null!;
                    if (AppGlobals.Config.LauncherConfig.DownloadTip.ShowGameDownloadTip)
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
                if (AppGlobals.TrainerList.Count > 0)
                {
                    logger.Info($"[管理] 添加修改器列表");
                    //添加卡片
                    foreach (var trainer in AppGlobals.TrainerList)
                    {
                        //定义卡片
                        var card = new UserCard
                        {
                            Title = trainer.Name,
                            Icon = GameManager.ParseToGameIcons(trainer.Icon),
                            isActive = trainer.Name == AppGlobals.Config.CurrentTrainer ? true : false,
                            Version = $"{trainer.Version}", //拼接，示例:"英文原版 1.0.0.1051"
                            Background = System.Windows.Media.Brushes.Transparent,
                            Tag = trainer,
                            Margin = new Thickness(0, 0, 0, 5)
                        };
                        card.MouseDoubleClick += SelectTrainer;
                        card.MouseRightButtonUp += SetTrainer;
                        logger.Info($"[管理] 添加修改器: Title=\"{card.Title}\"  Icon=\"{card.Icon}\"  isCurrent=\"{card.isActive}\"  Version=\"{card.Version}\"");
                        stackPanel_Trainer.Children.Add(card);//添加

                    }
                }
                else
                {
                    logger.Warn($"[管理] 未发现任何修改器");
                    AppGlobals.Config.CurrentTrainer = null!;
                    if (AppGlobals.Config.LauncherConfig.DownloadTip.ShowTrainerDownloadTip)
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
                logger.Info($"[管理] ");
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "在加载后初始化 PageManage 发生错误", ex);
            }
        }
        #endregion

        //tab动画
        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsInitialized)
            {
                if (e.OriginalSource != sender)
                    return;

                var selectItem = ((TabControl)sender).SelectedContent;
                UserScrollViewer animControl = null!;

                if (selectItem is UserScrollViewer)
                {
                    animControl = (UserScrollViewer)selectItem;
                }
                else
                {
                    return;
                }

                animControl.BeginAnimation(MarginProperty, null);
                animControl.BeginAnimation(OpacityProperty, null);

                animControl.Margin = new Thickness(0, 25, 0, 0);
                animControl.Opacity = 0;

                var margniAnim = new ThicknessAnimation
                {
                    To = new Thickness(0),
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
                };
                var opacAnim = new DoubleAnimation
                {
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
                };
                animControl.BeginAnimation(MarginProperty, margniAnim);
                animControl.BeginAnimation(OpacityProperty, opacAnim);
            }
        }

        public PageManage() => InitializeComponent();

        private void Page_Loaded(object sender, RoutedEventArgs e) => Initialize();

        //选择游戏
        private void SelectGame(object sender, MouseButtonEventArgs e)
        {
            try
            {
                notificationManager.Show(new NotificationContent
                {
                    Title = "选择游戏",
                    Message = $"已选择 \"{((UserCard)sender).Title}\" 作为启动游戏",
                    Type = NotificationType.Information
                });

                //更新控件
                foreach (var card in stackPanel_Game.Children)
                {
                    ((UserCard)card).isActive = (card == sender);
                    ((UserCard)card).SetLabels();
                }

                AppGlobals.Config.CurrentGame = $"{((UserCard)sender).Title}";
                ConfigManager.SaveConfig();
                logger.Info($"[管理] 选择游戏: {AppGlobals.Config.CurrentGame}");
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
                notificationManager.Show(new NotificationContent
                {
                    Title = "选择修改器",
                    Message = $"已选择 \"{((UserCard)sender).Title}\" 作为当前修改器",
                    Type = NotificationType.Information
                });

                //更新控件
                foreach (var card in stackPanel_Trainer.Children)
                {
                    ((UserCard)card).isActive = (card == sender);
                    ((UserCard)card).SetLabels();
                }


                AppGlobals.Config.CurrentTrainer = $"{((UserCard)sender).Title}";
                ConfigManager.SaveConfig();
                logger.Info($"[管理] 选择修改器: {AppGlobals.Config.CurrentTrainer}");
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
                this.NavigationService.Navigate(new PageManageSet((JsonGameInfo.Index)((UserCard)sender).Tag));
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "在处理选择事件时发生错误", ex);
            }
        }

        //设置修改器
        private async void SetTrainer(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var trainerConfig = (JsonTrainerInfo.Index)(((UserCard)sender).Tag);

                logger.Info($"[管理: 修改器设置] 开始设置修改器");
                //控件
                var buttonDelete = new Button
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(0, 0, 0, 10),
                    Foreground = new SolidColorBrush(Color.FromRgb(255, 100, 100)),
                    Content = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new ModernWpf.Controls.PathIcon
                            {
                                Width=15,
                                Height=15,
                                Data=Geometry.Parse("M280-120q-33 0-56.5-23.5T200-200v-520h-40v-80h200v-40h240v40h200v80h-40v520q0 33-23.5 56.5T680-120H280Zm400-600H280v520h400v-520ZM360-280h80v-360h-80v360Zm160 0h80v-360h-80v360ZM280-720v520-520Z"),
                                Margin=new Thickness(0,0,5,0)
                            },
                            new TextBlock
                            {
                                Text="删除修改器"
                            }
                        }
                    }
                };
                var buttonRename = new Button
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(0, 0, 0, 10),
                    Content = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new ModernWpf.Controls.PathIcon
                            {
                                Width=15,
                                Height=15,
                                Data=Geometry.Parse("M200-200h57l391-391-57-57-391 391v57Zm-80 80v-170l528-527q12-11 26.5-17t30.5-6q16 0 31 6t26 18l55 56q12 11 17.5 26t5.5 30q0 16-5.5 30.5T817-647L290-120H120Zm640-584-56-56 56 56Zm-141 85-28-29 57 57-29-28Z"),
                                Margin=new Thickness(0,0,5,0)
                            },
                            new TextBlock
                            {
                                Text="修改名称"
                            }
                        }
                    }
                };
                var buttonOpenFolder = new Button
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(0, 0, 0, 10),
                    Content = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new ModernWpf.Controls.PathIcon
                            {
                                Width=15,
                                Height=15,
                                Data=Geometry.Parse("M160-160q-33 0-56.5-23.5T80-240v-480q0-33 23.5-56.5T160-800h240l80 80h320q33 0 56.5 23.5T880-640H447l-80-80H160v480l96-320h684L837-217q-8 26-29.5 41.5T760-160H160Zm84-80h516l72-240H316l-72 240Zm0 0 72-240-72 240Zm-84-400v-80 80Z"),
                                Margin=new Thickness(0,0,5,0)
                            },
                            new TextBlock
                            {
                                Text="打开文件夹"
                            }
                        }
                    }
                };

                var dialog = new ContentDialog
                {
                    Title = "操作",
                    Content = new StackPanel
                    {
                        Children =
                        {
                            buttonDelete,buttonRename,buttonOpenFolder
                        }
                    },
                    CloseButtonText = "关闭",
                    DefaultButton = ContentDialogButton.Close
                };

                //删除
                buttonDelete!.Click += (async (s, e) =>
                {
                    logger.Info($"[管理: 修改器设置] 开始删除修改器");
                    dialog.Hide();

                    await DialogManager.ShowDialogAsync(new ContentDialog
                    {
                        Title = "确认删除",
                        Content = $"\"{trainerConfig.Name}\" 将被删除，一旦删除将永久消失(真的很久!)\n\n(此操作仅有这一次确认机会，点击删除按钮立即执行删除程序！)",
                        PrimaryButtonText = "删除",
                        CloseButtonText = "取消",
                        DefaultButton = ContentDialogButton.Close
                    }, (async () =>
                    {
                        logger.Info($"[管理: 修改器设置] 用户同意删除，开始删除...");

                        await Task.Run(() => Directory.Delete(Path.Combine(AppGlobals.TrainerDirectory, trainerConfig.Name), true));
                        logger.Info($"[管理: 修改器设置] 删除完毕");
                        await GameManager.LoadTrainerListAsync();

                        if (AppGlobals.TrainerList.Count > 0 && AppGlobals.Config.CurrentTrainer == trainerConfig.Name)
                        {
                            AppGlobals.Config.CurrentTrainer = AppGlobals.TrainerList[0].Name;
                        }
                        else
                        {
                            AppGlobals.Config.CurrentTrainer = null!;
                        }

                        notificationManager.Show(new NotificationContent
                        {
                            Title = "删除成功",
                            Message = $"\"{trainerConfig.Name}\" 已从您的修改器库内移除!",
                            Type = NotificationType.Success
                        });

                        this.NavigationService.Refresh();
                    }));
                });

                //重命名
                buttonRename!.Click += (async (s, e) =>
                {
                    logger.Info($"[管理: 修改器设置] 开始重命名...");
                    dialog.Hide();
                    var textBox = new TextBox
                    {
                        Text = trainerConfig.Name
                    };

                    await DialogManager.ShowDialogAsync(new ContentDialog
                    {
                        Title = "重命名",
                        Content = textBox,
                        PrimaryButtonText = "确定",
                        CloseButtonText = "取消",
                        DefaultButton = ContentDialogButton.Primary
                    }, (() =>
                    {
                        if (textBox.Text != null)
                        {
                            if (!Directory.Exists(Path.Combine(AppGlobals.TrainerDirectory, textBox.Text)))
                            {
                                string lastName = trainerConfig.Name;
                                trainerConfig.Name = textBox.Text;
                                Directory.Move(Path.Combine(AppGlobals.TrainerDirectory, lastName), Path.Combine(AppGlobals.TrainerDirectory, trainerConfig.Name));
                                Json.WriteJson(Path.Combine(AppGlobals.TrainerDirectory, trainerConfig.Name, ".pvzl.json"), trainerConfig);
                                notificationManager.Show(new NotificationContent
                                {
                                    Title = "更名成功",
                                    Message = $"修改器已更名为: {trainerConfig.Name}",
                                    Type = NotificationType.Success
                                });

                                logger.Info($"[管理: 修改器设置] 更名成功: {trainerConfig.Name}");

                                if (AppGlobals.Config.CurrentTrainer == lastName)
                                    AppGlobals.Config.CurrentTrainer = trainerConfig.Name;

                                this.NavigationService.Refresh();
                            }
                            else
                            {
                                logger.Info($"[管理: 修改器设置] {textBox.Text} 在库内已有相同名称，操作取消");
                                notificationManager.Show(new NotificationContent
                                {
                                    Title = "更名失败",
                                    Message = $"库内已有与 \"{textBox.Text}\" 同名修改器！",
                                    Type = NotificationType.Error
                                });
                            }
                        }
                        else
                        {
                            logger.Info($"[管理: 修改器设置] 用户输入为空，操作取消");
                            notificationManager.Show(new NotificationContent
                            {
                                Title = "更名失败",
                                Message = "新名称为空",
                                Type = NotificationType.Error
                            });
                        }
                    }));
                });

                //打开文件夹
                buttonOpenFolder!.Click += ((s, e) =>
                {
                    dialog.Hide();
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = Path.Combine(AppGlobals.TrainerDirectory, trainerConfig.Name),
                        UseShellExecute = true
                    });
                });

                await DialogManager.ShowDialogAsync(dialog);

            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", null!, ex);
            }
        }

        //导入游戏
        //我已经懒得碰这坨该死的屎山了，能跑就完事了
        // :(
        private async void button_Load_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                logger.Info($"[管理] 开始导入游戏");

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

                void cancelLoad() { isGameNameInputDone = true; isExeSelectDone = true; }
                var openFolderDialog = new OpenFolderDialog
                {
                    Title = "请选择游戏文件夹",
                    Multiselect = false
                };
                var textBox = new System.Windows.Controls.TextBox();//游戏名输入框
                var listBox = new System.Windows.Controls.ListBox();//选择exe


                if (openFolderDialog.ShowDialog() == true)//用户完成选择
                {
                    originPath = openFolderDialog.FolderName;
                    logger.Info($"[管理] 原文件夹: {originPath}");
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
                            if (Directory.Exists(System.IO.Path.Combine(AppGlobals.GameDirectory, textBox.Text)))
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
                                logger.Info($"[管理] 游戏名: {textBox.Text}");
                                //不存在，继续保存
                                isGameNameInputDone = true;
                                gameName = textBox.Text;
                                targetPath = System.IO.Path.Combine(AppGlobals.GameDirectory, gameName);

                                //复制文件夹
                                logger.Info($"[管理] 开始复制游戏");
                                StartLoad();
                                await DirectoryManager.CopyDirectoryAsync(originPath, targetPath, ((f) => textBlock_Loading.Text = $"复制文件: {f}"));
                                EndLoad();
                                logger.Info($"[管理] 复制结束");



                                //检测exe
                                gameFiles = Directory.GetFiles(targetPath);
                                foreach (var file in gameFiles)//过滤.exe文件
                                {
                                    if (file.EndsWith(".exe"))
                                    {
                                        logger.Info($"[管理] 检测到exe文件: {file}");
                                        gameExes.Add(System.IO.Path.GetFileName(file));
                                    }
                                }

                                //选择exe
                                if (gameExes.Count == 0)//无exe
                                {
                                    logger.Info($"[管理] 未发现exe。开始清理");
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
                                    logger.Info($"[管理] 仅检测到一个exe");
                                    gameExeName = gameExes[0];
                                }
                                else//多个exe
                                {
                                    logger.Info($"[管理] 检测到多个exe。开始解决问题");
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
                                            logger.Info($"[管理] 选择exe: {gameExeName}");
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
                                    logger.Info($"[管理] 保存游戏配置文件: {JsonConvert.SerializeObject(jsonContent)}");
                                    Json.WriteJson(System.IO.Path.Combine(targetPath, ".pvzl.json"), jsonContent);

                                    notificationManager.Show(new NotificationContent
                                    {
                                        Title = "导入成功",
                                        Message = $"{gameName} 已成功导入您的游戏库!",
                                        Type = NotificationType.Success
                                    });


                                    //当前选择:
                                    AppGlobals.Config.CurrentGame = gameName;
                                    ConfigManager.SaveConfig();

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
