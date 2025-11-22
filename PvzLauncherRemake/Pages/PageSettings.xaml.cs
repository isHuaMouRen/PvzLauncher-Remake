using Microsoft.Win32;
using Newtonsoft.Json;
using Notifications.Wpf;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using PvzLauncherRemake.Utils;
using System;
using System.Collections.Generic;
using System.IO;
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
using static System.Net.Mime.MediaTypeNames;

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageSettings.xaml 的交互逻辑
    /// </summary>
    public partial class PageSettings : ModernWpf.Controls.Page
    {
        private bool isInitialized = false;

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
        public void InitializeLoaded()
        {
            try
            {
                logger.Info("PageSettings 开始初始化");
                isInitialized = false;

                logger.Info($"当前配置文件: {JsonConvert.SerializeObject(AppInfo.Config)}");

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
                    if (Directory.Exists(AppInfo.Config.LauncherConfig.Background))
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


                    isInitialized = true;
                logger.Info("PageSettings 结束初始化");
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
                new NotificationManager().Show(new NotificationContent
                {
                    Title = "提示",
                    Message = "此设置项重启才能生效",
                    Type = NotificationType.Information
                });
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
    }
}
