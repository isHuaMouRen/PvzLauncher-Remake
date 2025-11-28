using HuaZi.Library.Downloader;
using HuaZi.Library.Json;
using ModernWpf.Controls;
using Newtonsoft.Json;
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
using static PvzLauncherRemake.Class.AppLogger;

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
                logger.Info($"[下载] 开始初始化...");
                StartLoad();

                using (var client = new HttpClient())
                {
                    string indexString = await client.GetStringAsync(AppInfo.DownloadIndexUrl);
                    logger.Info($"[下载] 获取下载索引: {indexString}");
                    DownloadIndex = Json.ReadJson<JsonDownloadIndex.Index>(indexString);
                }

                //中文原版
                logger.Info($"[下载] 开始加载中文原版游戏列表");
                listBox_zhOrigin.Items.Clear();
                foreach (var zhOriginGame in DownloadIndex.ZhOrigin)
                {
                    var card = new UserCard
                    {
                        Title = zhOriginGame.Name,
                        Description = zhOriginGame.Description,
                        Icon = "Origin",
                        Version = zhOriginGame.Version,
                        Size = zhOriginGame.Size.ToString(),
                        isNew = zhOriginGame.IsNew,
                        isRecommend = zhOriginGame.IsRecommend,
                        Tag = zhOriginGame
                    };
                    logger.Info($"[下载] 添加游戏卡片: Title: {card.Title} | Icon: {card.Icon} | Version: {card.Version}");
                    listBox_zhOrigin.Items.Add(card);
                }

                //中文改版
                logger.Info($"[下载] 开始加载中文改版游戏列表");
                listBox_zhRevision.Items.Clear();
                foreach (var zhRevisionGame in DownloadIndex.ZhRevision)
                {
                    var card = new UserCard
                    {
                        Title = zhRevisionGame.Name,
                        Description = zhRevisionGame.Description,
                        Icon = zhRevisionGame.Version.StartsWith("β") ? "Beta" :
                                zhRevisionGame.Version.StartsWith("TAT", StringComparison.OrdinalIgnoreCase) ? "Tat" : "Origin",
                        Version = zhRevisionGame.Version,
                        Size = zhRevisionGame.Size.ToString(),
                        isNew = zhRevisionGame.IsNew,
                        isRecommend = zhRevisionGame.IsRecommend,
                        Tag = zhRevisionGame
                    };
                    logger.Info($"[下载] 添加游戏卡片: Title: {card.Title} | Icon: {card.Icon} | Version: {card.Version}");
                    listBox_zhRevision.Items.Add(card);
                }

                //英文原版
                logger.Info($"[下载] 开始加载英文原版游戏列表");
                listBox_enOrigin.Items.Clear();
                foreach (var enOriginGame in DownloadIndex.EnOrigin)
                {
                    var card = new UserCard
                    {
                        Title = enOriginGame.Name,
                        Description = enOriginGame.Description,
                        Icon = "Origin",
                        Version = enOriginGame.Version,
                        Size = enOriginGame.Size.ToString(),
                        isNew = enOriginGame.IsNew,
                        isRecommend = enOriginGame.IsRecommend,
                        Tag = enOriginGame
                    };
                    logger.Info($"[下载] 添加游戏卡片: Title: {card.Title} | Icon: {card.Icon} | Version: {card.Version}");
                    listBox_enOrigin.Items.Add(card);
                }

                //修改器
                logger.Info($"[下载] 开始加载修改器列表");
                listBox_trainer.Items.Clear();
                foreach (var trainerInfo in DownloadIndex.Trainer)
                {
                    var card = new UserCard
                    {
                        Title = trainerInfo.Name,
                        Description = trainerInfo.Description,
                        Icon = "Origin",
                        Version = trainerInfo.Version,
                        Size = trainerInfo.Size.ToString(),
                        SupportVersion = trainerInfo.SupportVersion,
                        isNew = trainerInfo.IsNew,
                        isRecommend = trainerInfo.IsRecommend,
                        Tag = trainerInfo
                    };
                    logger.Info($"[下载] 添加修改器卡片: Title: {card.Title} | Icon: {card.Icon} | Version: {card.Version}");
                    listBox_trainer.Items.Add(card);
                }

                EndLoad();
                logger.Info($"[下载] 结束初始化");
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
                logger.Info($"[下载] 准备下载游戏...");

                var listbox = (ListBox)sender;
                var selectItem = (UserCard)listbox.SelectedItem;
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
                    logger.Info($"[下载] 用户同意下载");
                    try
                    {
                        //检测同名
                        bool isNameDone = false;
                        while (!isNameDone)
                        {
                            var textBox = new TextBox { Text = downloadIndex.Name };

                            if (Directory.Exists(savePath))
                            {
                                logger.Info($"[下载] 检测到同名文件夹，开始解决问题");
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
                                    logger.Info($"[下载] 同名冲突解决成功，新的名字: {textBox.Text}");
                                    savePath = System.IO.Path.Combine(AppInfo.GameDirectory, textBox.Text);
                                    isNameDone = true;
                                }
                            }
                            else
                                isNameDone = true;
                        }
                        StartLoad(true);
                        logger.Info($"[下载] 没有检测到冲突或冲突已解决，准备下载");
                        logger.Info($"[下载] 信息:\n" +
                            $"      保存位置: {savePath}\n" +
                            $"      URL: {downloadIndex.Url}\n" +
                            $"      临时下载文件位置: {Path.Combine(AppInfo.TempDiectory, "PVZLAUNCHERCACHE.zip")}");

                        //清除残留
                        if (File.Exists(System.IO.Path.Combine(AppInfo.TempDiectory, "PVZLAUNCHERCACHE.zip")))
                        {
                            logger.Info($"[下载] 检测到下载残留，开始清理");
                            await Task.Run(() => File.Delete(System.IO.Path.Combine(AppInfo.TempDiectory, "PVZLAUNCHERCACHE.zip")));
                        }

                        bool isDownloadDone = false;
                        string? downloadError = null;
                        //定义下载器
                        downloader = new Downloader
                        {
                            Url = downloadIndex.Url,
                            SavePath = System.IO.Path.Combine(AppInfo.TempDiectory, "PVZLAUNCHERCACHE.zip"),
                            Progress = ((p, s) =>
                            {
                                logger.Info($"[下载: 下载器] 下载进度: {Math.Round(p, 2)}  速度: {Math.Round(s / 1024, 2)} MB/s");
                                progressBar_Loading.Value = p;
                                textBlock_Loading.Text = $"下载中 {Math.Round(p, 2)}% ... ({Math.Round(s / 1024, 2)}MB/S)";
                            }),
                            Completed = ((s, e) =>
                            {
                                logger.Info($"[下载: 下载器] 下载任务完成:   成功: {s} | 失败信息: {e}");
                                if (s)
                                {
                                    isDownloadDone = true;
                                }
                                else
                                {
                                    downloadError = e;
                                    isDownloadDone = true;
                                }
                            })
                        };
                        logger.Info($"[下载] 开始下载...");
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
                        logger.Info($"[下载] 开始解压");
                        await Task.Run(() =>
                        {
                            //使用SharpCompress库解压
                            ArchiveFactory.WriteToDirectory(System.IO.Path.Combine(AppInfo.TempDiectory, "PVZLAUNCHERCACHE.zip"), savePath, new ExtractionOptions
                            {
                                ExtractFullPath = true,
                                Overwrite = true
                            });
                        });
                        logger.Info($"[下载] 解压完成");

                        //写Json
                        logger.Info($"[下载] 开始定义Json配置文件");

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
                            logger.Info($"[下载] {JsonConvert.SerializeObject(jsonContent)}");
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
                            logger.Info($"[下载] {JsonConvert.SerializeObject(jsonContent)}");
                            Json.WriteJson(System.IO.Path.Combine(savePath, ".pvzl.json"), jsonContent);
                            jsonContentName = jsonContent.Name;
                        }

                        notificationManager.Show(new NotificationContent
                        {
                            Title = "下载完成",
                            Message = $"\"{System.IO.Path.GetFileName(savePath)}\" 已添加进您的{(listbox.Tag.ToString() == "trainer" ? "修改器" : "游戏")}库!",
                            Type = NotificationType.Success
                        });
                        logger.Info($"[下载] 下载完成!!");

                        //设置当前项
                        if (listbox.Tag.ToString() != "trainer")
                            AppInfo.Config.CurrentGame = jsonContentName;
                        else
                            AppInfo.Config.CurrentTrainer = jsonContentName;

                        logger.Info($"[下载] 设置当前选中项: 游戏->{AppInfo.Config.CurrentGame}   修改器->{AppInfo.Config.CurrentTrainer}");
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
