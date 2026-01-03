using ModernWpf.Controls;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Utils.Services;
using PvzLauncherRemake.Utils.UI;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Documents;
using static PvzLauncherRemake.Utils.Configuration.LocalizeManager;

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

        private string ScreeshotRootUrl = $"{AppGlobals.ServiceRootUrl}/game-library/screenshots";

        #region image
        private void ImageMouseEnter(object sender)
        {
            var animation = new DoubleAnimation
            {
                From = 250,
                To = 260,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            };
            ((Image)sender).BeginAnimation(MaxHeightProperty, null);
            ((Image)sender).BeginAnimation(MaxHeightProperty, animation);
        }

        private void ImageMouseLeave(object sender)
        {
            var animation = new DoubleAnimation
            {
                From = 260,
                To = 250,
                Duration = TimeSpan.FromMilliseconds(1000),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            };
            ((Image)sender).BeginAnimation(MaxHeightProperty, null);
            ((Image)sender).BeginAnimation(MaxHeightProperty, animation);
        }
        #endregion

        #region animation
        private void StartImageAnimation(Image image)
        {
            //动画
            var thicknessAnimation = new ThicknessAnimation
            {
                From = new Thickness(-50, 0, 0, 0),
                To = new Thickness(0),
                Duration = TimeSpan.FromMilliseconds(1000),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            };
            var doubleAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(1000),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            };
            image.BeginAnimation(MarginProperty, null);
            image.BeginAnimation(OpacityProperty, null);
            image.BeginAnimation(MarginProperty, thicknessAnimation);
            image.BeginAnimation(OpacityProperty, doubleAnimation);
        }
        #endregion

        #region init
        public async void Initialize()
        {
            try
            {
                //卡片
                userCard.Title = Info.Name;
                userCard.Icon = GameIconConverter.ParseToGameIcons(Info.Icon);
                userCard.Version = Info.Version;
                userCard.isNew = Info.IsNew;
                userCard.isRecommend = Info.IsRecommend;


                //简介
                textBlock_Description.Text = Info.Description;

                //信息
                textBlock_Information.Inlines.Clear();
                //作者
                textBlock_Information.Inlines.Add(new Bold(new Run($"{GetLoc("I18N.PageDownloadConfirm", "Author")}: ")));
                for (int i = 0; i < Info.Author.Length; i++)
                {
                    textBlock_Information.Inlines.Add(new Run($"{Info.Author[i]}{(i != Info.Author.Length-1 ? " , " : null)}"));
                }

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
                            image.MouseEnter += ((s, e) => ImageMouseEnter(s));
                            image.MouseLeave += ((s, e) => ImageMouseLeave(s));

                            stackPanel_Screenshot.Children.Add(image);

                            StartImageAnimation(image);

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
            /*//确认下载
            bool confirm = false;
            await DialogManager.ShowDialogAsync(new ContentDialog
            {
                Title = "下载确认",
                Content = $"是否下载 \"{Info.Name}\"",
                PrimaryButtonText = "确定",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary
            }, (() => confirm = true));
            if (!confirm) return;*/

            //处理同名
            string savePath = await GameManager.ResolveSameName(Info.Name, BaseDirectory);

            //开始下载
            await GameManager.StartDownloadAsync(Info, savePath, IsTrainer);
        }
    }
}
