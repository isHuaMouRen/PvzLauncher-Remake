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

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageLaunch.xaml 的交互逻辑
    /// </summary>
    public partial class PageLaunch : ModernWpf.Controls.Page
    {
        #region Init
        public void Initialize() { }
        public void InitializeLoaded()
        {
            try
            {

            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", "加载后初始化 PageLaunch 发生错误", ex);
            }
        }
        #endregion

        public PageLaunch()
        {
            InitializeComponent();
            Initialize();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) { InitializeLoaded(); }
    }
}
