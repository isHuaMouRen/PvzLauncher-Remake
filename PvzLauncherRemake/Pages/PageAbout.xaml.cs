using ModernWpf.Controls;
using Notifications.Wpf;
using PvzLauncherRemake.Class;
using PvzLauncherRemake.Utils;
using PvzLauncherRemake.Utils.UI;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static PvzLauncherRemake.Class.AppLogger;

namespace PvzLauncherRemake.Pages
{
    /// <summary>
    /// PageAbout.xaml 的交互逻辑
    /// </summary>
    public partial class PageAbout : ModernWpf.Controls.Page
    {
        private long _eggCount = 0;

        private static readonly IReadOnlyList<(int clicks, string title, string message, NotificationType type, Action? action)>
            EasterEggs =
        [
            (10,  "香蒲", "你真的很无聊...",                               NotificationType.Information, null),
            (20,  "香蒲", "不是我说，你无聊的话可以去干其他事，能不能不要点我了", NotificationType.Information, null),
            (40,  "香蒲", "不 要 再 点 我 了 ! ! !",                   NotificationType.Warning,       null),
            (70,  "香蒲", "你可以去干一些有意义的事情，而不是在这里点一堆矢量路径！！！", NotificationType.Error, null),
            (100, "香蒲", "好了，到此为止。作者只做了100次点击的判断，后面没有了",     NotificationType.Success,      null),
            (130, "发生错误", "System.IndexOutOfRangeException: 索引超出了数组的边界。\r\n   在 Program.Main() 位置 C:\\Projects\\ArrayDemo\\Program.cs:第 11 行\r\n   在 System.Reflection.RuntimeMethodInfo.UnsafeInvokeInternal(Object obj, Object[] parameters, Object[] arguments)\r\n   在 System.Reflection.MethodBaseInvoker.InvokeWithFewArgs(Object obj, BindingFlags invokeAttr)",NotificationType.Error, null),
            (150, "香蒲", "看来骗不到你",                               NotificationType.Information, null),
            (200, "香蒲", "恭喜！200次点击",                           NotificationType.Information, null),
            (250, "香蒲", "好了，这次是真的没了，最大值就是250了。快走吧。", NotificationType.Information, null)
        ];

        public PageAbout()
        {
            InitializeComponent();

            textBlock_Version.Text = $"{AppGlobals.Version}{(AppGlobals.Arguments.isCIBuild ? " - CI" : AppGlobals.Arguments.isDebugBuild ? " - Debug" : null)}";
        }

        public void GoToUrl(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                logger.Info($"[关于] 跳转Url => {button.Tag}");
                Process.Start(new ProcessStartInfo
                {
                    FileName = button.Tag.ToString(),
                    UseShellExecute = true
                });
            }
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _eggCount++;
            logger.Info($"[关于] 触发彩蛋，当前点击次数: {_eggCount}");

            foreach (var (clicks, title, message, type, action) in EasterEggs)
            {
                if (_eggCount == clicks)
                {
                    new NotificationManager().Show(new NotificationContent
                    {
                        Title = title,
                        Message = message,
                        Type = type
                    });
                    action?.Invoke();
                    return;
                }
            }
        }

        private async void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                logger.Info($"[关于] 用户尝试进入控制台");

                if (Debugger.IsAttached)
                {
                    new NotificationManager().Show(new NotificationContent
                    {
                        Title = "开发者控制台",
                        Message = "检测到调试器附加，自动进入开发者控制台",
                        Type = NotificationType.Success
                    });
                    NavigationService?.Navigate(new PageDeveloper());
                    return;
                }

                var textBox = new TextBox();
                await DialogManager.ShowDialogAsync(new ContentDialog
                {
                    Title = "开发者控制台",
                    Content = new StackPanel
                    {
                        Children =
                        {
                            new TextBlock
                            {
                                Text="您正在进入开发者控制台，为避免意外，请输入Int32最大值与最小值的和",
                                Margin=new Thickness(0,0,0,5)
                            },
                            textBox
                        }
                    },
                    PrimaryButtonText = "确定",
                    CloseButtonText = "取消",
                    DefaultButton = ContentDialogButton.Primary
                }, (() =>
                {
                    if (textBox.Text == (Int32.MaxValue + Int32.MinValue).ToString())
                    {
                        NavigationService?.Navigate(new PageDeveloper());
                    }
                    else
                    {
                        new NotificationManager().Show(new NotificationContent
                        {
                            Title = "答案错误",
                            Message = $"您无法进入开发者控制台, \"{textBox.Text}\" 是错误的！",
                            Type = NotificationType.Error
                        });
                    }
                }));

            }
            catch (Exception ex)
            {
                ErrorReportDialog.Show("发生错误", null!, ex);
            }
        }
    }
}
