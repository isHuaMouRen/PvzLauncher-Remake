using ModernWpf.Controls;
using System.Windows;
using static PvzLauncherRemake.Class.AppLogger;

namespace PvzLauncherRemake.Utils
{
    public static class NavigationController
    {
        public static void Navigate(DependencyObject obj, string target)
        {
            if (string.IsNullOrWhiteSpace(target)) return;

            try
            {
                if (Window.GetWindow(obj) is not WindowMain window) return;
                if (window.FindName("navView") is not NavigationView navView) return;

                NavigationViewItem? targetItem = null;

                //用Tag匹配
                targetItem = navView.MenuItems.OfType<NavigationViewItem>()
                    .Concat(navView.FooterMenuItems.OfType<NavigationViewItem>())
                    .FirstOrDefault(item => item.Tag?.ToString() == target)!;

                //兼容
                if (targetItem == null)
                {
                    targetItem = navView.FindName($"navViewItem_{target}") as NavigationViewItem;
                }

                if (targetItem != null)
                {
                    navView.SelectedItem = targetItem;
                    logger.Info($"[导航控制器] 已导航 → {target} ({targetItem.Content})");
                }
                else
                {
                    logger.Warn($"[导航控制器] 未找到导航目标：\"{target}\"（Tag 或 x:Name=\"navViewItem_{target}\" 均不存在）");
                }
            }
            catch (System.Exception ex)
            {
                logger.Error($"[导航控制器] 导航时发生异常：{ex.Message}");
            }
        }
    }
}