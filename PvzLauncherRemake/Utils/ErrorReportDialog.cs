using ModernWpf.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static PvzLauncherRemake.Class.AppLogger;

namespace PvzLauncherRemake.Utils
{
    public static class ErrorReportDialog
    {
        public static async void Show(string title, string message, Exception ex)
        {
            logger.Error(
                $"{new string('=',10)}ERROR{new string('=', 10)}\n" +
                $"{title}\n" +
                $"{message}\n" +
                $"{ex}\n" +
                $"{new string('=', 10)}ERROR{new string('=', 10)}");

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var exceptionTextBox = new TextBox
            {
                Text = ex.ToString(),
                IsReadOnly = true,
                FontSize = 12,
                Padding = new Thickness(8),
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true
            };

            scrollViewer.Content = exceptionTextBox;

            var content = new StackPanel
            {
                Children =
                {
                    new System.Windows.Controls.ProgressBar
                    {
                        Value = 100,
                        Foreground = new SolidColorBrush(Color.FromRgb(220, 50, 50)),
                        Margin = new Thickness(0,0,0,10)
                    },
                    new TextBlock
                    {
                        Text = message,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0,0,0,10)
                    },
                    scrollViewer,

                    new TextBlock
                    {
                        Text = "您可以全选并复制上方内容反馈给开发者",
                    }
                }
            };

            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                PrimaryButtonText = "继续运行",
                CloseButtonText = "终止程序",
                DefaultButton = ContentDialogButton.Primary
            };

            await DialogManager.ShowDialogAsync(dialog, closeCallback: (() =>
            {
                Environment.Exit(1);
            }));
        }
    }
}