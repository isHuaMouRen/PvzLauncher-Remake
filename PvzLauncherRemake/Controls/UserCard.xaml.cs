using PvzLauncherRemake.Controls.Icons;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Utils;

namespace PvzLauncherRemake.Controls
{
    /// <summary>
    /// UserCard.xaml 的交互逻辑
    /// </summary>
    public partial class UserCard : UserControl
    {


        public string Title { get; set; } = "Title";
        public string Description { get; set; }
        public GameIcons Icon { get; set; } = GameIcons.Origin;
        public string Version { get; set; }
        public string Size { get; set; }
        public string SupportVersion { get; set; }
        public bool isRecommend { get; set; }
        public bool isNew { get; set; }
        public bool isActive { get; set; }
        public object AttachedProperty { get; set; }
        public bool BigIconMode { get; set; } = false;

        public UserCard()
        {
            InitializeComponent();
            Loaded += ((s, e) =>
            {
                if (BigIconMode)
                {
                    this.Height = 100;
                    grid_Icon.Height = 80; grid_Icon.Width = 80;
                    stackPanel_Title.Margin = new Thickness(90, 5, 5, 5);
                    stackPanel_Labels.Margin = new Thickness(90, 5, 5, 5);
                }



                textBlock_Title.Text = Title;
                textBlock_Description.Text = Description;

                //图标
                viewBox_Icon.Child = GameManager.ParseToUserControl(Icon);

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

        private void rectangle_MouseTrigger_MouseEnter(object sender, MouseEventArgs e)
        {
            var animation = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            };
            border.BeginAnimation(OpacityProperty, null);
            border.BeginAnimation(OpacityProperty, animation);
        }

        private void rectangle_MouseTrigger_MouseLeave(object sender, MouseEventArgs e)
        {
            var animation = new DoubleAnimation
            {
                To = 0,
                From = 1,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            };
            border.BeginAnimation(OpacityProperty, null);
            border.BeginAnimation(OpacityProperty, animation);
        }

        private void rectangle_MouseTrigger_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var animation = new DoubleAnimation
            {
                To = 0.98,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            };

            grid_Content_ScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            grid_Content_ScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            grid_Content_ScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            grid_Content_ScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
        }

        private void rectangle_MouseTrigger_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var animation = new DoubleAnimation
            {
                To = 1,
                From = 0.98,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            };

            grid_Content_ScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            grid_Content_ScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            grid_Content_ScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            grid_Content_ScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
        }
    }
}
