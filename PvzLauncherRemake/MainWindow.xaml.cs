using HuaZi.Library.Logger;
using ModernWpf.Controls;
using ModernWpf.Media.Animation;
using PvzLauncherRemake.Pages;
using PvzLauncherRemake.Utils;
using System.Runtime.CompilerServices;
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
                logger.Info($"{this.Name} 开始初始化");

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

                logger.Info($"{this.Name} 结束初始化");
            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", $"初始化 {this.Name} 发生错误", ex);
            }
        }

        public void InitializeLoaded()
        {

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