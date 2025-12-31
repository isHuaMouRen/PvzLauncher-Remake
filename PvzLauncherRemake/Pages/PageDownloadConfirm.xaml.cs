using ModernWpf.Controls;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Controls;
using PvzLauncherRemake.Utils;
using SharpCompress.Compressors.Xz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static PvzLauncherRemake.Utils.LocalizeManager;

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageDownloadConfirm.xaml 的交互逻辑
    /// </summary>
    public partial class PageDownloadConfirm : ModernWpf.Controls.Page
    {
        public dynamic Info { get; set; }
        public string BaseDirectory { get; set; }
        public bool IsTrainer { get; set; }

        private string ScreeshotRootUrl = $"{AppGlobals.ServiceRootUrl}/screenshots";

        #region init
        public async void Initialize()
        {
            try
            {
                //卡片
                userCard.Title = Info.Name;
                userCard.Icon = GameManager.ParseToGameIcons(Info.Icon);
                userCard.Version = Info.Version;


                //简介
                textBlock_Description.Text = Info.Description;

                stackPanel_Screenshot.Children.Clear();
                using (var client = new HttpClient())
                {
                    for (int i = 0; i < Info.Screenshot; i++)
                    {
                        string url = $"{ScreeshotRootUrl}/{Info.Version}/{i + 1}.png";

                        byte[] imageBytes = await client.GetByteArrayAsync(url);

                        using (var memoryStream = new MemoryStream(imageBytes))
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = memoryStream;           
                            bitmap.EndInit();
                            bitmap.Freeze();

                            var image = new Image
                            {
                                MaxHeight = 250,
                                Stretch = Stretch.Uniform,
                                Source = bitmap
                            };

                            stackPanel_Screenshot.Children.Add(image);
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show($"发生错误", null!, ex);
            }
        }
        #endregion

        public PageDownloadConfirm()
        {
            InitializeComponent();
            Loaded += ((s, e) => Initialize());
        }

        private async void button_Download_Click(object sender, RoutedEventArgs e)
        {
            //确认下载
            bool confirm = false;
            await DialogManager.ShowDialogAsync(new ContentDialog
            {
                Title = "下载确认",
                Content = $"是否下载 \"{Info.Name}\"",
                PrimaryButtonText = "确定",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary
            }, (() => confirm = true));
            if (!confirm) return;

            //处理同名
            string savePath = await GameManager.ResolveSameName(Info.Name, BaseDirectory);

            //开始下载
            await GameManager.StartDownloadAsync(Info, savePath, IsTrainer);
        }
    }
}
