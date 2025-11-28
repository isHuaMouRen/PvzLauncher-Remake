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
    /// UserCard.xaml 的交互逻辑
    /// </summary>
    public partial class UserCard : UserControl
    {
        public string Title { get; set; } = "Title";
        public string Description { get; set; }
        public string Icon { get; set; } = "origin";
        public string Version { get; set; }
        public string Size { get; set; }
        public string SupportVersion { get; set; }
        public bool isRecommend { get; set; }
        public bool isNew { get; set; }
        public bool isActive { get; set; }

        public UserCard()
        {
            InitializeComponent();
            Loaded += ((s, e) =>
            {
                textBlock_Title.Text = Title;
                textBlock_Description.Text = Description;

                //图标
                Viewbox[] icons = { viewBox_Origin, viewBox_Beta, viewBox_Tat };
                foreach (var icon in icons)
                {
                    if ($"viewBox_{Icon}" == icon.Name)
                        icon.Visibility = Visibility.Visible;
                    else
                        icon.Visibility = Visibility.Hidden;
                }

                SetLabels();
            });
        }

        public void SetLabels()
        {
            //清除
            stackPanel_Labels.Children.Clear();
            if (!string.IsNullOrEmpty(Version))
            {
                string xaml =
                    "<Grid Margin=\"0,0,5,0\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">" +
                        "<Rectangle Height=\"20\" RadiusY=\"3\" RadiusX=\"3\" Fill=\"#CC969696\"/>" +
                        $"<TextBlock Text=\"{Version}\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" Foreground=\"White\" Margin=\"5,0,5,0\"/>" +
                    "</Grid>";
                stackPanel_Labels.Children.Add(XamlReader.Parse(xaml) as Grid);
            }
            if (!string.IsNullOrEmpty(Size))
            {
                string xaml =
                    "<Grid Margin=\"0,0,5,0\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">" +
                        "<Rectangle Height=\"20\" RadiusY=\"3\" RadiusX=\"3\" Fill=\"#CC009696\"/>" +
                        $"<TextBlock Text=\"{Size} MB\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" Foreground=\"White\" Margin=\"5,0,5,0\"/>" +
                    "</Grid>";
                stackPanel_Labels.Children.Add(XamlReader.Parse(xaml) as Grid);
            }
            if (!string.IsNullOrEmpty(SupportVersion))
            {
                string xaml =
                    "<Grid Margin=\"0,0,5,0\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">" +
                        "<Rectangle Height=\"20\" RadiusY=\"3\" RadiusX=\"3\" Fill=\"#CC0000FF\"/>" +
                        $"<TextBlock Text=\"支持版本: {SupportVersion}\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" Foreground=\"White\" Margin=\"5,0,5,0\"/>" +
                    "</Grid>";
                stackPanel_Labels.Children.Add(XamlReader.Parse(xaml) as Grid);
            }
            if (isRecommend)
            {
                string xaml =
                    "<Grid Margin=\"0,0,5,0\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">" +
                        "<Rectangle Height=\"20\" RadiusY=\"3\" RadiusX=\"3\" Fill=\"#CC00FF00\"/>" +
                        $"<TextBlock Text=\"推荐\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" Foreground=\"White\" Margin=\"5,0,5,0\"/>" +
                    "</Grid>";
                stackPanel_Labels.Children.Add(XamlReader.Parse(xaml) as Grid);
            }
            if (isNew)
            {
                string xaml =
                    "<Grid Margin=\"0,0,5,0\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">" +
                        "<Rectangle Height=\"20\" RadiusY=\"3\" RadiusX=\"3\" Fill=\"#CC6400FF\"/>" +
                        $"<TextBlock Text=\"新\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" Foreground=\"White\" Margin=\"5,0,5,0\"/>" +
                    "</Grid>";
                stackPanel_Labels.Children.Add(XamlReader.Parse(xaml) as Grid);
            }
            if (isActive)
            {
                string xaml =
                    "<Grid Margin=\"0,0,5,0\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">" +
                        "<Rectangle Height=\"20\" RadiusY=\"3\" RadiusX=\"3\" Fill=\"#CCFF0000\"/>" +
                        $"<TextBlock Text=\"活动\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" Foreground=\"White\" Margin=\"5,0,5,0\"/>" +
                    "</Grid>";
                stackPanel_Labels.Children.Add(XamlReader.Parse(xaml) as Grid);
            }
        }
    }
}
