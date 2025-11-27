using HuaZi.Library.Logger;
using Microsoft.Win32;
using ModernWpf;
using ModernWpf.Controls;
using Newtonsoft.Json;
using Notifications.Wpf;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using PvzLauncherRemake.Utils;
using System.IO;
using System.Threading.Tasks;
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
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            });
            sp.BeginAnimation(OpacityProperty, new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
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
                StackPanel[] sps = { sp1, sp2, sp3, sp4, sp5, sp6, sp7, sp8, sp9, sp10 };
                foreach (var sp in sps)
                {
                    logger.Info($"[设置: 动画] 动画元素 {sp.Name} 归位");
                    sp.Margin = new Thickness(-500, sp.Margin.Top, sp.Margin.Right, sp.Margin.Bottom);
                    sp.Opacity = 0;
                }

                logger.Info($"[设置: 动画] 所有动画元素已归位");

                logger.Info($"[设置] 当前配置文件: {JsonConvert.SerializeObject(AppInfo.Config)}");

                isInitialized = false;

                //# 启动器设置
                //## 操作
                //### 游戏启动后操作
                switch (AppInfo.Config.LauncherConfig.LaunchedOperate)
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
                switch (AppInfo.Config.LauncherConfig.Theme)
                {
                    case "Light":
                        radioButton_Theme_Light.IsChecked = true;break;
                    case "Dark":
                        radioButton_Theme_Dark.IsChecked = true;break;
                }
                //### 窗口标题
                textBox_WindowTitle.Text = AppInfo.Config.LauncherConfig.WindowTitle;
                //### 标题图片
                radioButton_TitieImage_EN.IsChecked = false; radioButton_TitleImage_ZH.IsChecked = false;
                switch (AppInfo.Config.LauncherConfig.TitleImage)
                {
                    case "EN":
                        radioButton_TitieImage_EN.IsChecked = true; break;
                    case "ZH":
                        radioButton_TitleImage_ZH.IsChecked = true; break;
                }
                //### 背景
                radioButton_Background_Default.IsChecked = false; radioButton_Background_Custom.IsChecked = false;
                if (!string.IsNullOrEmpty(AppInfo.Config.LauncherConfig.Background))
                {
                    if (File.Exists(AppInfo.Config.LauncherConfig.Background))
                    {
                        radioButton_Background_Custom.IsChecked = true;
                        button_Background_Select.IsEnabled = true;
                        image_Background.Source = new BitmapImage(new Uri(AppInfo.Config.LauncherConfig.Background));
                    }
                    else
                    {
                        new NotificationManager().Show(new NotificationContent
                        {
                            Title = "背景设置项失效",
                            Message = $"\"{AppInfo.Config.LauncherConfig.Background}\" 不存在！已恢复为默认背景!",
                            Type = NotificationType.Error
                        });
                        AppInfo.Config.LauncherConfig.Background = null!;
                        ConfigManager.SaveAllConfig();
                        this.NavigationService.Refresh();
                    }
                }
                else
                {
                    button_Background_Select.IsEnabled = false;
                    radioButton_Background_Default.IsChecked = true;
                }
                //### NavigationView位置
                radioButton_NavViewLeft.IsChecked = false;radioButton_NavViewTop.IsChecked = false;
                switch (AppInfo.Config.LauncherConfig.NavigationViewAlign)
                {
                    case "Left":
                        radioButton_NavViewLeft.IsChecked = true;break;
                    case "Top":
                        radioButton_NavViewTop.IsChecked = true;break;
                }
                //## 更新
                //### 更新通道
                switch (AppInfo.Config.LauncherConfig.UpdateChannel)
                {
                    case "Stable":
                        comboBox_UpdateChannel.SelectedIndex = 0;break;
                    case "Development":
                        comboBox_UpdateChannel.SelectedIndex = 1;break;
                }
                //### 启动时检查更新
                checkBox_StartUpCheckUpdate.IsChecked = AppInfo.Config.LauncherConfig.StartUpCheckUpdate;
                //## 通知
                //### 下载通知
                checkBox_DownloadTipGame.IsChecked = AppInfo.Config.LauncherConfig.DownloadTip.ShowGameDownloadTip;
                checkBox_DownloadTipTrainer.IsChecked = AppInfo.Config.LauncherConfig.DownloadTip.ShowTrainerDownloadTip;


                //# 存档设置
                //## 存档隔离
                //### 启用存档隔离
                checkBox_EnableIsolationSave.IsChecked = AppInfo.Config.SaveConfig.EnableSaveIsolation;








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

        #region 启动器设置

        private void comboBox_LaunchedOperate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInitialized)
            {
                switch (comboBox_LaunchedOperate.SelectedIndex)
                {
                    case 0:
                        AppInfo.Config.LauncherConfig.LaunchedOperate = "None"; break;
                    case 1:
                        AppInfo.Config.LauncherConfig.LaunchedOperate = "Close"; break;
                    case 2:
                        AppInfo.Config.LauncherConfig.LaunchedOperate = "HideAndDisplay"; break;
                }
                ConfigManager.SaveAllConfig();
            }
        }

        private void radioButton_Theme_Light_Click(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                AppInfo.Config.LauncherConfig.Theme = (string)(((RadioButton)sender).Tag);
                ConfigManager.SaveAllConfig();
                switch (AppInfo.Config.LauncherConfig.Theme)
                {
                    case "Light":
                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;break;
                    case "Dark":
                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;break;
                }
            }
        }

        private void button_TitleReset_Click(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
                textBox_WindowTitle.Text = new JsonConfig.Index().LauncherConfig.WindowTitle;
        }

        private void textBox_WindowTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInitialized)
            {
                AppInfo.Config.LauncherConfig.WindowTitle = textBox_WindowTitle.Text;
                ((MainWindow)Window.GetWindow(this)).Title = AppInfo.Config.LauncherConfig.WindowTitle;
                ConfigManager.SaveAllConfig();
            }
        }

        private void radioButton_TitieImage_EN_Click(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                if (sender is RadioButton radioButton)
                {
                    if ((string)radioButton.Tag == "EN")
                        radioButton_TitleImage_ZH.IsChecked = false;
                    if ((string)radioButton.Tag == "ZH")
                        radioButton_TitieImage_EN.IsChecked = false;

                    AppInfo.Config.LauncherConfig.TitleImage = (string)radioButton.Tag;
                    ConfigManager.SaveAllConfig();
                }


            }
        }

        private void button_Background_Select_Click(object sender, RoutedEventArgs e)
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
                    AppInfo.Config.LauncherConfig.Background = dialog.FileName;
                    ConfigManager.SaveAllConfig();
                    image_Background.Source = new BitmapImage(new Uri(AppInfo.Config.LauncherConfig.Background));
                }
            }
        }

        private void radioButton_Background_Default_Click(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                if (sender is RadioButton radioButton)
                {
                    if ((string)radioButton.Tag == "Default")
                    {
                        button_Background_Select.IsEnabled = false;
                        AppInfo.Config.LauncherConfig.Background = null!;
                        radioButton_Background_Custom.IsChecked = false;
                    }
                    if ((string)radioButton.Tag == "Custom")
                    {
                        button_Background_Select.IsEnabled = true;
                        radioButton_Background_Default.IsChecked = false;
                    }
                    ConfigManager.SaveAllConfig();
                }


            }
        }

        private void radioButton_NavViewLeft_Checked(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                AppInfo.Config.LauncherConfig.NavigationViewAlign = (string)(((RadioButton)sender).Tag);
                ConfigManager.SaveAllConfig();

            }
        }

        private async void button_CheckUpdate_Click(object sender, RoutedEventArgs e)
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

        private void comboBox_UpdateChannel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInitialized)
            {
                AppInfo.Config.LauncherConfig.UpdateChannel = (string)(((ComboBoxItem)comboBox_UpdateChannel.SelectedItem).Tag);
                ConfigManager.SaveAllConfig();
            }
        }

        private void checkBox_StartUpCheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                AppInfo.Config.LauncherConfig.StartUpCheckUpdate = (bool)checkBox_StartUpCheckUpdate.IsChecked!;
                ConfigManager.SaveAllConfig();
            }
        }

        private void checkBox_DownloadTipGame_Click(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                AppInfo.Config.LauncherConfig.DownloadTip.ShowGameDownloadTip = (bool)checkBox_DownloadTipGame.IsChecked!;
                AppInfo.Config.LauncherConfig.DownloadTip.ShowTrainerDownloadTip = (bool)checkBox_DownloadTipTrainer.IsChecked!;
                ConfigManager.SaveAllConfig();
            }
        }

        #endregion

        #region 存档设置

        private async void button_SaveDelete_Click(object sender, RoutedEventArgs e)
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
                            StartLoad();
                            await Task.Run(() =>
                            {
                                Directory.Delete(AppInfo.SaveDirectory, true);
                            });
                            EndLoad();
                            new NotificationManager().Show(new NotificationContent
                            {
                                Title = "删除存档",
                                Message = "您的存档已经移除",
                                Type = NotificationType.Success
                            });
                        }
                        catch (Exception ex)
                        {
                            ErrorReportDialog.Show("发生错误", "发生错误", ex);
                        }
                        
                    }));
                }));
            }
        }

        private void checkBox_EnableIsolationSave_Click(object sender, RoutedEventArgs e)
        {
            if (isInitialized)
            {
                AppInfo.Config.SaveConfig.EnableSaveIsolation = (bool)checkBox_EnableIsolationSave.IsChecked!;
                ConfigManager.SaveAllConfig();
            }
        }

        #endregion
    }
}
