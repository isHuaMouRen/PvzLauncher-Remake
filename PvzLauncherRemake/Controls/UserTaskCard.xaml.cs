using System;
using System.Collections.Generic;
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

namespace PvzLauncherRemake.Controls
{
    /// <summary>
    /// UserTaskCard.xaml 的交互逻辑
    /// </summary>
    public partial class UserTaskCard : UserControl
    {
        public string Title { get; set; } = "Title";
        public double Progress { get; set; }
        public double Speed { get; set; }


        public UserTaskCard()
        {
            InitializeComponent();
        }

        public void UpdateControl()
        {
            textBlock_Title.Text = Title;
            textBlock_Progress.Text = Progress == 100 ? "解压中..." : $"{Math.Round(Progress, 2)}%";
            textBlock_Speed.Text = $"{Math.Round(Speed,2)}Mb/s";
            progressBar.Value = Progress;
            button_Cancel.IsEnabled = Progress == 100 ? false : true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
