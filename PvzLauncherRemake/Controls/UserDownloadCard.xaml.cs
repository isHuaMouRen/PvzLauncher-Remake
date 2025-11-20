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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PvzLauncherRemake.Controls
{
    /// <summary>
    /// UserDownloadCard.xaml 的交互逻辑
    /// </summary>
    public partial class UserDownloadCard : UserControl
    {
        public string Title { get; set; } = "Title";
        public string Description { get; set; } = null!;
        public string Version { get; set; } = "1.0.0.0";
        public string SupportVersion { get; set; } = null!;
        public string Icon { get; set; } = "origin";
        public bool isRecommend { get; set; } = false;
        public bool isNew { get; set; } = false;

        public UserDownloadCard()
        {
            InitializeComponent();
            Loaded += LoadUI;
        }

        public void LoadUI(object sender, RoutedEventArgs e)
        {
            textBlock_Title.Text = Title;
            textBlock_Description.Text = Description;

            List<Viewbox> icons = new List<Viewbox> { viewBox_beta, viewBox_origin, viewBox_tat };
            foreach (var icon in icons)
            {
                if (icon.Tag.ToString() != Icon)
                    icon.Visibility = Visibility.Hidden;
            }

            //添加lCard
            stackPanel_Labels.Children.Clear();
            if (!string.IsNullOrEmpty(Version))
            {
                string xaml = "<Grid Margin=\"0,0,5,0\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">" +
                                "<Rectangle Height=\"20\" Fill=\"#FF969696\" RadiusX=\"3\" RadiusY=\"3\"/>" +
                                $"<TextBlock Text=\"{Version}\" Foreground=\"White\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" Margin=\"5,0,5,0\"/>" +
                              "</Grid>";
                stackPanel_Labels.Children.Add(XamlReader.Parse(xaml) as Grid);
            }
            if (!string.IsNullOrEmpty(SupportVersion))
            {
                string xaml = "<Grid Margin=\"0,0,5,0\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">" +
                                "<Rectangle Height=\"20\" Fill=\"#FF3264FF\" RadiusX=\"3\" RadiusY=\"3\"/>" +
                                $"<TextBlock Text=\"支持版本: {SupportVersion}\" Foreground=\"White\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" Margin=\"5,0,5,0\"/>" +
                              "</Grid>";
                stackPanel_Labels.Children.Add(XamlReader.Parse(xaml) as Grid);
            }
            if (isRecommend)
            {
                string xaml = "<Grid Margin=\"0,0,5,0\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">" +
                                "<Rectangle Height=\"20\" Fill=\"#FF64FF64\" RadiusX=\"3\" RadiusY=\"3\"/>" +
                                "<TextBlock Text=\"推荐\" Foreground=\"White\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" Margin=\"5,0,5,0\" FontWeight=\"Bold\"/>" +
                              "</Grid>";
                stackPanel_Labels.Children.Add(XamlReader.Parse(xaml) as Grid);
            }
            if (isNew)
            {
                string xaml = "<Grid Margin=\"0,0,5,0\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">" +
                                "<Rectangle Height=\"20\" Fill=\"#FF6432FF\" RadiusX=\"3\" RadiusY=\"3\"/>" +
                                "<TextBlock Text=\"新\" Foreground=\"White\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" Margin=\"5,0,5,0\" FontWeight=\"Bold\"/>" +
                              "</Grid>";
                stackPanel_Labels.Children.Add(XamlReader.Parse(xaml) as Grid);
            }
        }
    }
}
