using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.Media.Animation;
using Newtonsoft.Json;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using PvzLauncherRemake.Pages;
using PvzLauncherRemake.Utils;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Navigation;
using static PvzLauncherRemake.Class.AppLogger;

namespace PvzLauncherRemake
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<string, Type> PageMap = new Dictionary<string, Type>();//Page预加载
        private NavigationTransitionInfo FrameAnimation = new DrillInNavigationTransitionInfo();//Frame切换动画

        #region Init
        public async void Initialize()
        {
            try
            {
                logger.Info($"[主窗口] 开始构造...");
                //初始化配置文件
                if (!File.Exists(System.IO.Path.Combine(AppInfo.ExecuteDirectory, "config.json")))
                {
                    logger.Info($"[主窗口] 未检测到配置文件，即将创建");
                    ConfigManager.CreateDefaultConfig();
                }//创建游戏目录
                if (!Directory.Exists(AppInfo.GameDirectory))
                {
                    logger.Info($"[主窗口] 未检测到游戏文件夹，即将创建");
                    Directory.CreateDirectory(AppInfo.GameDirectory);
                }
                if (!Directory.Exists(AppInfo.TrainerDirectory))
                {
                    logger.Info($"[主窗口] 未检测到修改器文件夹，即将创建");
                    Directory.CreateDirectory(AppInfo.TrainerDirectory);
                }

                //读配置
                ConfigManager.LoadConfig();

                //应用配置
                logger.Info($"[主窗口] 开始应用配置");
                logger.Info($"[主窗口] 当前配置文件: {JsonConvert.SerializeObject(AppInfo.Config)}");
                this.Title = AppInfo.Config.LauncherConfig.WindowTitle;
                this.Width = AppInfo.Config.LauncherConfig.WindowSize.Width;
                this.Height = AppInfo.Config.LauncherConfig.WindowSize.Height;
                switch (AppInfo.Config.LauncherConfig.NavigationViewAlign)
                {
                    case "Left":
                        navView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact; break;
                    case "Top":
                        navView.PaneDisplayMode = NavigationViewPaneDisplayMode.Top; break;
                }
                switch (AppInfo.Config.LauncherConfig.Theme)
                {
                    case "Light":
                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light; break;
                    case "Dark":
                        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark; break;
                }

                //注册事件
                logger.Info($"[主窗口] 注册窗口大小改变事件...");
                this.SizeChanged += ((sender, e) =>
                {
                    logger.Info($"[主窗口] 窗口大小改变:  Width: {this.Width}  Height: {this.Height}");
                    AppInfo.Config.LauncherConfig.WindowSize = new JsonConfig.WindowSize { Width = this.Width, Height = this.Height };
                    ConfigManager.SaveConfig();
                });


                //加载列表
                await GameManager.LoadGameListAsync();
                await GameManager.LoadTrainerListAsync();

                //预加载Page
                void AddType(Type t)
                {
                    PageMap.Add($"{t.Name}", t);
                    logger.Info($"[主窗口] 预加载Page: {t.Name}");
                }
                AddType(typeof(PageLaunch));
                AddType(typeof(PageManage));
                AddType(typeof(PageDownload));

                AddType(typeof(PageTask));
                AddType(typeof(PageSettings));
                AddType(typeof(PageAbout));

                //选择默认页
                navView.SelectedItem = navViewItem_Launch;

                logger.Info($"[主窗口] 构造完毕!");
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", $"初始化 MainWindow 发生错误", ex);
            }
        }

        public async Task InitializeLoaded()
        {
            try
            {
                logger.Info($"[主窗口] 开始初始化...");

                logger.Info($"[主窗口] 处理启动参数");

                //是否CI构建
#if CI
                AppInfo.Arguments.isCIBuild = true;
#endif
                //是否Debug构建
#if DEBUG
                AppInfo.Arguments.isDebugBuild = true;
#endif


                //处理启动参数
                string[] args = Environment.GetCommandLineArgs();
                foreach (var arg in args)
                {
                    logger.Info($"[主窗口] 传入的参数: {arg}");
                    switch (arg)
                    {
                        //外壳启动
                        case "-shell":
                            AppInfo.Arguments.isShell = true; break;
                        //更新启动，显示更新完毕对话框
                        case "-update":
                            AppInfo.Arguments.isUpdate = true; break;
                    }
                }



                logger.Info($"[主窗口] {new string('=', 10)}启动参数配置{new string('=', 10)}");
                logger.Info($"[主窗口] isShell={AppInfo.Arguments.isShell}");
                logger.Info($"[主窗口] isUpdate={AppInfo.Arguments.isUpdate}");
                logger.Info($"[主窗口] ");
                logger.Info($"[主窗口] IsCIBuild={AppInfo.Arguments.isCIBuild}");
                logger.Info($"[主窗口] {new string('=', 30)}");

                //参数检测
                if (!AppInfo.Arguments.isShell && !Debugger.IsAttached)//是否外壳启动
                {
                    await DialogManager.ShowDialogAsync(new ContentDialog
                    {
                        Title = "警告",
                        Content = "检测到程序非外壳启动, 此启动方式可能会导致某些意外的事情发生",
                        PrimaryButtonText = "改用外壳启动",
                        CloseButtonText = "忽略",
                        DefaultButton = ContentDialogButton.Primary
                    }, (() =>
                    {
                        //Primary=>改用外壳启动
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = System.IO.Path.Combine(AppInfo.RootDirectory, "PvzLauncher.exe"),
                            UseShellExecute = true
                        });
                        Environment.Exit(0);
                    }));
                }
                if (AppInfo.Arguments.isUpdate)//更新启动
                {
                    await DialogManager.ShowDialogAsync(new ContentDialog
                    {
                        Title = "更新完毕",
                        Content = $"您已更新到最新版 {AppInfo.Version} , 尽情享受吧！",
                        PrimaryButtonText = "确定",
                        DefaultButton = ContentDialogButton.Primary
                    });
                }


                //构建检测
                if (AppInfo.Arguments.isCIBuild)//CI
                {
                    await DialogManager.ShowDialogAsync(new ContentDialog
                    {
                        Title = "警告",
                        Content = $"您使用的是基于 {AppInfo.Version} 构建的CI版本\nCI构建每个提交的临时测试版本，因此CI版本及其不稳定，仅用于测试使用\n\n如果使用CI版本出现了BUG请不要反馈给开发者!",
                        PrimaryButtonText = "我明确风险且遇到BUG会反馈开发者",
                        CloseButtonText = "我明确风险并了解处理BUG的方法",
                        DefaultButton = ContentDialogButton.Primary
                    }, (() => Environment.Exit(0)));
                }
                else if (AppInfo.Arguments.isDebugBuild)//DEBUG
                {
                    await DialogManager.ShowDialogAsync(new ContentDialog
                    {
                        Title = "警告",
                        Content = $"您使用的是您自行构建的版本，此版本的稳定性与安全性无法得到保证，如果你自己改动代码导致了BUG，请不要反馈给开发者!",
                        PrimaryButtonText = "我明确风险且遇到BUG会反馈开发者",
                        CloseButtonText = "我明确风险并了解处理BUG的方法",
                        DefaultButton = ContentDialogButton.Primary
                    }, (() => Environment.Exit(0)));
                }










                //检查更新
                if (AppInfo.Config.LauncherConfig.StartUpCheckUpdate)
                {
                    logger.Info($"[主窗口] 检测更新");
                    await Updater.CheckUpdate(null!, true);
                }


                logger.Info($"[主窗口] 完成初始化!");
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", $"加载 MainWindow 发生错误", ex);
            }
        }
#endregion


        public MainWindow() { InitializeComponent(); Initialize(); }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(async () =>
            {
                await InitializeLoaded();
            }, System.Windows.Threading.DispatcherPriority.Normal);
        }

        private void navView_SelectionChanged(ModernWpf.Controls.NavigationView sender, ModernWpf.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            try
            {
                if (navView.SelectedItem is NavigationViewItem item)
                {
                    logger.Info($"[主窗口] 选择标签页: {item.Tag}");

                    frame.Navigate(PageMap[$"Page{item.Tag}"], null, FrameAnimation);
                }
                else
                    throw new Exception($"非法的项: {navView.SelectedItem}");
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "处理选择事件发生错误", ex);
            }
        }

        private void frame_Navigated(object sender, NavigationEventArgs e)
        {
            try
            {
                //判断是否显示返回箭头
                if (frame.Content is PageManageSet || frame.Content is PageDeveloper)
                {
                    navView.IsBackButtonVisible = NavigationViewBackButtonVisible.Visible;
                    navView.IsBackEnabled = true;
                }
                else
                {
                    navView.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
                    navView.IsBackEnabled = false;
                }
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "处理导航后事件发生错误", ex);
            }
        }

        private void navView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            frame.GoBack();
        }
    }
}