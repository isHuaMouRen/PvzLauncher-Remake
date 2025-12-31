using ModernWpf.Controls;
using ModernWpf.Media.Animation;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using PvzLauncherRemake.Pages;
using PvzLauncherRemake.Utils;
using PvzLauncherRemake.Utils.Configuration;
using PvzLauncherRemake.Utils.Services;
using PvzLauncherRemake.Utils.UI;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using static PvzLauncherRemake.Class.AppLogger;

namespace PvzLauncherRemake
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WindowMain : Window
    {
        private Dictionary<string, Type> PageMap = new Dictionary<string, Type>();//Page预加载
        private NavigationTransitionInfo FrameAnimation = new DrillInNavigationTransitionInfo();//Frame切换动画

        #region Init
        public async void Initialize()
        {
            try
            {
                //应用配置
                this.Title = AppGlobals.Config.LauncherConfig.WindowTitle;
                this.Width = AppGlobals.Config.LauncherConfig.WindowSize.Width;
                this.Height = AppGlobals.Config.LauncherConfig.WindowSize.Height;
                switch (AppGlobals.Config.LauncherConfig.NavigationViewAlign)
                {
                    case "Left":
                        navView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftCompact; break;
                    case "Top":
                        navView.PaneDisplayMode = NavigationViewPaneDisplayMode.Top; break;
                }

                //注册事件
                logger.Info($"[主窗口] 注册窗口大小改变事件...");
                this.SizeChanged += ((sender, e) =>
                {
                    logger.Info($"[主窗口] 窗口大小改变:  Width: {this.Width}  Height: {this.Height}");
                    AppGlobals.Config.LauncherConfig.WindowSize = new JsonConfig.WindowSize { Width = this.Width, Height = this.Height };
                    ConfigManager.SaveConfig();
                });

                //预加载Page
                void AddType(Type t)
                {
                    PageMap.Add($"{t.Name}", t);
                    logger.Info($"[主窗口] 预加载Page: {t.Name}");
                }
                AddType(typeof(PageLaunch));
                AddType(typeof(PageManage));
                AddType(typeof(PageDownload));
                AddType(typeof(PageToolbox));

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
                AppGlobals.Arguments.isCIBuild = true;
#endif
                //是否Debug构建
#if DEBUG
                AppGlobals.Arguments.isDebugBuild = true;
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
                            AppGlobals.Arguments.isShell = true; break;
                        //更新启动，显示更新完毕对话框
                        case "-update":
                            AppGlobals.Arguments.isUpdate = true; break;
                    }
                }



                logger.Info($"[主窗口] {new string('=', 10)}启动参数配置{new string('=', 10)}");
                logger.Info($"[主窗口] isShell={AppGlobals.Arguments.isShell}");
                logger.Info($"[主窗口] isUpdate={AppGlobals.Arguments.isUpdate}");
                logger.Info($"[主窗口] ");
                logger.Info($"[主窗口] IsCIBuild={AppGlobals.Arguments.isCIBuild}");
                logger.Info($"[主窗口] {new string('=', 30)}");

                //参数检测
                if (!AppGlobals.Arguments.isShell && !Debugger.IsAttached)//是否外壳启动
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
                            FileName = System.IO.Path.Combine(AppGlobals.RootDirectory, "PvzLauncher.exe"),
                            UseShellExecute = true
                        });
                        Environment.Exit(0);
                    }));
                }
                if (AppGlobals.Arguments.isUpdate)//更新启动
                {
                    await DialogManager.ShowDialogAsync(new ContentDialog
                    {
                        Title = "更新完毕",
                        Content = $"您已更新到最新版 {AppGlobals.Version} , 尽情享受吧！",
                        PrimaryButtonText = "确定",
                        DefaultButton = ContentDialogButton.Primary
                    });
                }


                //构建检测
                if (AppGlobals.Arguments.isCIBuild)//CI
                {
                    await DialogManager.ShowDialogAsync(new ContentDialog
                    {
                        Title = "警告",
                        Content = $"您使用的是基于 {AppGlobals.Version} 构建的CI版本\nCI构建是每个提交自动生成的，稳定性无法得到保证，因此仅用于测试使用\n\n如果使用CI版本出现了BUG请不要反馈给开发者!",
                        PrimaryButtonText = "我明确风险且遇到BUG会反馈开发者",
                        CloseButtonText = "我明确风险并了解处理BUG的方法",
                        DefaultButton = ContentDialogButton.Primary
                    }, (() => Environment.Exit(0)));
                }
                else if (AppGlobals.Arguments.isDebugBuild)//DEBUG
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
                if (AppGlobals.Config.LauncherConfig.StartUpCheckUpdate)
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


        public WindowMain() { InitializeComponent(); Initialize(); }

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

                if (frame.Content is PageManageSet || frame.Content is PageDeveloper || frame.Content is PageDownloadConfirm) 
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