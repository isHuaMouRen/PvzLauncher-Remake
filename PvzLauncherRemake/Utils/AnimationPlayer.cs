using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace PvzLauncherRemake.Utils
{
    public class AnimationPlayer
    {
        public static async Task PlayListBoxAnimation(ListBox listBox, double playDelay = 20)
        {
            List<UserControl> animationControls = new List<UserControl>();
            animationControls.Clear();

            if (listBox.Items.Count > 0)
            {
                if (listBox.Items[0] is not UserControl)
                    return;

                if (listBox.Items[0] is UserControl item)
                {
                    animationControls.Add(item);
                }

                var animation = new ThicknessAnimation
                {
                    To = new Thickness(0),
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new PowerEase { Power = 5, EasingMode = EasingMode.EaseOut }
                };

                foreach (var ctrl in animationControls)
                    ctrl.Margin = new Thickness(-ctrl.Width, 0, -ctrl.Width, 0);

                await Task.Delay(20);

                foreach (var ctrl in animationControls)
                {
                    ctrl.BeginAnimation(FrameworkElement.MarginProperty, animation);
                }
            }
        }
    }
}
