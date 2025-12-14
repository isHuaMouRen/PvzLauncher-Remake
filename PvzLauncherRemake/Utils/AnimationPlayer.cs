using ModernWpf.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace PvzLauncherRemake.Utils
{
    public class AnimationPlayer
    {
        public static async Task PlayListBoxAnimation(ListBox listBox, int playDelay = 20)
        {
            List<UserControl> animationControls = new List<UserControl>();
            animationControls.Clear();

            if (listBox.Items.Count > 0)
            {
                if (listBox.Items[0] is not UserControl)
                    return;

                foreach (var item in listBox.Items)
                {
                    animationControls.Add((UserControl)item);
                }

                var animation = new ThicknessAnimation
                {
                    To = new Thickness(0),
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
                };

                foreach (var ctrl in animationControls)
                    ctrl.Margin = new Thickness(0 - ctrl.ActualWidth, 0, 0 - ctrl.ActualWidth, 0);

                await Task.Delay(playDelay);

                foreach (var ctrl in animationControls)
                {
                    ctrl.BeginAnimation(FrameworkElement.MarginProperty, animation);
                    await Task.Delay(20);
                }
            }
        }
    }
}
