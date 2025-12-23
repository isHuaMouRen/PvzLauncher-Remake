using Microsoft.Win32;
using ModernWpf;
using ModernWpf.Controls;
using Newtonsoft.Json;
using Notifications.Wpf;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using PvzLauncherRemake.Utils;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using static PvzLauncherRemake.Class.AppLogger;

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageSettings.xaml 的交互逻辑
    /// </summary>
    public partial class PageSettings : ModernWpf.Controls.Page
    {
        private bool isInitialized = false;

        public void ShowRestartTip()
        {
            new NotificationManager().Show(new NotificationContent
            {
                Title = "提示",
                Message = "此设置项重启才能生效",
                Type = NotificationType.Information
            });
        }

        #region Animation
        public void StartAnimation(StackPanel sp)
        {
            sp.BeginAnimation(MarginProperty, new ThicknessAnimation
            {
                To = new Thickness(0, sp.Margin.Top, sp.Margin.Right, sp.Margin.Bottom),
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            });
            sp.BeginAnimation(OpacityProperty, new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            });
        }
        #endregion

        #region Load
        public void SetLoad(bool isLoad)
        {
            tabControl.IsEnabled = !isLoad;

            if (isLoad)
            {
                grid_Loading.Visibility = Visibility.Visible;
                tabControl.Effect = new BlurEffect { Radius = 10 };
            }
            else
            {
                grid_Loading.Visibility = Visibility.Hidden;
                tabControl.Effect = null;
            }
        }

        public void StartLoad() => SetLoad(true);
        public void EndLoad() => SetLoad(false);
        #endregion

        #region Init
        public void Initialize() { }
        public async void InitializeLoaded()
        {
            try
            {
                logger.Info($"[设置] 开始初始化");

                //动画归位
                StackPanel[] sps = { sp1, sp2, sp3, sp4, sp5, sp6, sp7, sp8, sp9, sp10, sp11, sp12 };
                foreach (var sp in sps)
                {
                    logger.Info($"[设置: 动画] 动画元素 {sp.Name} 归位");
                    sp.Margin = new Thickness(-500, sp.Margin.Top, sp.Margin.Right, sp.Margin.Bottom);
                    sp.Opacity = 0;
                }

                logger.Info($"[设置: 动画] 所有动画元素已归位");

                logger.Info($"[设置] 当前配置文件: {JsonConvert.SerializeObject(AppGlobals.Config)}");

                isInitialized = false;


                //# 启动器设置
                //## 操作
                //### 游戏启动后操作
                switch (AppGlobals.Config.LauncherConfig.LaunchedOperate)
                {
                    case "None":
                        comboBox_LaunchedOperate.SelectedIndex = 0; break;
                    case "Close":
                        comboBox_LaunchedOperate.SelectedIndex = 1; break;
                    case "HideAndDisplay":
                        comboBox_LaunchedOperate.SelectedIndex = 2; break;
                }
                //## 外观
                //### 主题
                radioButton_Theme_Light.IsChecked = false;
                radioButton_Theme_Dark.IsChecked = false;
                switch (AppGlobals.Config.LauncherConfig.Theme)
                {
                    case "Light":
                        radioButton_Theme_Light.IsChecked = true; break;
                    case "Dark":
                        radioButton_Theme_Dark.IsChecked = true; break;
                }
                //### 语言
                switch (AppGlobals.Config.LauncherConfig.Language)
                {
                    case "zh-CN":
                        comboBox_Launcher_Language.SelectedIndex = 0;break;
                    case "en-US":
                        comboBox_Launcher_Language.SelectedIndex = 1;break;
                }
                //### 窗口标题
                textBox_WindowTitle.Text = AppGlobals.Config.LauncherConfig.WindowTitle;
                //### 标题图片
                radioButton_TitieImage_EN.IsChecked = false; radioButton_TitleImage_ZH.IsChecked = false;
                switch (AppGlobals.Config.LauncherConfig.TitleImage)
                {
                    case "EN":
                        radioButton_TitieImage_EN.IsChecked = true; break;
                    case "ZH":
                        radioButton_TitleImage_ZH.IsChecked = true; break;
                }
                //### 背景
                radioButton_Background_Default.IsChecked = false; radioButton_Background_Custom.IsChecked = false;
                if (!string.IsNullOrEmpty(AppGlobals.Config.LauncherConfig.Background))
                {
                    if (File.Exists(AppGlobals.Config.LauncherConfig.Background))
                    {
                        radioButton_Background_Custom.IsChecked = true;
                        button_Background_Select.IsEnabled = true;
                        image_Background.Source = new BitmapImage(new Uri(AppGlobals.Config.LauncherConfig.Background));
                    }
                    else
                    {
                        new NotificationManager().Show(new NotificationContent
                        {
                            Title = "背景设置项失效",
                            Message = $"\"{AppGlobals.Config.LauncherConfig.Background}\" 不存在！已恢复为默认背景!",
                            Type = NotificationType.Error
                        });
                        AppGlobals.Config.LauncherConfig.Background = null!;
                        ConfigManager.SaveConfig();
                        this.NavigationService.Refresh();
                    }
                }
                else
                {
                    button_Background_Select.IsEnabled = false;
                    radioButton_Background_Default.IsChecked = true;
                }
                //### NavigationView位置
                radioButton_NavViewLeft.IsChecked = false; radioButton_NavViewTop.IsChecked = false;
                switch (AppGlobals.Config.LauncherConfig.NavigationViewAlign)
                {
                    case "Left":
                        radioButton_NavViewLeft.IsChecked = true; break;
                    case "Top":
                        radioButton_NavViewTop.IsChecked = true; break;
                }
                //## 更新
                //### 更新通道
                switch (AppGlobals.Config.LauncherConfig.UpdateChannel)
                {
                    case "Stable":
                        comboBox_UpdateChannel.SelectedIndex = 0; break;
                    case "Development":
                        comboBox_UpdateChannel.SelectedIndex = 1; break;
                }
                //### 启动时检查更新
                checkBox_StartUpCheckUpdate.IsChecked = AppGlobals.Config.LauncherConfig.StartUpCheckUpdate;
                //## 通知
                //### 下载通知
                checkBox_DownloadTipGame.IsChecked = AppGlobals.Config.LauncherConfig.DownloadTip.ShowGameDownloadTip;
                checkBox_DownloadTipTrainer.IsChecked = AppGlobals.Config.LauncherConfig.DownloadTip.ShowTrainerDownloadTip;

                //# 游戏设置
                //## 游戏配置
                //### 全屏
                switch (AppGlobals.Config.GameConfig.FullScreen)
                {
                    case "Default":
                        comboBox_Game_FullScreen.SelectedIndex = 0;break;
                    case "FullScreen":
                        comboBox_Game_FullScreen.SelectedIndex = 1;break;
                    case "Windowed":
                        comboBox_Game_FullScreen.SelectedIndex = 2;break;
                }
                //### 位置
                switch (AppGlobals.Config.GameConfig.StartUpLocation)
                {
                    case "Default":
                        comboBox_Game_Location.SelectedIndex = 0; break;
                    case "Center":
                        comboBox_Game_Location.SelectedIndex = 1; break;
                    case "LeftTop":
                        comboBox_Game_Location.SelectedIndex = 2; break;
                }


                //# 存档设置
                //## 存档隔离
                //### 启用存档隔离
                checkBox_EnableIsolationSave.IsChecked = AppGlobals.Config.SaveConfig.EnableSaveIsolation;








                logger.Info($"[设置] 设置项应用完毕");
                isInitialized = true;
                logger.Info($"[设置: 动画] 动画元素开始执行");
                foreach (var sp in sps)
                {
                    logger.Info($"[设置: 动画] 元素 {sp.Name} 开始播放动画");
                    await Task.Delay(50);
                    StartAnimation(sp);
                }
                logger.Info($"[设置] 完成初始化");
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", null!, ex);
            }
        }
        #endregion

        public PageSettings()
        {
            InitializeComponent();
            Initialize();
            Loaded += ((sender, e) => InitializeLoaded());
        }

        //tabControl动画
        private async void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsInitialized)
            {
                var tabItem = (ScrollViewer)tabControl.SelectedContent;


                tabItem.BeginAnimation(MarginProperty, null);
                tabItem.BeginAnimation(OpacityProperty, null);

                tabItem.Margin = new Thickness(0, 25, 0, 0);
                tabItem.Opacity = 0;

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
                tabItem.BeginAnimation(MarginProperty, margniAnim);
                tabItem.BeginAnimation(OpacityProperty, opacAnim);
            }
        }

        #region 启动器设置

        private void Launcher_LaunchOperate(object sender, SelectionChangedEventArgs e)
        {
            if (isInitialized)
            {
                switch (comboBox_LaunchedOperate.SelectedIndex)
                {
                    case 0:
                        AppGlobals.Config.LauncherConfig.LaunchedOperate = "None"; break;
                    case 1:
                        AppGlobals.Config.LauncherConfig.LaunchedOperate = "Close"; break;
                    case 2:
                        AppGlobals.Config.LauncherConfig.LaunchedOperate = "HideAndDisplay"; break;
                }
                ConfigManager.SaveConfig();
            }
        }

        private void Launcher_Theme(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                AppGlobals.Config.LauncherConfig.Theme = (string)(((RadioButton)sender).Tag);
                ConfigManager.SaveConfig();
                switch (AppGlobals.Config.LauncherConfig.Theme)
                {
                    case "Light":
                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light; break;
                    case "Dark":
                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark; break;
                }
            }
        }

        private void Launcher_Language(object sender, SelectionChangedEventArgs e)
        {
            if (isInitialized)
            {
                var comboBox = (ComboBox)sender;
                AppGlobals.Config.LauncherConfig.Language = ((ComboBoxItem)comboBox.SelectedItem).Tag.ToString()!;
                ConfigManager.SaveConfig();

                LocalizeManager.SwitchLanguage(AppGlobals.Config.LauncherConfig.Language);
            }
        }

        private void Launcher_TitleReset(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
                textBox_WindowTitle.Text = new JsonConfig.Index().LauncherConfig.WindowTitle;
        }

        private void Launcher_Title(object sender, TextChangedEventArgs e)
        {
            if (isInitialized)
            {
                AppGlobals.Config.LauncherConfig.WindowTitle = textBox_WindowTitle.Text;
                ((WindowMain)Window.GetWindow(this)).Title = AppGlobals.Config.LauncherConfig.WindowTitle;
                ConfigManager.SaveConfig();
            }
        }

        private void Launcher_TitleImageLanguage(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                if (sender is RadioButton radioButton)
                {
                    if ((string)radioButton.Tag == "EN")
                        radioButton_TitleImage_ZH.IsChecked = false;
                    if ((string)radioButton.Tag == "ZH")
                        radioButton_TitieImage_EN.IsChecked = false;

                    AppGlobals.Config.LauncherConfig.TitleImage = (string)radioButton.Tag;
                    ConfigManager.SaveConfig();
                }


            }
        }

        private void Launcher_BackgroundCustom(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "图像文件|*.png;*.jpg;*.webp;*.bmp",
                    Multiselect = false,
                    CheckFileExists = true
                };
                if (dialog.ShowDialog() == true)
                {
                    AppGlobals.Config.LauncherConfig.Background = dialog.FileName;
                    ConfigManager.SaveConfig();
                    image_Background.Source = new BitmapImage(new Uri(AppGlobals.Config.LauncherConfig.Background));
                }
            }
        }

        private void Launcher_BackgroundSelect(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                if (sender is RadioButton radioButton)
                {
                    if ((string)radioButton.Tag == "Default")
                    {
                        button_Background_Select.IsEnabled = false;
                        AppGlobals.Config.LauncherConfig.Background = null!;
                        radioButton_Background_Custom.IsChecked = false;
                    }
                    if ((string)radioButton.Tag == "Custom")
                    {
                        button_Background_Select.IsEnabled = true;
                        radioButton_Background_Default.IsChecked = false;
                    }
                    ConfigManager.SaveConfig();
                }


            }
        }

        private void Launcher_NavigationViewAlign(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                AppGlobals.Config.LauncherConfig.NavigationViewAlign = (string)(((RadioButton)sender).Tag);
                ConfigManager.SaveConfig();

            }
        }

        private async void Launcher_CheckUpdate(object sender, RoutedEventArgs e)
        {
            try
            {
                StartLoad();

                await Updater.CheckUpdate((p, s) =>
                {
                    textBlock_Loading.Text = $"下载更新文件中 {Math.Round(p, 2)}% ... ({Math.Round(s / 1024, 2)} MB/S)";
                });

                EndLoad();
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "更新时发生错误", ex);
            }

        }

        private void Launcher_UpdateChannel(object sender, SelectionChangedEventArgs e)
        {
            if (isInitialized)
            {
                AppGlobals.Config.LauncherConfig.UpdateChannel = (string)(((ComboBoxItem)comboBox_UpdateChannel.SelectedItem).Tag);
                ConfigManager.SaveConfig();
            }
        }

        private void Launcher_StartUpCheckUpdate(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                AppGlobals.Config.LauncherConfig.StartUpCheckUpdate = (bool)checkBox_StartUpCheckUpdate.IsChecked!;
                ConfigManager.SaveConfig();
            }
        }

        private void Launcher_DownloadTips(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                AppGlobals.Config.LauncherConfig.DownloadTip.ShowGameDownloadTip = (bool)checkBox_DownloadTipGame.IsChecked!;
                AppGlobals.Config.LauncherConfig.DownloadTip.ShowTrainerDownloadTip = (bool)checkBox_DownloadTipTrainer.IsChecked!;
                ConfigManager.SaveConfig();
            }
        }

        private async void Launcher_ClearTemp(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                try
                {
                    StartLoad();
                    textBlock_Loading.Text = "扫描临时文件夹...";

                    string[] allTempFiles = { };//全部临时文件
                    List<string> pvzLauncherFiles = new List<string>();//PvzLauncher的临时文件
                    double tempFilesSize = 0;//缓存文件总大小


                    await Task.Run(() =>
                    {
                        allTempFiles = Directory.GetFiles(AppGlobals.TempDiectory);
                    });

                    if (!(allTempFiles.Length > 0))
                    {
                        new NotificationManager().Show(new NotificationContent
                        {
                            Title = "清理完成",
                            Message = "临时文件夹是空的，无需清除",
                            Type = NotificationType.Success
                        });
                        EndLoad();
                        return;
                    }

                    pvzLauncherFiles.Clear();
                    foreach (var file in allTempFiles)
                    {
                        if (Path.GetFileName(file).StartsWith("PvzLauncher", StringComparison.OrdinalIgnoreCase))
                        {
                            pvzLauncherFiles.Add(file);
                            tempFilesSize = tempFilesSize + new FileInfo(file).Length;
                        }
                    }

                    if (!(pvzLauncherFiles.Count > 0))
                    {
                        new NotificationManager().Show(new NotificationContent
                        {
                            Title = "清理完成",
                            Message = "没有需要清理的缓存文件",
                            Type = NotificationType.Success
                        });
                        EndLoad();
                        return;
                    }

                    bool isClear = false;
                    await DialogManager.ShowDialogAsync(new ContentDialog
                    {
                        Title = "发现缓存文件",
                        Content = $"发现了 {pvzLauncherFiles.Count} 个来自PvzLauncher的缓存文件, 共 {Math.Round(tempFilesSize / (1024 * 1024), 2)}MB, 是否清理?",
                        PrimaryButtonText = "清理",
                        CloseButtonText = "取消",
                        DefaultButton = ContentDialogButton.Primary
                    }, (() => isClear = true));

                    if (!isClear)
                    {
                        EndLoad();
                        return;
                    }


                    await Task.Run(() =>
                    {
                        foreach (var file in pvzLauncherFiles)
                        {
                            Dispatcher.BeginInvoke(() =>
                            {
                                textBlock_Loading.Text = $"正在删除 {Path.GetFileName(file)}";
                            });
                            File.Delete(file);
                        }
                    });

                    new NotificationManager().Show(new NotificationContent
                    {
                        Title = "清理完成",
                        Message = $"已清理所有缓存文件，共 {Math.Round(tempFilesSize / (1024 * 1024), 2)}MB",
                        Type = NotificationType.Success
                    });



                    EndLoad();
                }
                catch (Exception ex)
                {
                    ErrorReportDialog.Show("发生错误", null!, ex);
                }
                
            }
        }

        #endregion

        #region 游戏设置

        private void Game_FullScreen(object sender,SelectionChangedEventArgs e)
        {
            if (isInitialized)
            {
                AppGlobals.Config.GameConfig.FullScreen = ((ComboBoxItem)((ComboBox)sender).SelectedItem).Tag.ToString()!;
                ConfigManager.SaveConfig();
            }
        }

        private void Game_Location(object sender,SelectionChangedEventArgs e)
        {
            if (isInitialized)
            {
                AppGlobals.Config.GameConfig.StartUpLocation = ((ComboBoxItem)((ComboBox)sender).SelectedItem).Tag.ToString()!;
                ConfigManager.SaveConfig();
            }
        }

        #endregion

        #region 存档设置

        private async void Save_DeleteSave(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                await DialogManager.ShowDialogAsync(new ContentDialog
                {
                    Title = "警告",
                    Content = "此操作不可逆，一旦删除您的存档将会永久删除！(真的很久!)",
                    PrimaryButtonText = "删除",
                    CloseButtonText = "取消",
                    DefaultButton = ContentDialogButton.Close
                }, (async () =>
                {
                    await DialogManager.ShowDialogAsync(new ContentDialog
                    {
                        Title = "最后一次警告",
                        Content = "这将是最后一次警告，确认后存档立即删除，您现在还有取消的机会！",
                        PrimaryButtonText = "继续删除",
                        CloseButtonText = "取消",
                        DefaultButton = ContentDialogButton.Close
                    }, (async () =>
                    {
                        try
                        {

                            if (Directory.Exists(AppGlobals.SaveDirectory))
                            {
                                StartLoad();
                                await Task.Run(() =>
                                {
                                    Directory.Delete(AppGlobals.SaveDirectory, true);
                                });
                                EndLoad();
                                new NotificationManager().Show(new NotificationContent
                                {
                                    Title = "删除存档",
                                    Message = "您的存档已经移除",
                                    Type = NotificationType.Success
                                });
                            }
                            else
                            {
                                new NotificationManager().Show(new NotificationContent
                                {
                                    Title = "失败",
                                    Message = "存档不存在，无法删除",
                                    Type = NotificationType.Error
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorReportDialog.Show("发生错误", "发生错误", ex);
                        }

                    }));
                }));
            }
        }

        private async void Save_EnabledSaveIsolation(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                if (checkBox_EnableIsolationSave.IsChecked == true)
                    await DialogManager.ShowDialogAsync(new ContentDialog
                    {
                        Title = "警告",
                        Content = "开启存档隔离会导致当前存档丢失。请做好备份再开启！",
                        PrimaryButtonText = "继续开启",
                        SecondaryButtonText = "取消",
                        DefaultButton = ContentDialogButton.Primary
                    }, (() =>
                    {
                        AppGlobals.Config.SaveConfig.EnableSaveIsolation = true;
                    }), (() =>
                    {
                        checkBox_EnableIsolationSave.IsChecked = false;
                        AppGlobals.Config.SaveConfig.EnableSaveIsolation = false;
                    }), (() =>
                    {
                        checkBox_EnableIsolationSave.IsChecked = false;
                        AppGlobals.Config.SaveConfig.EnableSaveIsolation = false;
                    }));
                else
                    AppGlobals.Config.SaveConfig.EnableSaveIsolation = false;

                ConfigManager.SaveConfig();
            }
        }

        private async void Save_Move(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                if (AppGlobals.Config.SaveConfig.EnableSaveIsolation)
                {
                    if (AppGlobals.GameList.Count >= 2)
                    {
                        var listBox = new ListBox();
                        string originGameName = null!;
                        string targetGameName = null!;
                        foreach (var game in AppGlobals.GameList)
                        {
                            listBox.Items.Add(game.GameInfo.Name);
                        }

                        await DialogManager.ShowDialogAsync(new ContentDialog
                        {
                            Title = "存档迁移",
                            Content = new StackPanel
                            {
                                Children =
                            {
                                new TextBlock
                                {
                                    Text="存档迁移可以将某个游戏的存档复制到另一个游戏，请选择要复制的游戏",
                                    Margin=new Thickness(0,0,0,5)
                                },
                                listBox
                            }
                            },
                            PrimaryButtonText = "确定",
                            CloseButtonText = "取消",
                            DefaultButton = ContentDialogButton.Primary
                        }, (async () =>
                        {
                            if (listBox.SelectedItem != null)
                            {
                                originGameName = listBox.SelectedItem.ToString()!;

                                if (Directory.Exists(Path.Combine(AppGlobals.GameDirectory, originGameName, ".save")))
                                {

                                    var targetListBox = new ListBox();
                                    foreach (var game in AppGlobals.GameList)
                                    {
                                        if (game.GameInfo.Name != originGameName)
                                            targetListBox.Items.Add(game.GameInfo.Name);
                                    }

                                    await DialogManager.ShowDialogAsync(new ContentDialog
                                    {
                                        Title = "存档迁移",
                                        Content = new StackPanel
                                        {
                                            Children =
                                        {
                                            new TextBlock
                                            {
                                                Text="请选择要替换的游戏存档，此操作会将目标游戏的存档覆盖！",
                                                Margin=new Thickness(0,0,0,5)
                                            },
                                            targetListBox
                                        }
                                        },
                                        PrimaryButtonText = "确定",
                                        CloseButtonText = "取消",
                                        DefaultButton = ContentDialogButton.Primary
                                    }, (async () =>
                                    {
                                        if (targetListBox.SelectedItem != null)
                                        {
                                            targetGameName = targetListBox.SelectedItem.ToString()!;

                                            await DialogManager.ShowDialogAsync(new ContentDialog
                                            {
                                                Title = "操作确认",
                                                Content = new StackPanel
                                                {
                                                    Children =
                                                    {
                                                    new TextBlock
                                                    {
                                                        Text="请确认操作，此操作会将原游戏的存档复制到目标游戏\n这会导致目标游戏的存档被覆盖！",
                                                        Margin=new Thickness(0,0,0,5)
                                                    },
                                                    new TextBlock
                                                    {
                                                        Text=$"{originGameName} -> {targetGameName}",
                                                        HorizontalAlignment=HorizontalAlignment.Center
                                                    }
                                                    }
                                                },
                                                PrimaryButtonText = "确认",
                                                CloseButtonText = "取消",
                                                DefaultButton = ContentDialogButton.Primary
                                            }, (async () =>
                                            {
                                                StartLoad();

                                                await Task.Run(() =>
                                                {
                                                    if (Directory.Exists(Path.Combine(AppGlobals.GameDirectory, targetGameName, ".save")))
                                                        Directory.Delete(Path.Combine(AppGlobals.GameDirectory, targetGameName, ".save"), true);
                                                    else
                                                        Directory.CreateDirectory(Path.Combine(AppGlobals.GameDirectory, targetGameName, ".save"));
                                                });
                                                await DirectoryManager.CopyDirectoryAsync(Path.Combine(AppGlobals.GameDirectory, originGameName, ".save"), Path.Combine(AppGlobals.GameDirectory, targetGameName, ".save"));

                                                new NotificationManager().Show(new NotificationContent
                                                {
                                                    Title = "迁移成功",
                                                    Message = $"{originGameName} 的存档已迁移至 {targetGameName}",
                                                    Type = NotificationType.Success
                                                });

                                                EndLoad();
                                            }));
                                        }
                                        else
                                        {
                                            new NotificationManager().Show(new NotificationContent
                                            {
                                                Title = "操作中断",
                                                Message = "没有选择目标游戏",
                                                Type = NotificationType.Error
                                            });
                                        }
                                    }));
                                }
                                else
                                {
                                    new NotificationManager().Show(new NotificationContent
                                    {
                                        Title = "操作中断",
                                        Message = "原游戏无独立存档，请至少启动一次游戏并创建存档",
                                        Type = NotificationType.Error
                                    });
                                }
                            }
                            else
                            {
                                new NotificationManager().Show(new NotificationContent
                                {
                                    Title = "操作中断",
                                    Message = "没有选择任何游戏",
                                    Type = NotificationType.Error
                                });
                            }
                        }));
                    }
                    else
                    {
                        new NotificationManager().Show(new NotificationContent
                        {
                            Title = "无法迁移",
                            Message = "游戏库内少于两个游戏，无法使用此功能",
                            Type = NotificationType.Warning
                        });
                    }
                }
                else
                {
                    new NotificationManager().Show(new NotificationContent
                    {
                        Title = "提示",
                        Message = "请先启用存档隔离功能",
                        Type = NotificationType.Warning
                    });
                }
            }
        }



        #endregion
    }
}
