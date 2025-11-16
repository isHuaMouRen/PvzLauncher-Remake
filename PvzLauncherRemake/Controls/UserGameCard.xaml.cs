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

namespace PvzLauncherRemake.Controls
{
    /// <summary>
    /// UserGameCard.xaml 的交互逻辑
    /// </summary>
    public partial class UserGameCard : UserControl
    {
        public UIElement[] Icon { get; set; } = { new Rectangle { Height = 50, Width = 50, Fill = new SolidColorBrush(Color.FromRgb(255, 0, 255)) } };
        public string Title { get; set; } = "Title";
        public string Version { get; set; } = "1.0.0.0";


        public UserGameCard()
        {
            InitializeComponent();

            Loaded += LoadUI;
        }

        private void LoadUI(object sender, RoutedEventArgs e)
        {
            if (Icon.Length > 0)
            {
                grid_Icon.Children.Clear();
                foreach (var path in Icon)
                {
                    grid_Icon.Children.Add(path);
                }
            }

            textBlock_Title.Text = Title;
            textBlock_Version.Text = Version;
        }
    }
}
