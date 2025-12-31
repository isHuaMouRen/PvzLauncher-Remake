using Microsoft.Win32;
using ModernWpf.Controls;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Utils.FileSystem;
using PvzLauncherRemake.Utils.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageToolbox.xaml 的交互逻辑
    /// </summary>
    public partial class PageToolbox : ModernWpf.Controls.Page
    {
        #region Load
        private void SetLoad(bool isLoad = true)
        {
            stackpanel.IsEnabled = !isLoad;
            stackpanel.Effect = isLoad ? new BlurEffect { Radius = 10 } : null;
            grid_Loading.Visibility = isLoad ? Visibility.Visible : Visibility.Hidden;
        }
        private void StartLoad() => SetLoad(true);
        private void EndLoad() => SetLoad(false);
        #endregion


        public PageToolbox()
        {
            InitializeComponent();
        }

        private void button_Pak_Pak_Broswer_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Pak文件|*.pak",
                Title = "选择有效的Pak文件",
                Multiselect = false
            };
            if (dialog.ShowDialog() == true)
                textBox_Pak_Pak.Text = dialog.FileName;
        }

        private void button_Pak_Directory_Broswer_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "请选择目标路径",
                Multiselect = false
            };
            if (dialog.ShowDialog() == true)
                textBox_Pak_Directory.Text = dialog.FolderName;
        }

        private async void button_Pak_Pack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StartLoad();

                bool isPack = false;
                await DialogManager.ShowDialogAsync(new ContentDialog
                {
                    Title = "操作确认",
                    Content = $"即将开始打包，此操作会覆盖目标Pak文件",
                    PrimaryButtonText = "确认",
                    CloseButtonText = "取消",
                    DefaultButton = ContentDialogButton.Primary
                }, (() => isPack = true));

                if (!isPack)
                    return;

                string pak = textBox_Pak_Pak.Text;
                string dir = textBox_Pak_Directory.Text;

                await Task.Run(() =>
                {
                    try
                    {
                        PakManager.Pack(dir, pak);
                    }
                    catch (Exception ex) { ErrorReportDialog.Show("发生错误", null!, ex); }
                });

                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{textBox_Pak_Pak.Text}\"",
                    UseShellExecute = true
                });

            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", null!, ex);
            }
            finally
            {
                EndLoad();
            }
        }

        private async void button_Pak_Unpack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StartLoad();

                bool isUnpack = false;
                await DialogManager.ShowDialogAsync(new ContentDialog
                {
                    Title = "操作确认",
                    Content = $"即将开始解包，此操作会释放出大量文件，请不要选择桌面等文件夹！",
                    PrimaryButtonText = "确认",
                    CloseButtonText = "取消",
                    DefaultButton = ContentDialogButton.Primary
                }, (() => isUnpack = true));

                if (!isUnpack)
                    return;

                string pak = textBox_Pak_Pak.Text;
                string dir = textBox_Pak_Directory.Text;

                await Task.Run(() =>
                {
                    try
                    {
                        PakManager.Unpack(pak, dir);
                    }
                    catch (Exception ex) { ErrorReportDialog.Show("发生错误", null!, ex); }
                });

                Process.Start(new ProcessStartInfo
                {
                    FileName = $"{textBox_Pak_Directory.Text}",
                    UseShellExecute = true
                });

            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", null!, ex);
            }
            finally
            {
                EndLoad();
            }
        }
    }
}
