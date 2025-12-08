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
        public string Icon { get; set; } = "Origin";
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
                AddLabel(Version, Color.FromArgb(204, 150, 150, 150), false);
            if (!string.IsNullOrEmpty(Size))
                AddLabel($"{Size} MB", Color.FromArgb(204, 0, 150, 150), false);
            if (!string.IsNullOrEmpty(SupportVersion))
                AddLabel($"支持版本: {SupportVersion}", Color.FromArgb(204, 0, 0, 255), false);
            if (isRecommend)
                AddLabel($"推荐", Color.FromArgb(204, 0, 255, 0), true);
            if (isNew)
                AddLabel($"新", Color.FromArgb(204, 100, 0, 255), true);
            if (isActive)
                AddLabel($"活动", Color.FromArgb(204, 255, 0, 0), true);
        }

        public void AddLabel(string content, Color color, bool textBold)
        {
            var label = new Grid
            {
                Margin = new Thickness(0, 0, 5, 0),
                Children =
                {
                    new Rectangle
                    {
                        Height=20,
                        RadiusX=3,
                        RadiusY=3,
                        Fill=new SolidColorBrush(color)
                    },
                    new TextBlock
                    {
                        Text=content,
                        HorizontalAlignment=HorizontalAlignment.Center,
                        VerticalAlignment=VerticalAlignment.Center,
                        Foreground=new SolidColorBrush(Colors.White),
                        Margin=new Thickness(5,0,5,0)
                    }
                }
            };
            stackPanel_Labels.Children.Add(label);
        }
    }
}
