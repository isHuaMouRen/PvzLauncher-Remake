using HuaZi.Library.Json;
using ModernWpf.Controls;
using Newtonsoft.Json;
using Notifications.Wpf;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using PvzLauncherRemake.Utils;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using static PvzLauncherRemake.Class.AppLogger;

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageManageSet.xaml 的交互逻辑
    /// </summary>
    public partial class PageManageSet : ModernWpf.Controls.Page
    {
        private JsonGameInfo.Index GameInfo = null!;
        private NotificationManager notificationManage = new NotificationManager();

        #region Load
        public void StartLoad(bool isStart = true)
        {
            if (isStart)
            {
                grid_Loading.Visibility = Visibility.Visible;
                stackPanel_main.Effect = new BlurEffect { Radius = 10 };
                stackPanel_main.IsEnabled = false;
            }
            else
            {
                grid_Loading.Visibility = Visibility.Hidden;
                stackPanel_main.Effect = null;
                stackPanel_main.IsEnabled = true;
            }
        }
        public void EndLoad() => StartLoad(false);
        #endregion

        #region Init
        public void Initialize() { }
        public void InitializeLoaded()
        {
            try
            {
                logger.Info($"[游戏设置] 开始初始化...");

                //设置卡片
                userGameCard.Title = GameInfo.GameInfo.Name;
                string version =
                    GameInfo.GameInfo.VersionType == "zh_origin" ? "中文原版" :
                    GameInfo.GameInfo.VersionType == "zh_revison" ? "中文改版" :
                    GameInfo.GameInfo.VersionType == "en_origin" ? "英文原版" : "未知";
                userGameCard.Version = $"{version} {GameInfo.GameInfo.Version}";
                userGameCard.Icon =
                    GameInfo.GameInfo.Version.StartsWith("β") ? "Beta" : "Origin";
                logger.Info($"[游戏设置] 传入的游戏信息: {JsonConvert.SerializeObject(GameInfo)}");

                //统计信息
                textBlock_Record.Text =
                    $"首次启动: {DateTimeOffset.FromUnixTimeSeconds(GameInfo.Record.FirstPlay).ToOffset(TimeSpan.FromHours(8)).ToString()}\n" +
                    $"游玩时间: {(GameInfo.Record.PlayTime >= 60 ? GameInfo.Record.PlayTime / 60 : GameInfo.Record.PlayTime >= (60 * 60) ? GameInfo.Record.PlayTime / (60 * 60) : GameInfo.Record.PlayTime)}{(GameInfo.Record.PlayTime >= 60 ? "分钟" : GameInfo.Record.PlayTime >= (60 * 60) ? "小时" : "秒")}\n" +
                    $"启动次数: {GameInfo.Record.PlayCount}";

                logger.Info($"[游戏设置] 结束初始化");
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "加载后初始化 PageManageSet 发生错误", ex);
            }
        }
        #endregion

        public PageManageSet(JsonGameInfo.Index gameInfo)
        {
            InitializeComponent();
            Initialize();
            Loaded += ((sender, e) => InitializeLoaded());
            GameInfo = gameInfo;
        }

        private void button_OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Directory.Exists(System.IO.Path.Combine(AppInfo.GameDirectory, GameInfo.GameInfo.Name)))
                {
                    
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = System.IO.Path.Combine(AppInfo.GameDirectory, GameInfo.GameInfo.Name),
                        UseShellExecute = true
                    });
                }
                else
                    throw new Exception("目标文件夹不存在");
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", null!, ex);
            }
        }

        private async void button_Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
                await DialogManager.ShowDialogAsync(new ContentDialog
                {
                    Title = "确定操作",
                    Content = $"真的要删除这个游戏吗？\n\"{GameInfo.GameInfo.Name}\" 将会永久消失(真的很久!)",
                    PrimaryButtonText = "删除",
                    CloseButtonText = "取消",
                    DefaultButton = ContentDialogButton.Primary
                }, (async () =>
                {
                    var checkBox = new CheckBox
                    {
                        Content = "我确认永久删除此游戏",
                        IsChecked = false
                    };
                    await DialogManager.ShowDialogAsync(new ContentDialog
                    {
                        Title = "最后一次确认",
                        Content = new StackPanel
                        {
                            Children =
                            {
                                new TextBlock
                                {
                                    Text="这将是最后一次确认，请在下方勾选确认删除！",
                                    Margin=new Thickness(0,0,0,10)
                                },
                                checkBox
                            }
                        },
                        PrimaryButtonText = "确认删除",
                        CloseButtonText = "取消删除",
                        DefaultButton = ContentDialogButton.Primary
                    }, (async () =>
                    {
                        if (checkBox.IsChecked == true)
                        {
                            StartLoad();

                            await Task.Run(() => Directory.Delete(System.IO.Path.Combine(AppInfo.GameDirectory, GameInfo.GameInfo.Name), true));

                            notificationManage.Show(new NotificationContent
                            {
                                Title = "删除成功",
                                Message = $"\"{GameInfo.GameInfo.Name}\" 已成功从您的游戏库中移除",
                                Type = NotificationType.Success
                            });
                            //刷新游戏列表
                            await GameManager.LoadGameListAsync();

                            if (AppInfo.GameList.Count > 0 && AppInfo.Config.CurrentGame == GameInfo.GameInfo.Name)
                            {
                                AppInfo.Config.CurrentGame = AppInfo.GameList[0].GameInfo.Name;
                            }
                            else if (AppInfo.GameList.Count == 0)
                            {
                                AppInfo.Config.CurrentGame = null!;
                            }

                            this.NavigationService.GoBack();

                            EndLoad();
                        }
                    }));
                }));
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", null!, ex);
            }
        }

        private async void button_ChangeVersion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                

                //VersionType
                var comboBox = new ComboBox
                {
                    IsReadOnly = true,
                    Margin = new Thickness(0, 0, 0, 10),
                    Items =
                    {
                        new ComboBoxItem
                        {
                            Content="中文原版",
                            Tag="zh_origin"
                        },
                        new ComboBoxItem
                        {
                            Content="中文改版",
                            Tag="zh_revision"
                        },
                        new ComboBoxItem
                        {
                            Content="英文原版",
                            Tag="en_origin"
                        }
                    },
                    SelectedIndex = GameInfo.GameInfo.VersionType == "zh_origin" ? 0 : GameInfo.GameInfo.VersionType == "zh_revision" ? 1 : 2
                };
                //Version
                var textBox = new TextBox
                {
                    Text = GameInfo.GameInfo.Version,
                    Margin = new Thickness(0, 0, 0, 10)
                };

                await DialogManager.ShowDialogAsync(new ContentDialog
                {
                    Title = "更改版本信息",
                    Content = new StackPanel
                    {
                        Children =
                        {
                            new TextBlock
                            {
                                Text="版本类型:",
                                Margin=new Thickness(0,0,0,5)
                            },
                            comboBox,
                            new TextBlock
                            {
                                Text="版本号:",
                                Margin=new Thickness(0,0,0,5)
                            },
                            textBox
                        }
                    },
                    PrimaryButtonText = "确定",
                    CloseButtonText = "取消",
                    DefaultButton = ContentDialogButton.Primary
                }, (() =>
                {
                    GameInfo.GameInfo.Version = textBox.Text;
                    GameInfo.GameInfo.VersionType = (string)((ComboBoxItem)comboBox.SelectedItem).Tag;
                    Json.WriteJson(System.IO.Path.Combine(AppInfo.GameDirectory, GameInfo.GameInfo.Name, ".pvzl.json"), GameInfo);
                    
                    notificationManage.Show(new NotificationContent
                    {
                        Title = "成功",
                        Message = "您的版本信息已更改",
                        Type = NotificationType.Success
                    });

                    this.NavigationService.Refresh();
                }));
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", null!, ex);
            }
        }

        private async void button_SelectExecute_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                

                string[] files = Directory.GetFiles(System.IO.Path.Combine(AppInfo.GameDirectory, GameInfo.GameInfo.Name));
                List<string> exes = new List<string>();
                //过滤exe
                foreach (var file in files)
                    if (file.EndsWith(".exe"))
                        exes.Add(System.IO.Path.GetFileName(file));

                //有多个才更改
                if (exes.Count > 1)
                {
                    var listBox = new ListBox();
                    //添加exe
                    foreach (var exe in exes)
                        listBox.Items.Add(exe);

                    await DialogManager.ShowDialogAsync(new ContentDialog
                    {
                        Title = "更改可执行文件",
                        Content = new StackPanel
                        {
                            Children =
                            {
                                new TextBlock
                                {
                                    Text="请选择游戏可执行文件，启动游戏将启动此文件",
                                    Margin=new Thickness(0,0,0,10)
                                },
                                listBox
                            }
                        },
                        PrimaryButtonText = "确定",
                        CloseButtonText = "取消",
                        DefaultButton = ContentDialogButton.Primary
                    }, (() =>
                    {
                        if (listBox.SelectedItem != null)
                        {
                            GameInfo.GameInfo.ExecuteName = (string)listBox.SelectedItem;
                            Json.WriteJson(System.IO.Path.Combine(AppInfo.GameDirectory, GameInfo.GameInfo.Name, ".pvzl.json"), GameInfo);
                            notificationManage.Show(new NotificationContent
                            {
                                Title = "成功",
                                Message = $"可执行文件已更改为 \"{GameInfo.GameInfo.ExecuteName}\"",
                                Type = NotificationType.Success
                            });
                        }
                        else
                        {
                            notificationManage.Show(new NotificationContent
                            {
                                Title = "失败",
                                Message = "您没有选择任何可执行文件，因此操作取消",
                                Type = NotificationType.Error
                            });
                        }
                    }));
                }
                else
                {
                    notificationManage.Show(new NotificationContent
                    {
                        Title = "您无法更改",
                        Message = "此游戏目录下仅有一个可执行文件，无法更改",
                        Type = NotificationType.Error
                    });
                }

            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", null!, ex);
            }
        }

        private async void button_Rename_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var textBox = new TextBox { Text = GameInfo.GameInfo.Name };
                await DialogManager.ShowDialogAsync(new ContentDialog
                {
                    Title = "更改名称",
                    Content = textBox,
                    PrimaryButtonText = "确定",
                    CloseButtonText = "取消",
                    DefaultButton = ContentDialogButton.Primary
                }, (() =>
                {
                    if (textBox.Text != null) 
                    {
                        if(!Directory.Exists(Path.Combine(AppInfo.GameDirectory, textBox.Text)))
                        {
                            string lastName = GameInfo.GameInfo.Name;
                            GameInfo.GameInfo.Name = textBox.Text;
                            Directory.Move(Path.Combine(AppInfo.GameDirectory, lastName), Path.Combine(AppInfo.GameDirectory, GameInfo.GameInfo.Name));
                            Json.WriteJson(Path.Combine(AppInfo.GameDirectory, GameInfo.GameInfo.Name, ".pvzl.json"), GameInfo);
                            notificationManage.Show(new NotificationContent
                            {
                                Title = "更名成功",
                                Message = $"游戏已更名为 \"{GameInfo.GameInfo.Name}\"",
                                Type = NotificationType.Success
                            });

                            if (AppInfo.Config.CurrentGame == lastName)
                                AppInfo.Config.CurrentGame = GameInfo.GameInfo.Name;

                            this.NavigationService.Refresh();
                        }
                        else
                        {
                            notificationManage.Show(new NotificationContent
                            {
                                Title = "更名失败",
                                Message = $"游戏库下已有与\"{textBox.Text}\"同名游戏！",
                                Type = NotificationType.Error
                            });
                        }
                    }
                    else
                    {
                        notificationManage.Show(new NotificationContent
                        {
                            Title = "更名失败",
                            Message = "新名称为空",
                            Type = NotificationType.Error
                        });
                    }
                }));
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", null!, ex);
            }
        }
    }
}
