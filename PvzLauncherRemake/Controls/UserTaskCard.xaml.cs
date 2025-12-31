using PvzLauncherRemake.Class;
using PvzLauncherRemake.Utils.Services;
using System.Windows;
using System.Windows.Controls;

namespace PvzLauncherRemake.Controls
{
    /// <summary>
    /// UserTaskCard.xaml 的交互逻辑
    /// </summary>
    public partial class UserTaskCard : UserControl
    {
        public string Title { get; set; } = "Title";
        public double Progress { get; set; }
        public double ProgressCompress { get; set; }
        public double Speed { get; set; }
        public GameIcons Icon { get; set; } = GameIcons.Unknown;


        public UserTaskCard()
        {
            InitializeComponent();
        }

        public void UpdateControl()
        {
            viewBox_Icon.Child = GameManager.ParseToUserControl(Icon);

            textBlock_Title.Text = Title;

            textBlock_Progress_Download.Text = Progress == 100 ? "下载完成" : $"下载中... {Math.Round(Progress, 2)}%";
            textBlock_Speed_Download.Text = $"{Math.Round(Speed, 2)}Mb/s";
            progressBar_Download.Value = Progress;

            textBlock_Progress_Compress.Text = ProgressCompress == 100 ? "解压完毕" : ProgressCompress == 0 ? "等待下载完毕" : $"解压中... {Math.Round(ProgressCompress, 2)}%";
            progressBar_Compress.Value = ProgressCompress;

            button_Cancel.IsEnabled = Progress == 100 ? false : true;
            progressRing_Download.IsActive = (Progress == 100 || Progress == 0) ? false : true;
            progressRing_Compress.IsActive = (ProgressCompress == 100 || ProgressCompress == 0) ? false : true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
