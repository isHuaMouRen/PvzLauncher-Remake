using HuaZi.Library.Logger;
using ModernWpf.Controls;
using ModernWpf.Media.Animation;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Class.JsonConfigs;
using PvzLauncherRemake.Pages;
using PvzLauncherRemake.Utils;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
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

namespace PvzLauncherRemake
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<string, Type> PageMap = new Dictionary<string, Type>();//Page预加载
        private NavigationTransitionInfo FrameAnimation = new DrillInNavigationTransitionInfo();//Frame切换动画

        #region Initialize
        public void Initialize()
        {
            try
            {
                logger.Info($"MainWindow 开始初始化");

                //预加载
                void AddType(Type t)
                {
                    PageMap.Add($"{t.Name}", t);
                    logger.Info($"已将 {t.Name} 添加进预加载");
                }
                AddType(typeof(PageLaunch));
                AddType(typeof(PageManage));
                AddType(typeof(PageDownload));

                AddType(typeof(PageTask));
                AddType(typeof(PageSettings));
                AddType(typeof(PageAbout));

                //选择默认页
                navView.SelectedItem = navViewItem_Launch;
                logger.Info($"选择默认页: {((NavigationViewItem)navView.SelectedItem).Name}");

                //创建游戏目录
                if (!Directory.Exists(AppInfo.GameDirectory))
                {
                    logger.Info($"游戏目录 {AppInfo.GameDirectory} 不存在，即将创建");
                    Directory.CreateDirectory(AppInfo.GameDirectory);
                }

                //初始化配置文件
                if (!File.Exists(System.IO.Path.Combine(AppInfo.ExecutePath, "config.json")))
                {
                    AppInfo.Config = new JsonConfig.Index
                    {
                        CurrentGame = null!
                    };
                    ConfigManager.SaveAllConfig();
                }

                //读配置
                ConfigManager.ReadAllConfig();

                logger.Info($"MainWindow 结束初始化");
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", $"初始化 MainWindow 发生错误", ex);
            }
        }

        public async void InitializeLoaded()
        {
            try
            {
                logger.Info($"处理 MainWindow 加载事件");

                //处理启动参数
                string[] args = Environment.GetCommandLineArgs();
                foreach (var arg in args)
                {
                    switch (arg)
                    {
                        //外壳启动
                        case "-shell":
                            AppInfo.Arguments.isShell = true; break;
                    }
                }

                logger.Info("==========[启动参数信息]==========");
                logger.Info($"isShell: {AppInfo.Arguments.isShell}");
                logger.Info("==========[End]==========");

                //是否外壳启动
                if (!AppInfo.Arguments.isShell && !Debugger.IsAttached) 
                {
                    logger.Info("检测到没有使用Shell执行");
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
                            FileName = System.IO.Path.Combine(AppInfo.RootPath, "PvzLauncher.exe"),
                            UseShellExecute = true
                        });
                        Environment.Exit(0);
                    }));
                }

                logger.Info($"处理 MainWindow 加载事件完毕");
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", $"加载 MainWindow 发生错误", ex);
            }
        }
        #endregion


        public MainWindow() { InitializeComponent(); Initialize(); }

        private void Window_Loaded(object sender, RoutedEventArgs e) { InitializeLoaded(); }

        private void navView_SelectionChanged(ModernWpf.Controls.NavigationView sender, ModernWpf.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            try
            {
                if (navView.SelectedItem is NavigationViewItem item)
                {
                    logger.Info($"用户选择: {item.Tag} 页");

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
    }
}