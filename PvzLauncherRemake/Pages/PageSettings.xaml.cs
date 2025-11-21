using Newtonsoft.Json;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static PvzLauncherRemake.Class.AppLogger;

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageSettings.xaml 的交互逻辑
    /// </summary>
    public partial class PageSettings : ModernWpf.Controls.Page
    {
        private bool isInitialized = false;

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
                if(sender is RadioButton radioButton)
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
    }
}
