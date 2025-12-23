using System.Windows;
using System.Windows.Media.Animation;

namespace PvzLauncherRemake.Windows
{
    /// <summary>
    /// WindowSplash.xaml 的交互逻辑
    /// </summary>
    public partial class WindowSplash : Window
    {
        public WindowSplash()
        {
            InitializeComponent();
        }

        public void ShowAndFadeIn()
        {
            icon.Opacity = 0;
            this.Show();
            var animation = new DoubleAnimation
            {
                To = 1,
                From = 0,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            };
            icon.BeginAnimation(OpacityProperty, null);
            icon.BeginAnimation(OpacityProperty, animation);
        }

        public async Task FadeOutAndClose()
        {
            var animation = new DoubleAnimation
            {
                To = 0,
                From = 1,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
            };
            icon.BeginAnimation(OpacityProperty, null);
            icon.BeginAnimation(OpacityProperty, animation);
            await Task.Delay(500);
            this.Close();
        }
    }
}
