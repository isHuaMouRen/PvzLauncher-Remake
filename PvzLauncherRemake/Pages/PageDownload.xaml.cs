using HuaZi.Library.Downloader;
using HuaZi.Library.Json;
using ModernWpf.Controls;
using Notifications.Wpf;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using PvzLauncherRemake.Controls;
using PvzLauncherRemake.Utils;
using SharpCompress.Archives;
using SharpCompress.Common;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageDownload.xaml 的交互逻辑
    /// </summary>
    public partial class PageDownload : ModernWpf.Controls.Page
    {
        private NotificationManager notificationManager = new NotificationManager();
        private JsonDownloadIndex.Index DownloadIndex = null!;
        private Downloader downloader = null!;

        #region Load
        public void StartLoad(bool showProgressBar = false)
        {
            tabControl_Main.IsEnabled = false;
            tabControl_Main.Effect = new BlurEffect { Radius = 10 };
            grid_Loading.Visibility = Visibility.Visible;
            if (showProgressBar)
                progressBar_Loading.Visibility = Visibility.Visible;
        }
        public void EndLoad()
        {
            tabControl_Main.IsEnabled = true;
            tabControl_Main.Effect = null;
            grid_Loading.Visibility = Visibility.Hidden;
            progressBar_Loading.Visibility = Visibility.Hidden;
        }
        #endregion

        #region Init
        public void Initialize() { }
        public async void InitializeLoaded()
        {
            try
            {
                StartLoad();

                using (var client = new HttpClient())
                    DownloadIndex = Json.ReadJson<JsonDownloadIndex.Index>(await client.GetStringAsync(AppInfo.DownloadIndexUrl));

                //中文原版
                listBox_zhOrigin.Items.Clear();
                foreach (var zhOriginGame in DownloadIndex.ZhOrigin)
                {
                    var card = new UserDownloadCard
                    {
                        Title = zhOriginGame.Name,
                        Description = zhOriginGame.Description,
                        Icon = "origin",
                        Version = zhOriginGame.Version,
                        Size = zhOriginGame.Size,
                        isNew = zhOriginGame.IsNew,
                        isRecommend = zhOriginGame.IsRecommend,
                        Tag = zhOriginGame
                    };
                    
                    listBox_zhOrigin.Items.Add(card);
                }

                //中文改版
                listBox_zhRevision.Items.Clear();
                foreach (var zhRevisionGame in DownloadIndex.ZhRevision)
                {
                    var card = new UserDownloadCard
                    {
                        Title = zhRevisionGame.Name,
                        Description = zhRevisionGame.Description,
                        Icon = zhRevisionGame.Version.StartsWith("β") ? "beta" :
                                zhRevisionGame.Version.StartsWith("TAT", StringComparison.OrdinalIgnoreCase) ? "tat" : "origin",
                        Version = zhRevisionGame.Version,
                        Size = zhRevisionGame.Size,
                        isNew = zhRevisionGame.IsNew,
                        isRecommend = zhRevisionGame.IsRecommend,
                        Tag = zhRevisionGame
                    };
                    
                    listBox_zhRevision.Items.Add(card);
                }

                //英文原版
                listBox_enOrigin.Items.Clear();
                foreach (var enOriginGame in DownloadIndex.EnOrigin)
                {
                    var card = new UserDownloadCard
                    {
                        Title = enOriginGame.Name,
                        Description = enOriginGame.Description,
                        Icon = "origin",
                        Version = enOriginGame.Version,
                        Size = enOriginGame.Size,
                        isNew = enOriginGame.IsNew,
                        isRecommend = enOriginGame.IsRecommend,
                        Tag = enOriginGame
                    };
                    
                    listBox_enOrigin.Items.Add(card);
                }

                //修改器
                listBox_trainer.Items.Clear();
                foreach (var trainerInfo in DownloadIndex.Trainer)
                {
                    var card = new UserDownloadCard
                    {
                        Title = trainerInfo.Name,
                        Description = trainerInfo.Description,
                        Icon = "origin",
                        Version = trainerInfo.Version,
                        Size = trainerInfo.Size,
                        SupportVersion = trainerInfo.SupportVersion,
                        isNew = trainerInfo.IsNew,
                        isRecommend = trainerInfo.IsRecommend,
                        Tag = trainerInfo
                    };
                    
                    listBox_trainer.Items.Add(card);
                }

                EndLoad();
                
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "加载后初始化 PageDownload 发生错误", ex);
            }
        }
        #endregion

        public PageDownload()
        {
            InitializeComponent();
            Initialize();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) { InitializeLoaded(); }

        private async void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                

                var listbox = (ListBox)sender;
                var selectItem = (UserDownloadCard)listbox.SelectedItem;
                var downloadIndex = (string)listbox.Tag == "trainer"
                    ? (JsonDownloadIndex.TrainerInfo)selectItem.Tag
                    : (JsonDownloadIndex.GameInfo)selectItem.Tag;

                string savePath = listbox.Tag.ToString() == "trainer" ?
                    System.IO.Path.Combine(AppInfo.TrainerDirectory, downloadIndex.Name) :
                    System.IO.Path.Combine(AppInfo.GameDirectory, downloadIndex.Name);

                await DialogManager.ShowDialogAsync(new ContentDialog
                {
                    Title = "下载确认",
                    Content = $"名称: {downloadIndex.Name}\n\n版本: {downloadIndex.Version}",
                    PrimaryButtonText = "开始下载",
                    CloseButtonText = "取消",
                    DefaultButton = ModernWpf.Controls.ContentDialogButton.Primary
                }, (async () =>
                {
                    try
                    {
                        //检测同名
                        bool isNameDone = false;
                        while (!isNameDone)
                        {
                            var textBox = new TextBox { Text = downloadIndex.Name };

                            if (Directory.Exists(savePath))
                            {
                                await DialogManager.ShowDialogAsync(new ContentDialog
                                {
                                    Title = "发现冲突",
                                    Content = new StackPanel
                                    {
                                        Children =
                                        {
                                            new TextBlock
                                            {
                                                Text=$"在您的游戏库发现与 \"{downloadIndex.Name}\" 同名的文件夹，请使用其他名称",
                                                Margin=new Thickness(0,0,0,10)
                                            },
                                            textBox
                                        }
                                    },
                                    PrimaryButtonText = "确定",
                                    DefaultButton = ContentDialogButton.Primary
                                });
                                if (textBox.Text != downloadIndex.Name)
                                {
                                    savePath = System.IO.Path.Combine(AppInfo.GameDirectory, textBox.Text);
                                    isNameDone = true;
                                }
                            }
                            else
                                isNameDone = true;
                        }
                        StartLoad(true);

                        //清除残留
                        if (File.Exists(System.IO.Path.Combine(AppInfo.TempDiectory, "PVZLAUNCHERCACHE.zip")))
                            await Task.Run(() => File.Delete(System.IO.Path.Combine(AppInfo.TempDiectory, "PVZLAUNCHERCACHE.zip")));

                        bool isDownloadDone = false;
                        string? downloadError = null;
                        //定义下载器
                        downloader = new Downloader
                        {
                            Url = downloadIndex.Url,
                            SavePath = System.IO.Path.Combine(AppInfo.TempDiectory, "PVZLAUNCHERCACHE.zip"),
                            Progress = ((p, s) =>
                            {
                                progressBar_Loading.Value = p;
                                textBlock_Loading.Text = $"下载中 {Math.Round(p, 2)}% ... ({Math.Round(s / 1024, 2)}MB/S)";
                            }),
                            Completed = ((s, e) =>
                            {
                                if (s)
                                    isDownloadDone = true;
                                else
                                {
                                    downloadError = e;
                                    isDownloadDone = true;
                                }
                            })
                        };

                        downloader.StartDownload();

                        //等下载完毕
                        while (isDownloadDone == false)
                            await Task.Delay(1000);

                        //下载失败抛错误
                        if (!string.IsNullOrEmpty(downloadError))
                            throw new Exception(downloadError);


                        textBlock_Loading.Text = "解压中...";
                        //创建文件夹
                        if (!Directory.Exists(savePath))
                            Directory.CreateDirectory(savePath);
                        //解压
                        await Task.Run(() =>
                        {
                            //使用SharpCompress库解压
                            ArchiveFactory.WriteToDirectory(System.IO.Path.Combine(AppInfo.TempDiectory, "PVZLAUNCHERCACHE.zip"), savePath, new ExtractionOptions
                            {
                                ExtractFullPath = true,
                                Overwrite = true
                            });
                        });

                        //写Json
                        string jsonContentName = null!;
                        if (listbox.Tag.ToString() != "trainer")
                        {
                            JsonGameInfo.Index jsonContent = new JsonGameInfo.Index
                            {
                                GameInfo = new JsonGameInfo.GameInfo
                                {
                                    ExecuteName = downloadIndex.ExecuteName,
                                    Name = System.IO.Path.GetFileName(savePath),
                                    Version = downloadIndex.Version,
                                    VersionType = downloadIndex.VersionType
                                },
                                Record = new JsonGameInfo.Record
                                {
                                    FirstPlay = DateTimeOffset.Now.ToUnixTimeSeconds(),
                                    PlayCount = 0,
                                    PlayTime = 0
                                }
                            };
                            Json.WriteJson(System.IO.Path.Combine(savePath, ".pvzl.json"), jsonContent);
                            jsonContentName = jsonContent.GameInfo.Name;
                        }
                        else
                        {
                            JsonTrainerInfo.Index jsonContent = new JsonTrainerInfo.Index
                            {
                                Name = downloadIndex.Name,
                                ExecuteName = downloadIndex.ExecuteName,
                                Version = downloadIndex.Version
                            };
                            Json.WriteJson(System.IO.Path.Combine(savePath, ".pvzl.json"), jsonContent);
                        }

                        notificationManager.Show(new NotificationContent
                        {
                            Title = "下载完成",
                            Message = $"\"{System.IO.Path.GetFileName(savePath)}\" 已添加进您的{(listbox.Tag.ToString() == "trainer" ? "修改器" : "游戏")}库!",
                            Type = NotificationType.Success
                        });

                        //设置当前项
                        if (listbox.Tag.ToString() != "trainer")
                            AppInfo.Config.CurrentGame = jsonContentName;
                        NavigationController.Navigate(this, "Manage");

                        EndLoad();
                    }
                    catch (Exception ex)
                    {
                        ErrorReportDialog.Show("发生错误", "下载发生错误", ex);
                    }
                }));
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "下载游戏时发生错误", ex);
            }
        }
    }
}
